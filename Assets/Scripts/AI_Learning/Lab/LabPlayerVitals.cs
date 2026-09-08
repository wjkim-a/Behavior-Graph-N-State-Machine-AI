using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 학습장에서는 Player 가 죽어서 실험이 끊기면 안 된다.
    /// 그래서 Enemy 의 공격은 실제로 체력을 깎지만, 최소 체력 아래로는 내려가지 않고
    /// 시간이 지나면 천천히 회복된다.
    ///
    /// (교육용 환경이라는 점을 분명히 하기 위해 별도 컴포넌트로 분리했다.
    ///  실제 게임에서는 이런 처리를 하지 않는다.)
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class LabPlayerVitals : MonoBehaviour
    {
        [Tooltip("초당 회복량.")]
        public float regenPerSecond = 6f;

        [Tooltip("이 비율 아래로는 떨어지지 않는다.")]
        [Range(0f, 0.9f)]
        public float minRatio = 0.15f;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void Update()
        {
            if (_health == null)
                return;

            if (_health.HealthRatio < minRatio)
                _health.Heal((minRatio - _health.HealthRatio) * _health.MaxHealth);

            if (regenPerSecond > 0f && _health.HealthRatio < 1f)
                _health.Heal(regenPerSecond * Time.deltaTime);
        }
    }
}
