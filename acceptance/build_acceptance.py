#!/usr/bin/env python3
"""
Genera la colección Newman de PRUEBAS DE ACEPTACIÓN de EasierNotes.
Cada carpeta = una Historia de Usuario; cada request valida un criterio de
aceptación OFICIAL de la ERS, en el lenguaje del cliente. Trazable a los
escenarios Gherkin de criterios_aceptacion.feature.

Diferencia con la colección de caja negra:
  - Caja negra   -> valida el CONTRATO técnico de la API (status, JSON).
  - Aceptación   -> valida los CRITERIOS DE NEGOCIO de las HU (rapidez, flujo).
"""
import json

BASE = "{{baseUrl}}"

def req(name, method, path, body=None, tests=None, prereq=None):
    parts = [p for p in path.strip("/").split("/") if p]
    item = {
        "name": name,
        "request": {
            "method": method,
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "url": {"raw": BASE + path, "host": [BASE], "path": parts}
        }
    }
    if body is not None:
        item["request"]["body"] = {"mode": "raw",
            "raw": json.dumps(body, ensure_ascii=False, indent=2),
            "options": {"raw": {"language": "json"}}}
    ev = []
    if prereq:
        ev.append({"listen": "prerequest", "script": {"type": "text/javascript", "exec": prereq}})
    if tests:
        ev.append({"listen": "test", "script": {"type": "text/javascript", "exec": tests}})
    if ev:
        item["event"] = ev
    return item

# =====================================================================
#  HU CREAR NOTA — criterio ERS: < 0,5 s + se registra en BD
# =====================================================================
crear = {
    "name": "HU Crear nota (Javier Díaz) — Eficiencia de Desempeño",
    "item": [
        req("Crear nota lista en < 0,5 s", "POST", "/api/notes/create",
            tests=[
                "// Criterio de aceptación ERS: la nota se crea en menos de 0,5 s.",
                "pm.test('CA: la nota queda lista en menos de 0,5 segundos', function () {",
                "    pm.expect(pm.response.responseTime).to.be.below(500);",
                "});",
                "pm.test('CA: la nota queda registrada en el sistema', function () {",
                "    pm.response.to.have.status(200);",
                "    const n = pm.response.json();",
                "    pm.expect(n).to.have.property('id');",
                "    pm.collectionVariables.set('noteId', n.id);",
                "    pm.environment.set('noteId', n.id);",
                "});"
            ]),
        req("Verificar que la nota quedó registrada", "GET", "/api/notes/{{noteId}}",
            tests=[
                "pm.test('CA: la nota existe en el sistema tras crearla', function () {",
                "    pm.response.to.have.status(200);",
                "});"
            ]),
    ]
}

# =====================================================================
#  HU BUSCAR NOTA — criterio ERS: coincidencias + < 1 s + sin resultados
#  (La búsqueda es en cliente; a nivel de sistema validamos que GET /api/notes
#   responde rápido y trae los datos sobre los que el frontend filtra.)
# =====================================================================
buscar = {
    "name": "HU Buscar nota (Samuel Parra) — Eficiencia de Desempeño",
    "item": [
        req("Obtener notas para búsqueda en < 1 s", "GET", "/api/notes",
            tests=[
                "pm.test('CA: la búsqueda dispone de datos en menos de 1 segundo', function () {",
                "    pm.expect(pm.response.responseTime).to.be.below(1000);",
                "});",
                "pm.test('CA: el sistema devuelve la lista de notas', function () {",
                "    pm.response.to.have.status(200);",
                "    pm.expect(pm.response.json()).to.be.an('array');",
                "});"
            ]),
    ]
}

# =====================================================================
#  HU ESCRIBIR TEXTO EN NOTA — criterio ERS: edición se conserva
# =====================================================================
escribir = {
    "name": "HU Escribir texto en nota (Germán Parra)",
    "item": [
        req("Guardar contenido editado", "PUT", "/api/notes/update",
            body={"id": "{{noteId}}", "name": "Apuntes de la reunión", "html": "<p>Contenido importante guardado por el usuario.</p>", "categoryId": 1},
            prereq=[
                "let b = JSON.parse(pm.request.body.raw);",
                "b.id = Number(pm.collectionVariables.get('noteId'));",
                "pm.request.body.raw = JSON.stringify(b);"
            ],
            tests=[
                "pm.test('CA: el guardado responde con rapidez', function () {",
                "    pm.expect(pm.response.responseTime).to.be.below(1000);",
                "});",
                "pm.test('CA: el guardado se completa correctamente', function () {",
                "    pm.response.to.have.status(204);",
                "});"
            ]),
        req("Verificar que el contenido se conservó", "GET", "/api/notes/{{noteId}}",
            tests=[
                "pm.test('CA: al reabrir la nota el contenido se conserva íntegro', function () {",
                "    pm.response.to.have.status(200);",
                "    const n = pm.response.json();",
                "    pm.expect(n.html).to.include('Contenido importante');",
                "    pm.expect(n.name).to.eql('Apuntes de la reunión');",
                "});"
            ]),
    ]
}

# =====================================================================
#  Limpieza (dejar el sistema como estaba) — buena práctica de aceptación
# =====================================================================
limpieza = {
    "name": "Limpieza del escenario",
    "item": [
        req("Eliminar la nota de prueba", "DELETE", "/api/notes/delete/{{noteId}}",
            tests=[
                "pm.test('El escenario deja el sistema limpio', function () {",
                "    pm.expect([204, 404]).to.include(pm.response.code);",
                "});"
            ]),
    ]
}

collection = {
    "info": {
        "name": "EasierNotes — Pruebas de Aceptación (criterios del cliente)",
        "description": (
            "Ciclo de pruebas de ACEPTACIÓN sobre el sistema EasierNotes, ejecutado por el "
            "tester en su rol de cliente. Cada carpeta corresponde a una Historia de Usuario "
            "y cada prueba valida un criterio de aceptación oficial de la ERS, con foco en "
            "Eficiencia de Desempeño. Trazable a los escenarios Gherkin de "
            "criterios_aceptacion.feature. Requiere el sistema corriendo en {{baseUrl}}."
        ),
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    "item": [crear, buscar, escribir, limpieza],
    "variable": [
        {"key": "baseUrl", "value": "http://localhost:5219", "type": "string"},
        {"key": "noteId", "value": "", "type": "string"}
    ]
}

out = "acceptance/EasierNotes.acceptance_collection.json"
with open(out, "w", encoding="utf-8") as f:
    json.dump(collection, f, ensure_ascii=False, indent=2)

with open(out, encoding="utf-8") as f:
    r = json.load(f)
reqs = sum(len(c["item"]) for c in r["item"])
tests = 0
for c in r["item"]:
    for it in c["item"]:
        for e in it.get("event", []):
            if e["listen"] == "test":
                tests += sum(1 for l in e["script"]["exec"] if "pm.test(" in l)
print("Colección de aceptación generada y validada.")
print(f"  Historias de usuario (carpetas): {len(r['item'])}")
print(f"  Peticiones: {reqs}")
print(f"  Criterios validados (pm.test): {tests}")
