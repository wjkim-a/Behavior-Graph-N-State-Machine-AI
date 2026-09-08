using System.Collections.Generic;
using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// Enter / Execute / Exit 의 흐름을 눈으로 보기 위한 로그.
    ///
    /// 학습 포인트:
    ///   ENTER   : 상태에 처음 들어올 때 1회
    ///   EXECUTE : 상태가 유지되는 동안 계속 반복
    ///   EXIT    : 상태를 빠져나갈 때 1회
    ///
    /// EXECUTE 는 매 프레임 실행되므로 그대로 다 찍으면 읽을 수 없다.
    /// 그래서 화면 표시용으로는 일정 간격마다 한 줄씩만 남긴다.
    /// </summary>
    public class AIEventLog : MonoBehaviour
    {
        [Tooltip("보관할 최대 줄 수.")]
        public int capacity = 14;

        [Tooltip("EXECUTE 로그를 남기는 간격(초).")]
        public float executeLogInterval = 0.45f;

        private readonly List<string> _lines = new List<string>();
        private float _nextExecuteLogTime;
        private string _lastExecuteLabel;
        private int _executeRepeatCount;
        private int _executeLineIndex = -1;

        public IReadOnlyList<string> Lines => _lines;

        public void LogEnter(string label)
        {
            ResetExecuteRun();
            Add(label + "  ENTER");
            _nextExecuteLogTime = Time.time + executeLogInterval;
            _lastExecuteLabel = label;
        }

        /// <summary>
        /// EXECUTE 는 매 프레임 실행되므로 줄을 계속 추가하면 ENTER / EXIT 가 밀려 사라진다.
        /// 그래서 같은 상태가 이어지는 동안에는 한 줄을 반복 횟수로 갱신한다.
        ///   PATROL  EXECUTE  x 128
        /// 이렇게 하면 "EXECUTE 는 계속 반복된다" 는 사실도 보이고,
        /// ENTER / EXIT 줄도 화면에 남는다.
        /// </summary>
        public void LogExecute(string label)
        {
            if (Time.time < _nextExecuteLogTime && _lastExecuteLabel == label)
                return;

            _nextExecuteLogTime = Time.time + executeLogInterval;

            bool sameRun = _lastExecuteLabel == label && _executeLineIndex >= 0 &&
                           _executeLineIndex < _lines.Count;

            _lastExecuteLabel = label;

            if (sameRun)
            {
                _executeRepeatCount++;
                _lines[_executeLineIndex] = label + "  EXECUTE  x " + _executeRepeatCount;
                return;
            }

            _executeRepeatCount = 1;
            Add(label + "  EXECUTE  x 1");
            _executeLineIndex = _lines.Count - 1;
        }

        private void ResetExecuteRun()
        {
            _executeRepeatCount = 0;
            _executeLineIndex = -1;
        }

        public void LogExit(string label)
        {
            ResetExecuteRun();
            Add(label + "  EXIT");
        }

        public void LogRaw(string line)
        {
            ResetExecuteRun();
            Add(line);
        }

        private void Add(string line)
        {
            _lines.Add(line);
            while (_lines.Count > Mathf.Max(2, capacity))
            {
                _lines.RemoveAt(0);
                if (_executeLineIndex >= 0)
                    _executeLineIndex--;
            }
        }

        public void Clear()
        {
            _lines.Clear();
            _nextExecuteLogTime = 0f;
            _lastExecuteLabel = null;
            ResetExecuteRun();
        }
    }
}
