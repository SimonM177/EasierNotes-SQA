using System;
using EasierNotes.Domain.Notes;
using FluentAssertions;
using Moq;
using Xunit;

namespace EasierNotes.Tests.Unit
{
    /// <summary>
    /// Pruebas unitarias de la fábrica de notas por defecto — Caso de Prueba CP-PE-01.
    ///
    /// Subcaracterística ISO/IEC 25010: Comportamiento Temporal (creación de nota).
    /// Alcance: el tiempo (&lt; 0,5 s) lo mide JMeter. Aquí se verifica la CORRECCIÓN
    /// de la nota construida (HTML por defecto, categoría por defecto, nombre delegado),
    /// porque una operación rápida pero que produzca una nota mal formada no cumple la HU.
    ///
    /// Técnicas y prácticas aplicadas:
    ///   - Partición de Equivalencia (primera nota vs. notas subsiguientes).
    ///   - Verificación de estado (state-based) y de interacción (interaction-based con Moq).
    ///   - Doble de prueba (mock) para aislar la fábrica de la implementación del generador.
    /// </summary>
    public class DefaultNoteFactoryTests
    {
        // =================================================================
        //  PARTE A — Pruebas basadas en ESTADO, con la dependencia REAL.
        //  Verifican que la nota construida tenga los valores correctos.
        // =================================================================

        public class StateBased
        {
            private readonly DefaultNoteFactory _sut =
                new(new NoteNameGenerator()); // dependencia real

            [Fact(DisplayName = "La nota por defecto lleva el HTML inicial esperado")]
            public void CreateDefault_Always_SetsDefaultHtml()
            {
                // Act
                var note = _sut.CreateDefault(Array.Empty<string>());

                // Assert
                note.Html.Should().Be("<p> Comienza a plasmar tus ideas aquí...</p>");
            }

            [Fact(DisplayName = "La nota por defecto se asigna a la categoría por defecto (id 1)")]
            public void CreateDefault_Always_SetsDefaultCategory()
            {
                var note = _sut.CreateDefault(Array.Empty<string>());

                note.CategoryId.Should().Be(1);
            }

            [Fact(DisplayName = "EP: la primera nota se llama 'Nueva Nota'")]
            public void CreateDefault_NoExistingNotes_NamesItBase()
            {
                var note = _sut.CreateDefault(Array.Empty<string>());

                note.Name.Should().Be("Nueva Nota");
            }

            [Fact(DisplayName = "EP: con notas previas, la nueva recibe el sufijo correcto")]
            public void CreateDefault_WithExistingNotes_NamesItNumbered()
            {
                var existing = new[] { "Nueva Nota", "Nueva Nota (2)" };

                var note = _sut.CreateDefault(existing);

                note.Name.Should().Be("Nueva Nota (3)");
            }

            [Fact(DisplayName = "La nota recién creada no tiene Id asignado (lo pone la BD)")]
            public void CreateDefault_Always_LeavesIdUnset()
            {
                var note = _sut.CreateDefault(Array.Empty<string>());

                note.Id.Should().Be(0);
            }

            [Fact(DisplayName = "Lista de nombres nula lanza ArgumentNullException")]
            public void CreateDefault_NullList_Throws()
            {
                Action act = () => _sut.CreateDefault(null!);

                act.Should().Throw<ArgumentNullException>();
            }
        }

        // =================================================================
        //  PARTE B — Pruebas basadas en INTERACCIÓN, con MOQ.
        //  Aíslan la fábrica: verifican que DELEGA el nombrado en su
        //  colaborador, sin depender de la implementación concreta.
        // =================================================================

        public class InteractionBased
        {
            [Fact(DisplayName = "La fábrica delega el nombrado en el generador inyectado")]
            public void CreateDefault_DelegatesNamingToGenerator()
            {
                // Arrange: un mock que devuelve un nombre controlado.
                var mockGenerator = new Mock<INoteNameGenerator>();
                mockGenerator
                    .Setup(g => g.GenerateName(It.IsAny<string[]>()))
                    .Returns("NOMBRE_SIMULADO");
                var sut = new DefaultNoteFactory(mockGenerator.Object);

                // Act
                var note = sut.CreateDefault(Array.Empty<string>());

                // Assert (estado): la nota usa el nombre que devolvió el colaborador.
                note.Name.Should().Be("NOMBRE_SIMULADO");

                // Assert (interacción): se invocó al generador exactamente una vez.
                mockGenerator.Verify(g => g.GenerateName(It.IsAny<string[]>()), Times.Once);
            }

            [Fact(DisplayName = "La fábrica pasa al generador la lista de nombres recibida")]
            public void CreateDefault_PassesExistingNamesToGenerator()
            {
                // Arrange
                var existing = new[] { "Nueva Nota", "Reunión Q3" };
                var mockGenerator = new Mock<INoteNameGenerator>();
                mockGenerator
                    .Setup(g => g.GenerateName(existing))
                    .Returns("ok");
                var sut = new DefaultNoteFactory(mockGenerator.Object);

                // Act
                sut.CreateDefault(existing);

                // Assert: el colaborador recibió EXACTAMENTE la lista esperada.
                mockGenerator.Verify(g => g.GenerateName(existing), Times.Once);
            }

            [Fact(DisplayName = "Constructor con generador nulo lanza ArgumentNullException")]
            public void Constructor_NullGenerator_Throws()
            {
                Action act = () => new DefaultNoteFactory(null!);

                act.Should().Throw<ArgumentNullException>();
            }
        }
    }
}
