using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ReturnMainScenene : MonoBehaviour
{
    [SerializeField] private Button returnButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        returnButton.onClick.AddListener(() => {
            SceneManager.LoadScene("MainScene");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
