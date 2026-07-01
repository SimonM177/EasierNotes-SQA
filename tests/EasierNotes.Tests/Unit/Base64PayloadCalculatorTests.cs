using System;
using EasierNotes.Domain.Media;
using FluentAssertions;
using Xunit;

namespace EasierNotes.Tests.Unit
{
    /// <summary>
    /// Pruebas unitarias de la calculadora de payload Base64 — Caso de Prueba CP-PE-13.
    ///
    /// Subcaracterística ISO/IEC 25010: Utilización de Recursos (Eficiencia de Desempeño).
    /// Justificación: el tamaño del Data URL incrustado en la nota es lógica aritmética
    /// determinista; aislarla permite anticipar el consumo de capacidad (MEDIUMTEXT) y la
    /// inflación del payload SIN ejecutar navegador ni base de datos.
    ///
    /// Técnicas de diseño aplicadas (ISO/IEC/IEEE 29119-4):
    ///   1. Análisis de Valores Límite (BVA)  — fronteras de los bloques de 3 bytes de Base64
    ///                                           y frontera de capacidad de MEDIUMTEXT.
    ///   2. Partición de Equivalencia (EP)     — factor de inflación para entradas grandes.
    ///   3. Oráculo conocido                   — el valor real verificado para 5 MB.
    ///   4. Pruebas de excepción               — Guard Clauses ante entradas inválidas.
    /// </summary>
    public class Base64PayloadCalculatorTests
    {
        // =================================================================
        //  TÉCNICA 1 — VALORES LÍMITE sobre los bloques de 3 bytes
        //  Base64 codifica en bloques de 3 bytes -> 4 chars. Las fronteras
        //  0,1,2,3,4 son donde el padding produce los saltos.
        // =================================================================

        [Theory(DisplayName = "BVA: longitud Base64 en las fronteras de bloque de 3 bytes")]
        [InlineData(0, 0)]    // vacío -> 0 chars
        [InlineData(1, 4)]    // 1 byte  -> 4 chars (con padding "==")
        [InlineData(2, 4)]    // 2 bytes -> 4 chars (con padding "=")
        [InlineData(3, 4)]    // 3 bytes -> 4 chars (bloque completo)
        [InlineData(4, 8)]    // 4 bytes -> 8 chars (un bloque + inicio de otro)
        [InlineData(99, 132)] // múltiplo de 3
        [InlineData(100, 136)]
        public void Base64Length_AtBlockBoundaries_MatchesFormula(long input, long expected)
        {
            // Act
            long result = Base64PayloadCalculator.Base64Length(input);

            // Assert
            result.Should().Be(expected);
        }

        // =================================================================
        //  TÉCNICA 3 — ORÁCULO CONOCIDO (dato real del CP-PE-13)
        //  Una imagen de 5 MB produce 6.990.508 chars Base64 (verificado
        //  contra la codificación estándar). Es nuestro valor de referencia.
        // =================================================================

        [Fact(DisplayName = "Oráculo: imagen de 5 MB produce exactamente 6.990.508 chars Base64")]
        public void Base64Length_FiveMegabytes_Returns6990508()
        {
            // Arrange
            long fiveMB = 5L * 1024 * 1024; // 5.242.880

            // Act
            long result = Base64PayloadCalculator.Base64Length(fiveMB);

            // Assert
            result.Should().Be(6_990_508);
        }

        [Fact(DisplayName = "Data URL de imagen 5 MB jpeg incluye el prefijo de 23 caracteres")]
        public void DataUrlLength_FiveMegabytesJpeg_AddsPrefixLength()
        {
            // Arrange
            long fiveMB = 5L * 1024 * 1024;
            const string mime = "image/jpeg";
            long expectedPrefix = "data:image/jpeg;base64,".Length; // 23

            // Act
            long total = Base64PayloadCalculator.DataUrlLength(fiveMB, mime);

            // Assert
            total.Should().Be(6_990_508 + expectedPrefix); // 6.990.531
        }

        // =================================================================
        //  TÉCNICA 2 — PARTICIÓN DE EQUIVALENCIA sobre el factor de inflación
        //  Para entradas grandes, el factor converge a 4/3 ≈ 1.333.
        // =================================================================

        [Theory(DisplayName = "EP: el factor de inflación de entradas grandes ronda 1.333 (33%)")]
        [InlineData(1_000_000)]
        [InlineData(5_242_880)]
        [InlineData(10_000_000)]
        public void InflationFactor_LargeInputs_IsAboutOneThird(long size)
        {
            // Act
            double factor = Base64PayloadCalculator.InflationFactor(size, "image/jpeg");

            // Assert: entre 1.33 y 1.34 (el prefijo y el padding apenas mueven la aguja)
            factor.Should().BeInRange(1.33, 1.34);
        }

        // =================================================================
        //  CAPACIDAD — frontera de MEDIUMTEXT (riesgo central de CP-PE-13)
        // =================================================================

        [Fact(DisplayName = "Capacidad: una imagen de 5 MB SÍ cabe en MEDIUMTEXT")]
        public void FitsInMediumText_FiveMegabytes_ReturnsTrue()
        {
            long fiveMB = 5L * 1024 * 1024;

            bool fits = Base64PayloadCalculator.FitsInMediumText(fiveMB, "image/jpeg");

            fits.Should().BeTrue();
        }

        [Fact(DisplayName = "Capacidad: solo caben 2 imágenes de 5 MB en un MEDIUMTEXT")]
        public void MaxImagesInMediumText_FiveMegabytes_ReturnsTwo()
        {
            long fiveMB = 5L * 1024 * 1024;

            long maxImages = Base64PayloadCalculator.MaxImagesInMediumText(fiveMB, "image/jpeg");

            // 16.777.215 / 6.990.531 = 2 (la tercera desbordaría el campo)
            maxImages.Should().Be(2);
        }

        [Fact(DisplayName = "Capacidad: una imagen cercana al tope de MEDIUMTEXT NO cabe tras Base64")]
        public void FitsInMediumText_ImageNearCapacity_ReturnsFalse()
        {
            // Arrange: una imagen de 13 MB, al inflarse ~33%, supera los 16 MB de MEDIUMTEXT.
            long thirteenMB = 13L * 1024 * 1024;

            // Act
            bool fits = Base64PayloadCalculator.FitsInMediumText(thirteenMB, "image/png");

            // Assert
            fits.Should().BeFalse();
        }

        // =================================================================
        //  TÉCNICA 4 — PRUEBAS DE EXCEPCIÓN (Guard Clauses)
        // =================================================================

        [Fact(DisplayName = "Excepción: tamaño negativo en Base64Length lanza ArgumentOutOfRange")]
        public void Base64Length_NegativeSize_Throws()
        {
            Action act = () => Base64PayloadCalculator.Base64Length(-1);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory(DisplayName = "Excepción: MIME nulo o vacío en DataUrlLength lanza ArgumentException")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void DataUrlLength_NullOrEmptyMime_Throws(string? mime)
        {
            Action act = () => Base64PayloadCalculator.DataUrlLength(1000, mime!);

            act.Should().Throw<ArgumentException>();
        }

        [Fact(DisplayName = "Excepción: factor de inflación con tamaño 0 lanza ArgumentOutOfRange")]
        public void InflationFactor_ZeroSize_Throws()
        {
            Action act = () => Base64PayloadCalculator.InflationFactor(0, "image/jpeg");

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
