using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using World;
using Planes;

namespace UI {
    [RequireComponent(typeof(Canvas))]
    public class UIController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField, Min(0f)] private float _UIUpdateInterval = 0.5f;
        [SerializeField] private PlaneController _plane;
        [SerializeField] private Text _time;
        [SerializeField] private Text _TAS;
        [SerializeField] private Text _ALT;
        [SerializeField] private Text _VS;
        [SerializeField] private Text _RPM;
        [SerializeField] private Slider _thrust;
        [SerializeField] private Slider _brake;

        [Header("Settings")]
        [SerializeField, Min(0)] private int _stallTAS = 47;
        [SerializeField, Min(0)] private int _warnTAS = 57;

        private Canvas _canvas = null;

        private void Awake() {
            _canvas = GetComponent<Canvas>();

            if (!_time) Debug.LogWarning("UIController: no time assigned");
            if (!_TAS) Debug.LogWarning("UIController: no TAS assigned");
            if (!_plane) Debug.LogWarning("UIController: no plane assigned");
            if (!_ALT) Debug.LogWarning("UIController: no ALT assigned");
            if (!_VS) Debug.LogWarning("UIController: no VS assigned");
            if (!_RPM) Debug.LogWarning("UIController: no RPM assigned");
            if (!_thrust) Debug.LogWarning("UIController: no thrust assigned");
            if (!_brake) Debug.LogWarning("UIController: no brake assigned");
        }

        private void Start() {
            StartCoroutine(UpdateUI());
        }

        private void Update() {
            ReadInputs();
        }

        private void ReadInputs() {
            if (Input.GetKeyDown(KeyCode.Alpha0)) _canvas.enabled = !_canvas.enabled;
        }

        private IEnumerator UpdateUI() {
            var interval = new WaitForSeconds(_UIUpdateInterval);
            while (true) {
                if (_canvas.enabled) {
                    _time.text = DayNightCycle.Main.GetTimeAsString() + (Time.timeScale != 1f ? "*" : "");

                    int TAS = Mathf.RoundToInt(_plane.TAS);
                    _TAS.text = TAS.ToString();
                    if (TAS <= _stallTAS) _TAS.color = Color.red;
                    else if (TAS <= _warnTAS) _TAS.color = Color.yellow;
                    else _TAS.color = Color.black;

                    _ALT.text = Mathf.RoundToInt(_plane.MSL).ToString();
                    _VS.text = Mathf.RoundToInt(_plane.VS).ToString();

                    PropController prop = _plane.GetProp(0);
                    _RPM.text = Mathf.RoundToInt(prop.RPM).ToString();
                    _thrust.value = Mathf.InverseLerp(0f, prop.MaxRPM, prop.TargetRPM);

                    _brake.value = Input.GetAxis("Brake");
                }
                yield return interval;
            }
        }
    }
}