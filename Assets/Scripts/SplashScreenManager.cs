using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
    public GameObject clickToStartButton;
    private bool isVisible = true;
    public float blinkInterval = 1.0f;

    void Start()
    {
        InvokeRepeating(nameof(ToggleVisibility), 0f, blinkInterval);
    }

    /*
     * Se clica no botão do mouse, vai para o menu principal.
    */
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GoToMainMenu();
        }
    }

    private void ToggleVisibility()
    {
        isVisible = !isVisible;
        clickToStartButton.SetActive(isVisible);
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}