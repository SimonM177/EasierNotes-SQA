using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasierNotes.Api.Models
{
    /// <summary>
    /// Réplica fiel del modelo Category del sistema original (src/Backend/Models/Category.cs).
    /// El esquema "easier_notes" del [Table] original se omite para compatibilidad con
    /// SQLite en el entorno de pruebas (ver nota en Note.cs y en la clase base de tests).
    /// </summary>
    [Table("categorias")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("nombre")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
