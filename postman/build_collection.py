#!/usr/bin/env python3
"""
Genera la colección Postman de pruebas de CAJA NEGRA para la API de EasierNotes.
Formato: Postman Collection v2.1. Salida validada como JSON.

Cada request incluye scripts de test (pm.test) que verifican el CONTRATO de la API
sin conocer su implementación: código HTTP, estructura del JSON y tiempo de respuesta.
"""
import json

BASE = "{{baseUrl}}"

def event_test(script_lines):
    return [{
        "listen": "test",
        "script": {"type": "text/javascript", "exec": script_lines}
    }]

def request(name, method, url_path, body=None, tests=None, prerequest=None):
    raw_url = BASE + url_path
    # Construir el objeto url con host/path para Postman
    path_parts = [p for p in url_path.strip("/").split("/") if p]
    url_obj = {
        "raw": raw_url,
        "host": [BASE],
        "path": path_parts
    }
    item = {
        "name": name,
        "request": {
            "method": method,
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "url": url_obj
        }
    }
    if body is not None:
        item["request"]["body"] = {
            "mode": "raw",
            "raw": json.dumps(body, ensure_ascii=False, indent=2),
            "options": {"raw": {"language": "json"}}
        }
    events = []
    if prerequest:
        events.append({"listen": "prerequest",
                       "script": {"type": "text/javascript", "exec": prerequest}})
    if tests:
        events.extend(event_test(tests))
    if events:
        item["event"] = events
    return item

# ---------------------------------------------------------------------
#  UMBRAL de tiempo de respuesta (toca Eficiencia de Desempeño)
# ---------------------------------------------------------------------
# Criterio de la HU "Crear nota": < 500 ms. Para el resto usamos 1000 ms.
THRESHOLD_CREATE = 500
THRESHOLD_GENERAL = 1000

# =====================================================================
#  CARPETA 1 — Camino feliz (particiones válidas)
# =====================================================================
happy = {
    "name": "1. Camino feliz (entradas válidas)",
    "item": [
        request(
            "POST crear nota", "POST", "/api/notes/create",
            tests=[
                "pm.test('Código 200 OK', function () {",
                "    pm.response.to.have.status(200);",
                "});",
                "pm.test('La respuesta es un objeto nota con Id', function () {",
                "    const json = pm.response.json();",
                "    pm.expect(json).to.have.property('id');",
                "    pm.expect(json).to.have.property('name');",
                "    pm.expect(json).to.have.property('html');",
                "});",
                "pm.test('El nombre empieza por \"Nueva Nota\"', function () {",
                "    const json = pm.response.json();",
                "    pm.expect(json.name).to.include('Nueva Nota');",
                "});",
                f"pm.test('Tiempo de respuesta < {THRESHOLD_CREATE} ms (HU Crear nota)', function () {{",
                f"    pm.expect(pm.response.responseTime).to.be.below({THRESHOLD_CREATE});",
                "});",
                "// Guardar el id de la nota creada para las siguientes peticiones",
                "if (pm.response.code === 200) {",
                "    const created = pm.response.json();",
                "    pm.collectionVariables.set('noteId', created.id);",
                "    pm.environment.set('noteId', created.id);",
                "}"
            ]
        ),
        request(
            "GET todas las notas", "GET", "/api/notes",
            tests=[
                "pm.test('Código 200 OK', function () {",
                "    pm.response.to.have.status(200);",
                "});",
                "pm.test('La respuesta es un arreglo', function () {",
                "    pm.expect(pm.response.json()).to.be.an('array');",
                "});",
                "pm.test('Contiene al menos una nota', function () {",
                "    pm.expect(pm.response.json().length).to.be.above(0);",
                "});",
                f"pm.test('Tiempo de respuesta < {THRESHOLD_GENERAL} ms', function () {{",
                f"    pm.expect(pm.response.responseTime).to.be.below({THRESHOLD_GENERAL});",
                "});"
            ]
        ),
        request(
            "GET nota por id (existente)", "GET", "/api/notes/{{noteId}}",
            tests=[
                "pm.test('Código 200 OK', function () {",
                "    pm.response.to.have.status(200);",
                "});",
                "pm.test('Devuelve la nota solicitada', function () {",
                "    const json = pm.response.json();",
                "    pm.expect(String(json.id)).to.eql(String(pm.collectionVariables.get('noteId')));",
                "});"
            ]
        ),
        request(
            "PUT actualizar nota", "PUT", "/api/notes/update",
            body={"id": "{{noteId}}", "name": "Reunión Q3", "html": "<p>contenido actualizado</p>", "categoryId": 1},
            prerequest=[
                "// Insertar el id numérico real en el cuerpo (las variables van como texto)",
                "let body = JSON.parse(pm.request.body.raw);",
                "body.id = Number(pm.collectionVariables.get('noteId'));",
                "pm.request.body.raw = JSON.stringify(body);"
            ],
            tests=[
                "pm.test('Código 204 No Content', function () {",
                "    pm.response.to.have.status(204);",
                "});",
                f"pm.test('Tiempo de respuesta < {THRESHOLD_GENERAL} ms', function () {{",
                f"    pm.expect(pm.response.responseTime).to.be.below({THRESHOLD_GENERAL});",
                "});"
            ]
        ),
        request(
            "PATCH asociar a categoría", "PATCH", "/api/notes/addToCategory/{{noteId}}/1",
            tests=[
                "pm.test('Código 204 No Content', function () {",
                "    pm.response.to.have.status(204);",
                "});"
            ]
        ),
        request(
            "DELETE borrar nota", "DELETE", "/api/notes/delete/{{noteId}}",
            tests=[
                "pm.test('Código 204 No Content', function () {",
                "    pm.response.to.have.status(204);",
                "});"
            ]
        ),
    ]
}

# =====================================================================
#  CARPETA 2 — Casos negativos (particiones inválidas + valores límite)
# =====================================================================
negative = {
    "name": "2. Casos negativos (entradas inválidas)",
    "item": [
        request(
            "GET nota inexistente -> 404", "GET", "/api/notes/999999",
            tests=[
                "pm.test('Código 404 Not Found', function () {",
                "    pm.response.to.have.status(404);",
                "});"
            ]
        ),
        request(
            "PUT nota inexistente -> 404", "PUT", "/api/notes/update",
            body={"id": 999999, "name": "x", "html": "<p>y</p>", "categoryId": 1},
            tests=[
                "pm.test('Código 404 Not Found', function () {",
                "    pm.response.to.have.status(404);",
                "});"
            ]
        ),
        request(
            "DELETE nota inexistente -> 404", "DELETE", "/api/notes/delete/999999",
            tests=[
                "pm.test('Código 404 Not Found', function () {",
                "    pm.response.to.have.status(404);",
                "});"
            ]
        ),
        request(
            "PATCH nota inexistente -> 404", "PATCH", "/api/notes/addToCategory/999999/1",
            tests=[
                "pm.test('Código 404 Not Found', function () {",
                "    pm.response.to.have.status(404);",
                "});"
            ]
        ),
    ]
}

collection = {
    "info": {
        "name": "EasierNotes — Pruebas de Caja Negra (API)",
        "description": (
            "Pruebas de caja negra sobre la API REST de EasierNotes. "
            "Verifican el contrato de la API (códigos HTTP, estructura JSON y tiempo de "
            "respuesta) sin conocer la implementación. Técnicas 29119 aplicadas: "
            "partición de equivalencia (válidos vs inválidos), valores límite (ids y "
            "tiempos) y pruebas de la interfaz. Requiere la API corriendo en {{baseUrl}}."
        ),
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    "item": [happy, negative],
    "variable": [
        {"key": "baseUrl", "value": "http://localhost:5219", "type": "string"},
        {"key": "noteId", "value": "", "type": "string"}
    ]
}

out = "postman/EasierNotes.postman_collection.json"
with open(out, "w", encoding="utf-8") as f:
    json.dump(collection, f, ensure_ascii=False, indent=2)

# Validación: releer y contar
with open(out, encoding="utf-8") as f:
    reloaded = json.load(f)

total_requests = sum(len(folder["item"]) for folder in reloaded["item"])
total_tests = 0
for folder in reloaded["item"]:
    for req in folder["item"]:
        for ev in req.get("event", []):
            if ev["listen"] == "test":
                total_tests += sum(1 for line in ev["script"]["exec"] if "pm.test(" in line)

print("Colección generada y validada como JSON.")
print(f"  Carpetas: {len(reloaded['item'])}")
print(f"  Peticiones: {total_requests}")
print(f"  Aserciones (pm.test): {total_tests}")
print(f"  Archivo: {out}")
