namespace EasierNotes.Domain.Notes
{
    /// <summary>
    /// Réplica FIEL de la lógica de nombres del autor del sistema, extraída tal cual
    /// del método NotesController.Create():
    ///
    ///     Name = defaultNamesQty > 0 ? $"Nueva Nota ({defaultNamesQty + 1})" : "Nueva Nota";
    ///
    /// Esta clase no "mejora" nada: existe para PROBAR el comportamiento real del
    /// código original mediante pruebas de caracterización (characterization tests).
    /// Su valor es doble: documentar lo que el código hace hoy y, al hacerlo,
    /// EXPONER el defecto de nombres duplicados cuando se borra una nota intermedia.
    ///
    /// Patrón aplicado: Pure Function / Domain Service. La regla queda aislada del
    /// acceso a datos (el CountAsync a EF se sustituye por el parámetro 'existingDefaultCount').
    /// </summary>
    public sealed class LegacyNoteNameGenerator
    {
        public const string BaseName = "Nueva Nota";

        /// <summary>
        /// Genera el nombre de la siguiente nota por defecto, replicando la lógica original.
        /// </summary>
        /// <param name="existingDefaultCount">
        /// Cantidad de notas existentes cuyo nombre empieza por "Nueva Nota"
        /// (equivale al resultado de CountAsync(n => n.Name.StartsWith("Nueva Nota"))).
        /// </param>
        public string GenerateName(int existingDefaultCount)
        {
            return existingDefaultCount > 0
                ? $"{BaseName} ({existingDefaultCount + 1})"
                : BaseName;
        }
    }
}
