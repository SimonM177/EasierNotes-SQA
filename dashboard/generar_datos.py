#!/usr/bin/env python3
"""
generar_datos.py — Plantilla para producir datos.json con los datos EN VIVO
del dashboard SQA a partir de los resultados reales de las pruebas.

Adapta las rutas a tu proyecto. Este script muestra CÓMO leer cada fuente;
puedes ejecutarlo tras cada corrida para refrescar el dashboard.

Uso:
    python generar_datos.py
    -> genera datos.json, que luego cargas en el dashboard con el botón "Cargar datos"
"""
import json
import glob
import os
import xml.etree.ElementTree as ET

# ---------------------------------------------------------------
# 1) PRUEBAS: leer los archivos .trx que produce `dotnet test`
# ---------------------------------------------------------------
def leer_trx(carpeta="."):
    total = passed = failed = 0
    for trx in glob.glob(os.path.join(carpeta, "**", "*.trx"), recursive=True):
        try:
            tree = ET.parse(trx)
            ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
            counters = tree.find(".//t:ResultSummary/t:Counters", ns)
            if counters is not None:
                total += int(counters.get("total", 0))
                passed += int(counters.get("passed", 0))
                failed += int(counters.get("failed", 0))
        except Exception as e:
            print(f"  (aviso) no pude leer {trx}: {e}")
    return total, passed, failed

# ---------------------------------------------------------------
# 2) COBERTURA: leer el cobertura.xml (formato Cobertura de coverlet)
# ---------------------------------------------------------------
def leer_cobertura(carpeta="."):
    for cov in glob.glob(os.path.join(carpeta, "**", "coverage.cobertura.xml"), recursive=True):
        try:
            root = ET.parse(cov).getroot()
            linea = round(float(root.get("line-rate", 0)) * 100, 1)
            rama = round(float(root.get("branch-rate", 0)) * 100, 1)
            return linea, rama
        except Exception as e:
            print(f"  (aviso) no pude leer {cov}: {e}")
    return None, None

# ---------------------------------------------------------------
# 3) JMETER: leer un .jtl para calcular p95, promedio, etc.
# ---------------------------------------------------------------
def leer_jtl(ruta):
    if not os.path.exists(ruta):
        return None
    import csv
    tiempos, errores = [], 0
    with open(ruta) as f:
        for row in csv.DictReader(f):
            try:
                tiempos.append(int(row["elapsed"]))
                if row.get("success", "true") == "false":
                    errores += 1
            except (KeyError, ValueError):
                pass
    if not tiempos:
        return None
    tiempos.sort()
    n = len(tiempos)
    p95 = tiempos[int(n * 0.95)] if n else 0
    return {
        "muestras": n,
        "p95": p95,
        "promedio": round(sum(tiempos) / n),
        "mediana": tiempos[n // 2],
        "max": max(tiempos),
        "min": min(tiempos),
        "errorPct": round(errores / n * 100, 2),
    }

# ---------------------------------------------------------------
# Construir el objeto de datos (partiendo de la plantilla base)
# ---------------------------------------------------------------
def main():
    total, passed, failed = leer_trx()
    linea, rama = leer_cobertura()
    jm01 = leer_jtl("jmeter/resultados/jm01.jtl")  # ajusta la ruta

    # Carga la estructura base desde datos.js si quieres conservar el resto,
    # o define aquí los valores. Este ejemplo actualiza los que sí se pudieron leer.
    print("Resultados leídos:")
    print(f"  Pruebas: total={total} passed={passed} failed={failed}")
    print(f"  Cobertura: línea={linea}% rama={rama}%")
    print(f"  JMeter JM-01: {jm01}")

    # NOTA: por simplicidad, aquí solo se imprime. Para producir el datos.json
    # completo, copia la estructura de datos.js y sustituye los campos leídos.
    print("\nAdapta este script para volcar el objeto completo a datos.json.")
    print("La estructura esperada está en datos.js (window.SQA_DATA).")

if __name__ == "__main__":
    main()
