using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float playTime = 60f;
    [SerializeField] private int gemasVerticales = 3;
    [SerializeField] private int gemasHorizontales = 3;
    [SerializeField] private string configSceneName = "MainParte3";
    [SerializeField] private string gameSceneName = "ARParte3";
    [SerializeField] private bool useOcclusion = false;

    [Header("Game State")]
    private int gemasRecogidas = 0;
    private int totalGemas;
    private float tiempoRestante;
    private bool juegoActivo = false;
    private int planosDetectados = 0;

    // Events para actualizar UI
    public event System.Action<float> OnTiempoActualizado;
    public event System.Action<int, int> OnGemaRecogida; // (recogidas, total)
    public event System.Action<bool> OnJuegoTerminado; // true = victoria, false = derrota
    public event System.Action<int> OnPlanoDetectado; // numero de planos

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

    void Start()
    {
        CalcularTotalGemas();
        tiempoRestante = playTime;
    }

    void Update()
    {
        if (juegoActivo && EstaEnEscenaDeJuego())
        {
            ActualizarTiempo();
        }
    }

    void CalcularTotalGemas()
    {
        totalGemas = gemasVerticales * gemasHorizontales;
    }

    bool EstaEnEscenaDeJuego()
    {
        return SceneManager.GetActiveScene().name == gameSceneName;
    }

    void ActualizarTiempo()
    {
        tiempoRestante -= Time.deltaTime;
        OnTiempoActualizado?.Invoke(tiempoRestante);

        if (tiempoRestante <= 0)
        {
            TerminarJuego(false); // Perdio por tiempo
        }
    }

    public void IniciarJuego()
    {
        juegoActivo = true;
        gemasRecogidas = 0;
        tiempoRestante = playTime;
        planosDetectados = 0;
        CalcularTotalGemas();
    }

    public void RecogerGema()
    {
        if (!juegoActivo) return;

        gemasRecogidas++;
        OnGemaRecogida?.Invoke(gemasRecogidas, totalGemas);

        // Reproducir sonido
        AudioManager.Instance?.PlayGemSound();

        if (gemasRecogidas >= totalGemas)
        {
            TerminarJuego(true); // Gano por objetivo
        }
    }

    public void ActualizarPlanosDetectados(int cantidad)
    {
        planosDetectados = cantidad;
        OnPlanoDetectado?.Invoke(planosDetectados);
    }

    void TerminarJuego(bool victoria)
    {
        juegoActivo = false;
        OnJuegoTerminado?.Invoke(victoria);

        // Reproducir sonido de victoria/derrota
        AudioManager.Instance?.PlayEndSound(victoria);
    }

    public void CargarEscenaConfiguracion()
    {
        SceneManager.LoadScene(configSceneName);
    }

    public void CargarEscenaJuego()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReiniciarJuego()
    {
        gemasRecogidas = 0;
        tiempoRestante = playTime;
        planosDetectados = 0;
        juegoActivo = false;
        CargarEscenaConfiguracion();
    }

    // Getters
    public float GetPlayTime() => playTime;
    public int GetGemasVerticales() => gemasVerticales;
    public int GetGemasHorizontales() => gemasHorizontales;
    public bool GetUseOcclusion() => useOcclusion;
    public int GetGemasRecogidas() => gemasRecogidas;
    public int GetTotalGemas() => totalGemas;
    public float GetTiempoRestante() => tiempoRestante;
    public bool EstaJuegoActivo() => juegoActivo;
    public int GetPlanosDetectados() => planosDetectados;

    // Setters (para la escena inicial)
    public void SetPlayTime(float tiempo)
    {
        playTime = Mathf.Max(10f, tiempo); // Mínimo 10 segundos
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