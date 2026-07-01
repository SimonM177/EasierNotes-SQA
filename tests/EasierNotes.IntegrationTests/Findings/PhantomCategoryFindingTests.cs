using EasierNotes.Api.Controllers;
using EasierNotes.Api.Models;
using EasierNotes.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EasierNotes.IntegrationTests.Findings
{
    /// <summary>
    /// HALLAZGO 1 — Categoría fantasma (CategoryId = 1 hardcodeado).
    ///
    /// El NotesController crea toda nota con CategoryId = 1, asumiendo que esa categoría
    /// existe. Estos tests DOCUMENTAN ese acoplamiento como comportamiento verificado:
    ///   - En un motor SQL real (SQLite/MySQL) que aplica integridad referencial,
    ///     crear la nota SIN la categoría 1 FALLA con una violación de clave foránea.
    ///   - Con la categoría sembrada, la creación tiene éxito.
    ///
    /// Se ejecuta contra SQLite (motor SQL real). InMemory se omite deliberadamente
    /// porque NO aplica claves foráneas y, por tanto, no puede evidenciar este hallazgo.
    ///
    /// Técnica (ISO/IEC/IEEE 29119): prueba negativa / de robustez sobre integridad referencial.
    /// </summary>
    public class PhantomCategoryFindingTests
    {
        private static NotesController CreateController(Api.Data.AppDbContext ctx)
            => new(ctx, NullLogger<NotesController>.Instance);

        [Fact(DisplayName = "HALLAZGO: crear nota SIN la categoría 1 falla en SQL real (FK)")]
        public async Task Create_WithoutSeededCategory_ThrowsForeignKeyViolation()
        {
            // Arrange: base SQLite vacía, SIN sembrar la categoría 1.
            using var fixture = new SqliteDatabaseFixture();
            using var context = fixture.CreateContext();
            var controller = CreateController(context);

            // Act: la creación intenta insertar una nota con CategoryId = 1 inexistente.
            var act = async () => await controller.Create();

            // Assert: el motor SQL real rechaza la operación por violación de clave foránea.
            await act.Should().ThrowAsync<DbUpdateException>(
                because: "CategoryId = 1 apunta a una categoría que no existe");
        }

        [Fact(DisplayName = "Control: con la categoría 1 sembrada, la creación tiene éxito")]
        public async Task Create_WithSeededCategory_Succeeds()
        {
            // Arrange: misma base, pero ahora SÍ sembramos la categoría 1.
            using var fixture = new SqliteDatabaseFixture();
            using (var seed = fixture.CreateContext())
            {
                seed.Categories.Add(new Category { Id = 1, Name = "Otras" });
                await seed.SaveChangesAsync();
            }

            using var context = fixture.CreateContext();
            var controller = CreateController(context);

            // Act
            var result = await controller.Create();

            // Assert: ahora la nota se crea sin problemas.
            var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
            var note = ok.Value.Should().BeOfType<Note>().Subject;
            note.CategoryId.Should().Be(1);
        }
    }
}
