# EspumaPdf

Herramientas para PDF, locales y ligeras, hechas en C# con [Avalonia UI](https://avaloniaui.net/).
Tus archivos nunca salen de tu equipo.

> **En desarrollo.** El proyecto está migrando desde una versión anterior (Tauri + Angular).

## Herramientas

- **Unir PDF**: combina varios PDF en un solo archivo.
- **Extraer rango**: extrae un rango de páginas a un nuevo PDF.
- **Eliminar páginas**: quita las páginas seleccionadas.
- **Numerar páginas**: agrega el número de página en cada hoja.
- **Imagen a PDF**: convierte una o varias imágenes en un PDF.
- **PDF a imagen**: exporta cada página como imagen.
- **Organizar PDF**: reordena y rota páginas arrastrando miniaturas.

## Plataformas

Linux y Windows primero; Android y macOS más adelante.

## Compilar

Requiere el [SDK de .NET 10](https://dotnet.microsoft.com/download).

```sh
dotnet build EspumaPdf.slnx
dotnet test --solution EspumaPdf.slnx
```

## Estructura

```
src/
  Espuma.Application/         Casos de uso y motor PDF (sin dependencias de UI)
tests/
  Espuma.Application.Tests/   Tests de los casos de uso
```
