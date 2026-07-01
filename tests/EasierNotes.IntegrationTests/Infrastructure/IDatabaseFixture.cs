using EasierNotes.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace EasierNotes.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Contrato de infraestructura para las pruebas de integración.
    ///
    /// Implementa el patrón de "doble proveedor": las pruebas se escriben UNA vez
    /// contra esta abstracción y se ejecutan con cada motor de base de datos
    /// (EF Core InMemory y SQLite en memoria), sin duplicar el código de prueba.
    ///
    /// Cada implementación concreta decide cómo crear el AppDbContext y su ciclo de vida.
    /// </summary>
    public interface IDatabaseFixture : IDisposable
    {
        /// <summary>Crea un contexto listo para usar contra la base de datos del proveedor.</summary>
        AppDbContext CreateContext();

        /// <summary>Nombre del proveedor, para identificar el origen en los resultados.</summary>
        string ProviderName { get; }
    }
}
