using Dapper;
using Microsoft.Data.Sqlite;

namespace Products.API.Data
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration config, ILogger<DatabaseInitializer> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void Initialize()
        {
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "database"));
            Directory.CreateDirectory(basePath);
            var dbPath = Path.Combine(basePath, "app.db");
            var connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS products (
                    id             TEXT    PRIMARY KEY,
                    nombre         TEXT    NOT NULL,
                    descripcion    TEXT,
                    precio         REAL    NOT NULL,
                    stock          INTEGER NOT NULL DEFAULT 0,
                    categoria      TEXT    NOT NULL,
                    fecha_creacion TEXT    NOT NULL DEFAULT (datetime('now'))
                );
            """);

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS users (
                    id                 TEXT    PRIMARY KEY,
                    nombre             TEXT    NOT NULL,
                    apellido           TEXT    NOT NULL,
                    email              TEXT    NOT NULL UNIQUE,
                    password_hash      TEXT    NOT NULL,
                    fecha_registro     TEXT    NOT NULL DEFAULT (datetime('now')),
                    activo             INTEGER NOT NULL DEFAULT 1,
                    intentos_fallidos  INTEGER NOT NULL DEFAULT 0
                );
            """);

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS orders (
                    id             TEXT    PRIMARY KEY,
                    usuario_id     TEXT    NOT NULL,
                    total          REAL    NOT NULL DEFAULT 0,
                    estado         TEXT    NOT NULL DEFAULT 'Pendiente',
                    fecha_creacion TEXT    NOT NULL DEFAULT (datetime('now'))
                );
            """);

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS order_items (
                    id               TEXT    PRIMARY KEY,
                    order_id         TEXT    NOT NULL,
                    producto_id      TEXT    NOT NULL,
                    cantidad         INTEGER NOT NULL,
                    precio_unitario  REAL    NOT NULL,
                    FOREIGN KEY (order_id) REFERENCES orders(id)
                );
            """);

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS carts (
                    usuario_id          TEXT    PRIMARY KEY,
                    fecha_actualizacion TEXT    NOT NULL DEFAULT (datetime('now'))
                );
            """);

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS cart_items (
                    usuario_id  TEXT    NOT NULL,
                    producto_id TEXT    NOT NULL,
                    cantidad    INTEGER NOT NULL,
                    PRIMARY KEY (usuario_id, producto_id)
                );
            """);

            connection.Execute("""
                CREATE TABLE IF NOT EXISTS notifications (
                    id          TEXT    PRIMARY KEY,
                    usuario_id  TEXT    NOT NULL,
                    mensaje     TEXT    NOT NULL,
                    tipo        TEXT    NOT NULL,
                    estado      TEXT    NOT NULL DEFAULT 'Pendiente',
                    fecha_envio TEXT    NOT NULL DEFAULT (datetime('now'))
                );
            """);

            _logger.LogInformation("SQLite inicializado correctamente → {db}", connectionString);
        }
    }
}