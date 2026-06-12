using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Repository
{
    public class NotificationRepository
    {
        private readonly IConfiguration _config;

        public NotificationRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=../database/app.db");

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid usuarioId)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryAsync(
                "SELECT * FROM notifications WHERE usuario_id = @usuarioId ORDER BY fecha_envio DESC",
                new { usuarioId = usuarioId.ToString() });

            return result.Select(r => new Notification
            {
                Id = Guid.Parse((string)r.id),
                UsuarioId = Guid.Parse((string)r.usuario_id),
                Mensaje = (string)r.mensaje,
                Tipo = (string)r.tipo,
                Estado = (string)r.estado,
                FechaEnvio = DateTime.Parse((string)r.fecha_envio)
            });
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                INSERT INTO notifications (id, usuario_id, mensaje, tipo, estado)
                VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado)
            """, new
            {
                Id = notification.Id.ToString(),
                UsuarioId = notification.UsuarioId.ToString(),
                notification.Mensaje,
                notification.Tipo,
                notification.Estado
            });

            var result = await conn.QuerySingleOrDefaultAsync(
                "SELECT * FROM notifications WHERE id = @id",
                new { id = notification.Id.ToString() });

            return new Notification
            {
                Id = Guid.Parse((string)result.id),
                UsuarioId = Guid.Parse((string)result.usuario_id),
                Mensaje = (string)result.mensaje,
                Tipo = (string)result.tipo,
                Estado = (string)result.estado,
                FechaEnvio = DateTime.Parse((string)result.fecha_envio)
            };
        }
    }
}