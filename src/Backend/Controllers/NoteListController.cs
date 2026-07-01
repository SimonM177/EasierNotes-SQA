using backend.Models;
using backend.PuertaDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
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
            _logger.LogInformation("GET api/notes called");
            var notes = await _context.Notes.ToListAsync();
            _logger.LogInformation("Returning {Count} notes", notes.Count);
            return Ok(notes);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<Note>> GetById([FromRoute] long id)
        {
            _logger.LogInformation("GET api/notes/{Id} called", id);
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                _logger.LogWarning("Note with id {Id} not found", id);
                return NotFound();
            }
            _logger.LogInformation("Returning note {@Note}", note);
            return Ok(note);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Note>> Create()
        {
          var defaultNamesQty = await _context.Notes.CountAsync(n => n.Name.StartsWith("Nueva Nota"));

          var note = new Note {
            Name = defaultNamesQty > 0 ? $"Nueva Nota ({defaultNamesQty + 1})" : "Nueva Nota",
            Html = "<p> Comienza a plasmar tus ideas aquí...</p>",
            CategoryId = 1
          };
         
          _context.Notes.Add(note);
          await _context.SaveChangesAsync();
          return Ok(note);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(
            [FromBody] Note noteDto)
        {
            _logger.LogInformation("PUT api/notes/{Id} called with {@NoteDto}", noteDto.Id, noteDto);

            var existing = await _context.Notes.FindAsync(noteDto.Id);
            if (existing == null)
            {
                _logger.LogWarning("Note with id {Id} not found", noteDto.Id);
                return NotFound();
            }

            existing.Name       = noteDto.Name;
            existing.Html       = noteDto.Html;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated note with id {Id}", noteDto.Id);
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
            _logger.LogInformation("DELETE api/notes/{Id} called", id);

            var existing = await _context.Notes.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Note with id {Id} not found", id);
                return NotFound();
            }

            _context.Notes.Remove(existing);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted note with id {Id}", id);

            return NoContent();
        }

        [HttpPatch("addToCategory/{noteId:long}/{categoryId:long}")]
        public async Task<IActionResult> AddToCategory([FromRoute] long noteId, [FromRoute] long categoryId)
        {
            _logger.LogInformation("POST api/notes/addToCategory/{NoteId}/{CategoryId} called", noteId, categoryId);

            var note = await _context.Notes.FindAsync(noteId);
            if (note == null)
            {
                _logger.LogWarning("Note with id {Id} not found", noteId);
                return NotFound();
            }

            note.CategoryId = categoryId;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added note {NoteId} to category {CategoryId}", noteId, categoryId);
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
