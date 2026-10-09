# EspumaPdf

Herramientas para PDF, locales y ligeras, hechas en C# con [Avalonia UI](https://avaloniaui.net/).
Tus archivos nunca salen de tu equipo.

<p align="center"><img src="assets/icon.svg" width="128" alt="EspumaPdf"></p>

## Herramientas

- **Unir PDF**: combina varios PDF en un solo archivo.
- **Extraer rango**: extrae un rango de páginas a un nuevo PDF.
- **Eliminar páginas**: quita las páginas seleccionadas.
- **Numerar páginas**: agrega el número de página en cada hoja.
- **Imagen a PDF**: convierte una o varias imágenes en un PDF.
- **PDF a imagen**: exporta cada página como imagen.
- **Organizar PDF**: reordena y rota páginas arrastrando miniaturas.

## Descargar

Las versiones están en [Releases](https://github.com/ElAguilaPrograma/EspumaPdf/releases).

- **Linux:**
  - `EspumaPdf-<versión>-x86_64.AppImage`: dale permisos de ejecución y ábrelo.
  - `EspumaPdf-<versión>-linux-x64.tar.gz`: descomprímelo y ejecuta `./install.sh` para agregarlo al menú
    de aplicaciones (`./install.sh --uninstall` para quitarlo).
- **Windows:** `EspumaPdf-<versión>-win-x64.zip`. Descomprímelo y abre `EspumaPdf.exe`.

La app usa render por software para consumir menos memoria. Si prefieres aceleración por GPU, ábrela con
`--gpu`.

Android y macOS llegarán más adelante.

## Compilar

Requiere el [SDK de .NET 10](https://dotnet.microsoft.com/download).

```sh
dotnet build EspumaPdf.slnx
dotnet test --solution EspumaPdf.slnx
dotnet run --project src/Espuma.Desktop
```

## Publicar

Las versiones se compilan con Native AOT en GitHub Actions: al subir un tag `v*` se crea la release con los
paquetes de Linux y Windows. El workflow también se puede lanzar a mano para obtener los paquetes sin
publicar una release.

```sh
dotnet publish src/Espuma.Desktop -c Release -r linux-x64
```

## Estructura

```
src/
  Espuma.Application/         Casos de uso y motor PDF (sin dependencias de UI)
  Espuma.Presentation/        Interfaz con Avalonia (vistas, view models y tema)
  Espuma.Desktop/             Aplicación de escritorio para Linux y Windows
tests/
  Espuma.Application.Tests/   Tests de los casos de uso
packaging/linux/              AppImage, instalador y entrada del menú de aplicaciones
assets/                       Ícono de la aplicación (SVG)
```
