// Controla la interfaz de usuario y la transicion entre pantallas
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    // Textos para cronometro, contador de gemas y progreso de escaneo
    [SerializeField] private TextMeshProUGUI tiempoText;
    [SerializeField] private TextMeshProUGUI gemasText;
    [SerializeField] private TextMeshProUGUI planosText;
    // Paneles que se muestran segun la fase del juego
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject panelJuego;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TextMeshProUGUI mensajeFinalText;
    // Botones de la pantalla principal y final
    [SerializeField] private Button crearButton;
    [SerializeField] private Button reiniciarButton;
    [SerializeField] private Button salirButton;

    [Header("Tracker Reference")]
    [SerializeField] private ARPlaneTracker planeTracker;

    [Header("Colors")]
    // Colores dinamicos para indicar urgencia en el tiempo
    [SerializeField] private Color tiempoNormalColor = Color.white;
    [SerializeField] private Color tiempoWarningColor = Color.yellow;
    [SerializeField] private Color tiempoUrgentColor = Color.red;
    // Colores para el estado de planos
    [SerializeField] private Color planosInsuficientesColor = Color.red;
    [SerializeField] private Color planosListosColor = Color.green;

    // Estados posibles de la pantalla principal
    private enum Estado { Esperando, Escaneando, Jugando }
    private Estado estadoActual = Estado.Esperando;

    // Conecta los eventos del gestor de juego con la interfaz
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTiempoActualizado += ActualizarTiempo;
            GameManager.Instance.OnGemaRecogida += ActualizarGemas;
            GameManager.Instance.OnPlanoDetectado += ActualizarPlanos;
            GameManager.Instance.OnJuegoTerminado += MostrarPanelFinal;
        }

        if (crearButton != null) crearButton.onClick.AddListener(OnCrearClick);
        if (reiniciarButton != null) reiniciarButton.onClick.AddListener(OnReiniciarClick);
        if (salirButton != null) salirButton.onClick.AddListener(OnSalirClick);

        MostrarPanelInicio();
    }

    // Muestra la pantalla de inicio y reinicia los textos
    void MostrarPanelInicio()
    {
        estadoActual = Estado.Esperando;
        panelInicio?.SetActive(true);
        panelJuego?.SetActive(false);
        panelFin?.SetActive(false);

        int totalGemas = GameManager.Instance?.GetTotalGemas() ?? 0;
        planosText.text = $"Pulsa INICIAR ESCANEO para detectar planos...\nPlanos necesarios: {totalGemas}";
        planosText.color = Color.white;
        gemasText.text = $"Gemas: 0/{totalGemas}";
        tiempoText.text = "00:00";
        ActualizarTextoBoton();
    }

    // Maneja el boton de crear: primer click escanea, segundo genera
    void OnCrearClick()
    {
        if (estadoActual == Estado.Esperando)
        {
            planeTracker?.IniciarEscaneo();
            estadoActual = Estado.Escaneando;
            int planosNecesarios = planeTracker?.GetPlanosNecesarios() ?? 0;
            planosText.text = $"Escaneando... Muévete para capturar planos.\nPlanos: 0/{planosNecesarios}";
            planosText.color = planosInsuficientesColor;
        }
        else if (estadoActual == Estado.Escaneando)
        {
            // Verificar que hay suficientes planos antes de generar
            if (planeTracker != null && planeTracker.TienePlanosMinimos())
            {
                planeTracker.DetenerYGenerarGemas();
                estadoActual = Estado.Jugando;
                panelInicio?.SetActive(false);
                panelJuego?.SetActive(true);
            }
            else
            {
                // Mostrar advertencia si no hay suficientes planos
                int planosActuales = GameManager.Instance?.GetPlanosDetectados() ?? 0;
                int planosNecesarios = planeTracker?.GetPlanosNecesarios() ?? 0;
                planosText.text = $"¡PLANOS INSUFICIENTES!\nPlanos: {planosActuales}/{planosNecesarios}\nSigue escaneando...";
                planosText.color = planosInsuficientesColor;

                AudioManager.Instance?.PlayUIClickSound();
            }
        }
        ActualizarTextoBoton();
    }

    // Cambia el texto del boton segun el estado actual
    void ActualizarTextoBoton()
    {
        if (crearButton == null) return;
        var txt = crearButton.GetComponentInChildren<TextMeshProUGUI>();
        if (txt == null) return;

        switch (estadoActual)
        {
            case Estado.Esperando:
                txt.text = "INICIAR ESCANEO";
                crearButton.interactable = true;
                break;

            case Estado.Escaneando:
                // Verificar si hay suficientes planos
                bool tieneSuficientes = planeTracker != null && planeTracker.TienePlanosMinimos();
                txt.text = tieneSuficientes ? "DETENER Y CREAR GEMAS" : "CREAR GEMAS (Planos insuficientes)";
                crearButton.interactable = true;
                break;

            case Estado.Jugando:
                txt.text = "EN JUEGO";
                crearButton.interactable = false;
                break;
        }
    }

    // Actualiza el cronometro y cambia su color segun el tiempo restante
    void ActualizarTiempo(float tiempoRestante)
    {
        if (tiempoText == null) return;
        int m = Mathf.FloorToInt(tiempoRestante / 60f);
        int s = Mathf.FloorToInt(tiempoRestante % 60f);
        tiempoText.text = $"{m:00}:{s:00}";

        if (tiempoRestante > 30f) tiempoText.color = tiempoNormalColor;
        else if (tiempoRestante > 10f) tiempoText.color = tiempoWarningColor;
        else tiempoText.color = tiempoUrgentColor;
    }

    // Actualiza los contadores de gemas y planos en pantalla
    void ActualizarGemas(int recogidas, int total) => gemasText.text = $"Gemas: {recogidas}/{total}";

    void ActualizarPlanos(int cantidad)
    {
        if (estadoActual == Estado.Escaneando)
        {
            int planosNecesarios = planeTracker?.GetPlanosNecesarios() ?? 0;
            planosText.text = $"Escaneando planos...\nPlanos: {cantidad}/{planosNecesarios}";

            // Cambiar color según si hay suficientes planos
            if (cantidad >= planosNecesarios)
            {
                planosText.color = planosListosColor;
                planosText.text = $"¡PLANOS LISTOS!\nPlanos: {cantidad}/{planosNecesarios}\nPulsa CREAR GEMAS para comenzar";
            }
            else
            {
                planosText.color = planosInsuficientesColor;
            }

            // Actualizar el texto del botón
            ActualizarTextoBoton();
        }
        else
        {
            planosText.text = $"Planos detectados: {cantidad}";
        }
    }

    // Muestra la pantalla final con el resultado de la partida
    void MostrarPanelFinal(bool victoria)
    {
        panelJuego?.SetActive(false);
        panelFin?.SetActive(true);

        if (mensajeFinalText != null)
        {
            mensajeFinalText.color = victoria ? Color.green : Color.red;
            mensajeFinalText.text = victoria
                ? $"¡VICTORIA!\n\nHas recogido todas las gemas\nGemas: {GameManager.Instance.GetGemasRecogidas()}/{GameManager.Instance.GetTotalGemas()}\nTiempo restante: {Mathf.FloorToInt(GameManager.Instance.GetTiempoRestante())}s"
                : $"TIEMPO AGOTADO\n\nGemas recogidas: {GameManager.Instance.GetGemasRecogidas()}/{GameManager.Instance.GetTotalGemas()}\n¡Inténtalo de nuevo!";
        }
    }

    // Reinicia el juego o cierra la aplicacion segun el boton
    void OnReiniciarClick() => GameManager.Instance?.ReiniciarJuego();
    void OnSalirClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Desconecta los eventos al destruir el objeto para evitar errores
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTiempoActualizado -= ActualizarTiempo;
            GameManager.Instance.OnGemaRecogida -= ActualizarGemas;
            GameManager.Instance.OnPlanoDetectado -= ActualizarPlanos;
            GameManager.Instance.OnJuegoTerminado -= MostrarPanelFinal;
        }
    }
}