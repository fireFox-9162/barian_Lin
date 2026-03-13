#!/bin/bash
# install_to_unity_mac.sh  –  macOS / Linux 용 barian_Lin 설치 스크립트
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
echo "============================================================"
echo "  barian_Lin Unity Project Installer"
echo "============================================================"
echo ""

PROJECT_PATH=""

# Auto-search common locations
echo "[1/3] barian_Lin 프로젝트 탐색 중..."
SEARCH_DIRS=(
  "$HOME/Desktop/barian_Lin"
  "$HOME/Documents/barian_Lin"
  "$HOME/Documents/Unity Projects/barian_Lin"
  "$HOME/Unity Projects/barian_Lin"
  "$HOME/Projects/barian_Lin"
  "$HOME/My Project/barian_Lin"
  "/Users/$USER/Desktop/barian_Lin"
  "/Users/$USER/Documents/barian_Lin"
  "/Users/$USER/Documents/Unity Projects/barian_Lin"
)

for dir in "${SEARCH_DIRS[@]}"; do
  if [ -d "$dir/Assets" ]; then
    PROJECT_PATH="$dir"
    echo "   발견: $dir"
    break
  fi
done

# If not found, ask user
if [ -z "$PROJECT_PATH" ]; then
  echo ""
  echo "❌ 자동으로 찾지 못했습니다."
  echo ""
  read -p "barian_Lin 프로젝트 전체 경로를 입력하세요: " PROJECT_PATH
  if [ ! -d "$PROJECT_PATH/Assets" ]; then
    echo "❌ 올바른 Unity 프로젝트 경로가 아닙니다."
    exit 1
  fi
fi

echo ""
echo "[2/3] 파일 복사 중..."
echo "   대상: $PROJECT_PATH"

# Create directories
mkdir -p "$PROJECT_PATH/Assets/Sprites/Warrior"
mkdir -p "$PROJECT_PATH/Assets/Scripts"
mkdir -p "$PROJECT_PATH/Assets/Scripts/Editor"

# Copy sprites
cp -v "$SCRIPT_DIR/unity_project/Assets/Sprites/Warrior/"*.png \
      "$PROJECT_PATH/Assets/Sprites/Warrior/"
echo "   ✓ 스프라이트 복사 완료"

# Copy scripts
cp -v "$SCRIPT_DIR/unity_project/Assets/Scripts/"*.cs \
      "$PROJECT_PATH/Assets/Scripts/"
cp -v "$SCRIPT_DIR/unity_project/Assets/Scripts/Editor/"*.cs \
      "$PROJECT_PATH/Assets/Scripts/Editor/"
echo "   ✓ 스크립트 복사 완료"

echo ""
echo "[3/3] 완료!"
echo ""
echo "============================================================"
echo "  Unity Editor에서 다음을 실행하세요:"
echo ""
echo "  메뉴: Tools > Warrior Setup > Run Full Setup"
echo "  그 다음: Assets/Prefabs/Warrior.prefab 씬에 배치"
echo ""
echo "  조작:  A/D=이동  Space=점프  마우스좌클릭=공격"
echo "============================================================"
