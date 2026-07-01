using System.Collections.Generic;

namespace EasierNotes.Domain.Notes
{
    /// <summary>
    /// Abstracción del generador de nombres de nota.
    /// Introducir la interfaz permite (a) inyectar distintas implementaciones
    /// (legacy vs corregida) y (b) sustituir el colaborador por un doble de prueba
    /// (mock) en las pruebas unitarias de la fábrica. Aplica el Principio de
    /// Inversión de Dependencias (la fábrica depende de la abstracción, no de la clase).
    /// </summary>
    public interface INoteNameGenerator
    {
        string GenerateName(IEnumerable<string> existingNames);
    }
}
