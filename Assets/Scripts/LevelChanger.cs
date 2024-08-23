using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    enum levelList
    {
        Forest,
        Camp,
        Cave,
        AfterMaze,
        Pyramid
    };

    [SerializeField]
    private levelList Level;

    [SerializeField]
    private float transitionTime = 2f;

    [SerializeField]
    private GameObject loadScreen;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            switch (gameManager.CurrentGameState.Value)
            {
                case GameState.GO_TO_FOREST:
                    gameManager.SetGameState(GameState.INTRO_FOREST);
                    Destroy(this);
                    break;
                case GameState.GO_TO_CAMP:
                    gameManager.SetGameState(GameState.INTRO_CAMP);
                    Destroy(this);
                    break;
                case GameState.GO_TO_CAVE:
                    SceneManager.LoadScene("CaveAndPyramid");
                    break;
                case GameState.SOLVE_PUZZLE:
                    gameManager.SetGameState(GameState.INTRO_PYRAMID);
                    Destroy(this);
                    break;
                default:
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            switch (gameManager.CurrentGameState.Value)
            {
                case GameState.GO_TO_MAZE:
                    gameManager.SetGameState(GameState.GO_TO_PYRAMID);
                    Destroy(this);
                    break;
                default:
                    break;
            }
        }
    }
}