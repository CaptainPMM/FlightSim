using System.Collections.Generic;
using UnityEngine;
using World.Lights;
using Utils;

namespace Planes {
    [RequireComponent(typeof(Rigidbody), typeof(CGDrawer), typeof(CLDrawer))]
    public class PlaneController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField] private List<ControlSurfaceController> _controlSurfaces = new();
        [SerializeField] private List<PropController> _props = new();
        [SerializeField] private List<WheelController> _wheels = new();
        [SerializeField] private List<LightsController> _lights = new();

        public Vector3 Velocity => _rb.velocity;
        public float TAS => _rb.velocity.magnitude * 1.94384f; // m/s to knots conversion
        public float MSL => transform.position.y * 3.28084f; // m to feet conversion
        public float VS => _rb.velocity.y * 3.28084f * 60f; // m to feet/min conversion

        private Rigidbody _rb;

        private Dictionary<ControlSurfaceType, List<ControlSurfaceController>> _controlSurfacesByType;
        private Dictionary<WheelFunction, List<WheelController>> _wheelsByFunction;
        private Dictionary<World.Lights.LightType, LightsController> _lightsByType;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();

            // Init dictionaries
            _controlSurfacesByType = new();
            foreach (ControlSurfaceController cs in _controlSurfaces) {
                if (!_controlSurfacesByType.ContainsKey(cs.Type)) _controlSurfacesByType.Add(cs.Type, new());
                _controlSurfacesByType[cs.Type].Add(cs);
            }

            _wheelsByFunction = new();
            foreach (WheelController wc in _wheels) {
                foreach (WheelFunction wf in System.Enum.GetValues(typeof(WheelFunction))) {
                    if ((wc.WheelFunction & wf) > 0) {
                        if (!_wheelsByFunction.ContainsKey(wf)) _wheelsByFunction.Add(wf, new());
                        _wheelsByFunction[wf].Add(wc);
                    }
                }
            }

            _lightsByType = new();
            foreach (LightsController lc in _lights) {
                if (_lightsByType.TryAdd(lc.LightType, lc) == false) Debug.LogWarning("PlanceController: duplicate light type found: " + lc.LightType);
            }
        }

        private void Update() {
            ReadInputs();
        }

        private void ReadInputs() {
            ReadControlSurfaceInputs();
            ReadPropInputs();
            ReadWheelInputs();
            ReadLightInputs();
        }

        private void ReadControlSurfaceInputs() {
            _controlSurfacesByType[ControlSurfaceType.Aileron].ForEach(cs => cs.AxisLerpDeflection(Input.GetAxis("Roll")));
            _controlSurfacesByType[ControlSurfaceType.Elevator].ForEach(cs => cs.AxisLerpDeflection(Input.GetAxis("Pitch")));
            _controlSurfacesByType[ControlSurfaceType.Rudder].ForEach(cs => cs.AxisLerpDeflection(Input.GetAxis("Yaw")));

            // Trim inputs
            if (Input.GetKey(KeyCode.Q)) _controlSurfacesByType[ControlSurfaceType.Aileron].ForEach(cs => cs.Trim -= cs.TrimSpeed);
            if (Input.GetKey(KeyCode.E)) _controlSurfacesByType[ControlSurfaceType.Aileron].ForEach(cs => cs.Trim += cs.TrimSpeed);
            if (Input.GetKey(KeyCode.R)) _controlSurfacesByType[ControlSurfaceType.Elevator].ForEach(cs => cs.Trim -= cs.TrimSpeed);
            if (Input.GetKey(KeyCode.F)) _controlSurfacesByType[ControlSurfaceType.Elevator].ForEach(cs => cs.Trim += cs.TrimSpeed);
            if (Input.GetKey(KeyCode.K)) _controlSurfacesByType[ControlSurfaceType.Rudder].ForEach(cs => cs.Trim += cs.TrimSpeed);
            if (Input.GetKey(KeyCode.L)) _controlSurfacesByType[ControlSurfaceType.Rudder].ForEach(cs => cs.Trim -= cs.TrimSpeed);
        }

        private void ReadPropInputs() {
            if (Input.GetKeyDown(KeyCode.P)) {
                _props.ForEach(p => {
                    if (p.EngineOn) p.StopEngine();
                    else p.StartEngine();
                });
            } else {
                _props.ForEach(p => {
                    if (p.EngineOn) p.TargetRPM += (int)(Input.GetAxis("Thrust") * p.MaxRPM);
                });
            }
        }

        private void ReadWheelInputs() {
            _wheelsByFunction[WheelFunction.Brake].ForEach(wc => wc.Brake = Input.GetAxis("Brake") * wc.MaxBrake);
            _wheelsByFunction[WheelFunction.Steer].ForEach(wc => wc.SteerAngle = Input.GetAxis("Yaw") * wc.MaxSteerAngle);
            _wheelsByFunction[WheelFunction.Torque].ForEach(wc => wc.Torque += Input.GetAxis("WheelTorque") * wc.MaxTorque);
        }

        private void ReadLightInputs() {
            if (Input.GetKeyDown(KeyCode.N)) _lightsByType[World.Lights.LightType.Nav].Toggle();
            if (Input.GetKeyDown(KeyCode.B)) _lightsByType[World.Lights.LightType.Beacon].Toggle();
            if (Input.GetKeyDown(KeyCode.V)) _lightsByType[World.Lights.LightType.Strobe].Toggle();
            if (Input.GetKeyDown(KeyCode.X)) _lightsByType[World.Lights.LightType.Taxi].Toggle();
            if (Input.GetKeyDown(KeyCode.C)) _lightsByType[World.Lights.LightType.Landing].Toggle();
        }

        public PropController GetProp(int propIndex) {
            return _props[propIndex];
        }
    }
}