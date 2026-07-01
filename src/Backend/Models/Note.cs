using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    [Table("notas", Schema = "easier_notes")]
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