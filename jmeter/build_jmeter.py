#!/usr/bin/env python3
"""
Genera los planes de prueba JMeter (.jmx) para EasierNotes.
Formato: Apache JMeter 5.6.3. Salida validada como XML.

Cada plan cubre una subcaracterística de Eficiencia de Desempeño (ISO 25010):
  JM-01  Comportamiento Temporal   -> latencia p95 bajo carga (POST /create)
  JM-02  Capacidad                 -> punto de quiebre por concurrencia (GET /notes)
  JM-03  Utilización de Recursos   -> throughput sostenido (carga mixta)
"""
import xml.etree.ElementTree as ET
from xml.dom import minidom

HOST = "localhost"
PORT = "5219"

def el(tag, attrib=None, text=None):
    e = ET.Element(tag, attrib or {})
    if text is not None:
        e.text = text
    return e

def prop(tag, name, text=None, elem_type=None):
    """Crea <stringProp name="...">text</stringProp> u otros tipos."""
    e = ET.Element(tag, {"name": name})
    if text is not None:
        e.text = str(text)
    return e

def bool_prop(name, val):
    return prop("boolProp", name, "true" if val else "false")

def str_prop(name, val=""):
    return prop("stringProp", name, val)

def int_prop(name, val):
    return prop("intProp", name, val)

# --- Bloque HTTP defaults (host/puerto comunes) ---
def http_defaults():
    cfg = ET.Element("ConfigTestElement", {
        "guiclass": "HttpDefaultsGui",
        "testclass": "ConfigTestElement",
        "testname": "Valores por defecto HTTP (EasierNotes)",
        "enabled": "true"
    })
    args = ET.SubElement(cfg, "elementProp", {"name": "HTTPsampler.Arguments",
        "elementType": "Arguments", "guiclass": "HTTPArgumentsPanel",
        "testclass": "Arguments", "testname": "User Defined Variables", "enabled": "true"})
    coll = ET.SubElement(args, "collectionProp", {"name": "Arguments.arguments"})
    cfg.append(str_prop("HTTPSampler.domain", HOST))
    cfg.append(str_prop("HTTPSampler.port", PORT))
    cfg.append(str_prop("HTTPSampler.protocol", "http"))
    cfg.append(str_prop("HTTPSampler.path", ""))
    return cfg

# --- Thread Group ---
def thread_group(name, num_threads, ramp_seconds, loops):
    tg = ET.Element("ThreadGroup", {
        "guiclass": "ThreadGroupGui", "testclass": "ThreadGroup",
        "testname": name, "enabled": "true"})
    tg.append(str_prop("ThreadGroup.on_sample_error", "continue"))
    lc = ET.SubElement(tg, "elementProp", {"name": "ThreadGroup.main_controller",
        "elementType": "LoopController", "guiclass": "LoopControlPanel",
        "testclass": "LoopController", "testname": "Loop Controller", "enabled": "true"})
    lc.append(bool_prop("LoopController.continue_forever", False))
    lc.append(str_prop("LoopController.loops", str(loops)))
    tg.append(str_prop("ThreadGroup.num_threads", str(num_threads)))
    tg.append(str_prop("ThreadGroup.ramp_time", str(ramp_seconds)))
    tg.append(bool_prop("ThreadGroup.scheduler", False))
    tg.append(str_prop("ThreadGroup.duration", ""))
    tg.append(str_prop("ThreadGroup.delay", ""))
    tg.append(bool_prop("ThreadGroup.same_user_on_next_iteration", True))
    return tg

# --- HTTP Sampler ---
def http_sampler(name, method, path, body_json=None):
    s = ET.Element("HTTPSamplerProxy", {
        "guiclass": "HttpTestSampleGui", "testclass": "HTTPSamplerProxy",
        "testname": name, "enabled": "true"})
    args = ET.SubElement(s, "elementProp", {"name": "HTTPsampler.Arguments",
        "elementType": "Arguments"})
    coll = ET.SubElement(args, "collectionProp", {"name": "Arguments.arguments"})
    if body_json is not None:
        s.append(bool_prop("HTTPSampler.postBodyRaw", True))
        arg = ET.SubElement(coll, "elementProp", {"name": "", "elementType": "HTTPArgument"})
        arg.append(bool_prop("HTTPArgument.always_encode", False))
        arg.append(str_prop("Argument.value", body_json))
        arg.append(str_prop("Argument.metadata", "="))
    s.append(str_prop("HTTPSampler.domain", ""))
    s.append(str_prop("HTTPSampler.port", ""))
    s.append(str_prop("HTTPSampler.protocol", ""))
    s.append(str_prop("HTTPSampler.path", path))
    s.append(str_prop("HTTPSampler.method", method))
    s.append(bool_prop("HTTPSampler.follow_redirects", True))
    s.append(bool_prop("HTTPSampler.auto_redirects", False))
    s.append(bool_prop("HTTPSampler.use_keepalive", True))
    s.append(bool_prop("HTTPSampler.DO_MULTIPART_POST", False))
    return s

# --- Header Manager (Content-Type JSON) ---
def header_manager():
    hm = ET.Element("HeaderManager", {
        "guiclass": "HeaderPanel", "testclass": "HeaderManager",
        "testname": "Cabeceras HTTP", "enabled": "true"})
    coll = ET.SubElement(hm, "collectionProp", {"name": "HeaderManager.headers"})
    h = ET.SubElement(coll, "elementProp", {"name": "", "elementType": "Header"})
    h.append(str_prop("Header.name", "Content-Type"))
    h.append(str_prop("Header.value", "application/json"))
    return hm

# --- Duration Assertion (umbral de tiempo) ---
def duration_assertion(max_ms):
    da = ET.Element("DurationAssertion", {
        "guiclass": "DurationAssertionGui", "testclass": "DurationAssertion",
        "testname": f"Umbral de tiempo < {max_ms} ms", "enabled": "true"})
    da.append(str_prop("DurationAssertion.duration", str(max_ms)))
    return da

# --- Response Assertion (código 200) ---
def response_assertion(code="200"):
    ra = ET.Element("ResponseAssertion", {
        "guiclass": "AssertionGui", "testclass": "ResponseAssertion",
        "testname": f"Código de respuesta = {code}", "enabled": "true"})
    coll = ET.SubElement(ra, "collectionProp", {"name": "Asserion.test_strings"})
    coll.append(str_prop("49586", code))
    ra.append(str_prop("Assertion.custom_message", ""))
    ra.append(str_prop("Assertion.test_field", "Assertion.response_code"))
    ra.append(bool_prop("Assertion.assume_success", False))
    ra.append(int_prop("Assertion.test_type", 8))  # 8 = "equals"
    return ra

# --- Listeners ---
def aggregate_report():
    return ET.Element("ResultCollector", {
        "guiclass": "StatVisualizer", "testclass": "ResultCollector",
        "testname": "Reporte Agregado (p90/p95/p99)", "enabled": "true"})

def summary_report():
    return ET.Element("ResultCollector", {
        "guiclass": "SummaryReport", "testclass": "ResultCollector",
        "testname": "Informe Resumen", "enabled": "true"})

def result_collector_saveconfig(rc):
    """Añade la config de guardado que JMeter espera en cada listener."""
    rc.append(bool_prop("ResultCollector.error_logging", False))
    obj = ET.SubElement(rc, "objProp")
    ET.SubElement(obj, "name").text = "saveConfig"
    value = ET.SubElement(obj, "value", {"class": "SampleSaveConfiguration"})
    flags = {
        "time": "true", "latency": "true", "timestamp": "true", "success": "true",
        "label": "true", "code": "true", "message": "true", "threadName": "true",
        "dataType": "true", "encoding": "false", "assertions": "true",
        "subresults": "true", "responseData": "false", "samplerData": "false",
        "xml": "false", "fieldNames": "true", "responseHeaders": "false",
        "requestHeaders": "false", "responseDataOnError": "false",
        "saveAssertionResultsFailureMessage": "true", "assertionsResultsToSave": "0",
        "bytes": "true", "sentBytes": "true", "threadCounts": "true",
        "idleTime": "true", "connectTime": "true"
    }
    for k, v in flags.items():
        ET.SubElement(value, k).text = v
    rc.append(str_prop("filename", ""))
    return rc

def build_plan(plan_name, tg, samplers_with_children, filename):
    """Ensambla un Test Plan completo con un Thread Group y sus samplers."""
    root = ET.Element("jmeterTestPlan", {
        "version": "1.2", "properties": "5.0", "jmeter": "5.6.3"})
    ht = ET.SubElement(root, "hashTree")

    # Test Plan
    tp = ET.SubElement(ht, "TestPlan", {
        "guiclass": "TestPlanGui", "testclass": "TestPlan",
        "testname": plan_name, "enabled": "true"})
    tp.append(bool_prop("TestPlan.functional_mode", False))
    tp.append(bool_prop("TestPlan.tearDown_on_shutdown", True))
    tp.append(bool_prop("TestPlan.serialize_threadgroups", False))
    udv = ET.SubElement(tp, "elementProp", {"name": "TestPlan.user_defined_variables",
        "elementType": "Arguments", "guiclass": "ArgumentsPanel",
        "testclass": "Arguments", "testname": "User Defined Variables", "enabled": "true"})
    ET.SubElement(udv, "collectionProp", {"name": "Arguments.arguments"})
    tp.append(str_prop("TestPlan.user_define_classpath", ""))
    tp_ht = ET.SubElement(ht, "hashTree")

    # HTTP defaults + Header manager a nivel de plan
    tp_ht.append(http_defaults())
    tp_ht.append(ET.Element("hashTree"))
    tp_ht.append(header_manager())
    tp_ht.append(ET.Element("hashTree"))

    # Thread Group
    tp_ht.append(tg)
    tg_ht = ET.SubElement(tp_ht, "hashTree")

    # Samplers con sus assertions
    for sampler, children in samplers_with_children:
        tg_ht.append(sampler)
        s_ht = ET.SubElement(tg_ht, "hashTree")
        for child in children:
            s_ht.append(child)
            s_ht.append(ET.Element("hashTree"))

    # Listeners a nivel de Thread Group
    agg = aggregate_report(); result_collector_saveconfig(agg)
    tg_ht.append(agg); tg_ht.append(ET.Element("hashTree"))
    summ = summary_report(); result_collector_saveconfig(summ)
    tg_ht.append(summ); tg_ht.append(ET.Element("hashTree"))

    # Serializar con indentación
    rough = ET.tostring(root, encoding="unicode")
    pretty = minidom.parseString(rough).toprettyxml(indent="  ")
    # Reemplazar el encabezado por el que JMeter usa exactamente
    lines = pretty.split("\n")
    lines[0] = '<?xml version="1.0" encoding="UTF-8"?>'
    pretty = "\n".join(l for l in lines if l.strip())  # quitar líneas vacías
    with open(filename, "w", encoding="utf-8") as f:
        f.write(pretty)
    return filename

# =====================================================================
#  JM-01 — Comportamiento Temporal (POST /create, p95 < 500 ms)
# =====================================================================
tg1 = thread_group("Carga: 20 usuarios x 10 = 200 peticiones", 20, 5, 10)
sampler1 = http_sampler("POST crear nota", "POST", "/api/notes/create", body_json="")
plan1 = build_plan(
    "JM-01 Comportamiento Temporal — Crear nota bajo carga",
    tg1,
    [(sampler1, [duration_assertion(500), response_assertion("200")])],
    "jmeter/JM-01_comportamiento_temporal.jmx"
)

# =====================================================================
#  JM-02 — Capacidad (GET /notes, punto de quiebre por concurrencia)
# =====================================================================
tg2 = thread_group("Carga creciente: 50 usuarios x 20 (ramp 30s)", 50, 30, 20)
sampler2 = http_sampler("GET todas las notas", "GET", "/api/notes")
plan2 = build_plan(
    "JM-02 Capacidad — Concurrencia sobre listado de notas",
    tg2,
    [(sampler2, [duration_assertion(1000), response_assertion("200")])],
    "jmeter/JM-02_capacidad.jmx"
)

# =====================================================================
#  JM-03 — Utilización de Recursos (carga mixta sostenida, throughput)
# =====================================================================
tg3 = thread_group("Carga mixta sostenida: 30 usuarios x 30", 30, 10, 30)
sampler3a = http_sampler("POST crear nota", "POST", "/api/notes/create", body_json="")
sampler3b = http_sampler("GET todas las notas", "GET", "/api/notes")
plan3 = build_plan(
    "JM-03 Utilización de Recursos — Throughput con carga mixta",
    tg3,
    [
        (sampler3a, [response_assertion("200")]),
        (sampler3b, [response_assertion("200")]),
    ],
    "jmeter/JM-03_utilizacion_recursos.jmx"
)

import xml.dom.minidom as md
for f in [plan1, plan2, plan3]:
    md.parse(f)  # valida que es XML bien formado
    print(f"OK (XML válido): {f}")
