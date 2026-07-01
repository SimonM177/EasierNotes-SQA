using EasierNotes.IntegrationTests.Infrastructure;

namespace EasierNotes.IntegrationTests.Crud
{
    /// <summary>
    /// Ejecuta la batería CRUD completa contra el proveedor EF Core InMemory.
    /// xUnit descubre estos tests heredados y los corre con este fixture.
    /// </summary>
    public class NotesCrudInMemoryTests : NotesCrudIntegrationTests
    {
        public NotesCrudInMemoryTests() : base(new InMemoryDatabaseFixture()) { }
    }

    /// <summary>
    /// Ejecuta la MISMA batería CRUD contra el proveedor SQLite en memoria (SQL real).
    /// </summary>
    public class NotesCrudSqliteTests : NotesCrudIntegrationTests
    {
        public NotesCrudSqliteTests() : base(new SqliteDatabaseFixture()) { }
    }
}
