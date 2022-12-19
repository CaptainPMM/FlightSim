using UnityEngine;

namespace World {
    public class DayNightCycle : MonoBehaviour {
        private const int SECONDS_PER_DAY = 86400;
        private const float DEFAULT_START_TIME = 14.00f;

        [Header("Setup")]
        [SerializeField, Range(0f, 90f)] private float _sunMaxHeight = 55f;
        [SerializeField, Range(0f, 23.99f)] private float _startTime = DEFAULT_START_TIME;
        [SerializeField, Min(0f), Tooltip("86400 (seconds) is real-time")] private int _dayDuration = SECONDS_PER_DAY;

        [field: Header("Runtime")]
        [field: SerializeField, Range(0f, 23.99f)] public float CurrTime { get; set; } = DEFAULT_START_TIME;

        private void Awake() {
            Setup();
        }

        private void Setup() {
            CurrTime = _startTime;
            UpdateSunPos();
        }

        private void Update() {
            CurrTime = (CurrTime + (Time.deltaTime / _dayDuration * 24f)) % 24f;
            UpdateSunPos();
        }

        private void UpdateSunPos() {
            transform.rotation = Quaternion.Euler(_sunMaxHeight, 0f, 0f);
            transform.Rotate(0f, 360f * Mathf.InverseLerp(0f, 24f, CurrTime) + 180f, 0f, Space.Self);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying) Setup();
        }

        private void Reset() {
            Setup();
        }
#endif
    }
}