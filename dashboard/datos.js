// datos.js — Datos EN VIVO del dashboard SQA Easier-Notes
// Este archivo se regenera tras cada corrida de pruebas.
// El dashboard lo lee al cargar; también se puede subir un datos.json nuevo desde la interfaz.
window.SQA_DATA = {
  meta: {
    proyecto: "Easier-Notes",
    caracteristica: "Eficiencia de Desempeño",
    subcaracteristicas: ["Comportamiento Temporal", "Utilización de Recursos", "Capacidad"],
    norma: "ISO/IEC/IEEE 29119 · ISO/IEC 25010 · IEEE 1061",
    actualizado: "2026-07-14 (CP-PE-07 cubierto)",
    version: "1.0"
  },
  kpis: {
    totalPruebas: 97,
    pruebasVerdes: 97,
    coberturaLinea: 98.8,
    coberturaRama: 92.1,
    asercionesApi: 44,
    hallazgos: 4,
    p95: 14,
    umbralP95: 500,
    throughput: 3.0,
    errorPct: 1.0
  },
  niveles: [
    { nombre: "Unitarias", cantidad: 65, tipo: "Caja blanca", estado: "verde", enfoque: "Lógica de negocio aislada" },
    { nombre: "Integración", cantidad: 32, tipo: "Caja gris", estado: "verde", enfoque: "Controlador + BD real" },
    { nombre: "Caja negra", cantidad: 18, tipo: "Caja negra", estado: "verde", enfoque: "Contrato de la API" },
    { nombre: "Sistema", cantidad: 17, tipo: "Caja negra", estado: "verde", enfoque: "Recorridos E2E" },
    { nombre: "Aceptación", cantidad: 9, tipo: "Caja negra", estado: "parcial", enfoque: "Criterios del cliente" },
    { nombre: "Navegador (E2E)", cantidad: 2, tipo: "Caja negra", estado: "verde", enfoque: "Rendimiento del editor (Playwright)" }
  ],
  cobertura: { linea: 98.8, rama: 92.1, umbral: 80 },
  casos: [
    { id: "CP-PE-01", nombre: "Creación < 0,5 s", riesgo: "Alto", sub: "Comportamiento Temporal", nivel: "Unitario/Desempeño", estado: "Completo" },
    { id: "CP-PE-02", nombre: "Creación en el borde / conteo", riesgo: "Alto", sub: "Comportamiento Temporal", nivel: "Unitario/Integración", estado: "Completo" },
    { id: "CP-PE-13", nombre: "Payload de imagen 5MB", riesgo: "Alto", sub: "Utilización de Recursos", nivel: "Unitario", estado: "Completo" },
    { id: "CP-PE-15", nombre: "Validación de imagen", riesgo: "Alto", sub: "Utilización de Recursos", nivel: "Unitario/Aceptación", estado: "Completo" },
    { id: "CP-PE-03", nombre: "Guardado de 15 MB", riesgo: "Medio", sub: "Utilización de Recursos", nivel: "Integración", estado: "Parcial" },
    { id: "CP-PE-05", nombre: "Búsqueda sobre 10.000", riesgo: "Medio", sub: "Capacidad", nivel: "Desempeño", estado: "Parcial" },
    { id: "CP-PE-16", nombre: "Volumen 50.000", riesgo: "Medio", sub: "Capacidad", nivel: "Desempeño", estado: "Parcial" },
    { id: "CP-PE-07", nombre: "Tabla extensa (DOM)", riesgo: "Medio", sub: "Comportamiento Temporal", nivel: "Navegador (Playwright)", estado: "Completo" }
  ],
  desempeno: {
    jm01: {
      nombre: "JM-01 · Comportamiento Temporal",
      muestras: 400, p95: 14, promedio: 19, mediana: 9, max: 1222, min: 6,
      throughput: 3.0, errorPct: 1.0, umbral: 500,
      // distribución de latencias (histograma) — datos representativos de la corrida
      histograma: [
        { rango: "0-10", frec: 210 },
        { rango: "10-20", frec: 120 },
        { rango: "20-50", frec: 45 },
        { rango: "50-100", frec: 15 },
        { rango: "100-500", frec: 6 },
        { rango: ">500", frec: 4 }
      ]
    },
    jm02: { nombre: "JM-02 · Capacidad", estado: "pendiente" },
    jm03: { nombre: "JM-03 · Utilización de Recursos", estado: "pendiente" },
    cp07: {
      nombre: "CP-PE-07 · Editor (Playwright)",
      tecleoMs: 50.2, tecleoUmbral: 100, tecleoMax: 55,
      guardadoMs: 69, guardadoUmbral: 1000, httpStatus: 204,
      estado: "verde"
    }
  },
  tecnicas: [
    { nombre: "Valores límite", casos: 5 },
    { nombre: "Partición de equivalencia", casos: 4 },
    { nombre: "Tabla de decisión", casos: 1 },
    { nombre: "Pruebas de excepción", casos: 3 },
    { nombre: "Prueba de carga", casos: 3 },
    { nombre: "Caracterización", casos: 2 }
  ],
  ecosistema: [
    { herramienta: "xUnit + coverlet", capa: "Herramienta", rol: "Resultados y cobertura", datos: ".trx / cobertura.xml" },
    { herramienta: "Newman", capa: "Herramienta", rol: "Aserciones y tiempos", datos: "reportes htmlextra" },
    { herramienta: "JMeter", capa: "Herramienta", rol: "Latencia y throughput", datos: "reporte agregado / .jtl" },
    { herramienta: "Playwright", capa: "Herramienta", rol: "Rendimiento del editor (DOM)", datos: "reporte HTML" },
    { herramienta: "GitHub Actions", capa: "Gobernanza", rol: "Orquesta, agrega, versiona", datos: "artefacto por commit" },
    { herramienta: "JIRA", capa: "Repositorio", rol: "Registro de hallazgos", datos: "incidencias trazables" }
  ],
  gobernanza: [
    { propiedad: "Centralización", desc: "Los resultados se reúnen en un punto único (artefacto de CI)" },
    { propiedad: "Reproducibilidad", desc: "Se ejecutan sobre un entorno limpio en cada corrida" },
    { propiedad: "Trazabilidad", desc: "Cada métrica se ata a un commit y a un caso de prueba" },
    { propiedad: "Detección temprana", desc: "El fallo se reporta en el momento del cambio" }
  ],
  hallazgos: [
    { id: "H-01", desc: "Ausencia de validación de imágenes (tamaño/formato)", severidad: "Alta", nivel: "Unitario", jira: "EN-101" },
    { id: "H-02", desc: "Duplicación de nombres al borrar nota intermedia", severidad: "Media", nivel: "Integración", jira: "EN-102" },
    { id: "H-03", desc: "Inflación de tamaño por Base64 (~33%)", severidad: "Media", nivel: "Unitario", jira: "EN-103" },
    { id: "H-04", desc: "Categoría por defecto (id 1) no garantizada — falla FK", severidad: "Alta", nivel: "Integración", jira: "EN-104" }
  ],
  objetivos: [
    { id: "OBJ-PC-1", dim: "Proceso", desc: "Cobertura de línea ≥ 80%", meta: 80, valor: 98.8, estado: "Superado" },
    { id: "OBJ-PC-2", dim: "Proceso", desc: "100% pruebas superadas", meta: 100, valor: 100, estado: "Alcanzado" },
    { id: "OBJ-PC-3", dim: "Proceso", desc: "Automatización ≥ 90%", meta: 90, valor: 100, estado: "Alcanzado" },
    { id: "OBJ-PC-4", dim: "Proceso", desc: "≥ 3 hallazgos documentados", meta: 3, valor: 4, estado: "Superado" },
    { id: "OBJ-PD-1", dim: "Producto", desc: "p95 ≤ 500 ms", meta: 500, valor: 14, estado: "Alcanzado" },
    { id: "OBJ-PD-2", dim: "Producto", desc: "Inflación Base64 caracterizada", meta: 100, valor: 100, estado: "Alcanzado" },
    { id: "OBJ-PD-3", dim: "Producto", desc: "Capacidad caracterizada", meta: 100, valor: 50, estado: "Parcial" }
  ]
};
