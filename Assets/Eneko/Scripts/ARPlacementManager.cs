using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using TMPro;

namespace UnityEngine.XR.ARFoundation.Samples
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARPlacementManager : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARPlaneManager planeManager;

        [Header("Prefabs to Instantiate")]
        [SerializeField] private List<GameObject> prefabList = new List<GameObject>();

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI planesCountText;
        [SerializeField] private TMP_Dropdown prefabDropdown;

        private ARRaycastManager raycastManager;
        private List<ARRaycastHit> hits = new List<ARRaycastHit>();
        private List<GameObject> instantiatedObjects = new List<GameObject>();
        private int selectedPrefabIndex = 0;

        // Input System
        private InputAction pressAction;

        void Awake()
        {
            raycastManager = GetComponent<ARRaycastManager>();

            // Configurar InputAction para detección de toques
            pressAction = new InputAction("touch", binding: "<Pointer>/press");
            pressAction.performed += OnPress;
        }

        void OnEnable()
        {
            pressAction.Enable();
        }

        void OnDisable()
        {
            pressAction.Disable();
        }

        void OnDestroy()
        {
            pressAction.Dispose();
        }

        void Start()
        {
            // Configurar dropdown con nombres de prefabs
            SetupDropdown();

            // Actualizar contador inicial
            UpdatePlanesCount();
        }

        void Update()
        {
            // Actualizar contador de planos en tiempo real
            UpdatePlanesCount();
        }

        // Callback cuando se presiona la pantalla
        void OnPress(InputAction.CallbackContext context)
        {
            if (Pointer.current == null) return;

            Vector2 touchPosition = Pointer.current.position.ReadValue();
            PlacePrefabOnPlane(touchPosition);
        }

        // Actualiza el texto con el numero de planos detectados
        void UpdatePlanesCount()
        {
            if (planesCountText != null && planeManager != null)
            {
                int planeCount = planeManager.trackables.count;
                planesCountText.text = $"NumPlanos = {planeCount}";
            }
        }

        // Configura el dropdown con los nombres de los prefabs
        void SetupDropdown()
        {
            if (prefabDropdown != null && prefabList.Count > 0)
            {
                prefabDropdown.ClearOptions();

                List<string> options = new List<string>();
                foreach (GameObject prefab in prefabList)
                {
                    options.Add(prefab != null ? prefab.name : "Sin nombre");
                }

                prefabDropdown.AddOptions(options);
                prefabDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
        }

        // Callback cuando cambia la seleccion del dropdown
        void OnDropdownValueChanged(int index)
        {
            selectedPrefabIndex = index;
        }

        // Colocar el prefab seleccionado en un plano detectado
        void PlacePrefabOnPlane(Vector2 touchPosition)
        {
            if (raycastManager == null || prefabList.Count == 0) return;

            // Realizar raycast desde la posición del toque
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                // Instanciar el prefab seleccionado
                GameObject selectedPrefab = prefabList[selectedPrefabIndex];
                if (selectedPrefab != null)
                {
                    GameObject instantiatedObject = Instantiate(selectedPrefab, hitPose.position, hitPose.rotation);
                    instantiatedObjects.Add(instantiatedObject);
                }
            }
        }

        // Elimina todos los prefabs instanciados
        public void ClearAllInstantiatedObjects()
        {
            foreach (GameObject obj in instantiatedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            instantiatedObjects.Clear();
        }
    }
}