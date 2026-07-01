using EasierNotes.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EasierNotes.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Proveedor EF Core InMemory. Simula la base de datos enteramente en RAM.
    ///
    /// CARACTERÍSTICAS (a documentar en el informe del DevOps):
    ///   + Rapidísimo, sin instalación, ideal para verificar la LÓGICA de EF y del controlador.
    ///   - NO es un motor SQL real: ignora restricciones de columna (longitud), tipos y
    ///     claves foráneas. Por eso NO detecta truncamientos ni violaciones de integridad.
    ///
    /// Cada instancia usa un nombre de base único (Guid) para aislar las pruebas entre sí.
    /// </summary>
    public sealed class InMemoryDatabaseFixture : IDatabaseFixture
    {
        private readonly string _databaseName = $"EasierNotes_{Guid.NewGuid()}";

        public string ProviderName => "EF Core InMemory";

        public AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public void Dispose()
        {
            // La base InMemory se libera con el recolector; no hay conexión que cerrar.
        }
    }
}
