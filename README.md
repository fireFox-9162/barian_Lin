# barian_Lin — Unity Warrior Sprite Animation Project

## 프로젝트 개요
이 프로젝트는 픽셀 아트 전사 캐릭터(Warrior)를 위한 Unity 2D 스프라이트 애니메이션 시스템입니다.

---

## 📁 폴더 구조
```
barian_Lin/                          (Unity 프로젝트 루트)
├── Assets/
│   ├── Sprites/
│   │   └── Warrior/                 ← 스프라이트 시트 PNG 파일
│   │       ├── warrior_walk_right.png       (8 프레임)
│   │       ├── warrior_walk_left.png        (8 프레임)
│   │       ├── warrior_slash1_horizontal.png (6 프레임)
│   │       ├── warrior_slash2_downslash.png  (6 프레임)
│   │       ├── warrior_slash3_combo.png      (8 프레임)
│   │       ├── warrior_pickup.png            (5 프레임)
│   │       └── warrior_jump.png              (6 프레임)
│   │
│   ├── Scripts/
│   │   ├── WarriorController.cs     ← 캐릭터 물리 & 입력 처리
│   │   ├── SpriteAnimator.cs        ← 경량 스프라이트 애니메이터
│   │   ├── WarriorAnimatorSetup.cs  ← Animator ↔ Controller 브릿지
│   │   └── Editor/
│   │       └── WarriorSpriteImporter.cs ← 에디터 자동 임포트 툴
│   │
│   ├── Animations/
│   │   └── Warrior/                 ← Animation Clips & Controller (자동 생성)
│   │
│   └── Prefabs/
│       └── Warrior.prefab           ← 완성 프리팹 (자동 생성)
│
├── Packages/
│   └── manifest.json
└── ProjectSettings/
    └── ProjectSettings.asset        (프로젝트명: barian_Lin)
```

---

## 🎮 애니메이션 목록

| 파일명 | 클립 이름 | 프레임 수 | FPS | 루프 |
|--------|-----------|-----------|-----|------|
| warrior_walk_right.png | walk_right | 8 | 12 | ✅ |
| warrior_walk_left.png  | walk_left  | 8 | 12 | ✅ |
| warrior_slash1_horizontal.png | slash1 | 6 | 14 | ❌ |
| warrior_slash2_downslash.png  | slash2 | 6 | 14 | ❌ |
| warrior_slash3_combo.png      | slash3 | 8 | 14 | ❌ |
| warrior_pickup.png            | pickup | 5 |  8 | ❌ |
| warrior_jump.png              | jump   | 6 | 10 | ❌ |

---

## 🛠️ Unity 프로젝트(barian_Lin)에 적용하는 방법

### 방법 A — 자동 (추천)

1. **스프라이트 복사**
   ```
   이 저장소의 Assets/ 폴더를 barian_Lin/Assets/ 에 덮어씁니다
   ```

2. **Unity Editor 열기**  
   `barian_Lin` 프로젝트를 Unity Hub에서 엽니다.

3. **에디터 툴 실행**  
   Unity 메뉴 → `Tools` → `Warrior Sprite Importer` → **`Setup All`**  
   → 스프라이트 슬라이싱, Animation Clips, Animator Controller, Prefab이 모두 자동으로 생성됩니다.

4. **씬에 배치**  
   `Assets/Prefabs/Warrior.prefab` 을 Hierarchy에 드래그&드롭합니다.

---

### 방법 B — 수동 (단계별)

#### Step 1 : 스프라이트 임포트 설정
각 `warrior_*.png` 파일을 선택 후 Inspector에서:
- Texture Type: **Sprite (2D and UI)**
- Sprite Mode: **Multiple**
- Filter Mode: **Point (no filter)**  ← 픽셀아트용
- Compression: **None**
- Pixels Per Unit: **32** 권장

#### Step 2 : Sprite Editor에서 슬라이스
- Sprite Editor → Slice → Type: **Grid By Cell Size**
- Cell Size: **128 x 128** (각 프레임 크기)
- Apply 클릭

#### Step 3 : Animation Clip 생성
각 스프라이트 시트별로 슬라이스된 프레임들을 Animation 뷰에 드래그해 클립을 만듭니다.

#### Step 4 : Animator Controller 생성
1. `Assets/Animations/Warrior/` 폴더에서 Create → Animator Controller
2. State들을 추가하고 Transition 조건을 아래 표대로 설정합니다.

#### Step 5 : 캐릭터 오브젝트 구성
1. 빈 GameObject 생성 → 이름: `Warrior`
2. 컴포넌트 추가:
   - `Sprite Renderer`
   - `Rigidbody2D` (Freeze Rotation Z, Gravity Scale: 3)
   - `BoxCollider2D` (size: 0.6, 1.0 / offset: 0, 0.5)
   - `Animator` (Controller 연결)
   - `WarriorController` (스크립트)
3. 자식 오브젝트 `GroundCheck` 생성 → 위치: (0, -0.05, 0)
4. `WarriorController.groundCheck` 필드에 GroundCheck 연결

---

## 🎯 조작키

| 키 | 동작 |
|----|------|
| A / ← | 왼쪽 이동 |
| D / → | 오른쪽 이동 |
| Space | 점프 |
| Z | 공격 1 — 가로 참격 |
| X | 공격 2 — 수직 내려치기 |
| C | 공격 3 — 콤보 참격 |
| F / E | 아이템 줍기 |

---

## ⚙️ Animator Transitions 설정표

```
Any State  →→  slash1  (Trigger: Slash1, Can Transition to Self: false)
Any State  →→  slash2  (Trigger: Slash2)
Any State  →→  slash3  (Trigger: Slash3)
Any State  →→  pickup  (Trigger: PickUp)
Any State  →→  jump    (Trigger: Jump)

idle       →→  walk_right  (State == 1)
idle       →→  walk_left   (State == 2)
walk_right →→  idle        (State == 0)
walk_left  →→  idle        (State == 0)
jump       →→  idle        (IsGrounded == true, Exit Time)
slash1     →→  idle        (Exit Time 1.0)
slash2     →→  idle        (Exit Time 1.0)
slash3     →→  idle        (Exit Time 1.0)
pickup     →→  idle        (Exit Time 1.0)
```

---

## 📦 의존성

| 패키지 | 버전 |
|--------|------|
| Unity | 2022.3 LTS 이상 권장 |
| com.unity.2d.sprite | 1.0.0 |
| com.unity.2d.animation | 9.1.0 |
| com.unity.inputsystem | 1.7.0 |

---

## 🎨 스프라이트 사양

- **프레임 크기**: 128 × 128 px
- **배경**: 투명 (RGBA PNG)
- **스타일**: 픽셀 아트
- **캐릭터**: 금색 갑옷 + 파란 망토 전사, 대검 소지
