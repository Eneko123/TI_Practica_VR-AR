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
    [SerializeField] private Transform gemsRoot;

    [Header("Settings")]
    [SerializeField] private float gemSpawnHeight = 0.15f;
    [SerializeField] private float minPlaneArea = 0.5f;

    // Listas separadas por orientación
    private List<ARPlane> planosHorizontales = new List<ARPlane>();
    private List<ARPlane> planosVerticales = new List<ARPlane>();

    private bool escaneando = false;
    private bool gemasGeneradas = false;
    private List<GameObject> gemasActivas = new List<GameObject>();

    void Awake()
    {
        if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();
        if (gemsRoot == null) gemsRoot = new GameObject("GemsRoot").transform;
        planeManager.enabled = false;
    }

    void OnEnable() => planeManager.planesChanged += OnPlanesChanged;
    void OnDisable() => planeManager.planesChanged -= OnPlanesChanged;

    public void IniciarEscaneo()
    {
        if (escaneando || gemasGeneradas) return;
        Limpiar();
        escaneando = true;
        planeManager.enabled = true;

        foreach (var plane in planeManager.trackables) plane.gameObject.SetActive(true);
    }

    public void DetenerYGenerarGemas()
    {
        if (!escaneando || gemasGeneradas) return;

        if (!TienePlanosMinimos())
        {
            Debug.LogWarning($"Planos insuficientes. H: {planosHorizontales.Count}/{GetGemasHNecesarias()}, V: {planosVerticales.Count}/{GetGemasVNecesarias()}");
            return;
        }

        escaneando = false;
        planeManager.enabled = false;

        foreach (var plane in planeManager.trackables) plane.gameObject.SetActive(false);
        GenerarGemasInstantaneo();
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (!escaneando || gemasGeneradas) return;

        foreach (var p in args.added) ProcesarPlano(p, true);
        foreach (var p in args.updated) ProcesarPlano(p, true);
        foreach (var p in args.removed) ProcesarPlano(p, false);

        if (GameManager.Instance != null)
            GameManager.Instance.ActualizarPlanosDetectados(planosHorizontales.Count + planosVerticales.Count);
    }

    void ProcesarPlano(ARPlane p, bool añadir)
    {
        // Limpiamos primero para evitar duplicados y manejar cambios de orientación en tiempo real
        planosHorizontales.Remove(p);
        planosVerticales.Remove(p);

        if (!añadir) return;

        bool esValido = p.size.x * p.size.y >= minPlaneArea;
        bool esHorizontal = p.alignment == PlaneAlignment.HorizontalUp || p.alignment == PlaneAlignment.HorizontalDown;
        bool esVertical = p.alignment == PlaneAlignment.Vertical;

        if (esValido && esHorizontal)
        {
            planosHorizontales.Add(p);
            p.gameObject.SetActive(true);
        }
        else if (esValido && esVertical)
        {
            planosVerticales.Add(p);
            p.gameObject.SetActive(true);
        }
        else
        {
            p.gameObject.SetActive(false);
        }
    }

    void GenerarGemasInstantaneo()
    {
        if (gemasGeneradas || gemPrefab == null)
        {
            Debug.LogError("No se pueden generar gemas: falta prefab.");
            return;
        }

        gemasGeneradas = true;

        int hNeeded = GetGemasHNecesarias();
        int vNeeded = GetGemasVNecesarias();

        // Se instancia EXACTAMENTE la cantidad pedida, o la disponible si hay menos
        int hToSpawn = Mathf.Min(hNeeded, planosHorizontales.Count);
        int vToSpawn = Mathf.Min(vNeeded, planosVerticales.Count);

        for (int i = 0; i < hToSpawn; i++) InstanciarGema(planosHorizontales[i]);
        for (int i = 0; i < vToSpawn; i++) InstanciarGema(planosVerticales[i]);

        Debug.Log($"{gemasActivas.Count} gemas generadas (H: {hToSpawn}, V: {vToSpawn}). Iniciando cronómetro...");
        if (GameManager.Instance != null) GameManager.Instance.IniciarJuego();
    }

    void InstanciarGema(ARPlane plano)
    {
        Vector3 pos = plano.transform.TransformPoint(new Vector3(0, gemSpawnHeight, 0));
        GameObject gem = Instantiate(gemPrefab, pos, Quaternion.identity, gemsRoot);
        gemasActivas.Add(gem);
    }

    public bool TienePlanosMinimos()
    {
        return planosHorizontales.Count >= GetGemasHNecesarias() && planosVerticales.Count >= GetGemasVNecesarias();
    }

    public int GetPlanosNecesarios()
    {
        return GetGemasHNecesarias() + GetGemasVNecesarias();
    }

    // Métodos auxiliares que leen directamente del GameManager
    private int GetGemasHNecesarias() => GameManager.Instance != null ? GameManager.Instance.GetGemasHorizontales() : 0;
    private int GetGemasVNecesarias() => GameManager.Instance != null ? GameManager.Instance.GetGemasVerticales() : 0;

    void Limpiar()
    {
        foreach (var g in gemasActivas) if (g != null) Destroy(g);
        gemasActivas.Clear();
        planosHorizontales.Clear();
        planosVerticales.Clear();
        escaneando = false;
        gemasGeneradas = false;
    }
}