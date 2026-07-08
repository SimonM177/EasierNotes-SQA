# Pruebas de Aceptación — EasierNotes

Ciclo de pruebas de **aceptación** ejecutado por el **tester en su rol de cliente**
(analista funcional que conoce las necesidades del usuario). Valida los criterios de
aceptación oficiales de la ERS, con foco en **Eficiencia de Desempeño**.

- `criterios_aceptacion.feature` — criterios en Gherkin (lenguaje de negocio).
- `EasierNotes.acceptance_collection.json` — ciclo ejecutable (Newman/Postman).
- `build_acceptance.py` — generador de la colección.

## Qué se valida (trazabilidad a la ERS)

| Historia de Usuario | Criterio de aceptación (ERS) | Cómo se valida |
|---------------------|------------------------------|----------------|
| Crear nota (Javier Díaz) | Tarda menos de 0,5 s; se registra en la BD | Tiempo de respuesta + verificación de existencia |
| Buscar nota (Samuel Parra) | Muestra coincidencias; rapidez | Lista disponible en < 1 s |
| Escribir texto (Germán Parra) | Edición se conserva | Guardar y reabrir; el contenido persiste |
| Insertar imagen (Matías León) | Rechazar tipo/tamaño inválido | Documentado como NO cumplido (ver hallazgo) |

## Para la defensa: qué explicar

**1. Proceso de automatización.** Los criterios de aceptación de la ERS se tradujeron a
escenarios Gherkin (Dado–Cuando–Entonces) y de ahí a un ciclo ejecutable en Newman. Cada
petición valida un criterio de negocio, no un detalle técnico. El ciclo se ejecuta con un
solo comando y puede integrarse en el pipeline de CI.

**2. Ambiente de pruebas.** Sistema completo corriendo localmente: backend .NET 8 en
`http://localhost:5219`, base de datos MySQL 8 (esquema `easier_notes`) y, para el E2E
completo, el frontend Angular en `http://localhost:4200`. Las pruebas de aceptación de API
requieren backend + BD; las de sistema E2E requieren además el frontend.

**3. Datos de prueba.** La categoría por defecto (id 1) debe existir (creada con
`setup_database.sql`). El ciclo crea su propia nota de prueba, la usa y la elimina al final
(auto-limpieza), por lo que no ensucia la base ni depende de datos previos.

**4. Justificación de la automatización.** Un criterio de aceptación como "crear en menos
de 0,5 s" no puede verificarse a ojo de forma fiable ni repetible. Automatizarlo permite:
ejecutarlo en cada cambio, medir el tiempo con precisión, y demostrar cumplimiento con
evidencia objetiva y reproducible ante el cliente.

**5. Reflexiones (hallazgos de aceptación).**
- El criterio de "crear en < 0,5 s" se cumple en operación normal (en caliente). La primera
  petición tras arrancar el backend es más lenta por el arranque en frío (JIT + pool), lo
  que se documenta para no confundirlo con incumplimiento.
- El criterio de "rechazar imágenes inválidas" NO se cumple: el sistema acepta cualquier
  archivo (defecto confirmado en las pruebas unitarias CP-PE-15). Como cliente, este
  criterio se marca como NO ACEPTADO y se solicita su corrección.

## Cómo ejecutar

Requiere el sistema corriendo (ver `setup_database.sql` y arrancar el backend).

```bash
# Con Newman (línea de comandos)
newman run acceptance/EasierNotes.acceptance_collection.json \
  -e postman/EasierNotes.postman_environment.json

# Reporte HTML (opcional, útil como evidencia)
newman run acceptance/EasierNotes.acceptance_collection.json \
  -e postman/EasierNotes.postman_environment.json -r htmlextra
```

O en la app de Postman: importar la colección y ejecutarla con *Run collection*.
