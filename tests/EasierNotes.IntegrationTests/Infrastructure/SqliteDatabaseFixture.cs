using EasierNotes.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EasierNotes.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Proveedor SQLite en memoria. Es un MOTOR SQL REAL corriendo en RAM.
    ///
    /// CARACTERÍSTICAS (a documentar en el informe del DevOps):
    ///   + Respeta transacciones, tipos y claves foráneas: MUCHO más fiel que InMemory.
    ///   + Gratis y sin instalar un servidor de base de datos.
    ///   - No es MySQL: hay diferencias de dialecto puntuales (p. ej. el manejo de
    ///     longitudes de texto es más laxo que en MySQL).
    ///
    /// DETALLE TÉCNICO CLAVE: la base "in-memory" de SQLite vive mientras haya al menos
    /// una conexión abierta. Por eso se mantiene UNA conexión viva durante toda la vida
    /// del fixture; si se cerrara, la base y sus datos desaparecerían.
    /// </summary>
    public sealed class SqliteDatabaseFixture : IDatabaseFixture
    {
        private readonly SqliteConnection _connection;

        public string ProviderName => "SQLite (en memoria)";

        public SqliteDatabaseFixture()
        {
            // "DataSource=:memory:" + conexión persistente = base viva durante el fixture.
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Crea el esquema una sola vez sobre esta conexión.
            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        public AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection) // reutiliza la MISMA conexión abierta
                .Options;

            return new AppDbContext(options);
        }

        public void Dispose()
        {
            // Al cerrar la conexión se destruye la base en memoria. Se hace al final de todo.
            _connection.Dispose();
        }
    }
}
