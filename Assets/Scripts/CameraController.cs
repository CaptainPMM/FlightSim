using UnityEngine;

public class CameraController : MonoBehaviour {
    [Header("Setup")]
    [SerializeField] private Transform _camAnchor;
    [SerializeField] private Camera _cam;
    [SerializeField] private Vector2 _startRot = new Vector2(22f, 0f);
    [SerializeField, Min(0f)] private float _startDist = 10f;
    [SerializeField, Min(0f)] private float _minDist = 0.1f;
    [SerializeField, Min(0f)] private float _maxDist = 100f;
    [SerializeField, Min(0f)] private float _movementSpeed = 1f;
    [SerializeField] private Vector3 _movementBounds = new Vector3(10f, 3f, 10f);

#if UNITY_EDITOR
    [Header("Setup/Gizmos")]
    [SerializeField] private bool _gizmoShowMovBounds = true;
#endif

    [Header("Runtime")]
    [SerializeField] private bool _lockCam = false;

    private void Awake() {
        if (!_camAnchor) Debug.LogWarning("CameraController: no cam anchor assigned");
        if (!_cam) Debug.LogWarning("CameraController: no cam assigned");
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        SetInitCamPos();
    }

    private void Update() {
        ReadCamInputs();
    }

    private void SetInitCamPos() {
        if (!_camAnchor || !_cam) return;
        _cam.transform.position = _camAnchor.position;
        _cam.transform.localPosition -= new Vector3(0f, 0f, ClampedDistance(_startDist));
        _camAnchor.localRotation = Quaternion.Euler(_startRot.x, _startRot.y, 0f);
    }

    private void ReadCamInputs() {
        if (Input.GetKeyDown(KeyCode.L)) _lockCam = !_lockCam;
        if (!Application.isFocused || _lockCam) return;

        _camAnchor.localRotation = Quaternion.Euler(_camAnchor.localEulerAngles.x + Input.GetAxis("CamVertical"), _camAnchor.localEulerAngles.y + Input.GetAxis("CamHorizontal"), 0f);

        Vector3 camPos = _cam.transform.localPosition;
        camPos.z = -ClampedDistance(-(camPos.z + Input.GetAxis("CamDistance")));
        _cam.transform.localPosition = camPos;

        Vector3 anchorPos = _camAnchor.localPosition;
        if (Input.GetKey(KeyCode.RightArrow)) anchorPos.x += _movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow)) anchorPos.x -= _movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.U)) anchorPos.y += _movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.J)) anchorPos.y -= _movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow)) anchorPos.z += _movementSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) anchorPos.z -= _movementSpeed * Time.deltaTime;
        anchorPos.x = Mathf.Clamp(anchorPos.x, -_movementBounds.x * 0.5f, _movementBounds.x * 0.5f);
        anchorPos.y = Mathf.Clamp(anchorPos.y, -_movementBounds.y * 0.5f, _movementBounds.y * 0.5f);
        anchorPos.z = Mathf.Clamp(anchorPos.z, -_movementBounds.z * 0.5f, _movementBounds.z * 0.5f);
        _camAnchor.localPosition = anchorPos;
    }

    private float ClampedDistance(float dist) {
        return Mathf.Clamp(dist, _minDist, _maxDist);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        SetInitCamPos();
    }

    private void OnDrawGizmosSelected() {
        if (!_camAnchor || !_gizmoShowMovBounds) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(_camAnchor.position, _movementBounds);
    }
#endif
}