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
    /// HALLAZGO 3 — Ausencia de límite de longitud en el nombre de la nota.
    ///
    /// La especificación (ERS/DAS) indica un máximo para el nombre (50 en el DAS; el
    /// frontend usa maxlength=40). Sin embargo, el modelo Note NO declara [MaxLength],
    /// por lo que EF Core crea la columna SIN restricción de longitud.
    ///
    /// CONSECUENCIA (a documentar): un nombre de 500 caracteres se persiste COMPLETO,
    /// sin truncarse ni provocar error. La regla de negocio de longitud no existe a
    /// nivel de datos; tendría que aplicarse en el modelo o en el controlador.
    ///
    /// Estos tests fijan ese comportamiento real (characterization testing) en ambos
    /// proveedores, evidenciando que el límite de la ERS no está implementado.
    /// </summary>
    public abstract class NameLengthFindingTests
    {
        private readonly IDatabaseFixture _fixture;

        protected NameLengthFindingTests(IDatabaseFixture fixture)
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

        [Fact(DisplayName = "HALLAZGO: un nombre de 500 caracteres se persiste completo (sin límite)")]
        public async Task Update_NameWith500Chars_PersistsFullLength()
        {
            // Arrange: crear una nota y preparar un nombre de 500 caracteres.
            long id;
            using (var ctx = _fixture.CreateContext())
            {
                var created = await NewController(ctx).Create();
                id = ((Note)((OkObjectResult)created.Result!).Value!).Id;
            }

            string longName = new string('A', 500);

            // Act: actualizar la nota con el nombre largo.
            using (var ctx = _fixture.CreateContext())
            {
                var dto = new Note { Id = id, Name = longName, Html = "<p>x</p>" };
                await NewController(ctx).Update(dto);
            }

            // Assert: el nombre se guardó ÍNTEGRO (no se truncó a 50 ni a 40).
            using var verify = _fixture.CreateContext();
            var persisted = await verify.Notes.FindAsync(id);
            persisted!.Name.Should().HaveLength(500,
                because: "el modelo no declara [MaxLength]; no hay truncamiento");
        }

        [Theory(DisplayName = "Frontera: nombres de 40, 50 y 51 caracteres se guardan sin recorte")]
        [InlineData(40)]
        [InlineData(50)]
        [InlineData(51)]
        public async Task Update_NameAtBoundaryLengths_PersistsUnchanged(int length)
        {
            // Arrange
            long id;
            using (var ctx = _fixture.CreateContext())
            {
                var created = await NewController(ctx).Create();
                id = ((Note)((OkObjectResult)created.Result!).Value!).Id;
            }

            string name = new string('B', length);

            // Act
            using (var ctx = _fixture.CreateContext())
            {
                await NewController(ctx).Update(new Note { Id = id, Name = name, Html = "<p>x</p>" });
            }

            // Assert: la longitud persistida coincide exactamente con la enviada.
            using var verify = _fixture.CreateContext();
            var persisted = await verify.Notes.FindAsync(id);
            persisted!.Name.Should().HaveLength(length);
        }
    }

    public class NameLengthInMemoryTests : NameLengthFindingTests
    {
        public NameLengthInMemoryTests() : base(new InMemoryDatabaseFixture()) { }
    }

    public class NameLengthSqliteTests : NameLengthFindingTests
    {
        public NameLengthSqliteTests() : base(new SqliteDatabaseFixture()) { }
    }
}
