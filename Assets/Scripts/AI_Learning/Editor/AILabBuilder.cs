using System.Collections.Generic;
using Unity.Behavior;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace AILearning.LabEditor
{
    /// <summary>
    /// AI Learning Lab 의 프리팹과 씬을 자동으로 만든다.
    ///
    /// 기존 프로젝트를 건드리지 않는다.
    ///   - 기존 씬(TilemapSample)은 수정하지 않는다. Player / Enemy 오브젝트를 복사해서 쓴다.
    ///   - 기존 스크립트(PlayerController / EnemyFSM 등)도 삭제하지 않는다.
    ///   - 학습용 결과물은 모두 새 폴더와 새 씬에 만든다.
    ///
    /// 메뉴
    ///   AI Lab / 0. 전체 빌드      : 그래프 + 프리팹 + 씬을 한 번에
    ///   AI Lab / 1. Behavior Graph : 그래프만
    ///   AI Lab / 2. 프리팹         : 프리팹만
    ///   AI Lab / 3. 씬             : 씬만
    /// </summary>
    public static class AILabBuilder
    {
        private const string SourceScenePath = "Assets/Scenes/TilemapSample.unity";
        private const string PrefabFolder = "Assets/Prefabs/AI_Learning";
        private const string LabScenePath = "Assets/Scenes/AI_Learning_Lab.unity";
        private const string GroundTilePath = "Assets/Resources/TileMap/Tile/normalGroundTile_0.asset";

        private const string PlayerPrefab = PrefabFolder + "/LabPlayer.prefab";
        private const string EnemyBasePrefab = PrefabFolder + "/LabEnemy_Base.prefab";
        private const string EnemySimplePrefab = PrefabFolder + "/LabEnemy_01_Reactive.prefab";
        private const string EnemyStatePrefab = PrefabFolder + "/LabEnemy_02_StatePattern.prefab";
        private const string EnemyBtFullPrefab = PrefabFolder + "/LabEnemy_03_BehaviorGraph.prefab";
        private const string EnemyBtSelectorPrefab = PrefabFolder + "/LabEnemy_04_BT_Selector.prefab";
        private const string EnemyBtSequencePrefab = PrefabFolder + "/LabEnemy_05_BT_Sequence.prefab";

        // =============================================================
        //  메뉴
        // =============================================================

        [MenuItem("AI Lab/0. 전체 빌드 (그래프 + 프리팹 + 씬)", false, 1)]
        public static void BuildEverything()
        {
            LabBehaviorGraphBuilder.BuildAll();
            BuildPrefabs();
            BuildScene();
            Debug.Log("[AI Lab] 전체 빌드 완료. " + LabScenePath + " 를 열고 Play 하면 학습을 시작할 수 있다.");
        }

        [MenuItem("AI Lab/2. 학습용 프리팹 만들기", false, 11)]
        public static void BuildPrefabs()
        {
            EnsureFolders();

            // 기존 씬이 이미 열려 있으면 그것을 그대로 읽는다.
            // (열려 있지 않을 때만 추가로 열고, 끝나면 우리가 연 것만 닫는다.)
            Scene source = SceneManager.GetSceneByPath(SourceScenePath);
            bool openedHere = false;

            if (!source.IsValid() || !source.isLoaded)
            {
                source = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Additive);
                openedHere = true;
            }

            GameObject srcPlayer = FindRoot(source, "Player");
            GameObject srcEnemy = FindRoot(source, "Enemy");

            if (srcPlayer == null || srcEnemy == null)
            {
                Debug.LogError("[AI Lab] 기존 씬에서 Player / Enemy 를 찾지 못했다: " + SourceScenePath);
                if (openedHere) EditorSceneManager.CloseScene(source, true);
                return;
            }

            BuildPlayerPrefab(srcPlayer);
            BuildEnemyPrefabs(srcEnemy);

            if (openedHere)
                EditorSceneManager.CloseScene(source, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AI Lab] 프리팹을 만들었다: " + PrefabFolder);
        }

        [MenuItem("AI Lab/3. AI Learning Lab 씬 만들기", false, 12)]
        public static void BuildScene()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab) == null)
                BuildPrefabs();

            // 열려 있는 씬에 저장하지 않은 변경이 있으면 새 씬으로 바꾸면서 잃어버릴 수 있다.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene open = SceneManager.GetSceneAt(i);
                if (open.isDirty)
                {
                    Debug.LogError("[AI Lab] 열려 있는 씬 '" + open.name +
                                   "' 에 저장하지 않은 변경이 있다. 먼저 저장한 뒤 다시 실행하자.");
                    return;
                }
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateGround();
            CreateSystems();

            GameObject player = Instantiate(PlayerPrefab, LabNames.Player,
                new Vector3(LabLayout.Anchor01, LabLayout.PlayerY, 0f));
            if (player != null)
                player.tag = "Player";

            CreateZone01();
            CreateZone02();
            CreateZone04();
            CreateZone05();
            CreateZone06();

            WireSystems(player);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, LabScenePath);
            AddSceneToBuildSettings();

            Debug.Log("[AI Lab] 씬을 만들었다: " + LabScenePath);
        }

        [MenuItem("AI Lab/AI Learning Lab 씬 열기", false, 40)]
        public static void OpenLabScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LabScenePath) == null)
            {
                Debug.LogWarning("[AI Lab] 씬이 아직 없다. 먼저 'AI Lab / 0. 전체 빌드' 를 실행하자.");
                return;
            }
            EditorSceneManager.OpenScene(LabScenePath, OpenSceneMode.Single);
        }

        // =============================================================
        //  프리팹
        // =============================================================

        private static void BuildPlayerPrefab(GameObject srcPlayer)
        {
            GameObject copy = Object.Instantiate(srcPlayer);
            copy.name = LabNames.Player;

            Health health = GetOrAdd<Health>(copy);
            SetPrivateFloat(health, "_maxHealth", 200f);

            LabPlayerVitals vitals = GetOrAdd<LabPlayerVitals>(copy);
            vitals.regenPerSecond = 8f;
            vitals.minRatio = 0.15f;

            // 기존 이동 시스템을 그대로 쓴다. 값이 비어 있을 때만 보정한다.
            PlayerController controller = copy.GetComponent<PlayerController>();
            if (controller != null)
            {
                if (GetPrivateFloat(controller, "_moveSpeed") <= 0.01f)
                    SetPrivateFloat(controller, "_moveSpeed", 5f);
                if (GetPrivateFloat(controller, "_jumpSpeed") <= 0.01f)
                    SetPrivateFloat(controller, "_jumpSpeed", 5f);
            }

            PrefabUtility.SaveAsPrefabAsset(copy, PlayerPrefab);
            Object.DestroyImmediate(copy);
        }

        private static void BuildEnemyPrefabs(GameObject srcEnemy)
        {
            // 1) 공통 베이스 : 기존 Enemy 의 스프라이트 / Animator / Collider 를 그대로 물려받는다.
            SaveEnemyVariant(srcEnemy, EnemyBasePrefab, "LabEnemy_Base", null);

            // 2) STEP 01 : 구조 없는 AI
            SaveEnemyVariant(srcEnemy, EnemySimplePrefab, LabNames.Simple, go =>
            {
                SimpleReactiveAI simple = go.AddComponent<SimpleReactiveAI>();
                simple.noticeRange = 7.5f;
                simple.chaseRange = 4.5f;

                AIContext ctx = go.GetComponent<AIContext>();
                ctx.systemKind = AISystemKind.Reactive;
                ctx.displayName = "Enemy A (구조 없음)";
                ctx.detectRange = 4.5f;
                ctx.loseSightRange = 7.5f;

                go.AddComponent<LabAIStructure>().kind = LabStructureKind.Reactive;
            });

            // 3) STEP 02 / 03 / 05 : State Pattern
            SaveEnemyVariant(srcEnemy, EnemyStatePrefab, LabNames.State, go =>
            {
                go.AddComponent<EnemyStateMachine>();

                AIContext ctx = go.GetComponent<AIContext>();
                ctx.systemKind = AISystemKind.StatePattern;
                ctx.displayName = "State Pattern Enemy";

                go.AddComponent<LabAIStructure>().kind = LabStructureKind.StateMachine;
            });

            // 4) STEP 05 / 06 : Behavior Graph (완성형 전투 트리)
            SaveEnemyVariant(srcEnemy, EnemyBtFullPrefab, LabNames.CompareBt, go =>
            {
                AttachAgent(go, LabBehaviorGraphBuilder.CombatGraphPath);

                AIContext ctx = go.GetComponent<AIContext>();
                ctx.systemKind = AISystemKind.BehaviorGraph;
                ctx.displayName = "Behavior Graph Enemy";

                LabAIStructure structure = go.AddComponent<LabAIStructure>();
                structure.kind = LabStructureKind.BtFull;
                structure.graphAssetPath = LabBehaviorGraphBuilder.CombatGraphPath;
            });

            // 5) STEP 04-2 : Selector 실험
            SaveEnemyVariant(srcEnemy, EnemyBtSelectorPrefab, LabNames.BtSelector, go =>
            {
                AttachAgent(go, LabBehaviorGraphBuilder.SelectorGraphPath);

                AIContext ctx = go.GetComponent<AIContext>();
                ctx.systemKind = AISystemKind.BehaviorGraph;
                ctx.displayName = "Selector 실험 Enemy";

                LabAIStructure structure = go.AddComponent<LabAIStructure>();
                structure.kind = LabStructureKind.BtSelectorDemo;
                structure.graphAssetPath = LabBehaviorGraphBuilder.SelectorGraphPath;
            });

            // 6) STEP 04-1 : Sequence + Repeat 실험
            SaveEnemyVariant(srcEnemy, EnemyBtSequencePrefab, LabNames.BtSequence, go =>
            {
                AttachAgent(go, LabBehaviorGraphBuilder.SequenceGraphPath);
                go.AddComponent<LabSequencePoints>();

                AIContext ctx = go.GetComponent<AIContext>();
                ctx.systemKind = AISystemKind.BehaviorGraph;
                ctx.displayName = "Sequence 실험 Enemy";

                LabAIStructure structure = go.AddComponent<LabAIStructure>();
                structure.kind = LabStructureKind.BtSequenceDemo;
                structure.graphAssetPath = LabBehaviorGraphBuilder.SequenceGraphPath;
            });
        }

        private static void SaveEnemyVariant(GameObject srcEnemy, string path, string objectName,
            System.Action<GameObject> configure)
        {
            GameObject copy = Object.Instantiate(srcEnemy);
            copy.name = objectName;

            StripLegacyAI(copy);
            AddCoreAI(copy);

            configure?.Invoke(copy);

            PrefabUtility.SaveAsPrefabAsset(copy, path);
            Object.DestroyImmediate(copy);
        }

        /// <summary>기존 수업용 AI 스크립트를 학습장 사본에서만 떼어낸다. (원본 씬은 그대로)</summary>
        private static void StripLegacyAI(GameObject go)
        {
            RemoveIfPresent<EnemyFSM>(go);
            RemoveIfPresent<EnemyStatePattern>(go);
            RemoveIfPresent<EnemyAI>(go);
            RemoveIfPresent<BehaviorGraphAgent>(go);
        }

        /// <summary>두 구현이 공통으로 쓰는 부분. 감각 / 이동 / 공격 / 체력 / 로그.</summary>
        private static void AddCoreAI(GameObject go)
        {
            Health health = GetOrAdd<Health>(go);
            SetPrivateFloat(health, "_maxHealth", 100f);

            AIMotor motor = GetOrAdd<AIMotor>(go);
            motor.lockVerticalPosition = true;
            motor.spriteRenderer = go.GetComponentInChildren<SpriteRenderer>();

            AIAttack attack = GetOrAdd<AIAttack>(go);
            attack.damage = 10f;
            attack.interval = 0.9f;

            GetOrAdd<PatrolRoute>(go);
            GetOrAdd<AIEventLog>(go);

            AIContext ctx = GetOrAdd<AIContext>(go);
            ctx.detectRange = 5f;
            ctx.loseSightRange = 6.5f;
            ctx.attackRange = 1.4f;
            ctx.patrolSpeed = 1.6f;
            ctx.chaseSpeed = 3.1f;
            ctx.fleeSpeed = 3.6f;
            ctx.lowHealthRatio = 0.3f;
            ctx.fleeRegenPerSecond = 6f;

            GetOrAdd<AIRangeRing>(go);
        }

        private static void AttachAgent(GameObject go, string graphPath)
        {
            BehaviorGraphAgent agent = GetOrAdd<BehaviorGraphAgent>(go);

            BehaviorAuthoringGraph authoring = AssetDatabase.LoadAssetAtPath<BehaviorAuthoringGraph>(graphPath);
            if (authoring == null)
            {
                Debug.LogError("[AI Lab] Behavior Graph 를 찾지 못했다: " + graphPath);
            }
            else
            {
                BehaviorGraph runtime = authoring.BuildRuntimeGraph();
                agent.Graph = runtime;
            }

            GetOrAdd<BTNodeTrace>(go);
            GetOrAdd<BTBlackboardSync>(go);
        }

        // =============================================================
        //  씬 구성
        // =============================================================

        private static void CreateCamera()
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(LabLayout.Anchor01, LabLayout.CameraY, -10f);

            Camera cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = LabLayout.CameraSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.043f, 0.055f, 0.078f, 1f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            go.AddComponent<AudioListener>();

            LabCameraRig rig = go.AddComponent<LabCameraRig>();
            rig.cameraY = LabLayout.CameraY;
            rig.minX = LabLayout.CameraMinX;
            rig.maxX = LabLayout.CameraMaxX;
        }

        /// <summary>
        /// 단계별 플랫폼과 그 사이를 막는 벽을 깐다.
        ///
        /// 타일맵을 두 개로 나눈다.
        ///   GroundTilemap : 발판. "Ground" 태그가 붙는다. (PlayerController 의 점프 판정용)
        ///   WallTilemap   : 플랫폼 양 끝의 벽. 태그를 붙이지 않는다.
        ///                   벽에 닿았다고 점프가 되살아나면 벽을 타고 올라갈 수 있기 때문이다.
        /// </summary>
        private static void CreateGround()
        {
            GameObject gridGo = new GameObject("Grid");
            Grid grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(GroundTilePath);
            if (tile == null)
            {
                Debug.LogWarning("[AI Lab] 지면 타일을 찾지 못했다: " + GroundTilePath);
                return;
            }

            Tilemap ground = CreateTilemapLayer(gridGo, "GroundTilemap", -10, "Ground");
            Tilemap walls = CreateTilemapLayer(gridGo, "WallTilemap", -9, null);

            foreach (LabLayout.Platform p in LabLayout.Platforms)
            {
                // 발판은 두 줄만 깐다. 그 아래 빈 공간은 학습 패널이 놓이는 자리다.
                // 벽이 서는 자리(양 끝 바깥 한 칸)에도 발판을 깔아 벽의 밑동을 만든다.
                for (int x = p.MinX - 1; x <= p.MaxX + 1; x++)
                {
                    ground.SetTile(new Vector3Int(x, -4, 0), tile);
                    ground.SetTile(new Vector3Int(x, -5, 0), tile);
                }

                // 양 끝 바깥에 벽을 세운다. 플랫폼 사이의 빈 공간으로 떨어질 수 없다.
                for (int y = -3; y < -3 + LabLayout.WallHeight; y++)
                {
                    walls.SetTile(new Vector3Int(p.MinX - 1, y, 0), tile);
                    walls.SetTile(new Vector3Int(p.MaxX + 1, y, 0), tile);
                }
            }
        }

        private static Tilemap CreateTilemapLayer(GameObject gridGo, string name, int sortingOrder, string tag)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(gridGo.transform, false);
            if (!string.IsNullOrEmpty(tag))
                go.tag = tag;

            Tilemap tilemap = go.AddComponent<Tilemap>();
            TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            go.AddComponent<TilemapCollider2D>();
            return tilemap;
        }

        private static void CreateSystems()
        {
            GameObject go = new GameObject("LabSystems");
            go.AddComponent<SquadBlackboard>();
            go.AddComponent<LabDirector>();
            go.AddComponent<LabUI>();
        }

        private static void WireSystems(GameObject player)
        {
            GameObject systems = GameObject.Find("LabSystems");
            if (systems == null) return;

            LabDirector director = systems.GetComponent<LabDirector>();
            if (director != null && player != null)
                director.playerRoot = player.transform;

            LabUI ui = systems.GetComponent<LabUI>();
            if (ui != null)
                ui.director = director;

            SquadBlackboard board = systems.GetComponent<SquadBlackboard>();
            if (board != null)
            {
                board.autoAlarmOnDetect = true;
                board.alarmHoldSeconds = 5f;
            }

            LabCameraRig rig = Object.FindFirstObjectByType<LabCameraRig>();
            if (rig != null && player != null)
                rig.target = player.transform;
        }

        // 각 구역 ------------------------------------------------------

        private static void CreateZone01()
        {
            CreateSign("Sign_01", LabLayout.SimpleX, "STEP 01",
                "AI 란 무엇인가  ·  구조 없는 AI", new Color(0.70f, 0.80f, 0.95f));

            GameObject enemy = Instantiate(EnemySimplePrefab, LabNames.Simple,
                new Vector3(LabLayout.SimpleX, LabLayout.EnemyY, 0f));
            if (enemy == null) return;

            // 이 AI 는 순찰하지 않는다. 제자리에서 거리만 본다.
            PatrolRoute route = enemy.GetComponent<PatrolRoute>();
            if (route != null) route.points = new Transform[0];

            SetLane(enemy, LabLayout.SimpleLaneL, LabLayout.SimpleLaneR);
        }

        private static void CreateZone02()
        {
            CreateSign("Sign_02", LabLayout.StateX, "STEP 02  ·  03",
                "State 와 State Pattern", new Color(0.42f, 0.82f, 0.55f));

            GameObject enemy = Instantiate(EnemyStatePrefab, LabNames.State,
                new Vector3(LabLayout.StateX, LabLayout.EnemyY, 0f));
            if (enemy == null) return;

            SetPatrol(enemy, "W_State", LabLayout.StatePatrolLeft, LabLayout.StatePatrolRight,
                LabLayout.StateLaneL, LabLayout.StateLaneR);
        }

        private static void CreateZone04()
        {
            CreateSign("Sign_04a", LabLayout.SeqEnemyX + 1f, "STEP 04 - 1",
                "SEQUENCE + REPEAT 실험", new Color(0.80f, 0.78f, 0.45f));

            GameObject seq = Instantiate(EnemyBtSequencePrefab, LabNames.BtSequence,
                new Vector3(LabLayout.SeqEnemyX, LabLayout.EnemyY, 0f));

            GameObject pointA = CreateWaypoint("Seq_PointA", LabLayout.SeqPointAX,
                new Color(0.50f, 0.85f, 0.85f, 0.8f));
            GameObject pointB = CreateWaypoint("Seq_PointB", LabLayout.SeqPointBX,
                new Color(0.50f, 0.85f, 0.85f, 0.8f));

            if (seq != null)
            {
                LabSequencePoints points = seq.GetComponent<LabSequencePoints>();
                if (points != null)
                {
                    points.pointA = pointA;
                    points.pointB = pointB;
                    points.moveSpeed = 1.6f;
                    points.waitSeconds = 1.0f;
                }

                // Sequence 실험 AI 는 Player 를 쫓지 않는다. 감지를 사실상 끈다.
                AIContext ctx = seq.GetComponent<AIContext>();
                if (ctx != null)
                {
                    ctx.detectRange = 0.01f;
                    ctx.loseSightRange = 0.01f;
                    ctx.attackRange = 0.01f;
                }

                SetLane(seq, LabLayout.SeqLaneL, LabLayout.SeqLaneR);
            }

            CreateSign("Sign_04b", LabLayout.SelEnemyX + 0.5f, "STEP 04 - 2",
                "SELECTOR + CONDITION 실험", new Color(1.00f, 0.70f, 0.25f));

            GameObject sel = Instantiate(EnemyBtSelectorPrefab, LabNames.BtSelector,
                new Vector3(LabLayout.SelEnemyX, LabLayout.EnemyY, 0f));
            if (sel != null)
            {
                // 학습자가 STEP 04 시작 지점에 서 있을 때는 아직 발견되지 않도록
                // 감지 범위를 조금 좁게 둔다. (직접 걸어가면 CHASE 로 바뀐다)
                AIContext selCtx = sel.GetComponent<AIContext>();
                if (selCtx != null)
                {
                    selCtx.detectRange = 3.5f;
                    selCtx.loseSightRange = 5f;
                }

                SetPatrol(sel, "W_Selector", LabLayout.SelPatrolLeft, LabLayout.SelPatrolRight,
                    LabLayout.SelLaneL, LabLayout.SelLaneR);
            }
        }

        private static void CreateZone05()
        {
            CreateSign("Sign_05", LabLayout.CompareCenterX, "STEP 05  ·  06",
                "같은 요구사항, 두 가지 구조", new Color(0.98f, 0.55f, 0.55f));

            GameObject a = Instantiate(EnemyStatePrefab, LabNames.CompareState,
                new Vector3(LabLayout.CompareStateX, LabLayout.EnemyY, 0f));
            if (a != null)
            {
                AIContext ctx = a.GetComponent<AIContext>();
                if (ctx != null) ctx.displayName = "A  ·  State Pattern";
                SetPatrol(a, "W_CmpA", LabLayout.CompareStateLeft, LabLayout.CompareStateRight,
                    LabLayout.CompareStateLaneL, LabLayout.CompareStateLaneR);
            }

            GameObject b = Instantiate(EnemyBtFullPrefab, LabNames.CompareBt,
                new Vector3(LabLayout.CompareBtX, LabLayout.EnemyY, 0f));
            if (b != null)
            {
                AIContext ctx = b.GetComponent<AIContext>();
                if (ctx != null) ctx.displayName = "B  ·  Behavior Graph";
                SetPatrol(b, "W_CmpB", LabLayout.CompareBtLeft, LabLayout.CompareBtRight,
                    LabLayout.CompareBtLaneL, LabLayout.CompareBtLaneR);
            }
        }

        private static void CreateZone06()
        {
            CreateSign("Sign_06", LabLayout.SquadBX, "STEP 06",
                "공유 BLACKBOARD 실험  ·  3인조", new Color(0.45f, 0.70f, 1.00f));

            CreateSquadMember(LabNames.SquadA, "Squad A", LabLayout.SquadAX);
            CreateSquadMember(LabNames.SquadB, "Squad B", LabLayout.SquadBX);
            CreateSquadMember(LabNames.SquadC, "Squad C", LabLayout.SquadCX);
        }

        private static void CreateSquadMember(string objectName, string displayName, float x)
        {
            GameObject go = Instantiate(EnemyBtFullPrefab, objectName,
                new Vector3(x, LabLayout.EnemyY, 0f));
            if (go == null) return;

            AIContext ctx = go.GetComponent<AIContext>();
            if (ctx != null)
            {
                ctx.displayName = displayName;

                // 한 마리만 자극해도 구분이 되도록 감지 범위를 좁게 둔다.
                ctx.detectRange = 3.2f;
                ctx.loseSightRange = 4.2f;

                // 공유 칠판의 경보에 반응한다. 이것이 이 실험의 핵심이다.
                ctx.reactToSharedAlarm = true;
            }

            SetPatrol(go, "W_" + objectName,
                x - LabLayout.SquadPatrolSpan, x + LabLayout.SquadPatrolSpan,
                x - LabLayout.SquadLaneSpan, x + LabLayout.SquadLaneSpan);
        }

        // 도우미 ------------------------------------------------------

        private static GameObject Instantiate(string prefabPath, string objectName, Vector3 position)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("[AI Lab] 프리팹을 찾지 못했다: " + prefabPath);
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = objectName;
            instance.transform.position = position;
            return instance;
        }

        /// <summary>순찰 지점 두 개를 만들고, 이동 범위를 자기 레인 안으로 묶는다.</summary>
        private static void SetPatrol(GameObject enemy, string prefix, float leftX, float rightX,
            float laneMinX, float laneMaxX)
        {
            GameObject left = CreateWaypoint(prefix + "_L", leftX, new Color(0.42f, 0.82f, 0.55f, 0.7f));
            GameObject right = CreateWaypoint(prefix + "_R", rightX, new Color(0.42f, 0.82f, 0.55f, 0.7f));

            PatrolRoute route = enemy.GetComponent<PatrolRoute>();
            if (route != null)
                route.points = new[] { left.transform, right.transform };

            SetLane(enemy, laneMinX, laneMaxX);
        }

        /// <summary>
        /// Enemy 의 이동 한계를 정한다.
        ///
        /// 레인은 Enemy 마다 겹치지 않게 LabLayout 에서 미리 갈라 두었다.
        /// 예전에는 "순찰 구간 ± 4.5" 로 자동 계산했는데, 그러면 옆 Enemy 의
        /// 자리까지 넘어가서 서로 밀고 겹쳐 버렸다. (STEP 05 / 06 에서 특히 심했다)
        /// </summary>
        private static void SetLane(GameObject enemy, float laneMinX, float laneMaxX)
        {
            AIMotor motor = enemy.GetComponent<AIMotor>();
            if (motor == null) return;

            motor.minX = laneMinX;
            motor.maxX = laneMaxX;
        }

        private static GameObject CreateWaypoint(string name, float x, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.position = new Vector3(x, LabLayout.GroundY + 0.12f, 0f);

            LabWaypointMarker marker = go.AddComponent<LabWaypointMarker>();
            marker.color = color;
            marker.size = 0.2f;
            return go;
        }

        private static void CreateSign(string name, float x, string title, string subtitle, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.position = new Vector3(x, LabLayout.SignY, 0f);

            LabZoneSign sign = go.AddComponent<LabZoneSign>();
            sign.title = title;
            sign.subtitle = subtitle;
            sign.color = color;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
                AssetDatabase.CreateFolder("Assets/Prefabs", "AI_Learning");
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                    return root;
            }
            return null;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T existing = go.GetComponent<T>();
            return existing != null ? existing : go.AddComponent<T>();
        }

        private static void RemoveIfPresent<T>(GameObject go) where T : Component
        {
            T[] all = go.GetComponents<T>();
            foreach (T c in all)
            {
                if (c != null)
                    Object.DestroyImmediate(c, true);
            }
        }

        private static void SetPrivateFloat(Component target, string fieldName, float value)
        {
            if (target == null) return;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null) return;
            prop.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static float GetPrivateFloat(Component target, string fieldName)
        {
            if (target == null) return 0f;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            return prop != null ? prop.floatValue : 0f;
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene s in scenes)
            {
                if (s.path == LabScenePath)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(LabScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
