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

        // Hacer visibles los planos durante el escaneo
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(true);
        }
    }

    // Detiene el escaneo y genera las gemas en los planos encontrados
    public void DetenerYGenerarGemas()
    {
        if (!escaneando || gemasGeneradas) return;

        // Verificar que hay suficientes planos
        int planosNecesarios = GameManager.Instance.GetTotalGemas();
        if (planosValidos.Count < planosNecesarios)
        {
            Debug.LogWarning($"Planos insuficientes: {planosValidos.Count}/{planosNecesarios}");
            return;
        }

        escaneando = false;
        planeManager.enabled = false; // Congela deteccion actual

        // Ocultar los planos una vez generadas las gemas
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }

        GenerarGemasInstantaneo();    // Generacion sin espera
    }

    // Se ejecuta cuando se detectan, actualizan o eliminan planos
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (!escaneando || gemasGeneradas) return;

        // Procesar planos añadidos
        foreach (var p in args.added)
        {
            if (EsPlanoValido(p))
            {
                planosValidos.Add(p);
                p.gameObject.SetActive(true); // Mostrar plano en tiempo real
            }
        }

        // Procesar planos actualizados
        foreach (var p in args.updated)
        {
            if (EsPlanoValido(p) && !planosValidos.Contains(p))
            {
                planosValidos.Add(p);
                p.gameObject.SetActive(true);
            }
            else if (!EsPlanoValido(p) && planosValidos.Contains(p))
            {
                planosValidos.Remove(p);
            }
        }

        // Procesar planos eliminados
        foreach (var p in args.removed)
        {
            planosValidos.Remove(p);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActualizarPlanosDetectados(planosValidos.Count);
        }
    }

    // Verifica si un plano tiene la orientacion y la dimension adecuadas
    bool EsPlanoValido(ARPlane p)
    {
        bool horizontal = p.alignment == PlaneAlignment.HorizontalUp || p.alignment == PlaneAlignment.HorizontalDown;
        bool vertical = p.alignment == PlaneAlignment.Vertical;
        return (horizontal || vertical) && (p.size.x * p.size.y >= minPlaneArea);
    }

    // Coloca UNA gema por cada plano detectado
    void GenerarGemasInstantaneo()
    {
        if (gemasGeneradas || gemPrefab == null || planosValidos.Count == 0)
        {
            Debug.LogError("No se pueden generar gemas: falta prefab o no hay planos validos.");
            return;
        }

        gemasGeneradas = true; // Bloquea llamadas repetidas

        int totalGemas = GameManager.Instance.GetTotalGemas();

        // Instanciar UNA gema por plano (hasta alcanzar el total necesario)
        for (int i = 0; i < Mathf.Min(totalGemas, planosValidos.Count); i++)
        {
            ARPlane plano = planosValidos[i];
            Vector3 pos = CalcularPosicionCentralEnPlano(plano);

            GameObject gem = Instantiate(gemPrefab, pos, Quaternion.identity, gemsRoot);
            gemasActivas.Add(gem);
        }

        Debug.Log($"{gemasActivas.Count} gemas generadas (una por plano). Iniciando cronometro...");
        if (GameManager.Instance != null) GameManager.Instance.IniciarJuego();
    }

    // Calcula la posicion central de la gema en el plano
    Vector3 CalcularPosicionCentralEnPlano(ARPlane plane)
    {
        // Colocar la gema en el centro del plano
        return plane.transform.TransformPoint(new Vector3(0, gemSpawnHeight, 0));
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

    // Metodo publico para verificar si hay suficientes planos
    public bool TienePlanosMinimos()
    {
        return planosValidos.Count >= GameManager.Instance.GetTotalGemas();
    }

    // Obtener el numero de planos necesarios
    public int GetPlanosNecesarios()
    {
        return GameManager.Instance != null ? GameManager.Instance.GetTotalGemas() : 0;
    }
}