using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTouchHandler : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Camera arCamera;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        if (raycastManager == null)
            raycastManager = GetComponent<ARRaycastManager>();

        if (arCamera == null)
            arCamera = Camera.main;
    }

    void Update()
    {
        // Detectar toques en la pantalla
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                HandleTouch(touch.position);
            }
        }

        // Para testing en el editor con mouse
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
#endif
    }

    void HandleTouch(Vector2 screenPosition)
    {
        // Raycast desde la camara AR
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // Detectar objetos 3D (gemas)
        if (Physics.Raycast(ray, out hit))
        {
            // Verificar si toco una gema
            GemCollectable gem = hit.collider.GetComponent<GemCollectable>();
            if (gem != null)
            {
                gem.OnGemTouched();
                return;
            }
        }
    }
}