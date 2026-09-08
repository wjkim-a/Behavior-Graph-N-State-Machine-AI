using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 공격 실행 담당. "공격할지 말지" 는 판단하지 않는다.
    /// State Pattern 의 AttackState 와 Behavior Graph 의 Attack 노드가
    /// 똑같이 이 컴포넌트의 TryAttack() 을 호출한다.
    /// </summary>
    public class AIAttack : MonoBehaviour
    {
        [Tooltip("한 번 공격할 때 주는 피해량.")]
        public float damage = 10f;

        [Tooltip("공격 사이의 최소 간격(초).")]
        public float interval = 0.9f;

        [Tooltip("공격 순간 잠깐 보여줄 이펙트(옵션).")]
        public SpriteRenderer flashRenderer;

        private float _cooldownLeft;
        private float _flashLeft;

        /// <summary>지금 공격할 수 있는가.</summary>
        public bool IsReady => _cooldownLeft <= 0f;

        /// <summary>남은 쿨다운 비율 0~1. UI 표시용.</summary>
        public float CooldownRatio => interval <= 0f ? 0f : Mathf.Clamp01(_cooldownLeft / interval);

        /// <summary>지금까지 성공한 공격 횟수. 미션 판정에 사용.</summary>
        public int AttackCount { get; private set; }

        private void Update()
        {
            if (_cooldownLeft > 0f)
                _cooldownLeft -= Time.deltaTime;

            if (_flashLeft > 0f)
            {
                _flashLeft -= Time.deltaTime;
                if (_flashLeft <= 0f && flashRenderer != null)
                    flashRenderer.enabled = false;
            }
        }

        /// <summary>
        /// 쿨다운이 끝났으면 target 에게 피해를 준다.
        /// </summary>
        /// <returns>실제로 공격이 발생하면 true.</returns>
        public bool TryAttack(Health target)
        {
            if (!IsReady)
                return false;

            _cooldownLeft = interval;
            AttackCount++;

            if (flashRenderer != null)
            {
                flashRenderer.enabled = true;
                _flashLeft = 0.12f;
            }

            if (target != null)
                target.TakeDamage(damage);

            return true;
        }

        public void ResetAttack()
        {
            _cooldownLeft = 0f;
            AttackCount = 0;
            _flashLeft = 0f;
            if (flashRenderer != null)
                flashRenderer.enabled = false;
        }
    }
}
