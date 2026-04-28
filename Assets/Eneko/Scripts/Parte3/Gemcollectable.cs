using UnityEngine;

public class GemCollectable : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioClip collectSound;

    private Vector3 startPosition;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isCollected)
        {
            AnimateGem();
        }
    }

    void AnimateGem()
    {
        // Rotacion continua
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Movimiento de flotacion
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // Este metodo se llama cuando se toca la gema
    public void OnGemTouched()
    {
        if (isCollected) return;

        CollectGem();
    }

    void CollectGem()
    {
        isCollected = true;

        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecogerGema();
        }

        // Reproducir efecto de particulas
        if (collectEffect != null)
        {
            ParticleSystem effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        // Reproducir sonido
        if (collectSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(collectSound);
        }

        // Destruir la gema
        Destroy(gameObject);
    }

    // Deteccion por tap usando raycast desde el ARRaycastManager
    void OnMouseDown()
    {
        OnGemTouched();
    }
}