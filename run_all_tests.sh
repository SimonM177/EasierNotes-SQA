#!/usr/bin/env bash
# =====================================================================
#  Ciclo de pruebas automatizado — EasierNotes
#  Ejecuta en secuencia las colecciones de Caja Negra, Sistema y Aceptación
#  contra el sistema desplegado, y genera un reporte HTML de evidencia.
#
#  Uso (con el backend + MySQL corriendo):
#     bash run_all_tests.sh
#
#  Requisitos: Node.js, Newman y el reporter htmlextra:
#     npm install -g newman newman-reporter-htmlextra
# =====================================================================
set -e

ENV="postman/EasierNotes.postman_environment.json"
OUT="test-reports"
mkdir -p "$OUT"

echo "==================================================="
echo " CICLO DE PRUEBAS AUTOMATIZADO — EasierNotes"
echo " $(date)"
echo "==================================================="

echo ""
echo ">> 1/3  Pruebas de CAJA NEGRA (contrato de la API)"
newman run postman/EasierNotes.postman_collection.json -e "$ENV" \
  -r cli,htmlextra --reporter-htmlextra-export "$OUT/01_caja_negra.html"

echo ""
echo ">> 2/3  Pruebas de SISTEMA (recorridos E2E)"
newman run system/EasierNotes.system_collection.json -e "$ENV" \
  -r cli,htmlextra --reporter-htmlextra-export "$OUT/02_sistema.html"

echo ""
echo ">> 3/3  Pruebas de ACEPTACIÓN (criterios del cliente)"
newman run acceptance/EasierNotes.acceptance_collection.json -e "$ENV" \
  -r cli,htmlextra --reporter-htmlextra-export "$OUT/03_aceptacion.html"

echo ""
echo "==================================================="
echo " CICLO COMPLETADO. Reportes HTML en: $OUT/"
echo "==================================================="
