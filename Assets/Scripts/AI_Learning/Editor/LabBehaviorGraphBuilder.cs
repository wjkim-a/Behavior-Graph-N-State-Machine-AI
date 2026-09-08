using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEditor;
using UnityEngine;

namespace AILearning.LabEditor
{
    /// <summary>
    /// Behavior Graph 에셋을 코드로 만든다.
    ///
    /// 왜 코드로 만드는가?
    ///   학습자가 그래프를 직접 조립하는 것은 이 수업의 목표가 아니다.
    ///   완성된 그래프를 열어서 "구조를 관찰" 하는 것이 목표다.
    ///   그래서 메뉴 한 번으로 항상 같은 그래프가 만들어지도록 자동화했다.
    ///
    /// 만들어지는 그래프 (Assets/Resources/Behavior/AILab/)
    ///   AILab_Combat.asset       STEP 05 / 06 용 완성형 전투 트리 (5개 가지)
    ///   AILab_SelectorDemo.asset STEP 04 용 Selector 실험 트리 (3개 가지)
    ///   AILab_SequenceDemo.asset STEP 04 용 Sequence + Repeat 실험 트리
    ///
    /// 만든 뒤에는 Project 창에서 에셋을 더블클릭하면 Behavior 그래프 창이 열린다.
    /// </summary>
    public static class LabBehaviorGraphBuilder
    {
        public const string Folder = "Assets/Resources/Behavior/AILab";

        public const string CombatGraphPath = Folder + "/AILab_Combat.asset";
        public const string SelectorGraphPath = Folder + "/AILab_SelectorDemo.asset";
        public const string SequenceGraphPath = Folder + "/AILab_SequenceDemo.asset";

        // Behavior 패키지의 기본 노드 이름
        private const string NodeOnStart = "On Start";
        private const string NodeSelector = "Try In Order";
        private const string NodeSequence = "Sequence";
        private const string NodeRepeat = "Repeat";
        private const string NodeGuard = "Conditional Guard";

        [MenuItem("AI Lab/1. Behavior Graph 만들기", false, 10)]
        public static void BuildAll()
        {
            EnsureFolder();

            BuildCombatGraph();
            BuildSelectorDemoGraph();
            BuildSequenceDemoGraph();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AI Lab] Behavior Graph 3개를 만들었다: " + Folder);
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Behavior"))
                AssetDatabase.CreateFolder("Assets/Resources", "Behavior");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/Resources/Behavior", "AILab");
        }

        // =============================================================
        //  1) 완성형 전투 트리
        //
        //  On Start (Repeat)
        //    └ Try In Order  (Selector)
        //        ├ Sequence ─ [Guard: Is Dead]        → Lab Die
        //        ├ Sequence ─ [Guard: Low Health]     → Lab Flee
        //        ├ Sequence ─ [Guard: In Attack Range]→ Lab Attack
        //        ├ Sequence ─ [Guard: Player Detected]→ Lab Chase
        //        └ Lab Patrol                      (조건 없음 = 기본 행동)
        // =============================================================
        public static void BuildCombatGraph()
        {
            BehaviorAuthoringGraph asset = CreateGraph(CombatGraphPath);
            AddStandardVariables(asset);

            StartNodeModel start = (StartNodeModel)AddNode(asset, NodeOnStart, new Vector2(0f, 0f), null);
            start.Repeat = true;

            NodeModel selector = AddNode(asset, NodeSelector, new Vector2(0f, 190f), start);

            // 왼쪽부터가 곧 우선순위다. (자식 순서는 X 좌표로 결정된다)
            float[] columnX = { -760f, -380f, 0f, 380f, 760f };

            AddGuardedBranch(asset, selector, columnX[0], "Lab Is Dead", "Lab Die");
            AddGuardedBranch(asset, selector, columnX[1], "Lab Low Health", "Lab Flee");
            AddGuardedBranch(asset, selector, columnX[2], "Lab In Attack Range", "Lab Attack");
            AddGuardedBranch(asset, selector, columnX[3], "Lab Player Detected", "Lab Chase");

            // 마지막 가지는 조건이 없다. 그래서 앞의 조건이 모두 실패했을 때 실행된다.
            AddNode(asset, "Lab Patrol", new Vector2(columnX[4], 380f), selector);

            Finalize(asset);
        }

        // =============================================================
        //  2) Selector 실험 트리 (STEP 04)
        // =============================================================
        public static void BuildSelectorDemoGraph()
        {
            BehaviorAuthoringGraph asset = CreateGraph(SelectorGraphPath);
            AddStandardVariables(asset);

            StartNodeModel start = (StartNodeModel)AddNode(asset, NodeOnStart, new Vector2(0f, 0f), null);
            start.Repeat = true;

            NodeModel selector = AddNode(asset, NodeSelector, new Vector2(0f, 190f), start);

            AddGuardedBranch(asset, selector, -380f, "Lab In Attack Range", "Lab Attack");
            AddGuardedBranch(asset, selector, 0f, "Lab Player Detected", "Lab Chase");
            AddNode(asset, "Lab Patrol", new Vector2(380f, 380f), selector);

            Finalize(asset);
        }

        // =============================================================
        //  3) Sequence + Repeat 실험 트리 (STEP 04)
        //
        //  On Start
        //    └ Repeat (Forever)
        //        └ Sequence
        //            ├ Lab Move To Point (Seq_PointA)
        //            ├ Lab Wait (1초)
        //            └ Lab Move To Point (Seq_PointB)
        // =============================================================
        public static void BuildSequenceDemoGraph()
        {
            BehaviorAuthoringGraph asset = CreateGraph(SequenceGraphPath);
            AddStandardVariables(asset);

            VariableModel pointA = AddVariable<GameObject>(asset, "Seq_PointA", null);
            VariableModel pointB = AddVariable<GameObject>(asset, "Seq_PointB", null);
            VariableModel moveSpeed = AddVariable(asset, "MoveSpeed", 1.6f);
            VariableModel waitSeconds = AddVariable(asset, "WaitSeconds", 1.0f);

            StartNodeModel start = (StartNodeModel)AddNode(asset, NodeOnStart, new Vector2(0f, 0f), null);

            // Repeat 노드가 반복을 담당하는 것을 보여주기 위해 On Start 의 Repeat 은 끈다.
            start.Repeat = false;

            RepeatNodeModel repeat = (RepeatNodeModel)AddNode(asset, NodeRepeat, new Vector2(0f, 190f), start);
            repeat.Mode = RepeatNodeModel.RepeatMode.Forever;

            NodeModel sequence = AddNode(asset, NodeSequence, new Vector2(0f, 360f), repeat);

            BehaviorGraphNodeModel moveA =
                (BehaviorGraphNodeModel)AddNode(asset, "Lab Move To Point", new Vector2(-320f, 560f), sequence);
            LinkField(moveA, "Point", pointA);
            LinkField(moveA, "Speed", moveSpeed);

            BehaviorGraphNodeModel wait =
                (BehaviorGraphNodeModel)AddNode(asset, "Lab Wait", new Vector2(0f, 560f), sequence);
            LinkField(wait, "Seconds", waitSeconds);

            BehaviorGraphNodeModel moveB =
                (BehaviorGraphNodeModel)AddNode(asset, "Lab Move To Point", new Vector2(320f, 560f), sequence);
            LinkField(moveB, "Point", pointB);
            LinkField(moveB, "Speed", moveSpeed);

            Finalize(asset);
        }

        // =============================================================
        //  공통 도우미
        // =============================================================

        private static BehaviorAuthoringGraph CreateGraph(string path)
        {
            BehaviorAuthoringGraph existing = AssetDatabase.LoadAssetAtPath<BehaviorAuthoringGraph>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);

            BehaviorAuthoringGraph asset = ScriptableObject.CreateInstance<BehaviorAuthoringGraph>();
            asset.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(asset, path);

            // 그래프는 반드시 Blackboard 를 가져야 한다.
            asset.EnsureAssetHasBlackboard();

            // "Self" (그래프를 소유한 GameObject) 변수를 보장한다.
            GraphAssetProcessor.EnsureBlackboardGraphOwnerVariable(asset.Blackboard);

            return asset;
        }

        /// <summary>
        /// 세 그래프가 공통으로 갖는 Blackboard 변수.
        /// BTBlackboardSync 컴포넌트가 매 프레임 실제 값을 여기에 써 넣는다.
        /// </summary>
        private static void AddStandardVariables(BehaviorAuthoringGraph asset)
        {
            AddVariable<GameObject>(asset, BTBlackboardSync.VarTarget, null);
            AddVariable(asset, BTBlackboardSync.VarPlayerDetected, false);
            AddVariable(asset, BTBlackboardSync.VarInAttackRange, false);
            AddVariable(asset, BTBlackboardSync.VarHealthPercent, 100f);
            AddVariable(asset, BTBlackboardSync.VarAlarmActive, false);
        }

        private static VariableModel AddVariable<T>(BehaviorAuthoringGraph asset, string name, T value)
        {
            VariableModel existing = asset.Blackboard.Variables.FirstOrDefault(v => v.Name == name);
            if (existing != null)
                return existing;

            TypedVariableModel<T> variable = new TypedVariableModel<T>
            {
                Name = name,
                m_Value = value
            };
            asset.Blackboard.Variables.Add(variable);
            return variable;
        }

        private static void LinkField(BehaviorGraphNodeModel node, string fieldName, VariableModel variable)
        {
            if (node == null || variable == null)
                return;
            node.SetField(fieldName, variable, variable.Type);
        }

        /// <summary>조건이 붙은 가지 하나를 만든다. Sequence 안에 Guard 와 Action 을 순서대로 넣는다.</summary>
        private static void AddGuardedBranch(BehaviorAuthoringGraph asset, NodeModel selector, float columnX,
            string conditionName, string actionName)
        {
            NodeModel sequence = AddNode(asset, NodeSequence, new Vector2(columnX, 380f), selector);

            // Guard 가 Action 보다 왼쪽에 있어야 먼저 실행된다.
            BehaviorGraphNodeModel guard =
                (BehaviorGraphNodeModel)AddNode(asset, NodeGuard, new Vector2(columnX - 130f, 570f), sequence);
            AddCondition(guard, conditionName);

            AddNode(asset, actionName, new Vector2(columnX + 130f, 570f), sequence);
        }

        private static void AddCondition(BehaviorGraphNodeModel node, string conditionName)
        {
            ConditionInfo info = FindCondition(conditionName);
            if (info == null)
            {
                Debug.LogError("[AI Lab] Condition 을 찾지 못했다: " + conditionName);
                return;
            }

            IConditionalNodeModel conditional = node as IConditionalNodeModel;
            if (conditional == null)
            {
                Debug.LogError("[AI Lab] 이 노드는 조건을 받을 수 없다: " + node);
                return;
            }

            ConditionModel model = new ConditionModel(node, null, info);
            conditional.ConditionModels.Add(model);
            conditional.RequiresAllConditionsTrue = true;
        }

        private static NodeModel AddNode(BehaviorAuthoringGraph asset, string nodeName, Vector2 position,
            NodeModel parent)
        {
            NodeInfo info = FindNode(nodeName);
            if (info == null)
            {
                Debug.LogError("[AI Lab] 노드를 찾지 못했다: " + nodeName);
                return null;
            }

            PortModel parentPort = null;
            if (parent != null && parent.TryDefaultOutputPortModel(out PortModel port))
                parentPort = port;

            return asset.CreateNode(info.ModelType, position, parentPort, new object[] { info });
        }

        private static NodeInfo FindNode(string nodeName)
        {
            List<NodeInfo> all = Unity.Behavior.NodeRegistry.NodeInfos;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Name == nodeName)
                    return all[i];
            }
            return null;
        }

        private static ConditionInfo FindCondition(string conditionName)
        {
            foreach (ConditionInfo info in Unity.Behavior.NodeRegistry.Instance.m_TypeIDToConditionInfo.Values)
            {
                if (info.Name == conditionName)
                    return info;
            }
            return null;
        }

        private static void Finalize(BehaviorAuthoringGraph asset)
        {
            asset.SetAssetDirty();

            // Blackboard 변수를 추가한 뒤에는 런타임 Blackboard 도 다시 만들어져야 한다.
            // BuildRuntimeGraph 는 "Blackboard 의 VersionTimestamp 가 달라졌을 때만" 다시 만든다.
            // 그래서 여기서 타임스탬프만 갱신해 주고, 실제 재생성은 BuildRuntimeGraph 에 맡긴다.
            // (여기서 직접 BuildRuntimeBlackboard 를 호출하면 타임스탬프가 같아져서
            //  오히려 재생성이 건너뛰어지고, 실행 중 변수를 찾지 못하게 된다.)
            BehaviorBlackboardAuthoringAsset blackboard = asset.Blackboard as BehaviorBlackboardAuthoringAsset;
            if (blackboard != null)
                blackboard.SetAssetDirty();

            asset.BuildRuntimeGraph();
            asset.SaveAsset();
            EditorUtility.SetDirty(asset);
        }
    }
}
