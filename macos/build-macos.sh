#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BUILD_DIR="$SCRIPT_DIR/dist-macos"
APP_NAME="七夕浪漫3D爱心粒子"
APP_DIR="$BUILD_DIR/$APP_NAME.app"
CONTENTS_DIR="$APP_DIR/Contents"
MACOS_DIR="$CONTENTS_DIR/MacOS"
RESOURCES_DIR="$CONTENTS_DIR/Resources"
EXECUTABLE_NAME="QixiRomanticHeartParticles"

case "$BUILD_DIR" in
  "$SCRIPT_DIR"/dist-macos) ;;
  *) echo "Unsafe build directory: $BUILD_DIR" >&2; exit 1 ;;
esac

rm -rf "$BUILD_DIR"
mkdir -p "$MACOS_DIR" "$RESOURCES_DIR"

xcrun swiftc \
  "$SCRIPT_DIR/main.swift" \
  -O \
  -framework AppKit \
  -framework WebKit \
  -o "$MACOS_DIR/$EXECUTABLE_NAME"

cp "$SCRIPT_DIR/Info.plist" "$CONTENTS_DIR/Info.plist"
cp "$SCRIPT_DIR/index.html" "$RESOURCES_DIR/index.html"
chmod +x "$MACOS_DIR/$EXECUTABLE_NAME"

# Ad-hoc signing lets the locally built app keep a coherent bundle signature.
codesign --force --deep --sign - "$APP_DIR"

ditto -c -k --sequesterRsrc --keepParent \
  "$APP_DIR" "$BUILD_DIR/$APP_NAME-macOS.zip"

hdiutil create \
  -volname "$APP_NAME" \
  -srcfolder "$APP_DIR" \
  -ov \
  -format UDZO \
  "$BUILD_DIR/$APP_NAME-macOS.dmg"

echo "macOS build complete:"
echo "  $BUILD_DIR/$APP_NAME-macOS.zip"
echo "  $BUILD_DIR/$APP_NAME-macOS.dmg"
