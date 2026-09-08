# AI PROGRAMMING LAB — 2D Behavior Tree & State Pattern 체험 학습 환경

Unity 6.3 (6000.3.23f1) / 2D URP / `com.unity.behavior` 1.0.13

학습자는 **코드를 한 줄도 쓰지 않는다.** Play 버튼을 누르고 키만 눌러서
AI 의 행동을 관찰하고, 구조를 확인하고, 두 방식을 비교하고, 스스로 진단한다.

---

## 1. 바로 시작하기

1. Unity 에서 `Assets/Scenes/AI_Learning_Lab.unity` 를 연다.
   (메뉴 `AI Lab / AI Learning Lab 씬 열기` 도 같은 일을 한다.)
2. Game View 종횡비를 **16:9 (1920x1080 권장)** 로 맞춘다.
   UI 는 1920x1080 기준으로 배치되어 있다.
3. Play.

씬이 없거나 다시 만들고 싶으면 메뉴에서 한 번에 재생성할 수 있다.

| 메뉴 | 하는 일 |
| --- | --- |
| `AI Lab / 0. 전체 빌드 (그래프 + 프리팹 + 씬)` | 아래 3개를 순서대로 전부 |
| `AI Lab / 1. Behavior Graph 만들기` | Behavior Graph 에셋 3개 생성 |
| `AI Lab / 2. 학습용 프리팹 만들기` | 기존 Player / Enemy 를 복사해 학습용 프리팹 생성 |
| `AI Lab / 3. AI Learning Lab 씬 만들기` | 학습장 씬 생성 (지형 / AI / UI 배치) |

> 전체 빌드는 열려 있는 씬을 새 씬으로 교체한다.
> 저장하지 않은 변경이 있으면 빌드가 중단되고 콘솔에 안내가 나온다.

---

## 2. 조작

| 키 | 동작 |
| --- | --- |
| `A` `D` / `←` `→` | Player 이동 (기존 PlayerController 그대로 사용) |
| `Space` | 점프 |
| `1` ~ `6` | STEP 이동 (해당 학습 구역으로 Player 를 옮긴다) |
| `Tab` | 관찰할 AI 전환 (AI MONITOR / 다이어그램 대상이 바뀐다) |
| `F` | 선택한 Enemy 에게 피해 15 |
| `G` | 선택한 Enemy 즉시 사망 |
| `T` | 선택한 Enemy 체력 회복 |
| `R` | 실험 리셋 (모든 Enemy 를 처음 상태로) |
| `Z` | 공유 Blackboard 의 경보(AlarmActive) 토글 |
| `D` | 구조 다이어그램 열기 / 닫기 |
| `C` | 두 구조 나란히 비교 |
| `B` | Blackboard 패널 |
| `M` | 미션 목록 |
| `Q` | 자기진단 (SELF CHECK) |
| `V` | 감지 / 공격 범위 원 표시 |
| `H` | UI 전체 숨기기 (순수 게임 화면만 보기) |

화면 위쪽 약 1/3 은 항상 Game View 전용으로 비워 둔다. 어떤 패널도 AI 를 가리지 않는다.

---

## 3. 학습 흐름과 학습장 배치

학습장은 하나의 긴 지면이고, 구역마다 표지판이 떠 있다.
`1`~`6` 키를 누르면 그 구역 입구로 이동한다.

```
x = -30            -12          -1    9         25  28        36 39 42
    │               │            │    │          │   │         │  │  │
 [STEP 01]      [STEP 02·03]  [STEP 04-1][04-2] [ STEP 05·06 ] [ STEP 06 ]
 구조 없는 AI    State Pattern  Sequence  Selector  A: State    공유 Blackboard
                                +Repeat  +Condition B: Graph     3인조
```

| STEP | 배우는 것 | 구역의 AI |
| --- | --- | --- |
| 01 | AI 는 조건을 보고 행동을 고른다 | `Enemy_01_Simple` — if/else 만 쓴 AI |
| 02 | State 와 State Transition | `Enemy_02_StatePattern` |
| 03 | State Pattern / State Machine / Enter·Execute·Exit | 같은 Enemy, 구조 다이어그램 중심 |
| 04 | Behavior Tree 구성요소 | `Enemy_04_BT_Sequence`, `Enemy_04_BT_Selector` |
| 05 | 같은 요구사항을 두 방식으로 구현 | `Enemy_05_A_StatePattern`, `Enemy_05_B_BehaviorGraph` |
| 06 | 두 구조 비교 + Blackboard + 자유 실험 | `Enemy_06_Squad_A/B/C` |

미션은 **지금 보고 있는 STEP 의 것만** 자동 채점된다.
AI 가 실제로 그 상태 / 노드를 실행했는지를 기록해서 판정하므로,
직접 만들어 낸 장면만 체크된다. (총 31개)

---

## 4. 이 프로젝트의 핵심 설계 — 왜 비교가 정직한가

`AIContext` 하나가 **감각(조건 판정)과 몸(이동 / 공격 / 체력)** 을 모두 갖고 있다.

```
                        AIContext
      (PlayerDetected / InAttackRange / IsLowHealth / IsDead
       TickPatrol / TickChase / TickAttack / TickFlee / TickDead)
                            ▲
              ┌─────────────┴─────────────┐
              │                           │
     EnemyStateMachine            Behavior Graph 노드
     (State Pattern)              (Lab Patrol / Chase / ... 와
                                   Lab Player Detected / ... 조건)
```

State Pattern 버전과 Behavior Graph 버전은
**감지 거리 · 이동 속도 · 공격력 · 조건 판정 코드까지 완전히 같은 것을 쓴다.**
다른 것은 오직 *판단 구조* 뿐이다.

그래서 STEP 05 에서 두 Enemy 를 같은 거리에 두면 **동시에 같은 행동을 한다.**
학습자는 "결과는 같다. 그런데 구조는 다르다" 를 눈으로 확인하게 된다.

---

## 5. 만들어진 것

### 5.1 씬
- `Assets/Scenes/AI_Learning_Lab.unity` — 학습장 (Build Settings 에 자동 등록)
- 기존 `Assets/Scenes/TilemapSample.unity` 은 **수정하지 않았다.**

### 5.2 Behavior Graph 에셋 (코드로 자동 생성)
`Assets/Resources/Behavior/AILab/`

| 에셋 | 구조 |
| --- | --- |
| `AILab_Combat.asset` | `On Start(Repeat)` → `Try In Order` → 5개 가지 |
| `AILab_SelectorDemo.asset` | `On Start(Repeat)` → `Try In Order` → 3개 가지 |
| `AILab_SequenceDemo.asset` | `On Start` → `Repeat` → `Sequence` → Move A / Wait / Move B |

`AILab_Combat` 의 실제 구조:

```
On Start (Repeat)
└─ Try In Order  (Selector)
   ├─ Sequence ─ [Conditional Guard : Lab Is Dead]         → Lab Die
   ├─ Sequence ─ [Conditional Guard : Lab Low Health]      → Lab Flee
   ├─ Sequence ─ [Conditional Guard : Lab In Attack Range] → Lab Attack
   ├─ Sequence ─ [Conditional Guard : Lab Player Detected] → Lab Chase
   └─ Lab Patrol                              (조건 없음 = 기본 행동)
```

Project 창에서 에셋을 더블클릭하면 Unity 의 Behavior 그래프 창이 열린다.
학습자가 그래프를 만들 필요는 없고, **완성된 것을 열어서 관찰**하면 된다.

Blackboard 변수: `Self`, `Target`, `PlayerDetected`, `InAttackRange`,
`HealthPercent`, `AlarmActive` (+ Sequence 실험은 `Seq_PointA/B`, `MoveSpeed`, `WaitSeconds`).
`BTBlackboardSync` 가 매 프레임 실제 값을 써 넣으므로 Play 중에 그래프 창의
Blackboard 를 열면 값이 살아 움직인다.

### 5.3 프리팹
`Assets/Prefabs/AI_Learning/`

| 프리팹 | 내용 |
| --- | --- |
| `LabPlayer` | 기존 Player 복사 + `Health` + `LabPlayerVitals` |
| `LabEnemy_Base` | 기존 Enemy 복사 + 공통 AI 부품만 |
| `LabEnemy_01_Reactive` | + `SimpleReactiveAI` |
| `LabEnemy_02_StatePattern` | + `EnemyStateMachine` |
| `LabEnemy_03_BehaviorGraph` | + `BehaviorGraphAgent`(Combat) + 추적기 |
| `LabEnemy_04_BT_Selector` | + `BehaviorGraphAgent`(SelectorDemo) |
| `LabEnemy_05_BT_Sequence` | + `BehaviorGraphAgent`(SequenceDemo) + `LabSequencePoints` |

기존 Enemy 의 스프라이트 / Animator / Collider2D 를 그대로 물려받고,
기존 AI 스크립트(`EnemyFSM`, `EnemyStatePattern`, `EnemyAI`)만 **사본에서** 떼어냈다.
원본 스크립트와 원본 씬은 그대로 남아 있다.

### 5.4 스크립트
`Assets/Scripts/AI_Learning/`

```
Core/            AIContext, AIMotor, AIAttack, PatrolRoute, Health,
                 AIEventLog, SquadBlackboard, SimpleReactiveAI, IAIBrain
StatePattern/    EnemyStateMachine, AIState + Patrol/Chase/Attack/Flee/DeadState
BehaviorGraph/   LabNodeHelper, BTNodeTrace, BTBlackboardSync, LabSequencePoints
  Actions/       LabCombatActions (Patrol/Chase/Attack/Flee/Die)
                 LabBasicActions  (MoveToPoint/Wait/FacePlayer/Idle)
  Conditions/    LabConditions    (PlayerDetected/AttackRange/LowHealth/IsDead/Alarm/Nearby)
Lab/             LabDirector(학습 진행·미션 채점), LabStep, LabContent,
                 LabLayout, LabNames, LabCameraRig, LabPlayerVitals
UI/              LabUI(화면 전체), LabUIKit, LabDiagrams(3종 다이어그램),
                 LabDiagramSpecs, EnemyWorldLabel, LabAIStructure
Debug/           AIRangeRing, LabWaypointMarker, LabZoneSign
Editor/          AILabBuilder, LabBehaviorGraphBuilder   ← 씬·프리팹·그래프 자동 생성
```

모든 스크립트에 **학습용 주석**이 들어 있다.
"무엇을 했는가" 보다 "왜 그렇게 나눴는가" 를 설명하는 방향으로 썼다.

---

## 6. 화면 구성

```
┌──────────── 상단바 : STEP / 제목 / 미션 진행률 / Player HP ────────────┐
│                                                                        │
│        Game View  — 어떤 패널도 가리지 않는 영역                       │
│        Enemy 머리 위에 STATE / NODE 라벨과 HP 바가 떠 있다             │
│        V 키를 누르면 감지·공격 범위가 원으로 표시된다                  │
├───────────────┬────────────────────────────────┬───────────────────────┤
│ 지금 배우는   │  구조 다이어그램 (D)           │  AI MONITOR           │
│ 내용과 질문   │  두 구조 비교 (C)              │  ─────────────        │
│               │  자기진단 (Q)                  │  MISSION (M) 또는     │
│               │                                │  BLACKBOARD (B)       │
└───────────────┴────────────────────────────────┴───────────────────────┘
┌──────────────────────── 하단바 : 조작 안내 ────────────────────────────┐
```

### 다이어그램은 살아 있다
- **State Machine 다이어그램** : 현재 상태 상자가 밝아지고, 방금 발동한 전환 규칙이 강조된다.
- **Behavior Tree 다이어그램** : 조건 노드가 `TRUE`/`FALSE` 로 색이 바뀌고,
  지금 실행 중인 Action 노드가 강조된다. 가지에는 평가 순서 번호가 붙는다.
- **if/else 다이어그램** (STEP 01) : 지금 선택된 조건 줄이 강조된다.

`Tab` 으로 AI 를 바꾸면 그 AI 의 구조 그림으로 자동 전환된다.

---

## 7. 체험할 수 있는 실험

| 실험 | 방법 | 관찰 |
| --- | --- | --- |
| A | 멀리 서 있기 | `PATROL` |
| B | 감지 범위 안으로 접근 | `PATROL → CHASE` |
| C | 더 접근 | `CHASE → ATTACK` |
| D | 도망 | `ATTACK → CHASE → PATROL` |
| E | `F` 로 HP 를 30% 아래로 | 어떤 상태에 있든 `→ FLEE` |
| F | `G` 로 HP 0 | `→ DEAD` (나가는 화살표가 없는 최종 상태) |
| G | STEP 06 에서 3인조 중 한 마리에게 접근, 또는 `Z` | 세 마리가 동시에 반응 |
| H | STEP 05 에서 두 Enemy 가운데 서기 | 같은 프레임에 같은 행동 |
| I | STEP 04 Sequence Enemy 관찰 | `Move A → Wait → Move B` 가 Repeat 로 반복 |

---

## 8. 알려진 사항 / 설계 결정

- **한글 폰트** : 프로젝트에 폰트 파일을 복사하지 않고
  `Font.CreateDynamicFontFromOSFont` 로 OS 폰트(맑은 고딕 등)를 런타임에 사용한다.
  폰트 라이선스 문제 없이 한글이 표시된다.
- **`Application.runInBackground = true`** 를 `LabDirector.Awake` 에서 켠다.
  이걸 켜지 않으면 Unity 에디터가 포커스를 잃는 순간 실험이 멈춘다.
  프로젝트 설정(PlayerSettings)은 건드리지 않고 이 씬에서만 적용된다.
- **Player 는 죽지 않는다** : `LabPlayerVitals` 가 체력을 15% 아래로 내려가지 않게 하고
  천천히 회복시킨다. 실험이 중간에 끊기지 않게 하기 위한 교육용 처리다.
- **Enemy 이동은 물리를 쓰지 않는다** : 평지 위에서 X 축만 이동하고 Y 는 고정한다.
  덕분에 상태 전환 관찰이 흔들림 없이 재현된다.
- **Behavior Tree 의 Action 은 한 프레임 분량만 실행하고 `Success` 를 돌려준다.**
  트리가 매 프레임 Root 부터 다시 평가되어야 "더 우선순위가 높은 조건이 방금 참이 되었는가"
  를 확인할 수 있기 때문이다. (자세한 이유는 `LabCombatActions.cs` 주석에 적어 두었다.)
- **공유 Blackboard 에는 3인조만 등록된다** (`AIContext.reactToSharedAlarm`).
  그러지 않으면 학습장 반대편 Enemy 가 Player 를 봤다는 이유로 경보가 켜져서
  실험이 헷갈리게 된다.
- 콘솔에 뜨는 `Unity.Behavior.ActionNodeModel ... missing the [Serializable] attribute`
  경고는 Behavior 패키지 자체의 경고이며 이 프로젝트와 무관하다.

---

## 9. 최종 성공 기준

학습자가 아래 질문에 **직접 본 장면을 근거로** 자기 말로 답할 수 있으면 성공이다.
(`Q` 키로 화면에서도 볼 수 있다.)

1. State 란 무엇인가?
2. State Transition 이란 무엇인가?
3. State Pattern 은 어떤 문제를 해결하기 위한 패턴인가?
4. Enter / Execute / Exit 의 역할은 무엇인가?
5. Behavior Tree 란 무엇인가?
6. Sequence 는 무엇을 하는가?
7. Selector 는 무엇을 하는가?
8. Condition 과 Action 의 차이는 무엇인가?
9. Blackboard 는 왜 필요한가?
10. 같은 Enemy 를 두 방식으로 만들었을 때 구조적으로 어떤 차이가 있는가?
11. 상태 중심 AI 와 행동 조합 AI 중 각각 어떤 구조가 더 적합한가?
