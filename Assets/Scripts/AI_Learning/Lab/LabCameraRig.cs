using UnityEngine;

namespace AILearning
{
    /// <summary>
    /// 학습용 카메라.
    ///
    /// 화면 아래쪽 절반은 학습 패널(다이어그램 / 비교 / 자기진단)이 차지한다.
    /// 그래서 카메라를 지면보다 아래로 내려서, 실제 플레이 장면이
    /// 화면 위쪽에 오도록 맞춘다. 아래쪽 빈 공간이 패널 자리가 된다.
    /// </summary>
    public class LabCameraRig : MonoBehaviour
    {
        public Transform target;

        [Tooltip("카메라 Y 위치. 지면(-3)보다 아래로 두어 플레이 장면을 화면 위쪽에 올린다.")]
        public float cameraY = -4.73f;

        [Tooltip("따라가는 부드러움. 0 이면 즉시 따라간다.")]
        public float smooth = 0.12f;

        [Tooltip("카메라가 이동할 수 있는 X 범위.")]
        public float minX = LabLayout.CameraMinX;
        public float maxX = LabLayout.CameraMaxX;

        private Camera _cam;
        private float _velocity;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam != null)
            {
                _cam.orthographic = true;
                _cam.orthographicSize = 6f;
            }
        }

        private void Start()
        {
            if (target == null)
            {
                PlayerController pc = FindFirstObjectByType<PlayerController>();
                if (pc != null) target = pc.transform;
            }

            SnapToTarget();
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(target.position.x, minX, maxX);
            p.y = cameraY;
            transform.position = p;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            float wanted = Mathf.Clamp(target.position.x, minX, maxX);
            Vector3 p = transform.position;

            p.x = smooth <= 0f
                ? wanted
                : Mathf.SmoothDamp(p.x, wanted, ref _velocity, smooth);

            p.y = cameraY;
            transform.position = p;
        }
    }
}
