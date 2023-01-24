using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class GfxManager : MonoBehaviour {
    public static GfxManager Inst { get; private set; }

    [Header("Setup")]
    [SerializeField] private Volume _activeVolume = null;
    [SerializeField] private HDAdditionalCameraData _activeCam = null;

    [Header("Settings")]
    [SerializeField] private bool _enableWater = true;
    [SerializeField] private bool _enableClouds = true;
    [SerializeField] private HDAdditionalCameraData.AntialiasingMode _antiAliasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;

    public bool EnableWater {
        get => _enableWater;
        set {
            _enableWater = value;
            _activeVolume.profile.components.Find(vc => vc.GetType() == typeof(WaterRendering)).active = _enableWater;
        }
    }

    public bool EnableClouds {
        get => _enableClouds;
        set {
            _enableClouds = value;
            _activeVolume.profile.components.Find(vc => vc.GetType() == typeof(VolumetricClouds)).active = _enableClouds;
        }
    }

    public HDAdditionalCameraData.AntialiasingMode AntiAliasing {
        get => _antiAliasing;
        set {
            _antiAliasing = value;
            _activeCam.antialiasing = _antiAliasing;
        }
    }

    private void Awake() {
        Inst = this;
        if (!_activeVolume) Debug.LogWarning("GfxManager: no active volume assigned");
        if (!_activeCam) Debug.LogWarning("GfxManager: no active cam assigned");
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Time.timeScale *= 0.5f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) Time.timeScale *= 2f;

        if (Input.GetKeyDown(KeyCode.Alpha8)) EnableWater = !EnableWater;
        if (Input.GetKeyDown(KeyCode.Alpha9)) EnableClouds = !EnableClouds;
        if (Input.GetKeyDown(KeyCode.Y)) AntiAliasing = (HDAdditionalCameraData.AntialiasingMode)(((int)_antiAliasing + 1) % System.Enum.GetValues(typeof(HDAdditionalCameraData.AntialiasingMode)).Length);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (!_activeVolume) return;
        EnableWater = _enableWater;
        EnableClouds = _enableClouds;

        if (!_activeCam) return;
        AntiAliasing = _antiAliasing;
    }
#endif
}