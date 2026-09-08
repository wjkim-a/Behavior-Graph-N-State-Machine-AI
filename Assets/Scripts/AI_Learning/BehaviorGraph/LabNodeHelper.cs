using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// Behavior Graph 의 노드들이 공통으로 쓰는 도우미.
    ///
    /// 핵심: 노드는 자기만의 감각을 갖지 않는다.
    /// State Pattern 이 쓰는 것과 완전히 같은 AIContext 를 찾아서 사용한다.
    /// 그래서 "결과는 같고 구조만 다르다" 는 비교가 정직하게 성립한다.
    /// </summary>
    public static class LabNodeHelper
    {
        public static AIContext Context(GameObject owner)
        {
            return owner != null ? owner.GetComponent<AIContext>() : null;
        }

        public static BTNodeTrace Trace(GameObject owner)
        {
            return owner != null ? owner.GetComponent<BTNodeTrace>() : null;
        }

        /// <summary>Action 노드가 실행될 때 자기 이름을 보고한다.</summary>
        public static void ReportAction(GameObject owner, string nodeName, string because = null)
        {
            BTNodeTrace trace = Trace(owner);
            if (trace != null)
            {
                trace.ReportAction(nodeName, because);
                trace.ReportTick(nodeName);
            }
        }

        /// <summary>Condition 노드가 평가될 때 결과를 보고한다.</summary>
        public static bool ReportCondition(GameObject owner, string conditionName, bool result)
        {
            BTNodeTrace trace = Trace(owner);
            if (trace != null)
                trace.ReportCondition(conditionName, result);
            return result;
        }
    }
}
