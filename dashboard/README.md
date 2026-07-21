# Dashboard SQA · Easier-Notes

Dashboard profesional multipágina para visualizar las métricas del aseguramiento de la calidad.
Construido en HTML + CSS + JavaScript con **ApexCharts**. No requiere instalación ni servidor.

## Cómo abrirlo

Simplemente abre `index.html` en tu navegador (doble clic). Funciona sin internet:
ApexCharts está incluido localmente (`apexcharts.min.js`).

## Páginas

1. **Resumen** — KPIs principales, pirámide de pruebas, cobertura, técnicas, estado de casos.
2. **Niveles de prueba** — distribución por nivel, enfoque de caja (blanca/gris/negra), detalle.
3. **Desempeño** — resultados de JMeter: latencia p95, distribución, margen vs umbral, las 3 subcaracterísticas.
4. **Resultados** — tabla de casos con **filtros interactivos** (por riesgo, estado) y búsqueda.
5. **Ecosistema** — flujo de gobernanza de datos y las herramientas integradas (incluye JIRA como repositorio de hallazgos).
6. **Hallazgos** — defectos con su ticket JIRA y el logro de objetivos.

## Datos EN VIVO — cómo actualizarlos

El dashboard lee sus datos de `datos.js` (variable `window.SQA_DATA`).
Hay dos formas de actualizar los datos tras correr las pruebas:

### Opción 1: Editar datos.js
Abre `datos.js` y actualiza los valores (p. ej., cuando corras JM-02/JM-03, pon sus
resultados en la sección `desempeno`). Guarda y recarga la página.

### Opción 2: Cargar un datos.json en vivo (botón "Cargar datos")
1. Genera un `datos.json` con los resultados de tu última corrida (usa `generar_datos.py`).
2. En el dashboard, pulsa **"↻ Cargar datos"** y selecciona el `datos.json`.
3. El dashboard se actualiza al instante con los nuevos números.

El botón **"Actualizar"** relee los datos y refresca todas las gráficas.

## Generar datos.json desde resultados reales

El script `generar_datos.py` es una plantilla que puedes adaptar para leer tus
archivos `.trx` (pruebas), el reporte de cobertura y los `.jtl` de JMeter, y producir
un `datos.json` con la estructura correcta. Ejecuta:

```bash
python generar_datos.py
```

## Paleta

Los colores solicitados están aplicados en toda la interfaz:
`#1d3f72` (azul) · `#e6c56b` (oro) · `#8a5e8d` (morado) · `#d29e5b` (ámbar) · `#f4a462` (coral).

## Archivos

- `index.html` — estructura
- `estilos.css` — diseño, paleta, animaciones, responsive
- `app.js` — lógica, navegación, gráficas
- `datos.js` — los datos que se muestran (edítalo para actualizar)
- `apexcharts.min.js` — librería de gráficas (local)
- `generar_datos.py` — plantilla para producir datos.json desde resultados reales
