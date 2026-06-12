using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Repository
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=../database/app.db");

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = CreateConnection();
            var result = await conn.QuerySingleOrDefaultAsync(
                "SELECT * FROM users WHERE email = @email", new { email });

            if (result == null) return null;

            return new User
            {
                Id = Guid.Parse((string)result.id),
                Nombre = (string)result.nombre,
                Apellido = (string)result.apellido,
                Email = (string)result.email,
                PasswordHash = (string)result.password_hash,
                FechaRegistro = DateTime.Parse((string)result.fecha_registro),
                Activo = (long)result.activo == 1,
                IntentosFallidos = (int)(long)result.intentos_fallidos
            };
        }

        public async Task<User> CreateAsync(User user)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                INSERT INTO users (id, nombre, apellido, email, password_hash)
                VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash)
            """, new
            {
                Id = user.Id.ToString(),
                user.Nombre,
                user.Apellido,
                user.Email,
                user.PasswordHash
            });
            return await GetByEmailAsync(user.Email);
        }

        public async Task UpdateAsync(User user)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
                UPDATE users 
                SET activo = @Activo, intentos_fallidos = @IntentosFallidos
                WHERE id = @Id
            """, new
            {
                Activo = user.Activo ? 1 : 0,
                user.IntentosFallidos,
                Id = user.Id.ToString()
            });
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            var result = await conn.QuerySingleOrDefaultAsync(
                "SELECT * FROM users WHERE id = @id", new { id = id.ToString() });

            if (result == null) return null;

            return new User
            {
                Id = Guid.Parse((string)result.id),
                Nombre = (string)result.nombre,
                Apellido = (string)result.apellido,
                Email = (string)result.email,
                PasswordHash = (string)result.password_hash,
                FechaRegistro = DateTime.Parse((string)result.fecha_registro),
                Activo = (long)result.activo == 1,
                IntentosFallidos = (int)(long)result.intentos_fallidos
            };
        }
    }
}