using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Repository
{
    public class ProductsRepository
    {
        private readonly IConfiguration _config;

        public ProductsRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=../database/app.db");

        public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
        {
            using var conn = CreateConnection();
            var sql = "SELECT * FROM products WHERE 1=1";
            if (!string.IsNullOrWhiteSpace(categoria))
                sql += " AND categoria = @categoria";
            if (!string.IsNullOrWhiteSpace(nombre))
                sql += " AND nombre LIKE @nombre";

            var result = await conn.QueryAsync(sql, new
            {
                categoria,
                nombre = $"%{nombre}%"
            });

            return result.Select(r => new Product
            {
                Id = Guid.Parse((string)r.id),
                Nombre = (string)r.nombre,
                Descripcion = (string?)r.descripcion,
                Precio = (decimal)(double)r.precio,
                Stock = (int)(long)r.stock,
                Categoria = (string)r.categoria,
                FechaCreacion = DateTime.Parse((string)r.fecha_creacion)
            });
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            var result = await conn.QuerySingleOrDefaultAsync(
                "SELECT * FROM products WHERE id = @id", new { id = id.ToString() });

            if (result == null) return null;

            return new Product
            {
                Id = Guid.Parse((string)result.id),
                Nombre = (string)result.nombre,
                Descripcion = (string?)result.descripcion,
                Precio = (decimal)(double)result.precio,
                Stock = (int)(long)result.stock,
                Categoria = (string)result.categoria,
                FechaCreacion = DateTime.Parse((string)result.fecha_creacion)
            };
        }

        public async Task<Product> CreateAsync(Product product)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                INSERT INTO products (id, nombre, descripcion, precio, stock, categoria)
                VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria)
            """, new
            {
                Id = product.Id.ToString(),
                product.Nombre,
                product.Descripcion,
                product.Precio,
                product.Stock,
                product.Categoria
            });
            return (await GetByIdAsync(product.Id))!;
        }

        public async Task<Product?> UpdateAsync(Guid id, Product product)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("""
                UPDATE products
                SET nombre = @Nombre,
                    descripcion = @Descripcion,
                    precio = @Precio,
                    stock = @Stock,
                    categoria = @Categoria
                WHERE id = @Id
            """, new
            {
                Id = id.ToString(),
                product.Nombre,
                product.Descripcion,
                product.Precio,
                product.Stock,
                product.Categoria
            });
            if (rows == 0) return null;
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(
                "DELETE FROM products WHERE id = @id", new { id = id.ToString() });
            return rows > 0;
        }

        public async Task<bool> ExistsWithNameAndCategoriaAsync(string nombre, string categoria, Guid? excludeId = null)
        {
            using var conn = CreateConnection();
            var sql = "SELECT COUNT(*) FROM products WHERE nombre = @nombre AND categoria = @categoria";
            if (excludeId.HasValue)
                sql += " AND id != @excludeId";
            var count = await conn.ExecuteScalarAsync<long>(sql, new
            {
                nombre,
                categoria,
                excludeId = excludeId?.ToString()
            });
            return count > 0;
        }

        public async Task UpdateStockAsync(Guid id, int nuevoStock)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "UPDATE products SET stock = @nuevoStock WHERE id = @id",
                new { nuevoStock, id = id.ToString() });
        }
    }
}