// @ts-check
const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

/**
 * CP-PE-07 · Rendimiento del editor con tabla extensa (DOM pesado)
 * Subcaracterística: Comportamiento Temporal
 *
 * La nota se abre en su propia URL: /note/{id}
 * El editor es <div class="editor" contenteditable="true">.
 */

// ====== AJUSTA ESTO ======
const APP_URL = 'http://localhost:4200';
const NOTA_URL = APP_URL + '/note/2';   // entra DIRECTO a la nota 1
const SELECTOR_EDITOR = '.editor';
const UMBRAL_TECLA_MS = 100;
const UMBRAL_GUARDADO_MS = 1000;
// ==========================

async function abrirEditor(page) {
  // Entra directo a la nota por su URL (sin pasar por la lista)
  await page.goto(NOTA_URL, { waitUntil: 'networkidle' });
  const editor = page.locator(SELECTOR_EDITOR).first();
  await editor.waitFor({ state: 'visible', timeout: 15000 });
  return editor;
}

test.describe('CP-PE-07 · Editor con tabla extensa', () => {

  test('Fluidez de tecleo con tabla 50x50 cargada', async ({ page }) => {
    const editor = await abrirEditor(page);

    // Inyectar la tabla extensa (2500 celdas + 200 párrafos) en el editor
    const html = fs.readFileSync(path.join(__dirname, 'tabla_50x50.html'), 'utf-8');
    await editor.evaluate((el, contenido) => { el.innerHTML = contenido; }, html);
    await page.waitForTimeout(300);

    // Medir el tiempo de 20 pulsaciones con el DOM pesado
    await editor.click();
    const tiempos = [];
    for (let i = 0; i < 20; i++) {
      const inicio = Date.now();
      await page.keyboard.type('x');
      tiempos.push(Date.now() - inicio);
    }

    const promedio = tiempos.reduce((a, b) => a + b, 0) / tiempos.length;
    const max = Math.max(...tiempos);
    console.log(`\n[CP-PE-07] Tecleo con tabla extensa:`);
    console.log(`  Promedio por tecla: ${promedio.toFixed(1)} ms`);
    console.log(`  Máximo por tecla:   ${max} ms`);
    console.log(`  Umbral:             ${UMBRAL_TECLA_MS} ms`);

    expect(promedio, `Promedio por tecla ${promedio.toFixed(1)}ms`).toBeLessThan(UMBRAL_TECLA_MS);
  });

  test('Tiempo de guardado de una nota con tabla extensa', async ({ page }) => {
    const editor = await abrirEditor(page);

    const html = fs.readFileSync(path.join(__dirname, 'tabla_50x50.html'), 'utf-8');
    await editor.evaluate((el, contenido) => { el.innerHTML = contenido; }, html);

    // Medir el guardado: disparamos blur y esperamos la respuesta del API
    // El guardado se dispara con el botón de guardar (imagen alt="Guardar")
    const botonGuardar = page.locator('img[alt="Guardar"]').first();
    const inicio = Date.now();
    const [respuesta] = await Promise.all([
      page.waitForResponse(r => /\/api\/notes/.test(r.url()) && r.request().method() !== 'GET', { timeout: 10000 }).catch(() => null),
      botonGuardar.click(),
    ]);
    const duracion = Date.now() - inicio;

    console.log(`\n[CP-PE-07] Guardado de nota con tabla extensa:`);
    console.log(`  Tiempo de guardado: ${duracion} ms`);
    console.log(`  Umbral:             ${UMBRAL_GUARDADO_MS} ms`);
    if (respuesta) console.log(`  Respuesta HTTP:     ${respuesta.status()}`);

    expect(duracion, `Guardado ${duracion}ms`).toBeLessThan(UMBRAL_GUARDADO_MS);
  });

});