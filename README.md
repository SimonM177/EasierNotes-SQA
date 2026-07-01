# EasierNotes — Pruebas Unitarias (Eficiencia de Desempeño)

<!--
  BADGE DE ESTADO DE LA CI:
  Reemplaza USUARIO y REPOSITORIO por los tuyos de GitHub para que la insignia
  muestre en verde/rojo si el último pipeline pasó. Ejemplo:
  ![CI](https://github.com/simon/EasierNotes/actions/workflows/ci.yml/badge.svg)
-->
![CI](https://github.com/USUARIO/REPOSITORIO/actions/workflows/ci.yml/badge.svg)


Proyecto de pruebas unitarias del sistema **Easier-Notes**, enfocado en la
característica **Eficiencia de Desempeño** (ISO/IEC 25010) y trazado a los
Casos de Prueba del plan (ISO/IEC/IEEE 29119).

Stack de pruebas: **xUnit + FluentAssertions + Moq** sobre **.NET 8**.

## Estructura de la solución

```
EasierNotes/
├── EasierNotes.sln
├── src/
│   └── EasierNotes.Domain/            # Código de producción (unidades bajo prueba)
│       ├── Validation/                # ImageValidator, Result, Reason  (CP-PE-15)
│       ├── Media/                     # Base64PayloadCalculator         (CP-PE-13)
│       └── Notes/                     # Generadores de nombre + Factory (CP-PE-02 / 01)
└── tests/
    └── EasierNotes.Tests/             # Pruebas xUnit
        └── Unit/
```

> El código de `EasierNotes.Domain` se obtuvo **refactorizando** la lógica que en el
> sistema original estaba embebida en `NotesController`, para extraerla a unidades
> puras y poder probarla unitariamente sin base de datos ni red.

## Cómo ejecutar las pruebas

Desde la carpeta raíz `EasierNotes/`:

```bash
# Restaurar paquetes y compilar
dotnet restore
dotnet build

# Ejecutar todas las pruebas
dotnet test

# Ejecutar con cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar solo un caso de prueba concreto (ej. CP-PE-15)
dotnet test --filter "FullyQualifiedName~ImageValidatorTests"
```

En Visual Studio: abrir `EasierNotes.sln` → menú *Prueba* → *Ejecutar todas las pruebas*.

## Mapa de trazabilidad: Caso de Prueba → Unidad → Pruebas

| CP | Subcaracterística 25010 | Unidad bajo prueba | Archivo de pruebas | Casos |
|----|-------------------------|--------------------|--------------------|------:|
| CP-PE-15 | Utilización de Recursos | `ImageValidator` | `ImageValidatorTests.cs` | 22 |
| CP-PE-13 | Utilización de Recursos | `Base64PayloadCalculator` | `Base64PayloadCalculatorTests.cs` | 20 |
| CP-PE-02 | Comportamiento Temporal | `LegacyNoteNameGenerator`, `NoteNameGenerator` | `NoteNameGeneratorTests.cs` | 12 |
| CP-PE-01 | Comportamiento Temporal | `DefaultNoteFactory` | `DefaultNoteFactoryTests.cs` | 9 |
| | | | **Total** | **63** |

## Técnicas de diseño de pruebas aplicadas (ISO/IEC/IEEE 29119-4)

- **Análisis de Valores Límite (BVA):** fronteras de 5 MB (CP-PE-15), bloques de 3 bytes de Base64 (CP-PE-13), conteos 0/1/2 (CP-PE-02).
- **Partición de Equivalencia (EP):** clases de tipo MIME válido/ inválido, factor de inflación, primera nota vs. subsiguientes.
- **Tabla de Decisión:** combinación tamaño × formato de imagen (D1–D5, CP-PE-15).
- **Pruebas de caracterización:** fijan el comportamiento real del código del autor (CP-PE-02) y exponen el defecto de nombres duplicados.
- **Pruebas de excepción y casos defensivos:** entradas nulas, vacías o negativas.

## Patrones de diseño utilizados en el código y en las pruebas

- **Result Object** (`ImageValidationResult`): veredicto + causa, sin excepciones para el flujo normal.
- **Guard Clauses / Pure Function:** validadores y calculadora sin dependencias externas.
- **Factory Method** (`DefaultNoteFactory`): construcción centralizada de la nota por defecto.
- **Inversión de Dependencias** (`INoteNameGenerator`): permite inyectar y simular el colaborador.
- **AAA (Arrange–Act–Assert)** y convención `Metodo_Escenario_ResultadoEsperado` en cada prueba.
- **State-based vs. Interaction-based testing** (este último con Moq) en `DefaultNoteFactoryTests`.

## Hallazgos documentados por las pruebas

1. **Validación de imagen ausente (CP-PE-15):** el sistema original no valida tamaño ni formato; el validador y sus pruebas formalizan la regla que debería existir.
2. **Inflación de payload (CP-PE-13):** una imagen de 5 MB ocupa ~6,99 MB en Base64; solo caben 2 imágenes de 5 MB en un campo MEDIUMTEXT.
3. **Nombres duplicados (CP-PE-02):** la lógica basada en conteo genera nombres repetidos al borrar una nota intermedia; la versión corregida (máximo sufijo) lo resuelve.
4. **Acoplamiento frágil (CP-PE-01):** `CategoryId = 1` está hardcodeado y asume que esa categoría existe.

## Pruebas de integración

Además de las unitarias, la solución incluye **pruebas de integración** que ejercen el
`NotesController` real contra una base de datos, a través de EF Core.

- Proyecto de sistema bajo prueba: `src/EasierNotes.Api` (réplica fiel del backend original).
- Proyecto de pruebas: `tests/EasierNotes.IntegrationTests`.

**Patrón de doble proveedor:** los casos se escriben una sola vez en una clase base
abstracta (`NotesCrudIntegrationTests`) y se ejecutan contra dos motores mediante clases
derivadas:

| Proveedor | Qué aporta | Limitación |
|-----------|------------|------------|
| EF Core InMemory | Rápido, sin instalación; verifica la lógica de EF y del controlador | No es SQL real: ignora longitudes, tipos y claves foráneas |
| SQLite en memoria | Motor SQL real; respeta transacciones y tipos | No es MySQL: diferencias de dialecto puntuales |

Primera tanda: **CRUD básico** (10 casos × 2 proveedores = 20 pruebas de integración).

**Segunda tanda — hallazgos** (documentan comportamientos límite y defectos):

| Hallazgo | Qué evidencia | Proveedores |
|----------|---------------|-------------|
| Categoría fantasma | Crear sin la categoría 1 falla por clave foránea en SQL real | SQLite |
| Nombres duplicados | Tras borrar una nota intermedia, la nueva duplica un nombre existente | InMemory + SQLite |
| Longitud del nombre | Un nombre de 500 caracteres se persiste completo (falta [MaxLength]) | InMemory + SQLite |

En total: 20 (CRUD) + 12 (hallazgos) = **32 pruebas de integración**.

```bash
# Ejecutar solo las pruebas de integración
dotnet test tests/EasierNotes.IntegrationTests
```

## Integración Continua (CI/CD)

El repositorio incluye un pipeline de **Integración Continua** con GitHub Actions
(`.github/workflows/ci.yml`) que se ejecuta automáticamente en cada `push` y cada
`pull_request`. El pipeline:

1. Descarga el código e instala el SDK de .NET 8.
2. Restaura dependencias y compila la solución en configuración Release.
3. Ejecuta las 97 pruebas (unitarias + integración) con recolección de cobertura.
4. Publica los resultados (`.trx` y cobertura) como artefacto descargable.

**Alcance:** el pipeline cubre la **CI** (build + test automáticos). No incluye
**CD** (despliegue continuo) porque el proyecto es académico y no dispone de un
entorno de producción donde publicar la aplicación; añadirlo no aportaría valor real.

Para activarlo: subir el repositorio a GitHub. El pipeline corre solo; su estado se
ve en la pestaña **Actions** y en el badge del encabezado de este README.

## Notas sobre paquetes

Las versiones de los paquetes en el `.csproj` son estables y compatibles con .NET 8;
pueden actualizarse desde el Administrador de paquetes NuGet. Sobre **Moq**, se fija
la 4.20.72 (posterior a la retirada del componente SponsorLink). Si se desea evitar
Moq por completo, **NSubstitute** es una alternativa equivalente para los dobles de prueba.
