#!/usr/bin/env python3
"""
Genera la colección de PRUEBAS DE SISTEMA (E2E) de EasierNotes.
A diferencia de la caja negra (peticiones sueltas) y la aceptación (criterios de
negocio), aquí se ejercitan RECORRIDOS COMPLETOS de usuario (user journeys) sobre
el sistema desplegado (backend real + MySQL), encadenando operaciones y verificando
el estado del sistema tras cada paso.

Nivel de prueba: SISTEMA (caja negra sobre el sistema integrado y en ejecución).
"""
import json

BASE = "{{baseUrl}}"

def req(name, method, path, body=None, tests=None, prereq=None):
    parts = [p for p in path.strip("/").split("/") if p]
    item = {"name": name, "request": {
        "method": method,
        "header": [{"key": "Content-Type", "value": "application/json"}],
        "url": {"raw": BASE + path, "host": [BASE], "path": parts}}}
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

def set_id_from_response(varname):
    return [
        f"const n = pm.response.json();",
        f"pm.collectionVariables.set('{varname}', n.id);",
        f"pm.environment.set('{varname}', n.id);"
    ]

# =====================================================================
#  RECORRIDO A — Ciclo de vida completo de una nota
# =====================================================================
recorrido_a = {
    "name": "Recorrido A — Ciclo de vida completo de una nota",
    "item": [
        req("A1. Crear nota", "POST", "/api/notes/create",
            tests=[
                "pm.test('A1: la nota se crea (200)', () => pm.response.to.have.status(200));",
                *set_id_from_response('notaA'),
                "pm.test('A1: tiene identificador válido', function () {",
                "    pm.expect(pm.response.json().id).to.be.above(0);",
                "});"
            ]),
        req("A2. Verificar que existe en el sistema", "GET", "/api/notes/{{notaA}}",
            tests=[
                "pm.test('A2: la nota recién creada se puede consultar', () => pm.response.to.have.status(200));"
            ]),
        req("A3. Editar el contenido", "PUT", "/api/notes/update",
            body={"id": "{{notaA}}", "name": "Plan de proyecto", "html": "<p>Fase 1: análisis.</p>", "categoryId": 1},
            prereq=[
                "let b = JSON.parse(pm.request.body.raw);",
                "b.id = Number(pm.collectionVariables.get('notaA'));",
                "pm.request.body.raw = JSON.stringify(b);"
            ],
            tests=[
                "pm.test('A3: la edición se guarda (204)', () => pm.response.to.have.status(204));"
            ]),
        req("A4. Verificar que el cambio persiste", "GET", "/api/notes/{{notaA}}",
            tests=[
                "pm.test('A4: el contenido editado persiste en el sistema', function () {",
                "    const n = pm.response.json();",
                "    pm.expect(n.name).to.eql('Plan de proyecto');",
                "    pm.expect(n.html).to.include('Fase 1');",
                "});"
            ]),
        req("A5. Asociar a una categoría", "PATCH", "/api/notes/addToCategory/{{notaA}}/1",
            tests=[
                "pm.test('A5: la asociación a categoría se realiza (204)', () => pm.response.to.have.status(204));"
            ]),
        req("A6. Eliminar la nota", "DELETE", "/api/notes/delete/{{notaA}}",
            tests=[
                "pm.test('A6: la nota se elimina (204)', () => pm.response.to.have.status(204));"
            ]),
        req("A7. Verificar que ya no existe", "GET", "/api/notes/{{notaA}}",
            tests=[
                "pm.test('A7: tras eliminar, la nota ya no existe (404)', () => pm.response.to.have.status(404));"
            ]),
    ]
}

# =====================================================================
#  RECORRIDO B — Numeración de notas por defecto (regla de negocio E2E)
# =====================================================================
recorrido_b = {
    "name": "Recorrido B — Numeración de notas por defecto",
    "item": [
        req("B0. Preparar: contar notas 'Nueva Nota' actuales", "GET", "/api/notes",
            tests=[
                "const arr = pm.response.json();",
                "const qty = arr.filter(n => n.name && n.name.startsWith('Nueva Nota')).length;",
                "pm.collectionVariables.set('baseQty', qty);",
                "pm.test('B0: sistema disponible para el recorrido', () => pm.response.to.have.status(200));"
            ]),
        req("B1. Crear primera nota por defecto", "POST", "/api/notes/create",
            tests=[
                *set_id_from_response('notaB1'),
                "pm.test('B1: la nota recibe un nombre por defecto', function () {",
                "    pm.expect(pm.response.json().name).to.include('Nueva Nota');",
                "});"
            ]),
        req("B2. Crear segunda nota por defecto", "POST", "/api/notes/create",
            tests=[
                *set_id_from_response('notaB2'),
                "pm.test('B2: la segunda nota lleva un sufijo numérico', function () {",
                "    pm.expect(pm.response.json().name).to.match(/Nueva Nota \\(\\d+\\)/);",
                "});"
            ]),
        req("B3. Limpieza B1", "DELETE", "/api/notes/delete/{{notaB1}}",
            tests=["pm.test('B3: limpieza ok', () => pm.expect([204,404]).to.include(pm.response.code));"]),
        req("B4. Limpieza B2", "DELETE", "/api/notes/delete/{{notaB2}}",
            tests=["pm.test('B4: limpieza ok', () => pm.expect([204,404]).to.include(pm.response.code));"]),
    ]
}

# =====================================================================
#  RECORRIDO C — Robustez ante datos inexistentes
# =====================================================================
recorrido_c = {
    "name": "Recorrido C — Robustez ante recursos inexistentes",
    "item": [
        req("C1. Consultar nota inexistente", "GET", "/api/notes/999999",
            tests=["pm.test('C1: consultar inexistente -> 404', () => pm.response.to.have.status(404));"]),
        req("C2. Editar nota inexistente", "PUT", "/api/notes/update",
            body={"id": 999999, "name": "x", "html": "<p>y</p>", "categoryId": 1},
            tests=["pm.test('C2: editar inexistente -> 404', () => pm.response.to.have.status(404));"]),
        req("C3. Eliminar nota inexistente", "DELETE", "/api/notes/delete/999999",
            tests=["pm.test('C3: eliminar inexistente -> 404', () => pm.response.to.have.status(404));"]),
        req("C4. El sistema sigue operativo tras los errores", "GET", "/api/notes",
            tests=["pm.test('C4: el sistema sigue respondiendo tras los 404', () => pm.response.to.have.status(200));"]),
    ]
}

collection = {
    "info": {
        "name": "EasierNotes — Pruebas de Sistema (E2E)",
        "description": (
            "Pruebas de SISTEMA sobre EasierNotes desplegado (backend .NET + MySQL). "
            "Ejercen RECORRIDOS completos de usuario de extremo a extremo por la interfaz "
            "HTTP del sistema, verificando el estado tras cada paso. Nivel: sistema "
            "(caja negra sobre el sistema integrado en ejecución). Requiere el sistema "
            "corriendo en {{baseUrl}} con su base de datos."
        ),
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    "item": [recorrido_a, recorrido_b, recorrido_c],
    "variable": [
        {"key": "baseUrl", "value": "http://localhost:5219", "type": "string"},
        {"key": "notaA", "value": "", "type": "string"},
        {"key": "notaB1", "value": "", "type": "string"},
        {"key": "notaB2", "value": "", "type": "string"},
        {"key": "baseQty", "value": "0", "type": "string"}
    ]
}

out = "system/EasierNotes.system_collection.json"
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
print("Colección de sistema (E2E) generada y validada.")
print(f"  Recorridos: {len(r['item'])}")
print(f"  Peticiones: {reqs}")
print(f"  Aserciones (pm.test): {tests}")
