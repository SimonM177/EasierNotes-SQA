using EasierNotes.Api.Controllers;
using EasierNotes.Api.Data;
using EasierNotes.Api.Models;
using EasierNotes.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EasierNotes.IntegrationTests.Crud
{
    /// <summary>
    /// Pruebas de integración del CRUD básico de notas — ejercen el NotesController
    /// REAL contra una base de datos real (a través de EF Core).
    ///
    /// PATRÓN DE DOBLE PROVEEDOR: esta clase es ABSTRACTA y define los casos una sola vez.
    /// Las clases derivadas (una por proveedor: InMemory y SQLite) inyectan el fixture
    /// concreto, de modo que la MISMA batería de pruebas se ejecuta contra ambos motores.
    ///
    /// Alcance de esta primera tanda: operaciones CRUD básicas (crear, leer, actualizar,
    /// borrar). Los casos que exigen comportamiento de un SQL real (truncamiento,
    /// integridad referencial) se abordan en una segunda tanda.
    /// </summary>
    public abstract class NotesCrudIntegrationTests
    {
        private readonly IDatabaseFixture _fixture;

        protected NotesCrudIntegrationTests(IDatabaseFixture fixture)
        {
            _fixture = fixture;
            SeedDefaultCategory();
        }

        /// <summary>
        /// Prepara el estado previo mínimo: asegura que exista la categoría con id = 1.
        /// El controlador crea notas con CategoryId = 1; en un motor SQL real (SQLite/MySQL)
        /// esa clave foránea DEBE apuntar a una categoría existente, o la inserción falla.
        /// Sembrar la categoría es el "Arrange" correcto para probar el CRUD de notas de
        /// forma aislada. (El caso que documenta la ausencia de la categoría se aborda por
        /// separado, como hallazgo intencional.)
        /// </summary>
        private void SeedDefaultCategory()
        {
            using var context = _fixture.CreateContext();
            if (!context.Categories.Any(c => c.Id == 1))
            {
                context.Categories.Add(new Category { Id = 1, Name = "Otras" });
                context.SaveChanges();
            }
        }

        /// <summary>Crea un controlador real conectado a un contexto nuevo del proveedor.</summary>
        private NotesController CreateController(AppDbContext context)
            => new(context, NullLogger<NotesController>.Instance);

        // =================================================================
        //  CREATE
        // =================================================================

        [Fact(DisplayName = "CREATE: la primera nota se persiste con nombre 'Nueva Nota'")]
        public async Task Create_FirstNote_PersistsWithBaseName()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);

            // Act
            var result = await controller.Create();

            // Assert (respuesta HTTP)
            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var note = ok.Value.Should().BeOfType<Note>().Subject;
            note.Name.Should().Be("Nueva Nota");

            // Assert (persistencia real: se lee con un contexto NUEVO para forzar ir a la BD)
            using var verifyContext = _fixture.CreateContext();
            var persisted = await verifyContext.Notes.FindAsync(note.Id);
            persisted.Should().NotBeNull();
            persisted!.Html.Should().Be("<p> Comienza a plasmar tus ideas aquí...</p>");
            persisted.CategoryId.Should().Be(1);
        }

        [Fact(DisplayName = "CREATE: la segunda nota por defecto recibe el sufijo (2)")]
        public async Task Create_SecondDefaultNote_GetsSuffixTwo()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);

            // Act: dos creaciones consecutivas
            await controller.Create();
            var second = await controller.Create();

            // Assert
            var note = (second.Result as OkObjectResult)!.Value as Note;
            note!.Name.Should().Be("Nueva Nota (2)");

            // Y en la BD hay exactamente 2 notas
            using var verify = _fixture.CreateContext();
            (await verify.Notes.CountAsync()).Should().Be(2);
        }

        [Fact(DisplayName = "CREATE: asigna Id autogenerado (> 0) tras persistir")]
        public async Task Create_Note_AssignsGeneratedId()
        {
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);

            var result = await controller.Create();

            var note = (result.Result as OkObjectResult)!.Value as Note;
            note!.Id.Should().BeGreaterThan(0);
        }

        // =================================================================
        //  READ
        // =================================================================

        [Fact(DisplayName = "READ: GetById devuelve la nota existente")]
        public async Task GetById_ExistingNote_ReturnsIt()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);
            var created = await controller.Create();
            var createdNote = (created.Result as OkObjectResult)!.Value as Note;

            // Act
            var result = await controller.GetById(createdNote!.Id);

            // Assert
            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var note = ok.Value.Should().BeOfType<Note>().Subject;
            note.Id.Should().Be(createdNote.Id);
        }

        [Fact(DisplayName = "READ: GetById de un id inexistente devuelve NotFound")]
        public async Task GetById_NonExistingNote_ReturnsNotFound()
        {
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);

            var result = await controller.GetById(9999);

            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact(DisplayName = "READ: GetAll devuelve todas las notas creadas")]
        public async Task GetAll_ReturnsAllNotes()
        {
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);
            await controller.Create();
            await controller.Create();
            await controller.Create();

            var result = await controller.GetAll();

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var notes = ok.Value.Should().BeAssignableTo<List<Note>>().Subject;
            notes.Should().HaveCount(3);
        }

        // =================================================================
        //  UPDATE
        // =================================================================

        [Fact(DisplayName = "UPDATE: modifica nombre y html de una nota existente")]
        public async Task Update_ExistingNote_ChangesNameAndHtml()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);
            var created = await controller.Create();
            var id = ((created.Result as OkObjectResult)!.Value as Note)!.Id;

            // Act
            var dto = new Note { Id = id, Name = "Reunión Q3", Html = "<p>contenido nuevo</p>" };
            var result = await controller.Update(dto);

            // Assert (respuesta)
            result.Should().BeOfType<NoContentResult>();

            // Assert (persistencia con contexto nuevo)
            using var verify = _fixture.CreateContext();
            var updated = await verify.Notes.FindAsync(id);
            updated!.Name.Should().Be("Reunión Q3");
            updated.Html.Should().Be("<p>contenido nuevo</p>");
        }

        [Fact(DisplayName = "UPDATE: sobre id inexistente devuelve NotFound")]
        public async Task Update_NonExistingNote_ReturnsNotFound()
        {
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);

            var dto = new Note { Id = 12345, Name = "x", Html = "y" };
            var result = await controller.Update(dto);

            result.Should().BeOfType<NotFoundResult>();
        }

        // =================================================================
        //  DELETE
        // =================================================================

        [Fact(DisplayName = "DELETE: elimina una nota existente y ya no se encuentra")]
        public async Task Delete_ExistingNote_RemovesIt()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);
            var created = await controller.Create();
            var id = ((created.Result as OkObjectResult)!.Value as Note)!.Id;

            // Act
            var result = await controller.Delete(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();

            using var verify = _fixture.CreateContext();
            (await verify.Notes.FindAsync(id)).Should().BeNull();
        }

        [Fact(DisplayName = "DELETE: sobre id inexistente devuelve NotFound")]
        public async Task Delete_NonExistingNote_ReturnsNotFound()
        {
            using var context = _fixture.CreateContext();
            var controller = CreateController(context);

            var result = await controller.Delete(55555);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
