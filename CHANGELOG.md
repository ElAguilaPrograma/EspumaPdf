# Changelog

Todos los cambios importantes de EspumaPdf se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el proyecto usa
[versionado semántico](https://semver.org/lang/es/).

## [0.2.0] - 2026-10-09

Primera versión de **EspumaPdf**, reescrita desde cero como una aplicación nativa en C# con Avalonia UI. Antes
se llamaba PdfTools y estaba hecha con Tauri, Angular y un servidor local en ASP.NET.

### Agregado

- **Linux como plataforma principal**, además de Windows. Se distribuye como AppImage y como `.tar.gz` con un
  instalador que agrega la app al menú de aplicaciones.
- **Organizar PDF**, una herramienta nueva:
  - Reordena páginas arrastrando sus miniaturas.
  - Gira páginas una por una o varias a la vez.
  - Quita páginas.
  - Selecciona como en el explorador de archivos (clic, Ctrl y Shift).
  - Todo se puede deshacer y rehacer.
  - Atajos: `Supr`, `Ctrl+Z`, `Ctrl+Y`, `Ctrl+A`, `Ctrl+R` y `Esc`.
- Miniaturas de las páginas en todas las herramientas que trabajan sobre un PDF.
- **Extraer rango:** las páginas del rango se resaltan, y el rango se elige con clic y Shift+clic.
- **Eliminar páginas:** las páginas se marcan con clic, y con Shift+clic se marcan varias seguidas.
- **Numerar páginas:**
  - Cuatro formatos ("1 | 10", "1 de 10", "Página 1 de 10" y "1").
  - Vista previa del número sobre cada página.
- **Imagen a PDF:**
  - Opción de hoja A4 con la imagen ajustada, o tamaño original.
  - Soporte de WebP, BMP y GIF.
  - Las fotos del celular respetan su orientación.
- **PDF a imagen:** calidad de 72, 144 o 216 ppp, en PNG o JPEG.
- **Unir PDF:** cada archivo muestra su miniatura y su número de páginas, y el orden se cambia arrastrando.
- Arrastrar archivos desde el gestor de archivos directamente a la app.
- Diálogo **"Guardar como"** para elegir dónde guardar cada resultado, y botón **"Abrir"** en la notificación
  final.
- Progreso visible y botón para **cancelar** las operaciones largas.
- Mensajes de error claros en español: PDF protegido con contraseña, PDF dañado, archivo que no es PDF y otros.
- Nuevo ícono: Espuma mordiendo un PDF.

### Cambiado

- La interfaz es nativa, con tema oscuro, barra lateral de herramientas y pantalla de inicio. En ventanas
  angostas, la barra lateral se reduce a íconos.
- **Arranca más rápido y usa mucha menos memoria:** en Linux abre en menos de un segundo y ocupa unos 50 MB de
  RAM en reposo. La app usa render por software; con `--gpu` se activa la aceleración por GPU.
- **PDF a imagen es unas 4 veces más rápido** y escribe el ZIP directo al disco, sin cargarlo completo en memoria.
- Ya no queda un servidor ni un proceso en segundo plano. La app no se minimiza a la bandeja del sistema ni
  arranca con el equipo: al cerrar la ventana, se cierra.

### Corregido

- Numerar páginas ya no se salta la primera página.
- Los números de página aparecen en la esquina inferior derecha también en páginas giradas.
- Los PDF con datos extra antes del encabezado ahora se abren correctamente.
- Los mensajes de error ya no confunden imágenes con PDF.
- Si una operación falla o se cancela, el archivo incompleto se borra.
- No se puede guardar encima de un archivo que se está usando como entrada, lo que antes podía borrar el
  original.

### Eliminado

- La versión basada en Tauri, Angular y el servidor local en ASP.NET.
- Las dependencias iText y ImageSharp. ImageSharp tenía vulnerabilidades de seguridad conocidas.

## [0.1.0]

Versión anterior, publicada como PdfTools para Windows.

[0.2.0]: https://github.com/ElAguilaPrograma/EspumaPdf/compare/release...v0.2.0
[0.1.0]: https://github.com/ElAguilaPrograma/EspumaPdf/tree/release
