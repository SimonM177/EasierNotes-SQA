#!/usr/bin/env bash
# ============================================================
#  ejecutar_todo.sh  ·  Easier-Notes SQA
#  Unifica TODA la ejecución: pruebas de código + JMeter +
#  pipeline de métricas -> Excel + datos.json para el dashboard.
#
#  Uso:   bash ejecutar_todo.sh
#  Requisito previo: MySQL y el backend corriendo en localhost:5219
# ============================================================
set -e
RESULTADOS="./resultados"
SALIDA="./dashboard"

echo "============================================"
echo " Ejecución completa SQA - Easier-Notes"
echo "============================================"

# --- 1) Pruebas de código (xUnit) ---
echo ""
echo "[1/5] Pruebas de código (xUnit)..."
dotnet test EasierNotes.sln --logger "trx;LogFileName=resultados.trx" \
  --collect:"XPlat Code Coverage" --results-directory "$RESULTADOS"

# --- 2) Pruebas de API (Newman) ---
echo ""
echo "[2/5] Pruebas de API (Newman): caja negra + sistema + aceptacion..."
newman run postman/EasierNotes.postman_collection.json -e postman/EasierNotes.postman_environment.json
newman run system/EasierNotes.system_collection.json -e postman/EasierNotes.postman_environment.json
newman run acceptance/EasierNotes.acceptance_collection.json -e postman/EasierNotes.postman_environment.json

# --- 3) Pruebas de desempeño (JMeter): 3 planes ---
echo ""
echo "[3/5] Pruebas de desempeno (JMeter): 3 planes..."
jmeter -n -t jmeter/JM-01_comportamiento_temporal.jmx -l "$RESULTADOS/jm01.jtl"
jmeter -n -t jmeter/JM-02_capacidad.jmx              -l "$RESULTADOS/jm02.jtl"
jmeter -n -t jmeter/JM-03_utilizacion_recursos.jmx  -l "$RESULTADOS/jm03.jtl"

# --- 4) Pipeline de métricas (Python + Pandas) ---
echo ""
echo "[4/5] Procesando metricas (Python + Pandas)..."
python pipeline_metricas.py --dir "$RESULTADOS" --out "$SALIDA"

# --- 5) Listo ---
echo ""
echo "[5/5] Completado."
echo "  - Excel de auditoria: $SALIDA/Auditoria_Metricas_EasierNotes.xlsx"
echo "  - datos.json:         $SALIDA/datos.json"
echo ""
echo "Abre el dashboard (index.html) y pulsa 'Actualizar' o recarga."
