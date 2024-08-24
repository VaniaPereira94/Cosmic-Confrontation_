using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    private bool isPaused = false;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;

        // oculta menu de pausa ao abrir a cena
        pauseMenuPanel.SetActive(false);
    }

    /*
     * Se pressiona "ESC", abre o menu de pausa;
     * Com o menu de pausa ativo, se pressiona "ESC" ou clica no botão de continuar, retorna ao jogo.
    */
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // desbloqueia este atributo, uma vez que é bloqueado quando a câmera de jogo está ativa
            Cursor.lockState = CursorLockMode.None;

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (gameManager.CurrentActionDialogue != null)
        {
            gameManager.CurrentActionDialogue.Pause();
        }
        if (gameManager.BackgroundAaudioSource != null)
        {
            gameManager.BackgroundAaudioSource.Pause();
        }

        // desbloqueia este atributo, uma vez que é bloqueado quando a câmera de jogo está ativa
        Cursor.lockState = CursorLockMode.None;

        // congela o tempo
        Time.timeScale = 0;

        isPaused = true;
        pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        // volta ao estado normal da câmera de jogo
        Cursor.lockState = CursorLockMode.Locked;

        // descongela o tempo
        Time.timeScale = 1;

        isPaused = false;
        pauseMenuPanel.SetActive(false);

        if (gameManager.CurrentActionDialogue != null)
        {
            gameManager.CurrentActionDialogue.Play();
        }
        if (gameManager.BackgroundAaudioSource != null)
        {
            gameManager.BackgroundAaudioSource.Play();
        }

    }

    public void QuitGame()
    {
        // descongela o tempo
        Time.timeScale = 1;

        SceneManager.LoadScene("MainMenu");
    }
}