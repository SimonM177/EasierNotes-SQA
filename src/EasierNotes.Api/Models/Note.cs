using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasierNotes.Api.Models
{
    /// <summary>
    /// Réplica fiel del modelo Note del sistema original (src/Backend/Models/Note.cs).
    /// Se conservan las anotaciones de EF Core tal como las escribió el autor.
    ///
    /// OBSERVACIÓN (a exponer por las pruebas de integración):
    ///   - 'Name' NO declara [MaxLength]: el límite de 50/40 caracteres no existe a nivel de modelo.
    ///   - 'Html' NO declara el tipo MEDIUMTEXT explícitamente.
    /// El nombre de tabla usa el esquema "easier_notes" (propio de MySQL). Para las
    /// pruebas con SQLite, el esquema se neutraliza vía configuración del contexto.
    /// </summary>
    [Table("notas")]
    public class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("nombre")]
        public string Name { get; set; } = string.Empty;

        [Column("html")]
        public string Html { get; set; } = string.Empty;

        [Column("categoria_id")]
        public long CategoryId { get; set; }
    }
}
