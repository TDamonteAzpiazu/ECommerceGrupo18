using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Repository
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=../database/app.db");

        public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId)
        {
            using var conn = CreateConnection();
            var sql = "SELECT * FROM orders WHERE 1=1";
            if (usuarioId.HasValue)
                sql += " AND usuario_id = @usuarioId";

            var orders = await conn.QueryAsync(sql, new { usuarioId = usuarioId?.ToString() });

            var result = new List<Order>();
            foreach (var o in orders)
            {
                var order = MapOrder(o);
                var items = await conn.QueryAsync(
                    "SELECT * FROM order_items WHERE order_id = @orderId",
                    new { orderId = order.Id.ToString() });

                order.Items = items.Select(i => new OrderItem
                {
                    Id = Guid.Parse((string)i.id),
                    OrderId = Guid.Parse((string)i.order_id),
                    ProductoId = Guid.Parse((string)i.producto_id),
                    Cantidad = (int)(long)i.cantidad,
                    PrecioUnitario = (decimal)(double)i.precio_unitario
                }).ToList();

                result.Add(order);
            }
            return result;
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            var result = await conn.QuerySingleOrDefaultAsync(
                "SELECT * FROM orders WHERE id = @id", new { id = id.ToString() });

            if (result == null) return null;

            var order = MapOrder(result);
            var items = await conn.QueryAsync(
                "SELECT * FROM order_items WHERE order_id = @orderId",
                new { orderId = order.Id.ToString() });

            order.Items = items.Select(i => new OrderItem
            {
                Id = Guid.Parse((string)i.id),
                OrderId = Guid.Parse((string)i.order_id),
                ProductoId = Guid.Parse((string)i.producto_id),
                Cantidad = (int)(long)i.cantidad,
                PrecioUnitario = (decimal)(double)i.precio_unitario
            }).ToList();

            return order;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                INSERT INTO orders (id, usuario_id, total, estado)
                VALUES (@Id, @UsuarioId, @Total, @Estado)
            """, new
            {
                Id = order.Id.ToString(),
                UsuarioId = order.UsuarioId.ToString(),
                order.Total,
                order.Estado
            });

            foreach (var item in order.Items)
            {
                await conn.ExecuteAsync("""
                    INSERT INTO order_items (id, order_id, producto_id, cantidad, precio_unitario)
                    VALUES (@Id, @OrderId, @ProductoId, @Cantidad, @PrecioUnitario)
                """, new
                {
                    Id = item.Id.ToString(),
                    OrderId = order.Id.ToString(),
                    ProductoId = item.ProductoId.ToString(),
                    item.Cantidad,
                    item.PrecioUnitario
                });
            }

            return (await GetByIdAsync(order.Id))!;
        }

        public async Task<Order?> UpdateStatusAsync(Guid id, string estado)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("""
                UPDATE orders SET estado = @estado WHERE id = @id
            """, new { estado, id = id.ToString() });

            if (rows == 0) return null;
            return await GetByIdAsync(id);
        }

        private Order MapOrder(dynamic o) => new()
        {
            Id = Guid.Parse((string)o.id),
            UsuarioId = Guid.Parse((string)o.usuario_id),
            Total = (decimal)(double)o.total,
            Estado = (string)o.estado,
            FechaCreacion = DateTime.Parse((string)o.fecha_creacion)
        };

        public async Task<bool> HasActiveOrdersForProductAsync(Guid productoId)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<long>("""
                    SELECT COUNT(*) FROM order_items oi
                    INNER JOIN orders o ON o.id = oi.order_id
                    WHERE oi.producto_id = @productoId
                    AND o.estado IN ('Pendiente', 'Confirmada')
            """, new { productoId = productoId.ToString() });
            return count > 0;
        }
    }
}