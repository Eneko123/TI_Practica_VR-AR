using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI tiempoText;
    [SerializeField] private TextMeshProUGUI gemasText;
    [SerializeField] private TextMeshProUGUI planosText;
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject panelJuego;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TextMeshProUGUI mensajeFinalText;
    [SerializeField] private Button reiniciarButton;
    [SerializeField] private Button salirButton;

    [Header("Colors")]
    [SerializeField] private Color tiempoNormalColor = Color.white;
    [SerializeField] private Color tiempoWarningColor = Color.yellow;
    [SerializeField] private Color tiempoUrgentColor = Color.red;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            // Suscribirse a eventos
            GameManager.Instance.OnTiempoActualizado += ActualizarTiempo;
            GameManager.Instance.OnGemaRecogida += ActualizarGemas;
            GameManager.Instance.OnPlanoDetectado += ActualizarPlanos;
            GameManager.Instance.OnJuegoTerminado += MostrarPanelFinal;
        }

        // Configurar botones
        if (reiniciarButton != null)
            reiniciarButton.onClick.AddListener(OnReiniciarClick);

        if (salirButton != null)
            salirButton.onClick.AddListener(OnSalirClick);

        // Mostrar panel de inicio
        MostrarPanelInicio();
    }

    void MostrarPanelInicio()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (panelJuego != null) panelJuego.SetActive(false);
        if (panelFin != null) panelFin.SetActive(false);

        // Mostrar instrucciones de escaneo
        if (planosText != null)
            planosText.text = "Escanea el entorno para detectar planos...";
    }

    void OnEnable()
    {
        // Despues de unos segundos, ocultar el panel de inicio
        Invoke(nameof(OcultarPanelInicio), 3f);
    }

    void OcultarPanelInicio()
    {
        if (panelInicio != null) panelInicio.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(true);

        // Inicializar UI
        ActualizarTiempo(GameManager.Instance.GetPlayTime());
        ActualizarGemas(0, GameManager.Instance.GetTotalGemas());
        ActualizarPlanos(0);
    }

    void ActualizarTiempo(float tiempoRestante)
    {
        if (tiempoText == null) return;

        int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60f);

        tiempoText.text = $"{minutos:00}:{segundos:00}";

        // Cambiar color segun el tiempo restante
        if (tiempoRestante > 30f)
            tiempoText.color = tiempoNormalColor;
        else if (tiempoRestante > 10f)
            tiempoText.color = tiempoWarningColor;
        else
            tiempoText.color = tiempoUrgentColor;
    }

    void ActualizarGemas(int recogidas, int total)
    {
        if (gemasText == null) return;
        gemasText.text = $"Gemas: {recogidas}/{total}";
    }

    void ActualizarPlanos(int cantidad)
    {
        if (planosText == null) return;
        planosText.text = $"Planos detectados: {cantidad}";
    }

    void MostrarPanelFinal(bool victoria)
    {
        if (panelJuego != null) panelJuego.SetActive(false);
        if (panelFin != null) panelFin.SetActive(true);

        if (mensajeFinalText != null)
        {
            if (victoria)
            {
                mensajeFinalText.text = $"¡VICTORIA!\n\nHas recogido todas las gemas\n" +
                                      $"Gemas: {GameManager.Instance.GetGemasRecogidas()}/{GameManager.Instance.GetTotalGemas()}\n" +
                                      $"Tiempo restante: {Mathf.FloorToInt(GameManager.Instance.GetTiempoRestante())}s";
                mensajeFinalText.color = Color.green;
            }
            else
            {
                mensajeFinalText.text = $"TIEMPO AGOTADO\n\nGemas recogidas: {GameManager.Instance.GetGemasRecogidas()}/{GameManager.Instance.GetTotalGemas()}\n" +
                                      "¡Inténtalo de nuevo!";
                mensajeFinalText.color = Color.red;
            }
        }
    }

    void OnReiniciarClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReiniciarJuego();
        }
    }

    void OnSalirClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        // Desuscribirse de eventos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTiempoActualizado -= ActualizarTiempo;
            GameManager.Instance.OnGemaRecogida -= ActualizarGemas;
            GameManager.Instance.OnPlanoDetectado -= ActualizarPlanos;
            GameManager.Instance.OnJuegoTerminado -= MostrarPanelFinal;
        }

        // Limpiar listeners de botones
        if (reiniciarButton != null)
            reiniciarButton.onClick.RemoveAllListeners();

        if (salirButton != null)
            salirButton.onClick.RemoveAllListeners();
    }
}