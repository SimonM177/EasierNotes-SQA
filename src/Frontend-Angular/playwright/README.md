# CP-PE-07 · Prueba del editor con tabla extensa (Playwright)

Este paquete cierra el único caso que faltaba: **CP-PE-07**, el rendimiento del
editor cuando la nota tiene una tabla grande (50×50 = 2500 celdas + 200 párrafos).

Mide dos cosas:
1. **Fluidez de tecleo**: cuánto tarda cada pulsación (umbral < 100 ms).
2. **Tiempo de guardado**: cuánto tarda en persistir ese contenido (umbral < 1 s).

---

## PASO A PASO

### Paso 1 — Requisito previo
Ten corriendo **el frontend Angular** en `http://localhost:4200` y el backend en
`http://localhost:5219` (esta prueba usa la interfaz, no solo la API).

### Paso 2 — Instalar Playwright
Abre una terminal en esta carpeta y ejecuta:

```powershell
npm install
npx playwright install chromium
```

El primer comando instala Playwright; el segundo descarga el navegador Chromium
que Playwright controla. Esto solo se hace una vez.

### Paso 3 — Ajustar los selectores (IMPORTANTE)
Abre `cp-pe-07.spec.js` y revisa el bloque `AJUSTA ESTO A TU APLICACIÓN`:

```javascript
const APP_URL = 'http://localhost:4200';
const SELECTOR_EDITOR = '[contenteditable="true"]';  // el editor de tu nota
```

- Si tu editor usa otra clase o etiqueta, cámbiala aquí.
- Para saber el selector correcto: abre tu app en Chrome, clic derecho sobre el
  editor → Inspeccionar, y mira qué atributo/clase tiene.

### Paso 4 — Ejecutar la prueba
```powershell
npx playwright test
```
Verás el navegador abrirse solo, cargar la tabla, teclear y medir. En la terminal
aparecerán los tiempos:

```
[CP-PE-07] Tecleo con tabla extensa:
  Promedio por tecla: 12.3 ms
  Máximo por tecla:   45 ms
  Umbral:             100 ms
```

### Paso 5 — Ver el reporte visual (para capturas)
```powershell
npx playwright show-report
```
Abre un reporte HTML con el resultado, ideal para capturar en el informe.

---

## Qué concluyes (para la defensa)

- Si el **promedio por tecla < 100 ms** → el editor es fluido incluso con DOM pesado.
- Si **supera 100 ms** → hay lag perceptible con tablas grandes: ese es el hallazgo.
- El tiempo de guardado te dice si persistir contenido grande es rápido.

Con esto, **CP-PE-07 pasa de "excluido" a "ejecutado"**, y ya no tienes ningún
caso fuera del alcance.

---

## Si algo falla

- **"No encuentra el editor"** → ajusta `SELECTOR_EDITOR` en el Paso 3.
- **"No se abre la app"** → confirma que Angular corre en `localhost:4200`.
- **El guardado no se detecta** → ajusta cómo tu app dispara el guardado (botón,
  atajo o `blur`) en el segundo test.

## Archivos

- `cp-pe-07.spec.js` — la prueba (dos tests: tecleo y guardado)
- `tabla_50x50.html` — el dato de prueba (la tabla extensa)
- `playwright.config.js` — configuración
- `package.json` — dependencias
