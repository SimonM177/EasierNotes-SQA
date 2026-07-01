using EasierNotes.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace EasierNotes.Tests.Unit
{
    /// <summary>
    /// Pruebas unitarias del validador de imagen — Caso de Prueba CP-PE-15.
    ///
    /// Subcaracterística ISO/IEC 25010: Utilización de Recursos (Eficiencia de Desempeño).
    /// Justificación: rechazar de forma temprana y barata los archivos inválidos evita
    /// consumir memoria/tiempo cargando una imagen que debe descartarse. La regla que
    /// protege ese recurso es lógica pura y, por tanto, unitariamente testeable.
    ///
    /// Técnicas de diseño de pruebas aplicadas (ISO/IEC/IEEE 29119-4):
    ///   1. Análisis de Valores Límite (BVA)  — frontera de los 5 MB.
    ///   2. Partición de Equivalencia (EP)     — clases de tipos MIME válidos/ inválidos.
    ///   3. Tabla de Decisión                  — combinación tamaño × formato (D1–D5 del plan).
    ///
    /// Convención de nombres: Metodo_Escenario_ResultadoEsperado.
    /// Estructura de cada prueba: AAA (Arrange–Act–Assert).
    /// </summary>
    public class ImageValidatorTests
    {
        private readonly ImageValidator _sut = new(); // SUT = System Under Test

        // =================================================================
        //  TÉCNICA 1 — ANÁLISIS DE VALORES LÍMITE (frontera de 5 MB)
        //  Frontera: 5.242.880 bytes. Probamos: justo debajo, exacto, justo encima.
        // =================================================================

        [Fact(DisplayName = "BVA: 1 byte por debajo del límite de 5 MB es válido")]
        public void Validate_SizeJustBelowLimit_ReturnsValid()
        {
            // Arrange
            long size = ImageValidator.MaxSizeBytes - 1; // 5.242.879

            // Act
            var result = _sut.Validate(size, "image/jpeg");

            // Assert
            result.IsValid.Should().BeTrue();
            result.Reason.Should().Be(ImageRejectionReason.None);
        }

        [Fact(DisplayName = "BVA: exactamente 5 MB es válido (límite inclusivo)")]
        public void Validate_SizeExactlyAtLimit_ReturnsValid()
        {
            // Arrange
            long size = ImageValidator.MaxSizeBytes; // 5.242.880

            // Act
            var result = _sut.Validate(size, "image/png");

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact(DisplayName = "BVA: 1 byte por encima del límite es rechazado por tamaño")]
        public void Validate_SizeJustAboveLimit_ReturnsExceedsMaxSize()
        {
            // Arrange
            long size = ImageValidator.MaxSizeBytes + 1; // 5.242.881  (dato real del CP: imagen_5MB_mas1.png)

            // Act
            var result = _sut.Validate(size, "image/png");

            // Assert
            result.IsValid.Should().BeFalse();
            result.Reason.Should().Be(ImageRejectionReason.ExceedsMaxSize);
        }

        // =================================================================
        //  TÉCNICA 2 — PARTICIÓN DE EQUIVALENCIA (clases de tipo MIME)
        //  Clase válida: {image/jpeg, image/png}. Clase inválida: el resto.
        // =================================================================

        [Theory(DisplayName = "EP: formatos permitidos (.jpg, .png) son aceptados")]
        [InlineData("image/jpeg")]
        [InlineData("image/png")]
        [InlineData("IMAGE/JPEG")] // robustez: la comparación es case-insensitive
        public void Validate_AllowedMimeType_ReturnsValid(string mime)
        {
            // Arrange
            long size = 1_000_000; // 1 MB, claramente dentro del límite

            // Act
            var result = _sut.Validate(size, mime);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory(DisplayName = "EP: formatos no permitidos son rechazados por formato")]
        [InlineData("image/gif")]
        [InlineData("image/bmp")]
        [InlineData("image/webp")]
        [InlineData("application/pdf")]
        [InlineData("text/plain")]
        public void Validate_DisallowedMimeType_ReturnsUnsupportedFormat(string mime)
        {
            // Arrange
            long size = 1_000_000;

            // Act
            var result = _sut.Validate(size, mime);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Reason.Should().Be(ImageRejectionReason.UnsupportedFormat);
        }

        // =================================================================
        //  TÉCNICA 3 — TABLA DE DECISIÓN (tamaño × formato)
        //  Reproduce exactamente la tabla D1–D5 del plan de pruebas.
        //  Columnas: tamaño (bytes), MIME, ¿válida esperada?, razón esperada.
        // =================================================================

        [Theory(DisplayName = "Tabla de decisión D1–D5: combinación tamaño × formato")]
        // D1: 5 MB exactos + jpeg  -> VÁLIDA
        [InlineData(5_242_880, "image/jpeg", true, ImageRejectionReason.None)]
        // D2: 5 MB + 1 + png       -> rechazo por TAMAÑO
        [InlineData(5_242_881, "image/png", false, ImageRejectionReason.ExceedsMaxSize)]
        // D3: 1 MB + gif           -> rechazo por FORMATO
        [InlineData(1_048_576, "image/gif", false, ImageRejectionReason.UnsupportedFormat)]
        // D4: 8 MB + bmp           -> rechazo: el formato se evalúa antes que el tamaño
        [InlineData(8_388_608, "image/bmp", false, ImageRejectionReason.UnsupportedFormat)]
        // D5: 0,5 MB + jpeg        -> VÁLIDA
        [InlineData(524_288, "image/jpeg", true, ImageRejectionReason.None)]
        public void Validate_DecisionTable_ProducesExpectedVerdict(
            long size, string mime, bool expectedValid, ImageRejectionReason expectedReason)
        {
            // Act
            var result = _sut.Validate(size, mime);

            // Assert
            result.IsValid.Should().Be(expectedValid);
            result.Reason.Should().Be(expectedReason);
        }

        // =================================================================
        //  CASOS DEFENSIVOS (robustez ante entradas degeneradas)
        // =================================================================

        [Theory(DisplayName = "Defensivo: tamaño no positivo se rechaza como vacío/ inválido")]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-1024)]
        public void Validate_NonPositiveSize_ReturnsEmptyOrInvalidSize(long size)
        {
            var result = _sut.Validate(size, "image/jpeg");

            result.IsValid.Should().BeFalse();
            result.Reason.Should().Be(ImageRejectionReason.EmptyOrInvalidSize);
        }

        [Theory(DisplayName = "Defensivo: MIME nulo o vacío se rechaza por formato")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_NullOrEmptyMime_ReturnsUnsupportedFormat(string? mime)
        {
            var result = _sut.Validate(1_000_000, mime);

            result.IsValid.Should().BeFalse();
            result.Reason.Should().Be(ImageRejectionReason.UnsupportedFormat);
        }

        // =================================================================
        //  REPRESENTACIÓN TEXTUAL DEL RESULTADO (cobertura del ToString)
        //  Cubre ambas ramas del operador ternario de ImageValidationResult.
        // =================================================================

        [Fact(DisplayName = "ToString de un resultado válido devuelve 'Valid'")]
        public void ToString_ValidResult_ReturnsValid()
        {
            var result = _sut.Validate(1_000_000, "image/jpeg");

            result.ToString().Should().Be("Valid");
        }

        [Fact(DisplayName = "ToString de un resultado inválido incluye la razón del rechazo")]
        public void ToString_InvalidResult_IncludesReason()
        {
            var result = _sut.Validate(ImageValidator.MaxSizeBytes + 1, "image/png");

            result.ToString().Should().Be("Invalid(ExceedsMaxSize)");
        }
    }
}
