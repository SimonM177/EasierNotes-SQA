# Pruebas de Caja Negra (API) — Postman

Colección de pruebas de **caja negra** sobre la API REST de EasierNotes. Verifican el
**contrato** de la API (códigos HTTP, estructura del JSON y tiempo de respuesta) tratando
al backend como una caja cerrada, sin conocer su implementación interna.

- `EasierNotes.postman_collection.json` — la colección (2 carpetas, 10 peticiones, 18 aserciones).
- `EasierNotes.postman_environment.json` — variables de entorno (URL base).
- `build_collection.py` — generador de la colección (para regenerarla si cambia la API).
- `mock_server.js` — servidor de simulación mínimo (Node) que reproduce el contrato HTTP de la API, útil para probar la colección sin levantar el backend .NET real. **No es la API real.**

## Técnicas de prueba aplicadas (ISO/IEC/IEEE 29119-4)

- **Partición de equivalencia:** carpeta 1 (entradas válidas) vs. carpeta 2 (entradas inválidas).
- **Valores límite:** ids inexistentes (999999) y umbrales de tiempo de respuesta.
- **Pruebas de la interfaz (API contract):** cada endpoint devuelve el código HTTP y la estructura esperada.
- **Eficiencia de Desempeño:** se verifica el tiempo de respuesta (< 500 ms en crear, según la HU; < 1000 ms en el resto).

## Requisito previo: la API debe estar corriendo

Las pruebas de caja negra golpean la API real, así que el backend debe estar levantado y
accesible en `http://localhost:5219` (con su base de datos MySQL). Si la URL o el puerto
cambian, edita la variable `baseUrl` del environment.

## Cómo ejecutar

### Opción A — App de Postman (interfaz gráfica)
1. Abrir Postman → *Import* → seleccionar los dos archivos `.json`.
2. Elegir el environment "EasierNotes Local" (arriba a la derecha).
3. Clic derecho sobre la colección → *Run collection* → *Run*.

### Opción B — Newman (línea de comandos, gratis)
Newman es el corredor de Postman por consola; permite automatizar e integrar en CI.

```bash
# Instalar Newman (requiere Node.js)
npm install -g newman

# Ejecutar la colección con su environment
newman run postman/EasierNotes.postman_collection.json \
  -e postman/EasierNotes.postman_environment.json

# Generar un reporte HTML (opcional)
npm install -g newman-reporter-htmlextra
newman run postman/EasierNotes.postman_collection.json \
  -e postman/EasierNotes.postman_environment.json \
  -r htmlextra
```

## Orden de ejecución

La carpeta 1 (camino feliz) crea una nota y guarda su `id` en una variable de colección,
que reutilizan las peticiones siguientes (GET por id, PUT, PATCH, DELETE). Ejecutar la
colección completa en orden garantiza que ese encadenamiento funcione.
