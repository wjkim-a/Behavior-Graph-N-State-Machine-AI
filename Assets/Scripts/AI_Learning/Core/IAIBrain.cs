using System.Collections.Generic;

namespace AILearning
{
    /// <summary>
    /// AI 를 구현하는 "방식"을 구분하는 값.
    /// 같은 요구사항을 두 가지 구조로 구현했다는 사실을 UI 에서 표시하기 위해 사용한다.
    /// </summary>
    public enum AISystemKind
    {
        /// <summary>상태(State) 객체를 교체하며 행동을 바꾸는 방식.</summary>
        StatePattern = 0,

        /// <summary>노드 트리를 평가해서 실행할 행동을 고르는 방식.</summary>
        BehaviorGraph = 1,

        /// <summary>구조 없이 조건문만으로 만든 가장 단순한 AI (STEP 01 용).</summary>
        Reactive = 2
    }

    /// <summary>
    /// AI Monitor UI 는 "State Pattern 인지 Behavior Graph 인지" 몰라도 동작해야 한다.
    /// 그래서 두 구현이 공통으로 이 인터페이스를 구현하고, UI 는 이 인터페이스만 읽는다.
    /// (이것 자체가 "구조를 분리하면 읽는 쪽이 단순해진다"는 예시이기도 하다.)
    /// </summary>
    public interface IAIBrain
    {
        /// <summary>어떤 방식으로 만든 AI 인가.</summary>
        AISystemKind Kind { get; }

        /// <summary>UI 에 표시할 시스템 이름. 예: "STATE MACHINE", "BEHAVIOR GRAPH".</summary>
        string SystemName { get; }

        /// <summary>지금 실행 중인 상태 / 노드 이름. 예: "CHASE".</summary>
        string CurrentLabel { get; }

        /// <summary>직전에 실행했던 상태 / 노드 이름.</summary>
        string PreviousLabel { get; }

        /// <summary>현재 라벨이 유지된 시간(초).</summary>
        float TimeInCurrent { get; }

        /// <summary>Enter / Execute / Exit 흐름을 보여주기 위한 최근 로그.</summary>
        IReadOnlyList<string> RecentEvents { get; }
    }
}
