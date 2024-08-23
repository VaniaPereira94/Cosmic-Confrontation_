using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject panelButtons;
    [SerializeField] private GameObject panelControls;
    [SerializeField] private GameObject panelCredits;

    public void PlayGame()
    {
        SceneManager.LoadScene("BeachAndForest");
    }

    public void OpenControls()
    {
        panelButtons.SetActive(false);
        panelControls.SetActive(true);
    }

    public void CloseControls()
    {
        panelControls.SetActive(false);
        panelButtons.SetActive(true);
    }

    public void OpenCredits()
    {
        panelButtons.SetActive(false);
        panelCredits.SetActive(true);
    }

    public void CloseCredits()
    {
        panelCredits.SetActive(false);
        panelButtons.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}