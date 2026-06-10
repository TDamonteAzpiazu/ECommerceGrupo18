using Dapper;
using Microsoft.Data.Sqlite;
using Cart.API.Models;

namespace Cart.API.Repository
{
    public class CartRepository
    {
        private readonly IConfiguration _config;

        public CartRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=../database/app.db");

        public async Task<ShoppingCart?> GetByUserIdAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();
            var items = await conn.QueryAsync(
                "SELECT * FROM cart_items WHERE usuario_id = @usuarioId",
                new { usuarioId = usuarioId.ToString() });

            var itemList = items.ToList();
            if (!itemList.Any()) return null;

            var fechaActualizacion = await conn.ExecuteScalarAsync<string>(
                "SELECT fecha_actualizacion FROM carts WHERE usuario_id = @usuarioId",
                new { usuarioId = usuarioId.ToString() });

            return new ShoppingCart
            {
                UsuarioId = usuarioId,
                Items = itemList.Select(i => new ShoppingCartItem   
                {
                    UsuarioId = Guid.Parse((string)i.usuario_id),
                    ProductoId = Guid.Parse((string)i.producto_id),
                    Cantidad = (int)(long)i.cantidad
                }).ToList(),
                FechaActualizacion = DateTime.Parse(fechaActualizacion!)
            };
        }

        public async Task UpsertItemAsync(Guid usuarioId, Guid productoId, int cantidad)
        {
            using var conn = CreateConnection();

            // Crear o actualizar el carrito
            await conn.ExecuteAsync("""
                INSERT INTO carts (usuario_id, fecha_actualizacion)
                VALUES (@usuarioId, datetime('now'))
                ON CONFLICT(usuario_id) DO UPDATE SET fecha_actualizacion = datetime('now')
            """, new { usuarioId = usuarioId.ToString() });

            // Crear o actualizar el item
            await conn.ExecuteAsync("""
                INSERT INTO cart_items (usuario_id, producto_id, cantidad)
                VALUES (@usuarioId, @productoId, @cantidad)
                ON CONFLICT(usuario_id, producto_id) DO UPDATE SET cantidad = @cantidad
            """, new { usuarioId = usuarioId.ToString(), productoId = productoId.ToString(), cantidad });
        }

        public async Task<bool> RemoveItemAsync(Guid usuarioId, Guid productoId)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(
                "DELETE FROM cart_items WHERE usuario_id = @usuarioId AND producto_id = @productoId",
                new { usuarioId = usuarioId.ToString(), productoId = productoId.ToString() });

            if (rows > 0)
                await conn.ExecuteAsync(
                    "UPDATE carts SET fecha_actualizacion = datetime('now') WHERE usuario_id = @usuarioId",
                    new { usuarioId = usuarioId.ToString() });

            return rows > 0;
        }

        public async Task<bool> ClearAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(
                "DELETE FROM cart_items WHERE usuario_id = @usuarioId",
                new { usuarioId = usuarioId.ToString() });

            await conn.ExecuteAsync(
                "DELETE FROM carts WHERE usuario_id = @usuarioId",
                new { usuarioId = usuarioId.ToString() });

            return rows > 0;
        }

        public async Task<ShoppingCartItem?> GetItemAsync(Guid usuarioId, Guid productoId)
        {
            using var conn = CreateConnection();
            var result = await conn.QuerySingleOrDefaultAsync(
                "SELECT * FROM cart_items WHERE usuario_id = @usuarioId AND producto_id = @productoId",
                new { usuarioId = usuarioId.ToString(), productoId = productoId.ToString() });

            if (result == null) return null;

            return new ShoppingCartItem
            {
                UsuarioId = Guid.Parse((string)result.usuario_id),
                ProductoId = Guid.Parse((string)result.producto_id),
                Cantidad = (int)(long)result.cantidad
            };
        }
    }
}