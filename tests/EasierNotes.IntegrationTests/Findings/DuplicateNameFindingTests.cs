using EasierNotes.Api.Controllers;
using EasierNotes.Api.Data;
using EasierNotes.Api.Models;
using EasierNotes.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EasierNotes.IntegrationTests.Findings
{
    /// <summary>
    /// HALLAZGO 2 — Nombres duplicados al borrar una nota intermedia.
    ///
    /// En las pruebas UNITARIAS (CP-PE-02) se demostró que la lógica basada en conteo
    /// (CountAsync sobre "Nueva Nota%") genera un sufijo repetido tras un borrado intermedio.
    /// Aquí se verifica el MISMO defecto de punta a punta: controlador real + EF + base real.
    ///
    /// Escenario: crear 3 notas por defecto, borrar la intermedia, crear una cuarta.
    /// Resultado observado: el nombre de la nueva colisiona con uno ya existente.
    ///
    /// Se ejecuta contra ambos proveedores (es lógica del controlador, no depende de FK).
    /// Requiere la categoría 1 sembrada para que las inserciones sean válidas en SQL real.
    /// </summary>
    public abstract class DuplicateNameFindingTests
    {
        private readonly IDatabaseFixture _fixture;

        protected DuplicateNameFindingTests(IDatabaseFixture fixture)
        {
            _fixture = fixture;
            using var ctx = _fixture.CreateContext();
            if (!ctx.Categories.Any(c => c.Id == 1))
            {
                ctx.Categories.Add(new Category { Id = 1, Name = "Otras" });
                ctx.SaveChanges();
            }
        }

        private NotesController NewController(AppDbContext ctx)
            => new(ctx, NullLogger<NotesController>.Instance);

        private async Task<Note> CreateNote()
        {
            using var ctx = _fixture.CreateContext();
            var result = await NewController(ctx).Create();
            return (Note)((OkObjectResult)result.Result!).Value!;
        }

        [Fact(DisplayName = "HALLAZGO: tras borrar la nota intermedia, la nueva nota duplica un nombre")]
        public async Task Create_AfterDeletingMiddleNote_ProducesDuplicateName()
        {
            // Arrange: crear 3 notas -> "Nueva Nota", "Nueva Nota (2)", "Nueva Nota (3)".
            var n1 = await CreateNote();
            var n2 = await CreateNote();
            var n3 = await CreateNote();

            n1.Name.Should().Be("Nueva Nota");
            n2.Name.Should().Be("Nueva Nota (2)");
            n3.Name.Should().Be("Nueva Nota (3)");

            // Act 1: borrar la intermedia ("Nueva Nota (2)").
            using (var ctx = _fixture.CreateContext())
            {
                await NewController(ctx).Delete(n2.Id);
            }

            // Act 2: crear una nueva nota. El conteo de "Nueva Nota%" vuelve a 2,
            // por lo que la lógica genera "Nueva Nota (3)"... que YA EXISTE (n3).
            var n4 = await CreateNote();

            // Assert: se confirma la colisión de nombres (defecto).
            n4.Name.Should().Be("Nueva Nota (3)");

            using var verify = _fixture.CreateContext();
            var withThatName = await verify.Notes.CountAsync(n => n.Name == "Nueva Nota (3)");
            withThatName.Should().Be(2, because: "existen dos notas con el mismo nombre: el defecto se materializó");
        }
    }

    public class DuplicateNameInMemoryTests : DuplicateNameFindingTests
    {
        public DuplicateNameInMemoryTests() : base(new InMemoryDatabaseFixture()) { }
    }

    public class DuplicateNameSqliteTests : DuplicateNameFindingTests
    {
        public DuplicateNameSqliteTests() : base(new SqliteDatabaseFixture()) { }
    }
}
