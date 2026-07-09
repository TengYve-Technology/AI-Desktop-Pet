// Assets/_Project/Scripts/Interaction/ClickDetector.cs

using UnityEngine;

public class ClickDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private LayerMask _clickableLayers = -1;
    [SerializeField] private float _maxRayDistance = 100f;
    [SerializeField] private bool _enableDebugLog = false;

    private bool _isPointerOverPet = false;
    private GameObject _currentHoveredObject = null;

    public event System.Action<GameObject, Vector3> OnPetClicked;
    public event System.Action<GameObject> OnPetHoverEnter;
    public event System.Action<GameObject> OnPetHoverExit;
    public event System.Action<GameObject> OnPetClickReleased;

    private void Start()
    {
        if (_targetCamera == null)
        {
            _targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        UpdatePointerOverPet();

        if (Input.GetMouseButtonDown(0))
        {
            if (_isPointerOverPet && _currentHoveredObject != null)
            {
                OnPetClicked?.Invoke(_currentHoveredObject, Input.mousePosition);
                if (_enableDebugLog)
                {
                    Debug.Log($"[ClickDetector] Pet clicked: {_currentHoveredObject.name}");
                }
            }
            else
            {
                if (_enableDebugLog)
                {
                    Debug.Log("[ClickDetector] Clicked empty area");
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_isPointerOverPet && _currentHoveredObject != null)
            {
                OnPetClickReleased?.Invoke(_currentHoveredObject);
            }
        }
    }

    private void UpdatePointerOverPet()
    {
        if (!IsMouseInWindow())
        {
            if (_isPointerOverPet)
            {
                _isPointerOverPet = false;
                OnPetHoverExit?.Invoke(_currentHoveredObject);
                _currentHoveredObject = null;
            }
            return;
        }

        Ray ray = _targetCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _maxRayDistance, _clickableLayers))
        {
            if (!_isPointerOverPet || _currentHoveredObject != hit.collider.gameObject)
            {
                if (_isPointerOverPet)
                {
                    OnPetHoverExit?.Invoke(_currentHoveredObject);
                }
                _isPointerOverPet = true;
                _currentHoveredObject = hit.collider.gameObject;
                OnPetHoverEnter?.Invoke(_currentHoveredObject);
            }
        }
        else
        {
            if (_isPointerOverPet)
            {
                _isPointerOverPet = false;
                OnPetHoverExit?.Invoke(_currentHoveredObject);
                _currentHoveredObject = null;
            }
        }
    }

    private bool IsMouseInWindow()
    {
        Rect windowRect = new Rect(0, 0, Screen.width, Screen.height);
        return windowRect.Contains(Input.mousePosition);
    }

    public bool IsPointerOverPet()
    {
        return _isPointerOverPet;
    }

    public GameObject GetCurrentHoveredObject()
    {
        return _currentHoveredObject;
    }
}