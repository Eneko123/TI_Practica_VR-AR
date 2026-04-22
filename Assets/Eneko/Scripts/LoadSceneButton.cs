using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneButton : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private string sceneName;
    [SerializeField] private GameObject canvasPanel;
    [SerializeField] private Button loadSelectedSceneButton;
    [SerializeField] private Button loadMainSceneButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        loadSelectedSceneButton.onClick.AddListener(LoadSelectedScene);

        if (loadMainSceneButton != null)
            loadMainSceneButton.onClick.AddListener(LoadMainScene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadSelectedScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    private void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            canvasPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            canvasPanel.SetActive(false);
        }
    }
}
