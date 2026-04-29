// Gestiona la deteccion de superficies en realidad aumentada y coloca las gemas
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaneTracker : MonoBehaviour
{
    [Header("References")]
    // Gestor nativo de deteccion de planos
    [SerializeField] private ARPlaneManager planeManager;
    // Objeto base que se instanciara por cada gema
    [SerializeField] private GameObject gemPrefab;
    // Contenedor para organizar las gemas creadas
    [SerializeField] private Transform gemsRoot;

    [Header("Settings")]
    // Altura fija sobre el plano donde apareceran las gemas
    [SerializeField] private float gemSpawnHeight = 0.15f;
    // Superficie minima que debe tener un plano para ser considerado util
    [SerializeField] private float minPlaneArea = 0.5f;

    // Lista de superficies que cumplen los requisitos de tamaño y orientacion
    private List<ARPlane> planosValidos = new List<ARPlane>();
    // Indica si el sistema esta buscando superficies activamente
    private bool escaneando = false;
    // Bloquea la generacion multiple en un mismo ciclo
    private bool gemasGeneradas = false;
    // Referencias a las gemas ya colocadas en la escena
    private List<GameObject> gemasActivas = new List<GameObject>();

    // Obtiene componentes necesarios y desactiva la deteccion al inicio
    void Awake()
    {
        if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();
        if (gemsRoot == null) gemsRoot = new GameObject("GemsRoot").transform;

        // Desactivado hasta que el usuario lo solicite
        planeManager.enabled = false;
    }

    // Se suscribe y se desuscribe de los eventos de cambio de planos
    void OnEnable() => planeManager.planesChanged += OnPlanesChanged;
    void OnDisable() => planeManager.planesChanged -= OnPlanesChanged;

    // Activa el escaneo de superficies para realidad aumentada
    public void IniciarEscaneo()
    {
        if (escaneando || gemasGeneradas) return;
        Limpiar();
        escaneando = true;
        planeManager.enabled = true;
    }

    // Detiene el escaneo y genera las gemas en los planos encontrados
    public void DetenerYGenerarGemas()
    {
        if (!escaneando || gemasGeneradas) return;
        escaneando = false;
        planeManager.enabled = false; // Congela deteccion actual
        GenerarGemasInstantaneo();    // Generacion sin espera
    }

    // Se ejecuta cuando se detectan, actualizan o eliminan planos
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (!escaneando || gemasGeneradas) return;

        foreach (var p in args.added) if (EsPlanoValido(p)) planosValidos.Add(p);
        foreach (var p in args.updated) if (EsPlanoValido(p) && !planosValidos.Contains(p)) planosValidos.Add(p);
        foreach (var p in args.removed) planosValidos.Remove(p);

        if (GameManager.Instance != null) GameManager.Instance.ActualizarPlanosDetectados(planosValidos.Count);
    }

    // Verifica si un plano tiene la orientacion y la dimension adecuadas
    bool EsPlanoValido(ARPlane p)
    {
        bool horizontal = p.alignment == PlaneAlignment.HorizontalUp || p.alignment == PlaneAlignment.HorizontalDown;
        bool vertical = p.alignment == PlaneAlignment.Vertical;
        return (horizontal || vertical) && (p.size.x * p.size.y >= minPlaneArea);
    }

    // Coloca las gemas en una rejilla sobre los planos detectados
    void GenerarGemasInstantaneo()
    {
        if (gemasGeneradas || gemPrefab == null || planosValidos.Count == 0)
        {
            Debug.LogError("No se pueden generar gemas: falta prefab o no hay planos validos.");
            return;
        }

        gemasGeneradas = true; // Bloquea llamadas repetidas

        int verticales = GameManager.Instance.GetGemasVerticales();
        int horizontales = GameManager.Instance.GetGemasHorizontales();
        int planoIndex = 0;

        // Bucle exacto: se ejecuta (V x H) veces en el mismo frame
        for (int v = 0; v < verticales; v++)
        {
            for (int h = 0; h < horizontales; h++)
            {
                ARPlane target = planosValidos[planoIndex % planosValidos.Count];
                Vector3 pos = CalcularPosicionEnPlano(target, v, h, verticales, horizontales);

                GameObject gem = Instantiate(gemPrefab, pos, Quaternion.identity, gemsRoot);
                gemasActivas.Add(gem);

                planoIndex++;
            }
        }

        Debug.Log($"{gemasActivas.Count} gemas generadas al instante. Iniciando cronometro...");
        if (GameManager.Instance != null) GameManager.Instance.IniciarJuego();
    }

    // Calcula la posicion exacta de cada gema dentro de un plano
    Vector3 CalcularPosicionEnPlano(ARPlane plane, int v, int h, int totalV, int totalH)
    {
        float espacioX = plane.size.x / (totalH + 1);
        float espacioZ = plane.size.y / (totalV + 1);
        float offsetX = (h + 1) * espacioX - plane.size.x / 2f;
        float offsetZ = (v + 1) * espacioZ - plane.size.y / 2f;
        return plane.transform.TransformPoint(new Vector3(offsetX, gemSpawnHeight, offsetZ));
    }

    // Elimina gemas anteriores y reinicia el estado del escaneo
    void Limpiar()
    {
        foreach (var g in gemasActivas) if (g != null) Destroy(g);
        gemasActivas.Clear();
        planosValidos.Clear();
        escaneando = false;
        gemasGeneradas = false;
    }
}