using System;
using System.Collections.Generic;
using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 여러 AI 가 함께 보는 "공유 칠판(Blackboard)".
    ///
    /// Blackboard 가 왜 필요한가?
    /// - AI 하나하나가 각자 정보를 따로 들고 있으면 서로 협력할 수 없다.
    /// - 정보를 한 곳(칠판)에 적어두면, 모든 AI 가 같은 정보를 보고 함께 반응할 수 있다.
    ///
    /// 이 예제에서 칠판에 적히는 정보:
    ///   AlarmActive              : 경보가 켜졌는가
    ///   LastKnownPlayerPosition  : 마지막으로 확인된 Player 위치
    ///   DetectedCount            : 지금 Player 를 보고 있는 AI 의 수
    /// </summary>
    public class SquadBlackboard : MonoBehaviour
    {
        public static SquadBlackboard Instance { get; private set; }

        [Header("칠판에 적히는 값")]
        [SerializeField] private bool _alarmActive;
        [SerializeField] private Vector3 _lastKnownPlayerPosition;

        [Header("동작 설정")]
        [Tooltip("한 명이라도 Player 를 발견하면 자동으로 경보를 켠다. (Blackboard 실험용)")]
        public bool autoAlarmOnDetect = true;

        [Tooltip("마지막 발견 이후 경보가 유지되는 시간(초).")]
        public float alarmHoldSeconds = 4f;

        /// <summary>경보 상태가 바뀔 때 알림. UI 가 구독한다.</summary>
        public event Action<bool> OnAlarmChanged;

        private readonly List<AIContext> _members = new List<AIContext>();
        private float _alarmTimer;

        /// <summary>경보가 켜져 있는가. 칠판의 핵심 값.</summary>
        public bool AlarmActive => _alarmActive;

        /// <summary>마지막으로 확인된 Player 위치.</summary>
        public Vector3 LastKnownPlayerPosition => _lastKnownPlayerPosition;

        /// <summary>지금 Player 를 직접 보고 있는 AI 의 수.</summary>
        public int DetectedCount { get; private set; }

        /// <summary>칠판을 함께 보는 AI 목록.</summary>
        public IReadOnlyList<AIContext> Members => _members;

        private void Awake()
        {
            // 씬에 하나만 존재하는 공유 칠판.
            Instance = this;
            _alarmActive = false;
            _alarmTimer = 0f;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Register(AIContext member)
        {
            if (member != null && !_members.Contains(member))
                _members.Add(member);
        }

        public void Unregister(AIContext member)
        {
            _members.Remove(member);
        }

        private void Update()
        {
            // 1) 칠판을 보는 AI 중 누가 Player 를 직접 보고 있는지 센다.
            int detected = 0;
            for (int i = 0; i < _members.Count; i++)
            {
                AIContext m = _members[i];
                if (m == null || m.IsDead)
                    continue;

                if (m.HasDirectSightOfPlayer)
                {
                    detected++;
                    _lastKnownPlayerPosition = m.PlayerPosition;
                }
            }
            DetectedCount = detected;

            // 2) 한 명이라도 봤으면 경보를 켜고, 잠시 유지한다.
            if (autoAlarmOnDetect && detected > 0)
                RaiseAlarm();

            if (_alarmTimer > 0f)
            {
                _alarmTimer -= Time.deltaTime;
                if (_alarmTimer <= 0f)
                    SetAlarm(false);
            }
        }

        /// <summary>경보를 켠다(유지 시간 갱신).</summary>
        public void RaiseAlarm()
        {
            _alarmTimer = alarmHoldSeconds;
            SetAlarm(true);
        }

        /// <summary>경보를 즉시 끈다.</summary>
        public void ClearAlarm()
        {
            _alarmTimer = 0f;
            SetAlarm(false);
        }

        public void ToggleAlarm()
        {
            if (_alarmActive) ClearAlarm();
            else RaiseAlarm();
        }

        private void SetAlarm(bool value)
        {
            if (_alarmActive == value)
                return;

            _alarmActive = value;
            OnAlarmChanged?.Invoke(value);
        }

        public void ResetBoard()
        {
            _alarmTimer = 0f;
            DetectedCount = 0;
            SetAlarm(false);
        }
    }
}
