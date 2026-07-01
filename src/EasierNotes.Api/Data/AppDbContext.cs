using EasierNotes.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EasierNotes.Api.Data
{
    /// <summary>
    /// Réplica del contexto de datos del sistema original (src/Backend/DoorDB/AppDbContext.cs).
    /// Expone los DbSet de Notes y Categories. La configuración del proveedor
    /// (MySQL en producción; InMemory o SQLite en pruebas) se inyecta desde fuera
    /// mediante DbContextOptions, sin acoplar el contexto a un motor concreto.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Note> Notes { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
    }
}
