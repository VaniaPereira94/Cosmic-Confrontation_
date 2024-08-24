using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using static Utils;

public class GameManager : MonoBehaviour
{
    /* ATRIBUTOS */

    private static GameManager _instance;

    [SerializeField] private GameObject _player;

    [SerializeField] public ReactiveProperty<GameState> _currentGameState = new();
    private List<MapAction> _currentMapActions = new();

    [SerializeField] private List<GameStateInfo> _gameStateList;
    [SerializeField] private List<MapAction> _mapActions;
    private List<GameObject> _medicineActions = new();

    [SerializeField] private float _actionButtonsVisibilityDistance = 20f;
    [SerializeField] private float _actionButtonsClickDistance = 2f;

    [SerializeField] private Canvas _canvas;

    [SerializeField] private AudioSource _backgroundAaudioSource;
    [SerializeField] private AudioSource _treasureChestAudioSource;

    private bool _isChangingPositon = false;
    private Vector3 positionToChange;
    private Vector3 rotationToChange;

    [SerializeField] public GameObject _playerCameraObject;

    private Vector3 _lastCheckPointPos;
    private ThirdPersonMovement _playerScript;

    [SerializeField] private GameObject _puzzleManagerObject;
    private PuzzleManager _puzzleManagerScript;

    [SerializeField] private GameObject _currentGoalPanel;
    [SerializeField] private TextMeshProUGUI _currentGoalTextMeshPro;

    [SerializeField] private GameObject _currentActionPanel;
    [SerializeField] private TextMeshProUGUI _currentActionTextMeshPro;

    private AudioSource _currentActionDialogue;

    [SerializeField] private GameObject _starship;

    private GameObject _targetToLook;
    private bool _isLookingToObject;

    [SerializeField] private Animator _treasureChestAnimator;


    /* PROPRIEDADES */

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }

            return _instance;
        }
    }

    public IReadOnlyReactiveProperty<GameState> CurrentGameState => _currentGameState;

    public List<MapAction> CurrentMapActions
    {
        get { return _currentMapActions; }
        set { _currentMapActions = value; }
    }

    public AudioSource BackgroundAaudioSource
    {
        get { return _backgroundAaudioSource; }
        set { _backgroundAaudioSource = value; }
    }

    public AudioSource CurrentActionDialogue
    {
        get { return _currentActionDialogue; }
        set { _currentActionDialogue = value; }
    }

    public Vector3 LastCheckPointPos
    {
        get { return _lastCheckPointPos; }
        set { _lastCheckPointPos = value; }
    }


    /* MÉTODOS */

    /*
     * Garante apenas uma instância de GameManager por cena.
    */
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /*
     * Esconde todos os botões de ação no mapa por padrão.
     * Observa o estado atual do jogo, para que sempre que haja uma mudança o evento seja acionado.
    */
    private void Start()
    {
        HideCurrentActionLabel();
        HideAllActionButtons();

        InvokeRepeating(nameof(ShowAndHideGoalLoop), 4f, 35f);

        _playerScript = _player.GetComponent<ThirdPersonMovement>();

        // assina o observável para detetar mudanças de estado
        _currentGameState.Subscribe(gameState =>
        {
            HandleGameStateChange(gameState);
        });
    }

    private void Update()
    {
        if (_player == null)
        {
            return;
        }

        if (_playerScript.IsDead)
        {
            Invoke(nameof(RestartGame), 4);
            return;
        }

        if (_isLookingToObject)
        {
            FreezePlayer();
            LookToObject();
        }

        // bloqueia outras ações quando está a resolver o puzzle
        if (_puzzleManagerScript != null)
        {
            if (_puzzleManagerScript.IsSolving)
            {
                if (_puzzleManagerScript.CheckPuzzleSolved())
                {
                    _puzzleManagerScript.AfterSolvePuzzle(_playerCameraObject, _playerScript);
                }

                _puzzleManagerScript.DoPlay();
            }

            return;
        }

        // não há necessidade de verificar se clicou no botão de ação,
        // quando o estado é apenas de mostrar uma cutscene
        if (_currentGameState.Value != GameState.INTRO_GAME ||
            _currentGameState.Value != GameState.INTRO_FOREST ||
            _currentGameState.Value != GameState.INTRO_CAMP ||
            _currentGameState.Value != GameState.INTRO_CAVE ||
            _currentGameState.Value != GameState.INTRO_PYRAMID)
        {
            CheckActionButtonsVisibilityDistance();
            CheckActionButtonsClickDistance();
            CenterMedicionActions();
        }
    }

    private void FixedUpdate()
    {
        if (_isChangingPositon)
        {
            ChangePlayerPosition(positionToChange, rotationToChange);
            _isChangingPositon = false;
        }
    }

    /*
     * Trata da mudança para os diferentes estados do jogo.
     * _currentMapActions[0] - porque existe sempre pelo menos uma ação do mapa associada a um game state
    */
    private void HandleGameStateChange(GameState nextGameState)
    {
        HideCurrentActionButtons();

        _currentGameState.Value = nextGameState;
        _currentMapActions = GetCurrentMapActions();

        if (_currentMapActions[0].gameStateInfo.hasNewPosition)
        {
            positionToChange = _currentMapActions[0].gameStateInfo.position;
            rotationToChange = _currentMapActions[0].gameStateInfo.rotation;
            _isChangingPositon = true;
        }

        // configurações específicas na mudança de estado
        switch (nextGameState)
        {
            // mostra a cutscene externa e trata do colisor no script LevelChanger
            case GameState.INTRO_GAME:
                ConfigVideoCutscene(nextGameState);
                break;

            // abre o baú e no fim mostra a cutscene final
            case GameState.FINISH_GAME:
                StartCoroutine(OpenChestAndStartCutscene(nextGameState));
                break;

            // mostra a cutscene dentro do unity e trata do colisor no script LevelChanger
            case GameState.INTRO_FOREST:
            case GameState.INTRO_CAMP:
            case GameState.INTRO_CAVE:
            case GameState.INTRO_PYRAMID:
                ConfigTimelineCutscene(nextGameState);
                break;

            // muda a posição da nave na praia
            case GameState.GO_TO_FOREST:
                _starship.SetActive(true);
                _starship.transform.localPosition = new Vector3(-19.17f, -4f, 65.87f);
                _starship.transform.localRotation = Quaternion.Euler(2.002f, -27.307f, -1.41f);
                break;

            // permite que o jogador comece a resolver o puzzle
            case GameState.SOLVE_PUZZLE:
                _puzzleManagerScript = _puzzleManagerObject.GetComponent<PuzzleManager>();
                _puzzleManagerScript.BeforeSolvePuzzle(_playerCameraObject, _playerScript);
                break;

            default:
                break;
        }
    }

    /*
     * Obtém as ações atuais do mapa associadas ao estado de jogo atual.
    */
    private List<MapAction> GetCurrentMapActions()
    {
        _currentMapActions.Clear();

        foreach (MapAction mapAction in _mapActions)
        {
            if (mapAction.gameStateInfo.gameState == _currentGameState.Value)
            {
                _currentMapActions.Add(mapAction);
            }
        }

        return _currentMapActions;
    }

    /*
     * Procura a ação atual que diz respeito ao objetivo,
     * que será a que tem "hasProgress" como true e devolve o título.
    */
    public string GetCurrentGoal()
    {
        foreach (MapAction mapAction in GameManager.Instance._currentMapActions)
        {
            if (mapAction.hasProgress)
            {
                return mapAction.title;
            }
        }

        return "";
    }

    private GameState GetNextGameState(GameState currentGameState)
    {
        int nextGameStateNumber = (int)currentGameState + 1;
        return (GameState)nextGameStateNumber;
    }

    private int GetLastGameStateInfoIndex()
    {
        int lastGameStateInfoIndex = 0;

        for (int i = 0; i < _gameStateList.Count; i++)
        {
            if (_currentGameState.Value == _gameStateList[i].gameState)
            {
                lastGameStateInfoIndex = i;
            }
        }

        return lastGameStateInfoIndex;
    }

    public void SetGameState(GameState newGameState)
    {
        _currentGameState.Value = newGameState;
    }

    private void ConfigVideoCutscene(GameState nextGameState)
    {
        OnVideoCutsceneStart(_currentMapActions[0].gameStateInfo.videoCutscene);

        nextGameState = GetNextGameState(_currentGameState.Value);

        // evento de término da cutscene
        _currentMapActions[0].gameStateInfo.videoCutscene.loopPointReached += (videoPlayer) => OnVideoCutsceneEnd(_currentMapActions[0].gameStateInfo.videoCutscene, nextGameState);
    }

    private void ConfigTimelineCutscene(GameState nextGameState)
    {
        _player.SetActive(false);

        GameObject timelineObject = _currentMapActions[0].gameStateInfo.timelineCutscene;
        PlayableDirector timeline = timelineObject.GetComponent<PlayableDirector>();

        timeline.played += (timelineObject) => OnTimelineCutsceneStart(timeline, _currentMapActions[0].gameStateInfo.timelineCutscene);
        OnTimelineCutsceneStart(timeline, timelineObject);

        nextGameState = GetNextGameState(_currentGameState.Value);

        // evento de término da cutscene
        timeline.stopped += (timeline) => OnTimelineCutsceneEnd(timeline, timelineObject, nextGameState);
    }

    private void OnVideoCutsceneStart(VideoPlayer videoPlayer)
    {
        if (_backgroundAaudioSource != null && _backgroundAaudioSource.isPlaying)
        {
            _backgroundAaudioSource.Pause();
        }

        Time.timeScale = 0f;

        _canvas.enabled = false;
        videoPlayer.enabled = true;
        videoPlayer.Play();
    }

    private void OnVideoCutsceneEnd(VideoPlayer videoPlayer, GameState nextGameState)
    {
        if (_backgroundAaudioSource != null && !_backgroundAaudioSource.isPlaying)
        {
            _backgroundAaudioSource.UnPause();
        }

        _canvas.enabled = true;
        videoPlayer.enabled = false;

        Time.timeScale = 1f;

        if (_currentMapActions[0].gameStateInfo.hasNewPosition)
        {
            int lastGameStateInfoIndex = GetLastGameStateInfoIndex();
            positionToChange = _gameStateList[lastGameStateInfoIndex].position;
            rotationToChange = _gameStateList[lastGameStateInfoIndex].rotation;

            _isChangingPositon = true;
        }

        if (_currentGameState.Value == GameState.FINISH_GAME)
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("MainMenu");
            return;
        }

        ChangeGameState(nextGameState);
    }

    private void OnTimelineCutsceneStart(PlayableDirector timeline, GameObject timelineObject)
    {
        _player.SetActive(false);

        if (_backgroundAaudioSource != null && _backgroundAaudioSource.isPlaying)
        {
            _backgroundAaudioSource.Pause();
        }

        _canvas.enabled = false;

        timelineObject.SetActive(true);
        timeline.Play();
    }

    private void OnTimelineCutsceneEnd(PlayableDirector timeline, GameObject timelineObject, GameState nextGameState)
    {
        if (_backgroundAaudioSource != null && !_backgroundAaudioSource.isPlaying)
        {
            _backgroundAaudioSource.UnPause();
        }

        _canvas.enabled = true;
        timelineObject.SetActive(false);

        if (_currentMapActions[0].gameStateInfo.hasNewPosition)
        {
            int lastGameStateInfoIndex = GetLastGameStateInfoIndex();
            positionToChange = _gameStateList[lastGameStateInfoIndex].position;
            rotationToChange = _gameStateList[lastGameStateInfoIndex].rotation;

            _isChangingPositon = true;
        }

        ChangeGameState(nextGameState);

        _player.SetActive(true);
    }

    private void ChangeGameState(GameState newGameState)
    {
        _currentGameState.Value = newGameState;
    }

    public void ChangePlayerPosition(Vector3 positon, Vector3 rotation)
    {
        _player.transform.localPosition = positon;
        _player.transform.localRotation = Quaternion.Euler(rotation);
    }

    /*
     * Ao iniciar a cena, todos os botões de ação são ocultos por padrão.
    */
    private void HideAllActionButtons()
    {
        foreach (MapAction mapAction in _mapActions)
        {
            if (mapAction.button != null)
            {
                mapAction.button.SetActive(false);
            }
        }
    }

    /*
     * Esconde os botões das ações do estado atual.
     * Utilizável quando o utilizador transita para o próximo estado de jogo e as ações do estado anterior já não interessam.
    */
    private void HideCurrentActionButtons()
    {
        foreach (MapAction mapAction in _currentMapActions)
        {
            if (mapAction.hasClick)
            {
                mapAction.button.SetActive(false);
            }
        }
    }

    private void HideCurrentActionLabel()
    {
        _currentActionPanel.SetActive(false);
    }

    private void HideCurrentGoalLabel()
    {
        _currentGoalPanel.SetActive(false);
    }

    /*
     * Mostra o texto da ação atual e fala, apenas por 4 segundos e depois volta a desaparecer
    */
    private IEnumerator ShowAndHideActionLabel(string text, GameObject dialogueObject, GameObject actionButtonObject)
    {
        _currentActionDialogue = dialogueObject.GetComponent<AudioSource>();
        float dialogueDuration = _currentActionDialogue.clip.length;
        _currentActionDialogue.Play();

        _targetToLook = actionButtonObject;
        _isLookingToObject = true;

        _currentActionTextMeshPro.text = text;
        _currentActionPanel.SetActive(true);

        yield return new WaitForSeconds(dialogueDuration + 1f);

        _currentActionDialogue = null;

        _isLookingToObject = false;
        _targetToLook = null;

        UnFreezePlayer();

        _currentActionPanel.SetActive(false);
    }

    private IEnumerator OpenChestAndStartCutscene(GameState nextGameState)
    {
        _treasureChestAnimator.SetBool("isOpen", true);
        _treasureChestAudioSource.Play();

        yield return new WaitForSeconds(10f);

        ConfigVideoCutscene(nextGameState);
    }

    private void ShowAndHideGoalLoop()
    {
        StartCoroutine(ShowAndHideGoalLabel());
    }

    /*
     * Mostra o texto do objetivo no canto superior direito apenas por 4 segundos a cada 20 segundos e depois volta a desaparecer.
    */
    private IEnumerator ShowAndHideGoalLabel()
    {
        float dialogueDuration = 0f;

        foreach (MapAction mapAction in _currentMapActions)
        {
            if (mapAction.hasProgress && mapAction.dialogue != null)
            {
                _currentGoalTextMeshPro.text = mapAction.title;

                _currentActionDialogue = mapAction.dialogue.GetComponent<AudioSource>();
                dialogueDuration = _currentActionDialogue.clip.length;
                _currentActionDialogue.Play();
            }
        }

        _currentGoalPanel.SetActive(true);

        yield return new WaitForSeconds(dialogueDuration + 1f);

        _currentGoalPanel.SetActive(false);
    }

    /*
     * Se o jogador estiver perto das ações de jogo, o botão de ação será visível, caso contrário continuará oculto.
    */
    private void CheckActionButtonsVisibilityDistance()
    {
        foreach (MapAction mapAction in _currentMapActions)
        {
            if (mapAction.hasClick)
            {
                if (_actionButtonsVisibilityDistance >= Utils.GetDistanceBetween2Objects(_player, mapAction.button))
                {
                    mapAction.button.SetActive(true);
                    CenterActionButtonInCamera(mapAction.button);
                }
            }
        }
    }

    /*
     * Se o jogador estiver muito perto das ações de jogo, o botão de ação poderá ser clicado, caso contrário não.
    */
    private void CheckActionButtonsClickDistance()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            foreach (MapAction mapAction in _currentMapActions)
            {
                if (mapAction.hasClick)
                {
                    if (_actionButtonsClickDistance >= Utils.GetDistanceBetween2Objects(_player, mapAction.button))
                    {
                        // aciona eventos
                        ProgressActionEvent(mapAction);
                        NoProgressActionEvent(mapAction);
                        return;
                    }
                }
            }
        }
    }

    /*
     * Quando ação clicada permite que o jogador progrida no jogo (passe para o próximo game state).
     * Quando o jogador alcança um objetivo, este não pode voltar ao objetivo anterior.
     * Exemplo: Quando encontra o esconderijo para a nave.
    */
    private void ProgressActionEvent(MapAction mapAction)
    {
        if (mapAction.hasProgress)
        {
            if (mapAction.hasClick)
            {
                mapAction.button.SetActive(false);
                mapAction.hasClick = false;
            }

            if (_currentGameState.Value == GameState.GO_TO_FOREST ||
                _currentGameState.Value == GameState.GO_TO_PYRAMID ||
                _currentGameState.Value == GameState.PICK_TREASURE)
            {
                GameState nextGameState = GetNextGameState(_currentGameState.Value);
                ChangeGameState(nextGameState);
            }

            if (mapAction.gameStateInfo.hasCutscene)
            {
                if (mapAction.gameStateInfo.cutsceneType == CutsceneType.EXTERNAL)
                {
                    OnVideoCutsceneStart(mapAction.gameStateInfo.videoCutscene);

                    GameState nextGameState = GetNextGameState(_currentGameState.Value);

                    // evento de término da cutscene
                    mapAction.gameStateInfo.videoCutscene.loopPointReached += (videoPlayer) => OnVideoCutsceneEnd(mapAction.gameStateInfo.videoCutscene, nextGameState);
                }
                else if (mapAction.gameStateInfo.cutsceneType == CutsceneType.INSIDE_EDITOR)
                {
                    if (_currentGameState.Value == GameState.HIDE_SHIP)
                    {
                        _starship.SetActive(false);
                    }

                    GameObject timelineObject = mapAction.gameStateInfo.timelineCutscene;
                    PlayableDirector timeline = timelineObject.GetComponent<PlayableDirector>();

                    timeline.played += (timelineObject) => OnTimelineCutsceneStart(timeline, mapAction.gameStateInfo.timelineCutscene);
                    OnTimelineCutsceneStart(timeline, timelineObject);

                    GameState nextGameState = GetNextGameState(_currentGameState.Value);

                    // evento de término da cutscene
                    timeline.stopped += (timeline) => OnTimelineCutsceneEnd(timeline, timelineObject, nextGameState);
                }
            }
        }
    }

    /*
     * Quando ação clicada não permite que o jogador progrida no jogo (passe para o próximo game state).
     * Exemplo: Quando o jogador tenta um caminho errado para a floresta. Quando observa uma pista na floresta.
    */
    private void NoProgressActionEvent(MapAction mapAction)
    {
        if (!mapAction.hasProgress)
        {
            if (mapAction.hasDialogue)
            {
                StartCoroutine(ShowAndHideActionLabel(mapAction.title, mapAction.dialogue, mapAction.button));
            }
        }
    }

    private void RestartGame()
    {
        if (_lastCheckPointPos != null) _player.transform.position = _lastCheckPointPos;
        _playerScript.IsDead = false;
        _playerScript.HealthManager.restoreHealth();
        CancelInvoke(nameof(RestartGame));
    }

    public void CenterActionButtonInCamera(GameObject actionButton)
    {
        Camera playerCamera = _playerCameraObject.GetComponent<Camera>();

        Vector3 cameraPosition = playerCamera.transform.position;
        Vector3 objectPosition = actionButton.transform.position;
        Vector3 direction = cameraPosition - objectPosition;

        actionButton.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0);
    }

    private void LookToObject()
    {
        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");

        Vector3 direction = _targetToLook.transform.position - playerPrefab.transform.position;
        direction.Normalize();

        // Calcula a rotação desejada
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));

        // suaviza a transição de rotação
        float rotationSpeed = 3f;
        playerPrefab.transform.rotation = Quaternion.Slerp(playerPrefab.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        Vector3 forwardDirection = playerPrefab.transform.forward;

        float dotProduct = Vector3.Dot(direction, forwardDirection);

        // verifica se o jogador está a olhar para o puzzle
        float angleThreshold = 0.9f;
        if (dotProduct > angleThreshold)
        {
            _isLookingToObject = false;
            _targetToLook = null;
        }
    }

    private void FreezePlayer()
    {
        _playerScript.freeze = true;

        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");

        PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
        playerAnimations.FreezeAllAnimations = true;

        Animator playerAnimatior = playerPrefab.GetComponent<Animator>();
        playerAnimatior.SetBool(Animations.WALKING, false);
    }

    private void UnFreezePlayer()
    {
        _playerScript.freeze = false;

        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");

        PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
        playerAnimations.FreezeAllAnimations = false;
    }

    public void addMedicine(GameObject medicineAction)
    {
        _medicineActions.Add(medicineAction);
    }

    public void removeMedicine(GameObject medicineAction)
    {
        _medicineActions.Remove(medicineAction);
    }

    private void CenterMedicionActions()
    {
        foreach (GameObject medicineAction in _medicineActions)
        {
            CenterActionButtonInCamera(medicineAction);
        }
    }
}