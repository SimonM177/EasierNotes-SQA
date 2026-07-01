namespace EasierNotes.Domain.Notes
{
    /// <summary>
    /// Espejo del modelo de dominio Note del sistema (src/Backend/Models/Note.cs).
    /// Se replica aquí, sin las anotaciones de EF Core ([Table], [Column], etc.),
    /// para que la lógica de dominio y sus pruebas sean autónomas y no arrastren
    /// la dependencia de Entity Framework a la capa unitaria.
    ///
    /// Los nombres y tipos de propiedad coinciden con el modelo real:
    ///   Id (long), Name (string), Html (string), CategoryId (long).
    /// </summary>
    public sealed class Note
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Html { get; set; } = string.Empty;
        public long CategoryId { get; set; }
    }
}
