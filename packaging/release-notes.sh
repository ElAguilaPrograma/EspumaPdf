#!/usr/bin/env bash
# Prints the section of a version from CHANGELOG.md, without its heading.
# Usage: release-notes.sh <version> [changelog]
set -euo pipefail

version=$1
changelog=${2:-CHANGELOG.md}

awk -v heading="## [$version]" '
    index($0, "## [") == 1 { if (found) exit; found = (index($0, heading) == 1); next }
    /^\[[^]]+\]: / { next }
    found
' "$changelog" | sed -e '/./,$!d' | tac | sed -e '/./,$!d' | tac
