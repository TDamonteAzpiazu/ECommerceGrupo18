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
                order.Items = (await GetItemsByOrderIdAsync(order.Id, conn)).ToList();
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
            order.Items = (await GetItemsByOrderIdAsync(order.Id, conn)).ToList();
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

        private async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(Guid orderId, SqliteConnection conn)
        {
            var items = await conn.QueryAsync(
                "SELECT * FROM order_items WHERE order_id = @orderId",
                new { orderId = orderId.ToString() });

            return items.Select(i => new OrderItem
            {
                Id = Guid.Parse((string)i.id),
                OrderId = Guid.Parse((string)i.order_id),
                ProductoId = Guid.Parse((string)i.producto_id),
                Cantidad = (int)(long)i.cantidad,
                PrecioUnitario = (decimal)(double)i.precio_unitario
            });
        }

        private Order MapOrder(dynamic o) => new()
        {
            Id = Guid.Parse((string)o.id),
            UsuarioId = Guid.Parse((string)o.usuario_id),
            Total = (decimal)(double)o.total,
            Estado = (string)o.estado,
            FechaCreacion = DateTime.Parse((string)o.fecha_creacion)
        };
    }
}