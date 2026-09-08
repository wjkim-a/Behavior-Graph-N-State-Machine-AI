using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 각 학습 구역의 안내 표지. LabUI 가 이 위치에 떠 있는 라벨을 그린다.
    /// 학습자가 지금 어느 구역에 있는지 바로 알 수 있게 한다.
    /// </summary>
    public class LabZoneSign : MonoBehaviour
    {
        [Tooltip("예: STEP 02 - 03")]
        public string title = "STEP 01";

        [Tooltip("예: State 와 State Pattern")]
        public string subtitle = "";

        [Tooltip("표지 색")]
        public Color color = new Color(0.35f, 0.75f, 1f, 1f);
    }
}
