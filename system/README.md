# Pruebas de Sistema (E2E) — EasierNotes

Pruebas de **sistema** sobre EasierNotes desplegado (backend .NET 8 + MySQL). Ejercen
**recorridos completos de usuario** (user journeys) de extremo a extremo por la interfaz
HTTP del sistema, verificando el estado del sistema tras cada paso.

- `EasierNotes.system_collection.json` — los recorridos ejecutables (Newman/Postman).
- `build_system.py` — generador de la colección.

## Nivel de prueba y justificación del alcance

Se trata del nivel de **sistema**: caja negra sobre el sistema **integrado y en ejecución**.
El alcance se define como **backend + base de datos**, porque la lógica de negocio y la
persistencia residen en la API; el frontend Angular es la capa de presentación que consume
esa API y su validación se aborda en las pruebas de **aceptación** con el cliente. El
sistema se ataca por su interfaz HTTP externa, sin conocer su implementación.

Diferencia con los niveles previos:
- **Integración:** invoca el controlador + EF + BD por código, con dobles de prueba.
- **Sistema (este):** golpea el sistema desplegado por HTTP, encadenando recorridos completos.

## Recorridos incluidos

| Recorrido | Qué valida | Pasos |
|-----------|-----------|-------|
| A — Ciclo de vida de una nota | Crear → consultar → editar → verificar persistencia → asociar categoría → eliminar → verificar ausencia | 7 |
| B — Numeración por defecto | El sistema nombra "Nueva Nota", "Nueva Nota (2)"… de extremo a extremo | 5 |
| C — Robustez ante inexistentes | Operar sobre notas inexistentes devuelve 404 sin corromper el sistema | 4 |

Total: 16 peticiones, 17 aserciones.

## Para la defensa: qué explicar

**1. Proceso de automatización.** Los recorridos se definieron como secuencias de peticiones
HTTP encadenadas (el id de una nota creada se reutiliza en los pasos siguientes). Se ejecutan
con Newman (el corredor de Postman por consola) con un solo comando, y pueden integrarse en CI.

**2. Ambiente de pruebas.** Sistema desplegado localmente: backend en `http://localhost:5219`
y MySQL 8 con el esquema `easier_notes`. El ambiente es idéntico al de ejecución real de la
aplicación (mismo backend, misma base), lo que da fidelidad a las pruebas de sistema.

**3. Datos de prueba.** Cada recorrido crea sus propios datos y los elimina al final
(auto-contenido). No depende de datos previos salvo la categoría por defecto (id 1), creada
con `setup_database.sql`. Esto hace los recorridos repetibles y sin efectos secundarios.

**4. Justificación de la automatización.** Un recorrido de 7 pasos verificado a mano es lento
y propenso a error. Automatizado, se ejecuta en segundos, es repetible en cada cambio y deja
evidencia objetiva (reporte). Permite confirmar que el sistema completo se comporta bien de
extremo a extremo, no solo pieza por pieza.

**5. Reflexiones.** Los recorridos confirman que las operaciones encadenadas mantienen la
integridad del sistema (un dato creado se puede editar, persiste, y se elimina limpiamente).
El recorrido C confirma robustez: el sistema no se corrompe ante operaciones inválidas.

## Ejecución

Requiere el sistema corriendo. Individual:

```bash
newman run system/EasierNotes.system_collection.json \
  -e postman/EasierNotes.postman_environment.json
```

Ciclo completo (caja negra + sistema + aceptación) con reportes HTML:

```bash
bash run_all_tests.sh
```
