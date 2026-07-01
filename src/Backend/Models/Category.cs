using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    [Table("categorias", Schema = "easier_notes")]
    public class Category
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }


        [Column("nombre")]
        public string Name { get; set; } = string.Empty;


        public ICollection<Note> Notes { get; set; } = [];

    }
}