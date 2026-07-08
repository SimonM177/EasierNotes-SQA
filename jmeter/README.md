# Pruebas de Desempeño (JMeter) — EasierNotes

Planes de prueba de **Eficiencia de Desempeño** (ISO/IEC 25010) con Apache JMeter 5.6.3.
Cubren las **tres subcaracterísticas** de la característica evaluada.

| Plan | Subcaracterística | Qué mide | Carga |
|------|-------------------|----------|-------|
| `JM-01_comportamiento_temporal.jmx` | Comportamiento Temporal | Latencia p95 de crear nota | 20 usuarios × 10 = 200 peticiones |
| `JM-02_capacidad.jmx` | Capacidad | Concurrencia sobre el listado | 50 usuarios × 20 (ramp 30s) |
| `JM-03_utilizacion_recursos.jmx` | Utilización de Recursos | Throughput con carga mixta | 30 usuarios × 30 (crear + leer) |

## Requisito previo

El sistema debe estar corriendo en `http://localhost:5219` (backend .NET + MySQL).
La categoría por defecto (id 1) debe existir (ver `setup_database.sql`), porque crear
notas la requiere.

## Cómo ejecutar (interfaz gráfica)

1. Abrir JMeter (`bin/jmeter.bat`).
2. `File → Open` → seleccionar el `.jmx` deseado.
3. Verificar que el backend está corriendo.
4. Clic en ▶ (Start) o `Ctrl+R`.
5. Ver resultados en el listener **"Reporte Agregado"**:
   - **95% Line** = p95 (métrica principal de tiempo).
   - **Throughput** = peticiones/segundo.
   - **Error %** = debe ser 0.

## Cómo ejecutar (línea de comandos, para evidencia)

El modo no-gráfico es el recomendado para generar reportes y para la defensa:

```bash
jmeter -n -t jmeter/JM-01_comportamiento_temporal.jmx -l resultados/jm01.jtl -e -o reportes/jm01
```

- `-n` modo no gráfico, `-t` plan, `-l` archivo de resultados, `-e -o` genera un reporte HTML.
- El reporte HTML (carpeta `reportes/jm01`) es excelente evidencia para el informe.

## Interpretación de las métricas (IEEE 1061)

- **Comportamiento Temporal (JM-01):** p95 ≤ 500 ms (criterio de la HU "Crear nota").
- **Capacidad (JM-02):** identificar el punto donde el tiempo se dispara o aparecen errores
  al aumentar la concurrencia (punto de quiebre).
- **Utilización de Recursos (JM-03):** throughput sostenido estable; complementar observando
  el consumo de CPU/memoria del proceso `dotnet` durante la ejecución (Administrador de tareas
  o `dotnet-counters`).

## Nota importante sobre el "arranque en frío"

La primera petición tras arrancar el backend es más lenta (compilación JIT + apertura del
pool de conexiones). Para medir el desempeño real, ejecutar cada plan **dos veces** y usar
los resultados de la segunda corrida, o descartar las primeras muestras.
