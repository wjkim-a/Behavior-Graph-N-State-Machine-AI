using UnityEngine;

namespace AILearning
{
    /// <summary>이 AI 가 어떤 구조로 만들어졌는지 표시. 다이어그램 패널이 이 값을 보고 그림을 고른다.</summary>
    public enum LabStructureKind
    {
        /// <summary>구조 없이 if / else 만 쓴 AI (STEP 01).</summary>
        Reactive = 0,

        /// <summary>State Pattern + State Machine.</summary>
        StateMachine = 1,

        /// <summary>Behavior Graph - 5개 가지를 가진 완성형 전투 트리.</summary>
        BtFull = 2,

        /// <summary>Behavior Graph - Selector 실험용 3개 가지 트리 (STEP 04).</summary>
        BtSelectorDemo = 3,

        /// <summary>Behavior Graph - Sequence + Repeat 실험용 트리 (STEP 04).</summary>
        BtSequenceDemo = 4
    }

    /// <summary>
    /// AI 오브젝트에 붙여서 "이 AI 의 구조는 무엇인가" 를 알려주는 표식.
    /// 학습용 UI 전용이며 AI 동작에는 영향을 주지 않는다.
    /// </summary>
    public class LabAIStructure : MonoBehaviour
    {
        public LabStructureKind kind = LabStructureKind.StateMachine;

        [Tooltip("Behavior Graph 를 쓰는 AI 라면, 학습자가 열어볼 그래프 에셋 경로를 표시한다.")]
        public string graphAssetPath = string.Empty;
    }
}
