# ============================================================
#  ejecutar_todo.ps1  ·  Easier-Notes SQA
#  Unifica TODA la ejecución: pruebas de código + JMeter +
#  pipeline de métricas -> Excel + datos.json para el dashboard.
#
#  Uso:   .\ejecutar_todo.ps1
#  Requisito previo: MySQL y el backend corriendo en localhost:5219
# ============================================================

$ErrorActionPreference = "Stop"
$resultados = "./resultados"
$salida = "./dashboard"   # el datos.json va directo a la carpeta del dashboard

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Ejecución completa SQA - Easier-Notes" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# --- 1) Pruebas de código (xUnit): unitarias + integración ---
Write-Host "`n[1/5] Pruebas de código (xUnit)..." -ForegroundColor Yellow
dotnet test EasierNotes.sln --logger "trx;LogFileName=resultados.trx" `
  --collect:"XPlat Code Coverage" --results-directory $resultados

# --- 2) Pruebas de caja negra, sistema y aceptación (Newman) ---
Write-Host "`n[2/5] Pruebas de API (Newman): caja negra + sistema + aceptacion..." -ForegroundColor Yellow
newman run postman/EasierNotes.postman_collection.json -e postman/EasierNotes.postman_environment.json
newman run system/EasierNotes.system_collection.json -e postman/EasierNotes.postman_environment.json
newman run acceptance/EasierNotes.acceptance_collection.json -e postman/EasierNotes.postman_environment.json

# --- 3) Pruebas de desempeño (JMeter): los 3 planes ---
Write-Host "`n[3/5] Pruebas de desempeno (JMeter): 3 planes..." -ForegroundColor Yellow
jmeter -n -t jmeter/JM-01_comportamiento_temporal.jmx -l "$resultados/jm01.jtl"
jmeter -n -t jmeter/JM-02_capacidad.jmx              -l "$resultados/jm02.jtl"
jmeter -n -t jmeter/JM-03_utilizacion_recursos.jmx  -l "$resultados/jm03.jtl"

# --- 4) Pipeline de metricas (Python + Pandas) -> Excel + JSON ---
Write-Host "`n[4/5] Procesando metricas (Python + Pandas)..." -ForegroundColor Yellow
python pipeline_metricas.py --dir $resultados --out $salida

# --- 5) Listo ---
Write-Host "`n[5/5] Completado." -ForegroundColor Green
Write-Host "  - Excel de auditoria: $salida/Auditoria_Metricas_EasierNotes.xlsx"
Write-Host "  - datos.json:         $salida/datos.json"
Write-Host "`nAbre el dashboard (index.html) y pulsa 'Actualizar' o recarga." -ForegroundColor Cyan
