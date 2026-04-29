// Controla el comportamiento individual de cada gema
using UnityEngine;
using System.Collections;
public class GemCollectable : MonoBehaviour
{
    [Header("Config")]
    // Distancia maxima desde la camara para activar la recoleccion
    [SerializeField] private float collectDistance = 1.5f;
    // Duracion de la animacion antes de eliminar el objeto
    [SerializeField] private float collectDuration = 0.3f;

    private Camera arCamera;
    private bool isCollected = false;

    // Busca la camara principal al iniciar
    void Awake()
    {
        arCamera = Camera.main;
        if (arCamera == null) Debug.LogError("No se encontro Camera.main. Asigna manualmente la camara AR.");
    }

    // Comprueba continuamente la separacion entre la gema y la lente
    void Update()
    {
        if (isCollected || arCamera == null) return;

        float distance = Vector3.Distance(transform.position, arCamera.transform.position);
        if (distance <= collectDistance)
        {
            Collect();
        }
    }

    // Inicia el proceso de recoleccion cuando el jugador se acerca
    void Collect()
    {
        isCollected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.RecogerGema();

        StartCoroutine(CollectRoutine());
    }

    // Reduce el tamaño de la gema progresivamente y la elimina al terminar
    private IEnumerator CollectRoutine()
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;
        while (t < collectDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / collectDuration);
            yield return null;
        }
        Destroy(gameObject);
    }
}