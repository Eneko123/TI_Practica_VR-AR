using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfigUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField playTimeInput;
    [SerializeField] private TMP_InputField gemasVerticalesInput;
    [SerializeField] private TMP_InputField gemasHorizontalesInput;
    [SerializeField] private Toggle occlusionToggle;
    [SerializeField] private Button startButton;

    [Header("Default Values")]
    [SerializeField] private float defaultPlayTime = 60f;
    [SerializeField] private int defaultGemasVerticales = 3;
    [SerializeField] private int defaultGemasHorizontales = 3;

    void Start()
    {
        // Configurar valores por defecto
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

        // Configurar listeners
        startButton.onClick.AddListener(OnStartButtonClicked);

        // Validar inputs en tiempo real
        playTimeInput.onValueChanged.AddListener(ValidatePlayTime);
        gemasVerticalesInput.onValueChanged.AddListener(ValidateGemasVerticales);
        gemasHorizontalesInput.onValueChanged.AddListener(ValidateGemasHorizontales);
    }

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

    void OnStartButtonClicked()
    {
        // Validar y guardar configuración
        if (!ValidarConfiguracion())
        {
            Debug.LogWarning("Configuración invalida");
            return;
        }

        GuardarConfiguracion();

        // Cargar escena de juego
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CargarEscenaJuego();
        }
    }

    bool ValidarConfiguracion()
    {
        // Validar tiempo
        if (!float.TryParse(playTimeInput.text, out float tiempo) || tiempo < 10f)
            return false;

        // Validar gemas verticales
        if (!int.TryParse(gemasVerticalesInput.text, out int verticales) || verticales < 1)
            return false;

        // Validar gemas horizontales
        if (!int.TryParse(gemasHorizontalesInput.text, out int horizontales) || horizontales < 1)
            return false;

        return true;
    }

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

    void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveAllListeners();
    }
}