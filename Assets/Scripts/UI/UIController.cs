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
        [SerializeField, Min(0)] private int _stallIAS = 47;
        [SerializeField, Min(0)] private int _warnIAS = 57;
        [SerializeField] private Text _time;
        [SerializeField] private Text _IAS;
        [SerializeField] private Text _ALT;
        [SerializeField] private Text _VS;
        [SerializeField] private Text _RPM;
        [SerializeField] private Slider _thrust;
        [SerializeField] private Slider _brake;

        private Canvas _canvas = null;

        private void Awake() {
            _canvas = GetComponent<Canvas>();

            if (!_time) Debug.LogWarning("UIController: no time assigned");
            if (!_IAS) Debug.LogWarning("UIController: no ias assigned");
            if (!_plane) Debug.LogWarning("UIController: no plane assigned");
            if (!_ALT) Debug.LogWarning("UIController: no alt assigned");
            if (!_VS) Debug.LogWarning("UIController: no vs assigned");
            if (!_RPM) Debug.LogWarning("UIController: no rpm assigned");
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
                    _time.text = DayNightCycle.Main.GetTimeAsString();

                    int IAS = Mathf.RoundToInt(_plane.IAS);
                    _IAS.text = IAS.ToString();
                    if (IAS <= _stallIAS) _IAS.color = Color.red;
                    else if (IAS <= _warnIAS) _IAS.color = Color.yellow;
                    else _IAS.color = Color.black;

                    _ALT.text = Mathf.RoundToInt(_plane.ALT).ToString();
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