using EasierNotes.Api.Data;
using EasierNotes.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasierNotes.Api.Controllers
{
    /// <summary>
    /// Réplica FIEL del NotesController del sistema original
    /// (src/Backend/Controllers/NoteListController.cs).
    ///
    /// La lógica no se altera: es el objeto real bajo prueba de integración.
    /// Las pruebas ejercitan estos métodos contra una base de datos (InMemory / SQLite)
    /// para verificar el comportamiento integrado del controlador + EF Core + BD.
    /// </summary>
    [ApiController]
    [Route("api/notes")]
    public class NotesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotesController> _logger;

        public NotesController(AppDbContext context, ILogger<NotesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Note>>> GetAll()
        {
            var notes = await _context.Notes.ToListAsync();
            return Ok(notes);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<Note>> GetById([FromRoute] long id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();
            return Ok(note);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Note>> Create()
        {
            var defaultNamesQty = await _context.Notes.CountAsync(n => n.Name.StartsWith("Nueva Nota"));

            var note = new Note
            {
                Name = defaultNamesQty > 0 ? $"Nueva Nota ({defaultNamesQty + 1})" : "Nueva Nota",
                Html = "<p> Comienza a plasmar tus ideas aquí...</p>",
                CategoryId = 1
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return Ok(note);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] Note noteDto)
        {
            var existing = await _context.Notes.FindAsync(noteDto.Id);
            if (existing == null)
                return NotFound();

            existing.Name = noteDto.Name;
            existing.Html = noteDto.Html;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes for note {Id}", noteDto.Id);
                throw;
            }

            return NoContent();
        }

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var existing = await _context.Notes.FindAsync(id);
            if (existing == null)
                return NotFound();

            _context.Notes.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("addToCategory/{noteId:long}/{categoryId:long}")]
        public async Task<IActionResult> AddToCategory([FromRoute] long noteId, [FromRoute] long categoryId)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note == null)
                return NotFound();

            note.CategoryId = categoryId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes for note {Id}", noteId);
                throw;
            }

            return NoContent();
        }
    }
}
