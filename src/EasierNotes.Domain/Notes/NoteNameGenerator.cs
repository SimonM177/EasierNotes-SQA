using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasierNotes.Domain.Notes
{
    /// <summary>
    /// Versión CORREGIDA del generador de nombres por defecto.
    ///
    /// A diferencia de la lógica original (que cuenta cuántas notas empiezan por
    /// "Nueva Nota"), esta versión calcula el SIGUIENTE sufijo a partir del MÁXIMO
    /// sufijo ya existente. Eso evita el defecto de nombres duplicados cuando se
    /// borra una nota intermedia.
    ///
    /// Reglas:
    ///   - Si no existe ninguna "Nueva Nota..."        -> "Nueva Nota"
    ///   - Si existe "Nueva Nota" pero ningún sufijo   -> "Nueva Nota (2)"
    ///   - Si el mayor sufijo existente es (k)          -> "Nueva Nota (k+1)"
    ///
    /// Patrón aplicado: Pure Function / Domain Service. Recibe la lista de nombres
    /// existentes (que la capa de datos provee) y devuelve el nuevo nombre, sin tocar la BD.
    /// </summary>
    public sealed class NoteNameGenerator : INoteNameGenerator
    {
        public const string BaseName = "Nueva Nota";

        // Coincide con "Nueva Nota" o "Nueva Nota (N)". Captura N cuando está presente.
        private static readonly Regex Pattern =
            new(@"^Nueva Nota(?: \((\d+)\))?$", RegexOptions.Compiled);

        /// <summary>
        /// Genera el nombre de la siguiente nota por defecto a partir de los nombres existentes.
        /// </summary>
        /// <param name="existingNames">Nombres de notas ya existentes en el sistema.</param>
        public string GenerateName(IEnumerable<string> existingNames)
        {
            if (existingNames is null)
                throw new ArgumentNullException(nameof(existingNames));

            int maxSuffix = 0;          // 0 = aún no hay ninguna "Nueva Nota"
            bool anyDefault = false;

            foreach (var name in existingNames)
            {
                if (name is null) continue;
                var match = Pattern.Match(name);
                if (!match.Success) continue;

                anyDefault = true;
                // "Nueva Nota" sin sufijo cuenta como sufijo 1.
                int suffix = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 1;
                if (suffix > maxSuffix) maxSuffix = suffix;
            }

            if (!anyDefault)
                return BaseName;                 // ninguna existe -> "Nueva Nota"

            return $"{BaseName} ({maxSuffix + 1})"; // siguiente al mayor sufijo
        }
    }
}
