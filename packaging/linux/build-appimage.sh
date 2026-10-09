#!/usr/bin/env bash
# Builds an AppImage from a linux-x64 publish folder.
# Usage: build-appimage.sh <publish-dir> <output.AppImage> [appimagetool]
set -euo pipefail

publish_dir=$(realpath "$1")
output=$(realpath -m "$2")
appimagetool=${3:-appimagetool}
here=$(dirname "$(realpath "$0")")

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
appdir="$work/EspumaPdf.AppDir"

mkdir -p "$appdir/usr/bin" "$appdir/usr/share/applications" "$appdir/usr/share/icons/hicolor/256x256/apps"
cp -a "$publish_dir"/. "$appdir/usr/bin/"
cp "$here/espumapdf.desktop" "$appdir/espumapdf.desktop"
cp "$here/espumapdf.desktop" "$appdir/usr/share/applications/"
cp "$here/espumapdf.png" "$appdir/espumapdf.png"
cp "$here/espumapdf.png" "$appdir/usr/share/icons/hicolor/256x256/apps/"

cat > "$appdir/AppRun" <<'APPRUN'
#!/bin/sh
here=$(dirname "$(readlink -f "$0")")
exec "$here/usr/bin/EspumaPdf" "$@"
APPRUN
chmod +x "$appdir/AppRun" "$appdir/usr/bin/EspumaPdf"

ARCH=x86_64 APPIMAGE_EXTRACT_AND_RUN=1 "$appimagetool" --no-appstream "$appdir" "$output"
