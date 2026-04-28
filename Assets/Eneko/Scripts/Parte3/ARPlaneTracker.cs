using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaneTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject gemPrefab;

    [Header("Gem Spawn Settings")]
    [SerializeField] private float gemSpawnHeight = 0.1f;
    [SerializeField] private float minPlaneSize = 0.5f; // Tamaño mínimo del plano para instanciar gemas

    private List<ARPlane> planosValidos = new List<ARPlane>();
    private bool gemasInstanciadas = false;
    private List<GameObject> gemasActivas = new List<GameObject>();

    void Awake()
    {
        if (planeManager == null)
            planeManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }

    void Start()
    {
        // Esperar a que GameManager esté listo
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IniciarJuego();
        }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Actualizar planos añadidos
        foreach (var plane in args.added)
        {
            if (EsPlanoValido(plane))
            {
                planosValidos.Add(plane);
            }
        }

        // Actualizar planos modificados
        foreach (var plane in args.updated)
        {
            if (EsPlanoValido(plane) && !planosValidos.Contains(plane))
            {
                planosValidos.Add(plane);
            }
        }

        // Remover planos eliminados
        foreach (var plane in args.removed)
        {
            planosValidos.Remove(plane);
        }

        // Actualizar contador de planos
        ActualizarContadorPlanos();

        // Verificar si podemos instanciar gemas
        VerificarInstanciarGemas();
    }

    bool EsPlanoValido(ARPlane plane)
    {
        // Verificar que el plano sea horizontal y tenga un tamaño mínimo
        if (plane.alignment != PlaneAlignment.HorizontalUp &&
            plane.alignment != PlaneAlignment.HorizontalDown)
            return false;

        // Verificar tamaño del plano
        Vector2 size = plane.size;
        return (size.x * size.y) >= minPlaneSize;
    }

    void ActualizarContadorPlanos()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActualizarPlanosDetectados(planosValidos.Count);
        }
    }

    void VerificarInstanciarGemas()
    {
        if (gemasInstanciadas) return;
        if (GameManager.Instance == null) return;

        int gemasNecesarias = GameManager.Instance.GetTotalGemas();
        int planosNecesarios = Mathf.CeilToInt(gemasNecesarias / 3f); // Al menos 1 plano por cada 3 gemas

        if (planosValidos.Count >= planosNecesarios)
        {
            InstanciarGemas();
        }
    }

    void InstanciarGemas()
    {
        if (gemPrefab == null)
        {
            Debug.LogError("Gem Prefab no asignado en ARPlaneTracker!");
            return;
        }

        int verticales = GameManager.Instance.GetGemasVerticales();
        int horizontales = GameManager.Instance.GetGemasHorizontales();

        // Seleccionar el plano más grande para instanciar las gemas
        ARPlane planoMasGrande = ObtenerPlanoMasGrande();

        if (planoMasGrande == null)
        {
            Debug.LogWarning("No hay planos suficientemente grandes para instanciar gemas");
            return;
        }

        // Calcular distribución de gemas
        Vector2 planeSize = planoMasGrande.size;
        float espacioX = planeSize.x / (horizontales + 1);
        float espacioZ = planeSize.y / (verticales + 1);

        Vector3 planeCenter = planoMasGrande.center;
        Quaternion planeRotation = planoMasGrande.transform.rotation;

        for (int v = 0; v < verticales; v++)
        {
            for (int h = 0; h < horizontales; h++)
            {
                // Calcular posición relativa al centro del plano
                float offsetX = (h + 1) * espacioX - planeSize.x / 2f;
                float offsetZ = (v + 1) * espacioZ - planeSize.y / 2f;

                Vector3 localPos = new Vector3(offsetX, gemSpawnHeight, offsetZ);
                Vector3 worldPos = planoMasGrande.transform.TransformPoint(localPos);

                // Instanciar gema
                GameObject gem = Instantiate(gemPrefab, worldPos, Quaternion.identity);
                gem.transform.SetParent(planoMasGrande.transform);
                gemasActivas.Add(gem);
            }
        }

        gemasInstanciadas = true;
        Debug.Log($"Gemas instanciadas: {gemasActivas.Count}");
    }

    ARPlane ObtenerPlanoMasGrande()
    {
        ARPlane planoMasGrande = null;
        float areaMaxima = 0f;

        foreach (var plane in planosValidos)
        {
            float area = plane.size.x * plane.size.y;
            if (area > areaMaxima)
            {
                areaMaxima = area;
                planoMasGrande = plane;
            }
        }

        return planoMasGrande;
    }

    // Método para limpiar gemas al reiniciar
    public void LimpiarGemas()
    {
        foreach (var gem in gemasActivas)
        {
            if (gem != null)
                Destroy(gem);
        }
        gemasActivas.Clear();
        gemasInstanciadas = false;
    }

    // Visualización de planos (opcional)
    public void MostrarPlanos(bool mostrar)
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(mostrar);
        }
    }
}