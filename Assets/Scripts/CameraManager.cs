using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cursor = UnityEngine.Cursor;
using Toggle = UnityEngine.UI.Toggle;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    [Header("Input Actions")]
    public InputAction mousePosition;
    public InputAction manualModeToggler;
    public InputAction zoomControls;

    [Header("References")]
    public Toggle manualModeIndicator;
    public TMP_Text sizeLabel;
    public GameObject manulModeTutorial;
    private Camera _camera;
    public Volume volume;
    private Bloom _bloom;

    [Header("Parameteres")]
    public float manualZoomModifier = 10;
    public float moveModifier;
    public bool useVisualEffects;

    private Vector3 _delta;

    private bool _manualMode;
    private float _zoomModifier = 1.0f;
    private float _manulZoomValue = 100;
    private Vector3 _positionVel;
    private float _sizeVel;
    private float _smoothTime = 1.0f;
    private float _manualSmoothTime = 0.5f;



    public Camera GetCamera()
    {
        return _camera;
    }

    private void Awake()
    {
        CameraManager.instance = this;
        mousePosition.Enable();
        manualModeToggler.Enable();
        zoomControls.Enable();
        _camera = GetComponent<Camera>();

        volume.profile.TryGet<Bloom>(out _bloom);
    }

    private void OnDisable()
    {
        mousePosition.Disable();
        manualModeToggler.Disable();
        zoomControls.Disable();
    }

    public void UpdateZoomModifier(float newValue)
    {
        _zoomModifier = 2 - newValue;
    }

    public void ToggleManualMode(bool state)
    {
        _manualMode = state;
        Cursor.visible = !_manualMode;
        if (_manualMode)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        manualModeIndicator.isOn = _manualMode;
        manulModeTutorial.SetActive(_manualMode);
    }

    void Update()
    {
        if (!BodyManager.instance.IsSimulationRunning())
        {
            return;
        }
        if (manualModeToggler.triggered)
        {
            ToggleManualMode(!_manualMode);
        }
        if (_manualMode)
        {
            HandleCameraManually();
        }
        else
        {
            HandleCameraAutomatically();
        }

        sizeLabel.text = _camera.orthographicSize.ToString(CultureInfo.InvariantCulture);
    }

    public void ToggleVisualEffects()
    {
        useVisualEffects = !useVisualEffects;
        _bloom.active = useVisualEffects;
    }

    private void HandleCameraManually()
    {
        _delta = -mousePosition.ReadValue<Vector2>();
        var offset = _delta * (_camera.orthographicSize * moveModifier);
        var cameraPos = _camera.transform.localPosition;
        var targetCamPos = cameraPos + offset;
        _camera.transform.localPosition = Vector3.SmoothDamp(cameraPos, targetCamPos, ref _positionVel, _smoothTime);
        if (zoomControls.triggered)
        {
            _manulZoomValue += manualZoomModifier * zoomControls.ReadValue<float>();
            if (_manulZoomValue < 32.0f)
            {
                _manulZoomValue = 32.0f;
            }
        }

        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _manulZoomValue,
            ref _sizeVel, _manualSmoothTime);
    }

    private void HandleCameraAutomatically()
    {
        Vector3 center = new Vector3();
        foreach (var body in BodyManager.instance.bodies)
        {
            center += body.position;
        }

        center /= BodyManager.instance.bodies.Count;
        float maxDistanceFromCenter = 0;
        foreach (var body in BodyManager.instance.bodies)
        {
            maxDistanceFromCenter = Mathf.Max(maxDistanceFromCenter, Vector3.Distance(body.position, center));
        }

        center.z = -BodyManager.instance.simulationParameters.sizeModifier - 1;
        transform.position = Vector3.SmoothDamp(transform.position, center, ref _positionVel, _smoothTime);


        float targetSize = maxDistanceFromCenter * _zoomModifier;
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, targetSize,
            ref _sizeVel, _smoothTime);
    }

    private void CalculateLensDistortion()
    {

    }
}