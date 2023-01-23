using UnityEngine;

namespace World {
    [RequireComponent(typeof(Light))]
    public class DayNightCycle : MonoBehaviour {
        public static DayNightCycle Main { get; private set; }

        private const int SECONDS_PER_DAY = 86400;
        private const float DEFAULT_START_TIME = 14.00f;

        private Light _light;

        [Header("Setup")]
        [SerializeField] private bool _mainTimeSource = true;
        [SerializeField, Range(0f, 90f)] private float _sunMaxHeight = 55f;
        [SerializeField, Range(0f, 23.99f)] private float _startTime = DEFAULT_START_TIME;
        [SerializeField, Min(0f), Tooltip("86400 (seconds) is real-time")] private int _dayDuration = SECONDS_PER_DAY;
        [SerializeField, Range(0f, 23.99f)] private float _enableSunLightTime = 05.00f;
        [SerializeField, Range(0f, 23.99f)] private float _disableSunLightTime = 19.00f;
        [SerializeField, Min(0f)] private float _dayDurModInputChange = 0.005f;

        [field: Header("Runtime")]
        [field: SerializeField, Range(0f, 23.99f)] public float CurrTime { get; set; } = DEFAULT_START_TIME;
        [field: SerializeField, Min(0.001f)] public float DayDurationModifier { get; set; } = 1f;

        private void Awake() {
            if (_mainTimeSource) Main = this;

            _light = GetComponent<Light>();
            if (!_light) Debug.LogWarning("DayNightCycle: no light found");

            Setup();
        }

        private void Setup() {
            CurrTime = _startTime;
            UpdateSunPos();
        }

        private void Update() {
            if (Input.GetKey(KeyCode.Alpha3)) DayDurationModifier += _dayDurModInputChange;
            if (Input.GetKey(KeyCode.Alpha4)) DayDurationModifier = Mathf.Max(0.001f, DayDurationModifier - _dayDurModInputChange);

            CurrTime = (CurrTime + (Time.deltaTime / (_dayDuration * DayDurationModifier) * 24f)) % 24f;

            if (!_light.enabled) {
                if (CurrTime > _enableSunLightTime && CurrTime < _disableSunLightTime) _light.enabled = true;
            } else if (CurrTime < _enableSunLightTime || CurrTime > _disableSunLightTime) _light.enabled = false;

            if (CurrTime > _enableSunLightTime && CurrTime < _disableSunLightTime) UpdateSunPos();
        }

        private void UpdateSunPos() {
            transform.rotation = Quaternion.Euler(_sunMaxHeight, 0f, 0f);
            transform.Rotate(0f, 360f * Mathf.InverseLerp(0f, 24f, CurrTime) + 180f, 0f, Space.Self);
        }

        public string GetTimeAsString() {
            string timeString = "";

            float hour = Mathf.FloorToInt(CurrTime);
            float minute = CurrTime - hour;

            timeString += hour.ToString("00") + ":";
            timeString += (Mathf.InverseLerp(0f, 1f, minute) * 60f).ToString("00");

            return timeString;
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