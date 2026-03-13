# barian_Lin — Warrior Sprite Animation System

픽셀아트 전사 캐릭터 (금색 사자 갑옷 + 파란 망토 + 대검) 를 위한 Unity 2D 스프라이트 애니메이션 시스템

---

## 🎮 조작키

| 키 | 동작 |
|----|------|
| `A` / `←` | 왼쪽 이동 |
| `D` / `→` | 오른쪽 이동 |
| `Space` | 점프 |
| **마우스 좌클릭** | 검 공격 |

---

## 🖼️ 스프라이트 시트

| 파일 | 동작 | 프레임 | FPS |
|------|------|--------|-----|
| `warrior_idle.png` | 대기 | 4 | 8 |
| `warrior_walk_right.png` | 오른쪽 이동 | 8 | 12 |
| `warrior_walk_left.png` | 왼쪽 이동 | 8 | 12 |
| `warrior_attack.png` | 검 공격 (예비→참격→이후) | 6 | 14 |
| `warrior_jump.png` | 점프 | 6 | 10 |

- 프레임 크기: **144 × 192 px** (48×64 픽셀 × 3배 스케일)
- 배경: 투명 RGBA PNG
- 스타일: 픽셀아트

---

## ⚡ barian_Lin Unity 프로젝트에 설치하는 법

### 방법 1 — 자동 설치 (추천)

#### Windows
```
install_to_unity_windows.bat  더블클릭
```

#### macOS / Linux
```bash
bash install_to_unity_mac.sh
# 또는
python3 install_to_unity.py
```

#### 경로 직접 지정
```bash
python3 install_to_unity.py "C:\Users\사용자\My Project\barian_Lin"
python3 install_to_unity.py "/Users/사용자/Documents/barian_Lin"
```

---

### 방법 2 — 수동 설치

1. `Assets/` 폴더 내용을 `barian_Lin/Assets/` 에 덮어붙여넣기
2. Unity Hub에서 `barian_Lin` 프로젝트 열기
3. 메뉴: **`Tools` → `Warrior Setup` → `▶ Run Full Setup`**  
   → 스프라이트 슬라이싱, Animation Clips, Animator Controller, Prefab 자동 생성
4. `Assets/Prefabs/Warrior.prefab` → Hierarchy 씬에 드래그&드롭
5. ▶ Play!

---

## 📁 파일 구조

```
barian_Lin/
├── Assets/
│   ├── Sprites/Warrior/
│   │   ├── warrior_idle.png
│   │   ├── warrior_walk_right.png
│   │   ├── warrior_walk_left.png
│   │   ├── warrior_attack.png
│   │   └── warrior_jump.png
│   │
│   ├── Scripts/
│   │   ├── WarriorController.cs        ← 이동·점프·공격 입력
│   │   ├── WarriorAnimator.cs          ← 스프라이트 프레임 재생기
│   │   ├── WarriorAnimatorBridge.cs    ← 상태→애니메이션 연결
│   │   └── Editor/
│   │       └── WarriorSetup.cs         ← Unity Editor 자동 셋업 툴
│   │
│   ├── Animations/Warrior/             ← (자동 생성됨)
│   └── Prefabs/Warrior.prefab          ← (자동 생성됨)
│
├── install_to_unity.py                 ← 자동 설치 (Python)
├── install_to_unity_windows.bat        ← 자동 설치 (Windows)
└── install_to_unity_mac.sh             ← 자동 설치 (macOS/Linux)
```

---

## 🔧 Unity 컴포넌트 구성

Warrior GameObject에 다음 컴포넌트들이 자동으로 붙습니다:

| 컴포넌트 | 역할 |
|---------|------|
| `SpriteRenderer` | 스프라이트 렌더링 (flipX로 좌우 전환) |
| `Rigidbody2D` | 물리 (중력 4, 회전 고정) |
| `BoxCollider2D` | 충돌 (0.55 × 1.1, offset y:0.55) |
| `Animator` | Unity 내장 애니메이터 |
| `WarriorController` | 입력 처리 + 히트박스 |
| `WarriorAnimator` | 경량 스프라이트 프레임 재생 |

---

## ⚙️ Inspector 설정 (WarriorController)

| 파라미터 | 기본값 | 설명 |
|---------|--------|------|
| Move Speed | 5 | 이동 속도 (m/s) |
| Jump Force | 12 | 점프 힘 |
| Ground Layer | - | 지면 레이어 (GroundLayer 설정 필수!) |
| Attack Cooldown | 0.5 | 공격 간격 (초) |
| Attack Range | 1.2 | 히트박스 반경 (m) |

> ⚠️ **중요**: GroundCheck 오브젝트의 Layer를 설정하고,  
> `WarriorController.groundLayer` 에 지면 레이어를 할당해야 점프가 작동합니다!

---

## 💻 GitHub

**https://github.com/fireFox-9162/barian_Lin**
