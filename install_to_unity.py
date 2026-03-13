#!/usr/bin/env python3
"""
install_to_unity.py  –  barian_Lin Unity 프로젝트 자동 설치 스크립트
실행:  python install_to_unity.py  [선택: Unity 프로젝트 경로]

자동으로 다음을 수행합니다:
  1. barian_Lin Unity 프로젝트 경로를 자동 탐색 (또는 인자로 지정)
  2. 스프라이트 시트를 Assets/Sprites/Warrior/ 에 복사
  3. C# 스크립트를 Assets/Scripts/ 에 복사
  4. 완료 메시지 출력

사용법:
  python install_to_unity.py
  python install_to_unity.py "C:/Users/이름/My Project/barian_Lin"
  python install_to_unity.py "/Users/이름/Documents/barian_Lin"
"""

import os, sys, shutil, glob, platform

# ── Source paths (relative to this script) ──────────────────────────────────
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SRC_SPRITES = os.path.join(SCRIPT_DIR, "unity_project", "Assets", "Sprites", "Warrior")
SRC_SCRIPTS = os.path.join(SCRIPT_DIR, "unity_project", "Assets", "Scripts")
SRC_EDITOR  = os.path.join(SCRIPT_DIR, "unity_project", "Assets", "Scripts", "Editor")

# ── Auto-search locations ─────────────────────────────────────────────────────
def find_unity_project():
    """Search common locations for barian_Lin Unity project"""
    system = platform.system()

    search_roots = []
    if system == "Windows":
        import os
        # Common Windows paths
        for drive in ["C:", "D:", "E:"]:
            for subfolder in ["Users", "Projects", "Unity"]:
                p = f"{drive}\\{subfolder}"
                if os.path.exists(p):
                    search_roots.append(p)
        # Desktop / Documents
        home = os.path.expanduser("~")
        search_roots += [
            os.path.join(home, "Desktop"),
            os.path.join(home, "Documents"),
            os.path.join(home, "Documents", "Unity Projects"),
            os.path.join(home, "My Project"),
        ]
    elif system == "Darwin":  # macOS
        home = os.path.expanduser("~")
        search_roots = [
            os.path.join(home, "Desktop"),
            os.path.join(home, "Documents"),
            os.path.join(home, "Documents", "Unity Projects"),
            os.path.join(home, "Unity Projects"),
            "/Users",
        ]
    else:  # Linux
        home = os.path.expanduser("~")
        search_roots = [
            home,
            os.path.join(home, "Desktop"),
            os.path.join(home, "Documents"),
            os.path.join(home, "Projects"),
            "/home",
        ]

    candidates = []
    for root in search_roots:
        if not os.path.isdir(root):
            continue
        try:
            for dirpath, dirnames, filenames in os.walk(root):
                # Limit depth to avoid very deep traversal
                depth = dirpath.replace(root, "").count(os.sep)
                if depth > 5:
                    dirnames.clear()
                    continue

                base = os.path.basename(dirpath)
                if base.lower() in ("barian_lin", "barian lin"):
                    # Check it's a Unity project (has ProjectSettings)
                    if os.path.isdir(os.path.join(dirpath, "Assets")):
                        candidates.append(dirpath)
                        print(f"  Found: {dirpath}")
        except PermissionError:
            continue

    return candidates


def copy_files(unity_root: str):
    """Copy sprites and scripts into Unity project"""
    # Destination paths
    dst_sprites = os.path.join(unity_root, "Assets", "Sprites", "Warrior")
    dst_scripts = os.path.join(unity_root, "Assets", "Scripts")
    dst_editor  = os.path.join(unity_root, "Assets", "Scripts", "Editor")

    os.makedirs(dst_sprites, exist_ok=True)
    os.makedirs(dst_scripts, exist_ok=True)
    os.makedirs(dst_editor,  exist_ok=True)

    # ── Sprites ────────────────────────────────────────────────────────────
    print("\n📦 Copying sprites...")
    for png in glob.glob(os.path.join(SRC_SPRITES, "*.png")):
        dst = os.path.join(dst_sprites, os.path.basename(png))
        shutil.copy2(png, dst)
        print(f"  ✓ {os.path.basename(png)}")

    # ── Scripts (non-editor) ──────────────────────────────────────────────
    print("\n📝 Copying scripts...")
    for cs in glob.glob(os.path.join(SRC_SCRIPTS, "*.cs")):
        dst = os.path.join(dst_scripts, os.path.basename(cs))
        shutil.copy2(cs, dst)
        print(f"  ✓ {os.path.basename(cs)}")

    # ── Editor scripts ─────────────────────────────────────────────────────
    print("\n🔧 Copying editor scripts...")
    for cs in glob.glob(os.path.join(SRC_EDITOR, "*.cs")):
        dst = os.path.join(dst_editor, os.path.basename(cs))
        shutil.copy2(cs, dst)
        print(f"  ✓ {os.path.basename(cs)}")


def main():
    print("=" * 60)
    print("  barian_Lin Unity Project Installer")
    print("=" * 60)

    # Get target path
    if len(sys.argv) > 1:
        unity_root = sys.argv[1]
        if not os.path.isdir(unity_root):
            print(f"❌ Path not found: {unity_root}")
            sys.exit(1)
        if not os.path.isdir(os.path.join(unity_root, "Assets")):
            print(f"❌ Not a Unity project (no Assets folder): {unity_root}")
            sys.exit(1)
        print(f"✅ Using specified path: {unity_root}")
    else:
        print("\n🔍 Searching for barian_Lin Unity project...")
        candidates = find_unity_project()

        if not candidates:
            print("\n❌ barian_Lin project not found automatically.")
            print("\n사용법:  python install_to_unity.py \"경로/barian_Lin\"")
            print("\n예시 (Windows):  python install_to_unity.py \"C:\\Users\\사용자\\My Project\\barian_Lin\"")
            print("예시 (macOS):    python install_to_unity.py \"/Users/사용자/Documents/barian_Lin\"")
            sys.exit(1)

        if len(candidates) == 1:
            unity_root = candidates[0]
            print(f"\n✅ Found: {unity_root}")
        else:
            print(f"\n여러 프로젝트를 발견했습니다:")
            for i, c in enumerate(candidates):
                print(f"  {i+1}. {c}")
            choice = input("\n번호를 선택하세요 (기본값: 1): ").strip() or "1"
            unity_root = candidates[int(choice)-1]

    # Copy
    copy_files(unity_root)

    print(f"""
✅ 설치 완료!  →  {unity_root}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 Unity Editor에서 다음 단계를 수행하세요:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

 1. Unity Hub에서 barian_Lin 프로젝트 열기
 2. 메뉴:  Tools  ▶  Warrior Setup  ▶  ▶ Run Full Setup
    → 스프라이트 슬라이싱, 애니메이션, Prefab 자동 생성
 3. Assets/Prefabs/Warrior.prefab → 씬에 드래그&드롭
 4. Play!

 조작키:
   A / ←       : 왼쪽 이동
   D / →       : 오른쪽 이동
   Space        : 점프
   마우스 좌클릭  : 검 공격
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
""")


if __name__ == "__main__":
    main()
