// @ts-check
const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

/**
 * CP-PE-07 · Rendimiento del editor con una tabla extensa (DOM pesado)
 * Subcaracterística: Comportamiento Temporal
 *
 * Mide DOS cosas:
 *   1) Fluidez de edición: cuánto tarda cada pulsación de tecla cuando el
 *      editor contiene una tabla 50x50 (2500 celdas) + 200 párrafos.
 *      Umbral propuesto: < 100 ms por tecla.
 *   2) Tiempo de guardado: cuánto tarda en persistir ese contenido grande.
 *      Umbral propuesto: < 1 s.
 *
 * NOTA: los selectores (URL, editor, botón) pueden variar según tu Angular.
 * Ajusta las constantes de abajo si tu app usa otras rutas o clases.
 */

// ====== AJUSTA ESTO A TU APLICACIÓN ======
const APP_URL = 'http://127.0.0.1:4200';        // tu frontend Angular
const SELECTOR_EDITOR = '[contenteditable="true"]'; // el editor de la nota
const SELECTOR_BOTON_NUEVA = 'text=Nueva';       // botón/acción para crear nota (ajusta)
const UMBRAL_TECLA_MS = 100;                     // < 100 ms por tecla
const UMBRAL_GUARDADO_MS = 1000;                 // < 1 s guardado
// ==========================================

test.describe('CP-PE-07 · Editor con tabla extensa', () => {

  test('Fluidez de tecleo con tabla 50x50 cargada', async ({ page }) => {
    // 1) Abrir la app
    await page.goto(APP_URL, { waitUntil: 'networkidle' });

    // 2) Localizar el editor
    const editor = page.locator(SELECTOR_EDITOR).first();
    await editor.waitFor({ state: 'visible', timeout: 15000 });

    // 3) Inyectar la tabla extensa directamente en el editor (DOM pesado)
    const html = fs.readFileSync(path.join(__dirname, 'tabla_50x50.html'), 'utf-8');
    await editor.evaluate((el, contenido) => { el.innerHTML = contenido; }, html);
    await page.waitForTimeout(300); // dejar que el navegador renderice el DOM

    // 4) Medir el tiempo de varias pulsaciones al final del contenido
    await editor.click();
    const tiempos = [];
    for (let i = 0; i < 20; i++) {
      const inicio = Date.now();
      await page.keyboard.type('x');          // una pulsación
      const fin = Date.now();
      tiempos.push(fin - inicio);
    }

    const promedio = tiempos.reduce((a, b) => a + b, 0) / tiempos.length;
    const max = Math.max(...tiempos);
    console.log(`\n[CP-PE-07] Tecleo con tabla extensa:`);
    console.log(`  Promedio por tecla: ${promedio.toFixed(1)} ms`);
    console.log(`  Máximo por tecla:   ${max} ms`);
    console.log(`  Umbral:             ${UMBRAL_TECLA_MS} ms`);

    // 5) Aserción: el promedio por tecla debe estar por debajo del umbral
    expect(promedio, `Promedio por tecla ${promedio.toFixed(1)}ms`).toBeLessThan(UMBRAL_TECLA_MS);
  });

  test('Tiempo de guardado de una nota con tabla extensa', async ({ page }) => {
    await page.goto(APP_URL, { waitUntil: 'networkidle' });
    const editor = page.locator(SELECTOR_EDITOR).first();
    await editor.waitFor({ state: 'visible', timeout: 15000 });

    const html = fs.readFileSync(path.join(__dirname, 'tabla_50x50.html'), 'utf-8');
    await editor.evaluate((el, contenido) => { el.innerHTML = contenido; }, html);

    // Medir el tiempo desde que se dispara el guardado hasta que la petición responde
    const inicio = Date.now();
    // Espera la respuesta del PUT/POST de guardado (ajusta la ruta si difiere)
    const [respuesta] = await Promise.all([
      page.waitForResponse(r => /\/api\/notes/.test(r.url()) && r.request().method() !== 'GET', { timeout: 10000 }).catch(() => null),
      // Dispara el guardado: ajusta a como tu app guarda (botón, atajo, blur...)
      editor.evaluate(el => el.blur()),
    ]);
    const duracion = Date.now() - inicio;

    console.log(`\n[CP-PE-07] Guardado de nota con tabla extensa:`);
    console.log(`  Tiempo de guardado: ${duracion} ms`);
    console.log(`  Umbral:             ${UMBRAL_GUARDADO_MS} ms`);
    if (respuesta) console.log(`  Respuesta HTTP:     ${respuesta.status()}`);

    expect(duracion, `Guardado ${duracion}ms`).toBeLessThan(UMBRAL_GUARDADO_MS);
  });

});
