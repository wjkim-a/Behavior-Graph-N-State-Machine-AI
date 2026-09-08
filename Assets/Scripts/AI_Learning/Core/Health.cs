using System;
using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 체력 시스템.
    /// 기존 프로젝트에 Health / TakeDamage / Damage 관련 코드가 전혀 없었기 때문에 새로 구현했다.
    /// State Pattern 과 Behavior Graph 가 "같은" 컴포넌트를 읽으므로 HP 데이터는 한 곳에만 존재한다.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;

        private float _currentHealth;
        private bool _initialized;

        /// <summary>피격 시 호출. 인자는 (남은 체력, 받은 피해량).</summary>
        public event Action<float, float> OnDamaged;

        /// <summary>체력이 0 이하가 되는 순간 1회 호출.</summary>
        public event Action OnDied;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;

        /// <summary>0 ~ 1 사이의 체력 비율. Flee 조건 판정에 사용한다.</summary>
        public float HealthRatio => _maxHealth <= 0f ? 0f : Mathf.Clamp01(_currentHealth / _maxHealth);

        public bool IsDead => _currentHealth <= 0f;

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Awake 순서에 의존하지 않도록, 값을 읽는 쪽에서도 안전하게 초기화될 수 있게 한다.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            _currentHealth = _maxHealth;
            _initialized = true;
        }

        public void TakeDamage(float amount)
        {
            EnsureInitialized();

            if (amount <= 0f || IsDead)
                return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            OnDamaged?.Invoke(_currentHealth, amount);

            if (_currentHealth <= 0f)
                OnDied?.Invoke();
        }

        public void Heal(float amount)
        {
            EnsureInitialized();

            if (amount <= 0f || IsDead)
                return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        }

        /// <summary>테스트 반복을 위한 초기화. AITestHarness 에서 사용한다.</summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _initialized = true;
        }
    }
}
