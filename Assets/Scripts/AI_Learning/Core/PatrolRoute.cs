using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 순찰 경로. Patrol 행동이 "어디로 갈지"를 이 컴포넌트에서 얻는다.
    ///
    /// 중요한 점: 이 컴포넌트는 State Pattern 도 Behavior Graph 도 아니다.
    /// 두 구현이 똑같이 이 데이터를 사용한다.
    /// => 그래서 "결과는 같고 구조만 다르다" 는 비교가 성립한다.
    /// </summary>
    public class PatrolRoute : MonoBehaviour
    {
        [Tooltip("순찰 지점들. 비어 있으면 시작 위치 좌우로 자동 생성한다.")]
        public Transform[] points;

        [Tooltip("자동 생성 시 좌우로 벌어지는 거리.")]
        public float autoSpan = 3f;

        [Tooltip("지점에 도착했다고 판단하는 거리.")]
        public float arriveThreshold = 0.15f;

        private Vector3[] _positions;
        private int _index;

        public int Index => _index;
        public int Count => _positions == null ? 0 : _positions.Length;

        private void Awake()
        {
            BuildPositions();
        }

        private void BuildPositions()
        {
            if (points != null && points.Length > 0)
            {
                _positions = new Vector3[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    _positions[i] = points[i] != null ? points[i].position : transform.position;
                }
                return;
            }

            // 순찰 지점을 지정하지 않아도 실습이 가능하도록 좌우 2지점을 자동으로 만든다.
            Vector3 origin = transform.position;
            _positions = new[]
            {
                origin + Vector3.left * autoSpan,
                origin + Vector3.right * autoSpan
            };
        }

        /// <summary>현재 목표 지점.</summary>
        public Vector3 CurrentTarget
        {
            get
            {
                if (_positions == null || _positions.Length == 0)
                    BuildPositions();
                return _positions[Mathf.Clamp(_index, 0, _positions.Length - 1)];
            }
        }

        /// <summary>다음 지점으로 넘어간다.</summary>
        public void Advance()
        {
            if (_positions == null || _positions.Length == 0)
                return;
            _index = (_index + 1) % _positions.Length;
        }

        /// <summary>도착했으면 true. Patrol 행동은 이 값을 보고 다음 지점으로 넘어간다.</summary>
        public bool HasArrived(Vector3 from)
        {
            return Mathf.Abs(from.x - CurrentTarget.x) <= arriveThreshold;
        }

        public void ResetRoute()
        {
            _index = 0;
        }

        private void OnDrawGizmosSelected()
        {
            if (points == null) return;
            Gizmos.color = Color.cyan;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null) continue;
                Gizmos.DrawWireSphere(points[i].position, 0.2f);
            }
        }
    }
}
