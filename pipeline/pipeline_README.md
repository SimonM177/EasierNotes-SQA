# Integración Python + Pandas + Excel · Pipeline de métricas

Esta es la **capa de procesamiento** del ecosistema de aseguramiento. Se sitúa entre
las herramientas que generan datos crudos y el dashboard que los visualiza.

## El flujo completo

```
Herramientas          Datos crudos           Python+Pandas        Excel            Dashboard
────────────          ────────────           ─────────────        ─────            ─────────
xUnit + coverlet  →   .trx, cobertura.xml  →  limpia y        →   auditoría   →    datos.json
Newman            →   reportes htmlextra   →  estructura      →   y validación →   (visualización)
JMeter            →   .jtl                 →  calcula métricas    humana
```

## Qué hace cada pieza

### Python + Pandas — transformación reproducible
El script `pipeline_metricas.py`:
1. **Lee** los archivos crudos: los `.trx` (resultados de `dotnet test`), el
   `cobertura.xml` (coverlet) y los `.jtl` (JMeter).
2. **Limpia y estructura** los datos en DataFrames de Pandas.
3. **Calcula** las métricas derivadas (p95 con `quantile(0.95)`, tasas, promedios).
4. **Exporta** dos salidas: el Excel de auditoría y el `datos.json`.

Es reproducible: cualquiera ejecuta el script sobre los mismos crudos y obtiene
idéntico resultado. No hay cálculos manuales ni opacos.

### Excel — capa de validación y auditoría rápida
`Auditoria_Metricas_EasierNotes.xlsx` tiene cinco hojas:
- **Resumen** — portada con el propósito y la fecha del proceso.
- **Métricas de proceso** — las 6 métricas de salud del aseguramiento.
- **Métricas de producto** — las 6 métricas de Eficiencia de Desempeño.
- **Niveles de prueba** — distribución por nivel.
- **Desempeño JMeter** — los 3 planes (JM-01, JM-02, JM-03), uno por subcaracterística.
  Los que aún no corriste aparecen como "pendiente" hasta que exista su `.jtl`.

La columna **Estado** está coloreada (verde = OK, rojo = requiere atención), de modo
que el Líder de Métricas puede **auditar los números a ojo antes de publicarlos** al
dashboard. Es el control humano de calidad sobre los propios datos de métricas.

### datos.json — alimenta el dashboard
El mismo pipeline genera el `datos.json` con la estructura que el dashboard consume.
Cárgalo en el dashboard con el botón "Cargar datos".

## Cómo ejecutarlo (secuencia completa)

```bash
# 1. Pruebas de código (xUnit) -> deja .trx y cobertura
dotnet test --logger "trx;LogFileName=resultados.trx" --collect:"XPlat Code Coverage" --results-directory ./resultados

# 2. Desempeño: los 3 planes JMeter -> cada uno deja su .jtl
jmeter -n -t jmeter/JM-01_comportamiento_temporal.jmx -l ./resultados/jm01.jtl
jmeter -n -t jmeter/JM-02_capacidad.jmx              -l ./resultados/jm02.jtl
jmeter -n -t jmeter/JM-03_utilizacion_recursos.jmx  -l ./resultados/jm03.jtl

# 3. Procesar (Python+Pandas -> Excel + JSON)
python pipeline_metricas.py --dir ./resultados --out ./resultados

# 4. Abrir el .xlsx para validar; 5. cargar datos.json en el dashboard
```

El pipeline busca `jm01.jtl`, `jm02.jtl` y `jm03.jtl` en la carpeta `--dir`.
Los que no existan aún se marcan como "pendiente" (no rompe nada). También funciona
sin ningún archivo: usa los datos de ejemplo reales del proyecto.

Si no encuentra los archivos crudos, usa datos de ejemplo para que el pipeline
siempre produzca una salida demostrable.

## Requisitos

```bash
pip install pandas openpyxl
```

## Por qué esta integración importa (para la defensa)

Añade dos propiedades a la gobernanza de datos:
- **Reproducibilidad del procesamiento**: la transformación de crudo a métrica es un
  script versionable, no un cálculo manual.
- **Punto de auditoría humana**: Excel introduce un control de calidad sobre los datos
  de métricas antes de su publicación.

Importante: Python+Pandas+Excel **no generan métricas nuevas** — procesan y validan las
que ya producen las herramientas. Son capa de *tratamiento*, no de *generación*.
