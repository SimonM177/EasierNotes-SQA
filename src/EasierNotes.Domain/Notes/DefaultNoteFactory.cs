using System;
using System.Collections.Generic;

namespace EasierNotes.Domain.Notes
{
    /// <summary>
    /// Fábrica de notas "por defecto" (Factory Method, GoF).
    ///
    /// Centraliza la construcción de una nota nueva tal como la realiza
    /// NotesController.Create(): nombre generado, HTML inicial y categoría por defecto.
    /// Sacar estas reglas y constantes del controlador a una unidad pura permite:
    ///   - Probar unitariamente que la nota se ensambla correctamente (sin BD).
    ///   - Eliminar "constantes mágicas" dispersas (el HTML y el CategoryId = 1).
    ///   - Exponer y documentar el acoplamiento frágil a la categoría con id fijo.
    ///
    /// Reutiliza <see cref="NoteNameGenerator"/> (la versión corregida de CP-PE-02)
    /// para nombrar la nota: composición de unidades de dominio.
    /// </summary>
    public sealed class DefaultNoteFactory
    {
        /// <summary>HTML inicial idéntico al del sistema original.</summary>
        public const string DefaultHtml = "<p> Comienza a plasmar tus ideas aquí...</p>";

        /// <summary>
        /// Categoría por defecto asumida por el sistema original (CategoryId = 1).
        /// Se expone como constante para hacer explícito (y testeable) este acoplamiento.
        /// </summary>
        public const long DefaultCategoryId = 1;

        private readonly INoteNameGenerator _nameGenerator;

        public DefaultNoteFactory(INoteNameGenerator nameGenerator)
        {
            _nameGenerator = nameGenerator ?? throw new ArgumentNullException(nameof(nameGenerator));
        }

        /// <summary>
        /// Construye una nota por defecto a partir de los nombres ya existentes.
        /// </summary>
        /// <param name="existingNames">Nombres de notas existentes (para nombrar la nueva).</param>
        public Note CreateDefault(IEnumerable<string> existingNames)
        {
            if (existingNames is null)
                throw new ArgumentNullException(nameof(existingNames));

            return new Note
            {
                Name = _nameGenerator.GenerateName(existingNames),
                Html = DefaultHtml,
                CategoryId = DefaultCategoryId
            };
        }
    }
}
