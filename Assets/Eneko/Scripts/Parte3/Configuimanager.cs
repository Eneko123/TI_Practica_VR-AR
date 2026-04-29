// Gestiona la pantalla de configuracion inicial del juego
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ConfigUIManager : MonoBehaviour
{
    [Header("UI References")]
    // Campos de entrada para ajustar tiempo y cantidad de gemas
    [SerializeField] private TMP_InputField playTimeInput;
    [SerializeField] private TMP_InputField gemasVerticalesInput;
    [SerializeField] private TMP_InputField gemasHorizontalesInput;
    [SerializeField] private Toggle occlusionToggle;
    [SerializeField] private Button startButton;

    [Header("Default Values")]
    // Valores predeterminados por si no hay un gestor activo
    [SerializeField] private float defaultPlayTime = 60f;
    [SerializeField] private int defaultGemasVerticales = 3;
    [SerializeField] private int defaultGemasHorizontales = 3;

    // Carga los valores actuales o los predeterminados y asigna validaciones
    void Start()
    {
        if (GameManager.Instance != null)
        {
            playTimeInput.text = GameManager.Instance.GetPlayTime().ToString();
            gemasVerticalesInput.text = GameManager.Instance.GetGemasVerticales().ToString();
            gemasHorizontalesInput.text = GameManager.Instance.GetGemasHorizontales().ToString();
            occlusionToggle.isOn = GameManager.Instance.GetUseOcclusion();
        }
        else
        {
            playTimeInput.text = defaultPlayTime.ToString();
            gemasVerticalesInput.text = defaultGemasVerticales.ToString();
            gemasHorizontalesInput.text = defaultGemasHorizontales.ToString();
            occlusionToggle.isOn = false;
        }

        startButton.onClick.AddListener(OnStartButtonClicked);

        playTimeInput.onValueChanged.AddListener(ValidatePlayTime);
        gemasVerticalesInput.onValueChanged.AddListener(ValidateGemasVerticales);
        gemasHorizontalesInput.onValueChanged.AddListener(ValidateGemasHorizontales);
    }

    // Limita el tiempo de juego entre diez y trescientos segundos
    void ValidatePlayTime(string value)
    {
        if (float.TryParse(value, out float tiempo))
        {
            if (tiempo < 10f)
                playTimeInput.text = "10";
            else if (tiempo > 300f)
                playTimeInput.text = "300";
        }
    }

    // Limita el numero de gemas verticales entre uno y diez
    void ValidateGemasVerticales(string value)
    {
        if (int.TryParse(value, out int gemas))
        {
            if (gemas < 1)
                gemasVerticalesInput.text = "1";
            else if (gemas > 10)
                gemasVerticalesInput.text = "10";
        }
    }

    // Limita el numero de gemas horizontales entre uno y diez
    void ValidateGemasHorizontales(string value)
    {
        if (int.TryParse(value, out int gemas))
        {
            if (gemas < 1)
                gemasHorizontalesInput.text = "1";
            else if (gemas > 10)
                gemasHorizontalesInput.text = "10";
        }
    }

    // Valida la configuracion, guarda los datos y carga la escena de juego
    void OnStartButtonClicked()
    {
        if (!ValidarConfiguracion())
        {
            Debug.LogWarning("Configuracion invalida");
            return;
        }

        GuardarConfiguracion();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CargarEscenaJuego();
        }
    }

    // Verifica que todos los campos tengan valores validos antes de empezar
    bool ValidarConfiguracion()
    {
        if (!float.TryParse(playTimeInput.text, out float tiempo) || tiempo < 10f)
            return false;

        if (!int.TryParse(gemasVerticalesInput.text, out int verticales) || verticales < 1)
            return false;

        if (!int.TryParse(gemasHorizontalesInput.text, out int horizontales) || horizontales < 1)
            return false;

        return true;
    }

    // Guarda los valores del formulario en el gestor del juego
    void GuardarConfiguracion()
    {
        if (GameManager.Instance == null) return;

        float playTime = float.Parse(playTimeInput.text);
        int verticales = int.Parse(gemasVerticalesInput.text);
        int horizontales = int.Parse(gemasHorizontalesInput.text);
        bool occlusion = occlusionToggle.isOn;

        GameManager.Instance.SetPlayTime(playTime);
        GameManager.Instance.SetGemasVerticales(verticales);
        GameManager.Instance.SetGemasHorizontales(horizontales);
        GameManager.Instance.SetUseOcclusion(occlusion);
    }

    // Elimina las escucha del boton al destruir el objeto
    void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveAllListeners();
    }
}