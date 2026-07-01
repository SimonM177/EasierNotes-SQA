using System;
using EasierNotes.Domain.Notes;
using FluentAssertions;
using Xunit;

namespace EasierNotes.Tests.Unit
{
    /// <summary>
    /// Pruebas unitarias de la generación de nombres de nota — Caso de Prueba CP-PE-02.
    ///
    /// Subcaracterística ISO/IEC 25010: Comportamiento Temporal (la operación de creación).
    /// Nota de alcance: el CP-PE-02 mide en JMeter el p95 del CountAsync con LIKE sobre
    /// 5.000 notas (desempeño). Aquí, mediante refactorización, se aísla y prueba la
    /// REGLA DE NEGOCIO de la creación (qué nombre se asigna), que el test de desempeño
    /// no verifica. Ambos niveles son complementarios.
    ///
    /// Técnicas de diseño aplicadas (ISO/IEC/IEEE 29119-4):
    ///   1. Partición de Equivalencia (EP)  — conteo = 0  vs  conteo > 0.
    ///   2. Análisis de Valores Límite (BVA) — conteos 0, 1, 2.
    ///   3. Pruebas de caracterización       — fijan el comportamiento real del código del autor.
    /// </summary>
    public class NoteNameGeneratorTests
    {
        // =================================================================
        //  PARTE A — CARACTERIZACIÓN DE LA LÓGICA DEL AUTOR (Legacy)
        //  Estas pruebas documentan lo que el código ORIGINAL hace hoy.
        // =================================================================

        public class LegacyBehavior
        {
            private readonly LegacyNoteNameGenerator _sut = new();

            [Fact(DisplayName = "Legacy EP: sin notas previas, el nombre es 'Nueva Nota'")]
            public void GenerateName_ZeroCount_ReturnsBaseName()
            {
                // Arrange
                int count = 0;

                // Act
                string name = _sut.GenerateName(count);

                // Assert
                name.Should().Be("Nueva Nota");
            }

            [Theory(DisplayName = "Legacy BVA: con N notas previas (N>0), el nombre es 'Nueva Nota (N+1)'")]
            [InlineData(1, "Nueva Nota (2)")]
            [InlineData(2, "Nueva Nota (3)")]
            [InlineData(5, "Nueva Nota (6)")]
            [InlineData(99, "Nueva Nota (100)")]
            public void GenerateName_PositiveCount_ReturnsNumberedName(int count, string expected)
            {
                _sut.GenerateName(count).Should().Be(expected);
            }

            /// <summary>
            /// PRUEBA QUE EXPONE EL DEFECTO.
            /// Escenario: existen "Nueva Nota" y "Nueva Nota (3)" (la (2) fue borrada).
            /// El conteo de notas que empiezan por "Nueva Nota" es 2, así que la lógica
            /// del autor genera "Nueva Nota (3)"... que YA EXISTE. Nombre duplicado.
            /// La prueba CONFIRMA el comportamiento defectuoso para dejarlo documentado.
            /// </summary>
            [Fact(DisplayName = "Legacy DEFECTO: tras borrar una intermedia, genera un nombre duplicado")]
            public void GenerateName_AfterDeletingMiddleNote_ProducesDuplicate()
            {
                // Arrange: quedan 2 notas por defecto ("Nueva Nota" y "Nueva Nota (3)").
                int countAfterDeletion = 2;
                string existingName = "Nueva Nota (3)";

                // Act
                string generated = _sut.GenerateName(countAfterDeletion);

                // Assert: colisiona con un nombre ya existente -> defecto confirmado.
                generated.Should().Be(existingName,
                    because: "la lógica basada en conteo reutiliza un sufijo ya usado");
            }
        }

        // =================================================================
        //  PARTE B — VERIFICACIÓN DE LA LÓGICA CORREGIDA
        //  Estas pruebas demuestran que la versión basada en el máximo sufijo
        //  resuelve el defecto anterior.
        // =================================================================

        public class FixedBehavior
        {
            private readonly NoteNameGenerator _sut = new();

            [Fact(DisplayName = "Fixed EP: lista vacía -> 'Nueva Nota'")]
            public void GenerateName_NoExistingNotes_ReturnsBaseName()
            {
                _sut.GenerateName(Array.Empty<string>()).Should().Be("Nueva Nota");
            }

            [Fact(DisplayName = "Fixed: existe 'Nueva Nota' sin sufijo -> 'Nueva Nota (2)'")]
            public void GenerateName_OnlyBaseExists_ReturnsTwo()
            {
                var existing = new[] { "Nueva Nota" };

                _sut.GenerateName(existing).Should().Be("Nueva Nota (2)");
            }

            [Fact(DisplayName = "Fixed: usa el MÁXIMO sufijo, no el conteo -> 'Nueva Nota (4)'")]
            public void GenerateName_UsesMaxSuffix_NotCount()
            {
                // Existen base, (2) y (4); el conteo es 3 pero el máximo sufijo es 4.
                var existing = new[] { "Nueva Nota", "Nueva Nota (2)", "Nueva Nota (4)" };

                _sut.GenerateName(existing).Should().Be("Nueva Nota (5)");
            }

            /// <summary>
            /// MISMA SITUACIÓN que el defecto del legacy, ahora resuelta.
            /// Existen "Nueva Nota" y "Nueva Nota (3)" (la (2) fue borrada).
            /// La versión corregida toma el máximo sufijo (3) y genera (4): SIN colisión.
            /// </summary>
            [Fact(DisplayName = "Fixed: tras borrar una intermedia, NO genera duplicado")]
            public void GenerateName_AfterDeletingMiddleNote_NoDuplicate()
            {
                var existing = new[] { "Nueva Nota", "Nueva Nota (3)" };

                string generated = _sut.GenerateName(existing);

                generated.Should().Be("Nueva Nota (4)");
                existing.Should().NotContain(generated, "el nombre nuevo no debe colisionar con uno existente");
            }

            [Fact(DisplayName = "Fixed: ignora nombres que no siguen el patrón por defecto")]
            public void GenerateName_IgnoresNonDefaultNames()
            {
                var existing = new[] { "Mi lista", "Reunión Q3", "Nueva Nota (2)" };

                _sut.GenerateName(existing).Should().Be("Nueva Nota (3)");
            }

            [Fact(DisplayName = "Fixed: lista nula lanza ArgumentNullException")]
            public void GenerateName_NullList_Throws()
            {
                Action act = () => _sut.GenerateName(null!);

                act.Should().Throw<ArgumentNullException>();
            }
        }
    }
}
