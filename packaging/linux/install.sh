#!/usr/bin/env bash
# Installs EspumaPdf for the current user and adds it to the application menu.
# Run it from the extracted release folder. Use --uninstall to remove it.
set -euo pipefail

app_dir="$HOME/.local/share/espumapdf"
bin_link="$HOME/.local/bin/EspumaPdf"
desktop_file="$HOME/.local/share/applications/espumapdf.desktop"
icon_file="$HOME/.local/share/icons/hicolor/256x256/apps/espumapdf.png"

if [[ "${1:-}" == "--uninstall" ]]; then
    rm -rf "$app_dir" "$bin_link" "$desktop_file" "$icon_file"
    echo "EspumaPdf se desinstaló."
    exit 0
fi

here=$(dirname "$(realpath "$0")")
mkdir -p "$app_dir" "$(dirname "$bin_link")" "$(dirname "$desktop_file")" "$(dirname "$icon_file")"
cp -a "$here"/bin/. "$app_dir/"
ln -sf "$app_dir/EspumaPdf" "$bin_link"
cp "$here/espumapdf.png" "$icon_file"
sed "s|^Exec=.*|Exec=$app_dir/EspumaPdf|" "$here/espumapdf.desktop" > "$desktop_file"
update-desktop-database "$(dirname "$desktop_file")" 2>/dev/null || true

echo "EspumaPdf se instaló en $app_dir y ya aparece en el menú de aplicaciones."
