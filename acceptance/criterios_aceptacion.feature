# =====================================================================
#  Criterios de Aceptación — EasierNotes (Gherkin)
#
#  Escritos por el TESTER en su rol de CLIENTE (analista funcional que
#  conoce las necesidades del usuario). Cada escenario deriva de un
#  criterio de aceptación OFICIAL de la ERS del proyecto, y se enfoca en
#  la característica evaluada: Eficiencia de Desempeño (ISO/IEC 25010).
#
#  Formato: Gherkin (Dado–Cuando–Entonces). Lenguaje de NEGOCIO, no técnico.
#  Trazabilidad: cada Feature indica la Historia de Usuario de origen.
# =====================================================================

# ---------------------------------------------------------------------
Característica: Crear una nota con rapidez
  Como usuario de EasierNotes
  Quiero crear una nota nueva
  Para empezar a escribir información de inmediato

  # Origen: HU "Crear nota" (Javier Díaz).
  # Criterios ERS: < 0,5 s | se registra en la BD | se muestra la interfaz de edición.

  Escenario: La nota se crea en menos de medio segundo
    Dado que el sistema está disponible
    Cuando el usuario crea una nota nueva
    Entonces la nota queda lista en menos de 0,5 segundos
    Y la nota queda registrada en el sistema

  Escenario: La nota nueva recibe un nombre por defecto
    Dado que no existen notas por defecto previas
    Cuando el usuario crea su primera nota
    Entonces la nota se llama "Nueva Nota"
    Y queda lista para escribir en ella

# ---------------------------------------------------------------------
Característica: Buscar notas con facilidad y rapidez
  Como usuario de EasierNotes
  Quiero buscar mis notas por su nombre
  Para acceder a ellas fácil y rápidamente

  # Origen: HU "Buscar nota" (Samuel Parra).
  # Criterios ERS: muestra coincidencias | selecciona una | mensaje si no hay resultados.
  # Requisito suplementario de referencia: responder una búsqueda en menos de 1 segundo.

  Escenario: Se muestran las notas cuyo nombre coincide
    Dado que existen varias notas guardadas
    Y algunas contienen la palabra "reunión" en su nombre
    Cuando el usuario busca "reunión"
    Entonces se muestran únicamente las notas cuyo nombre contiene "reunión"
    Y el resultado aparece en menos de 1 segundo

  Escenario: Búsqueda sin coincidencias
    Dado que existen varias notas guardadas
    Cuando el usuario busca un texto que no aparece en ningún nombre
    Entonces no se muestra ninguna nota
    Y el sistema lo comunica sin demora

# ---------------------------------------------------------------------
Característica: Guardar el contenido de una nota
  Como usuario de EasierNotes
  Quiero editar y guardar el texto de mi nota
  Para conservar mis apuntes e información relevante

  # Origen: HU "Escribir texto en nota" (Germán Parra).
  # Criterios ERS: fácil edición | botón para terminar de escribir.

  Escenario: El contenido editado se conserva
    Dado que el usuario tiene una nota abierta
    Cuando escribe texto y guarda los cambios
    Entonces al volver a abrir la nota el contenido se conserva íntegro
    Y el guardado responde con rapidez

# ---------------------------------------------------------------------
Característica: Insertar imágenes válidas en una nota
  Como usuario de EasierNotes
  Quiero insertar imágenes en mis notas
  Para complementar el texto con elementos visuales

  # Origen: HU "Insertar imagen" (Matías León).
  # Criterios ERS: seleccionar imagen | error si tipo/tamaño inválido (.jpg/.png, máx 5 MB) | insertar si es válida.
  # NOTA DE ACEPTACIÓN (feedback del cliente): la validación de tamaño/formato
  # NO está implementada en el sistema actual; este criterio se marca como
  # NO CUMPLIDO y se documenta como hallazgo de aceptación.

  Escenario: Imagen válida se inserta en la nota
    Dado que el usuario tiene una nota abierta
    Cuando inserta una imagen .jpg de 2 MB
    Entonces la imagen se inserta en el contenido de la nota

  Escenario: Imagen inválida debe ser rechazada
    Dado que el usuario tiene una nota abierta
    Cuando intenta insertar una imagen de más de 5 MB
    Entonces el sistema debería mostrar un mensaje de error
    Y no insertar la imagen
    # RESULTADO ESPERADO POR EL CLIENTE: rechazo.
    # RESULTADO REAL: el sistema la acepta (defecto confirmado en pruebas unitarias CP-PE-15).
