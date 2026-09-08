using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// AI 의 "몸"을 담당한다. 이동 / 방향 전환 / 정지만 한다.
    /// 판단은 하지 않는다. (판단은 State Pattern 또는 Behavior Graph 가 한다.)
    ///
    /// 이렇게 나누는 이유:
    /// - Patrol / Chase / Flee 는 모두 "옆으로 걷는다" 라는 같은 동작을 쓴다.
    /// - 그 공통 동작을 여기 한 곳에 두면, 구조(State / Tree)만 바꿔서 비교할 수 있다.
    /// </summary>
    public class AIMotor : MonoBehaviour
    {
        [Tooltip("스프라이트를 좌우로 뒤집을 렌더러. 비어 있으면 자식에서 자동으로 찾는다.")]
        public SpriteRenderer spriteRenderer;

        [Tooltip("Y 좌표를 시작 높이로 고정한다. 2D 플랫폼 위에서 학습용으로 흔들림 없이 걷게 한다.")]
        public bool lockVerticalPosition = true;

        [Tooltip("이 X 범위를 넘어가지 않는다. min == max 이면 제한하지 않는다.")]
        public float minX;
        public float maxX;

        private float _baseY;
        private float _lastFacing = 1f;

        /// <summary>이번 프레임에 실제로 움직인 속도(부호 포함). UI / 애니메이션 판단용.</summary>
        public float CurrentVelocityX { get; private set; }

        /// <summary>바라보는 방향. +1 = 오른쪽, -1 = 왼쪽.</summary>
        public float Facing => _lastFacing;

        private void Awake()
        {
            _baseY = transform.position.y;
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (lockVerticalPosition)
            {
                Vector3 p = transform.position;
                if (!Mathf.Approximately(p.y, _baseY))
                {
                    p.y = _baseY;
                    transform.position = p;
                }
            }
        }

        /// <summary>목표 X 좌표를 향해 speed 로 이동한다.</summary>
        public void MoveTowardsX(float targetX, float speed)
        {
            float x = transform.position.x;
            float dir = Mathf.Sign(targetX - x);

            if (Mathf.Abs(targetX - x) < 0.01f)
            {
                Stop();
                return;
            }

            Move(dir, speed);
        }

        /// <summary>목표에서 멀어지는 방향으로 이동한다. Flee 에서 사용.</summary>
        public void MoveAwayFromX(float targetX, float speed)
        {
            float dir = Mathf.Sign(transform.position.x - targetX);
            if (Mathf.Approximately(dir, 0f))
                dir = _lastFacing;
            Move(dir, speed);
        }

        /// <summary>방향(-1 / +1) 과 속도로 이동한다.</summary>
        public void Move(float direction, float speed)
        {
            direction = Mathf.Sign(direction);
            float delta = direction * speed * Time.deltaTime;

            Vector3 p = transform.position;
            p.x += delta;

            if (!Mathf.Approximately(minX, maxX))
                p.x = Mathf.Clamp(p.x, minX, maxX);

            transform.position = p;
            CurrentVelocityX = speed * direction;
            Face(direction);
        }

        /// <summary>제자리에 선다.</summary>
        public void Stop()
        {
            CurrentVelocityX = 0f;
        }

        /// <summary>특정 X 좌표를 바라본다. (이동 없이 방향만)</summary>
        public void FaceTowardsX(float targetX)
        {
            float dir = targetX - transform.position.x;
            if (Mathf.Abs(dir) > 0.01f)
                Face(Mathf.Sign(dir));
        }

        private void Face(float direction)
        {
            if (Mathf.Approximately(direction, 0f))
                return;

            _lastFacing = direction;
            if (spriteRenderer != null)
                spriteRenderer.flipX = direction < 0f;
        }

        /// <summary>실험 리셋에서 사용. 시작 위치로 되돌린다.</summary>
        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
            _baseY = position.y;
            CurrentVelocityX = 0f;
        }
    }
}
