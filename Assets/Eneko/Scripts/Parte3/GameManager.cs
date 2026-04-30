// Clase principal que controla el flujo completo del juego
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
public class GameManager : MonoBehaviour
{
    // Implementa un patron singleton para garantizar una unica instancia
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    // Tiempo total disponible para completar el objetivo
    [SerializeField] private float playTime = 60f;
    // Cantidad de gemas en el eje vertical de la rejilla
    [SerializeField] private int gemasVerticales = 3;
    // Cantidad de gemas en el eje horizontal de la rejilla
    [SerializeField] private int gemasHorizontales = 3;
    // Nombre de la escena de configuracion inicial
    [SerializeField] private string configSceneName = "MainParte3";
    // Nombre de la escena donde se desarrolla la partida
    [SerializeField] private string gameSceneName = "ARParte3";
    // Indica si se debe usar oclusion de planos en realidad aumentada
    [SerializeField] private bool useOcclusion = false;

    [Header("Game State")]
    // Contador de gemas que el jugador ha recogido
    private int gemasRecogidas = 0;
    // Total de gemas que se deben recoger para ganar
    private int totalGemas;
    // Tiempo que queda para terminar la partida
    private float tiempoRestante;
    // Indica si el cronometro esta corriendo
    private bool juegoActivo = false;
    // Numero de planos que el sistema de AR ha identificado
    private int planosDetectados = 0;

    // Eventos para comunicar cambios a la interfaz de usuario
    public event System.Action<float> OnTiempoActualizado;
    public event System.Action<int, int> OnGemaRecogida; // (recogidas, total)
    public event System.Action<bool> OnJuegoTerminado; // true = victoria, false = derrota
    public event System.Action<int> OnPlanoDetectado; // numero de planos

    // Inicializa la instancia unica y destruye duplicados si existen
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Calcula el total de gemas y establece el tiempo inicial
    void Start()
    {
        CalcularTotalGemas();
        tiempoRestante = playTime;
    }

    // Actualiza el cronometro cada frame mientras el juego esta activo
    void Update()
    {
        if (juegoActivo && EstaEnEscenaDeJuego())
        {
            ActualizarTiempo();
        }
    }

    // Suma las gemas verticales y horizontales para obtener el objetivo total
    // CORREGIDO: Ahora el total es la suma, no la multiplicación
    void CalcularTotalGemas()
    {
        totalGemas = gemasVerticales + gemasHorizontales;
    }

    // Verifica si el jugador se encuentra en la escena de juego
    bool EstaEnEscenaDeJuego()
    {
        return SceneManager.GetActiveScene().name == gameSceneName;
    }

    // Resta tiempo al cronometro y dispara la derrota si llega a cero
    void ActualizarTiempo()
    {
        tiempoRestante -= Time.deltaTime;
        OnTiempoActualizado?.Invoke(tiempoRestante);

        if (tiempoRestante <= 0)
        {
            TerminarJuego(false); // Perdio por tiempo
        }
    }

    // Reinicia las variables de estado y pone el cronometro en marcha
    public void IniciarJuego()
    {
        juegoActivo = true;
        gemasRecogidas = 0;
        tiempoRestante = playTime;
        planosDetectados = 0;
        CalcularTotalGemas();
    }

    // Suma una gema al contador y verifica si se alcanzo la victoria
    public void RecogerGema()
    {
        gemasRecogidas++;
        OnGemaRecogida?.Invoke(gemasRecogidas, totalGemas);
        Debug.Log($"Gema recogida: {gemasRecogidas}/{totalGemas}");

        if (gemasRecogidas >= totalGemas)
        {
            TerminarJuego(true);
        }
    }

    // Envoltorio que notifica a la interfaz sobre el progreso del escaneo
    public void ActualizarPlanosDetectados(int cantidad)
    {
        planosDetectados = cantidad;
        OnPlanoDetectado?.Invoke(planosDetectados);
    }

    // Detiene el cronometro y muestra el resultado final
    void TerminarJuego(bool victoria)
    {
        juegoActivo = false;
        OnJuegoTerminado?.Invoke(victoria);

        // Reproduce el audio correspondiente al resultado
        AudioManager.Instance?.PlayEndSound(victoria);
    }

    // Prepara el sistema de carga y vuelve al menu principal
    public void CargarEscenaConfiguracion()
    {
        LoaderUtility.Deinitialize();
        LoaderUtility.Initialize();
        SceneManager.LoadScene(configSceneName);
    }

    // Prepara el sistema de carga y entra a la partida
    public void CargarEscenaJuego()
    {
        LoaderUtility.Deinitialize();
        LoaderUtility.Initialize();
        SceneManager.LoadScene(gameSceneName);
    }

    // Limpia el estado actual y redirige al menu para empezar de nuevo
    public void ReiniciarJuego()
    {
        gemasRecogidas = 0;
        tiempoRestante = playTime;
        planosDetectados = 0;
        juegoActivo = false;
        CargarEscenaConfiguracion();
    }

    // Metodos publicos para leer valores de configuracion
    public float GetPlayTime() => playTime;
    public int GetGemasVerticales() => gemasVerticales;
    public int GetGemasHorizontales() => gemasHorizontales;
    public bool GetUseOcclusion() => useOcclusion;
    public int GetGemasRecogidas() => gemasRecogidas;
    public int GetTotalGemas() => totalGemas;
    public float GetTiempoRestante() => tiempoRestante;
    public bool EstaJuegoActivo() => juegoActivo;
    public int GetPlanosDetectados() => planosDetectados;

    // Metodos publicos para modificar la configuracion desde el menu
    public void SetPlayTime(float tiempo)
    {
        playTime = Mathf.Max(10f, tiempo); // Minimo diez segundos
        tiempoRestante = playTime;
    }

    public void SetGemasVerticales(int gemas)
    {
        gemasVerticales = Mathf.Max(1, gemas);
        CalcularTotalGemas();
    }

    public void SetGemasHorizontales(int gemas)
    {
        gemasHorizontales = Mathf.Max(1, gemas);
        CalcularTotalGemas();
    }

    public void SetUseOcclusion(bool value)
    {
        useOcclusion = value;
    }
}