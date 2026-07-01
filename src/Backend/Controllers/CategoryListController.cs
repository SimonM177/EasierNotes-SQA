using backend.Models;
using backend.PuertaDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            _logger.LogInformation("GET api/categories called");
            var categories = await _context.Categories.Include(c => c.Notes).ToListAsync();
            _logger.LogInformation("Returning {Count} categories", categories.Count);
            return Ok(categories);
        }

        [HttpPost]
        [Route("create/{name}")]
        public async Task<ActionResult<Category>> Create([FromRoute] string name)
        {
            _logger.LogInformation("POST api/categories called with {@Category}", name);
            var category = new Category { Name = name };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created category with id {Id}", category.Id);

            return Ok(category);
        }

        [HttpPut("update/{id:long}/{name}")]
        public async Task<IActionResult> Update(
            [FromRoute] long id,
            [FromRoute] string name)
        {
            _logger.LogInformation("PUT api/categories/{Id} called with {@Category}", id, name);

            var existing = await _context.Categories.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Note with id {Id} not found", id);
                return NotFound();
            }

            existing.Name = name;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated note with id {Id}", id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving changes for note {Id}", id);
                throw; // o return StatusCode(500, "Error al guardar la nota");
            }

            return NoContent();
        }

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            _logger.LogInformation("DELETE api/categories/{Id} called", id);

            var existing = await _context.Categories.FindAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Category with id {Id} not found", id);
                return NotFound();
            }

            _context.Categories.Remove(existing);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted category with id {Id}", id);

            return NoContent();
        }
    }
}
