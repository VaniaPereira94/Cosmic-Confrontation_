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

    [SerializeField] private Canvas _mainCanvas;

    [SerializeField] private AudioSource _backgroundAaudioSource;
    [SerializeField] private AudioSource _treasureChestAudioSource;
    private AudioSource _currentActionDialogue;
    private AudioSource _currentGoalDialogue;

    private bool _isChangingPositon = false;
    private Vector3 _positionToChange;
    private Vector3 _rotationToChange;

    [SerializeField] public GameObject _playerCameraObject;

    private Vector3 _lastCheckPointPos;
    private ThirdPersonMovement _playerScript;

    [SerializeField] private GameObject _puzzleManagerObject;
    private PuzzleManager _puzzleManagerScript;

    [SerializeField] private GameObject _currentActionPanel;
    [SerializeField] private TextMeshProUGUI _currentActionTextMeshPro;

    [SerializeField] private GameObject _currentGoalPanel;
    [SerializeField] private TextMeshProUGUI _currentGoalTextMeshPro;

    [SerializeField] private GameObject _starship;

    [SerializeField] private GameObject _colliderInFrontOfCampCutscene;

    [SerializeField] private GameObject _orb;
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

    public AudioSource CurrentGoalDialogue
    {
        get { return _currentGoalDialogue; }
        set { _currentGoalDialogue = value; }
    }

    public Vector3 LastCheckPointPos
    {
        get { return _lastCheckPointPos; }
        set { _lastCheckPointPos = value; }
    }

    public GameObject CurrentActionPanel
    {
        get { return _currentActionPanel; }
        set { _currentActionPanel = value; }
    }

    public GameObject CurrentGoalPanel
    {
        get { return _currentGoalPanel; }
        set { _currentGoalPanel = value; }
    }


    /* MÉTODOS */

    /*
     * Garante apenas uma instância de GameManager por cena.
    */
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "CaveAndPyramid")
        {
            RenderSettings.ambientIntensity = 0.3f;
        }

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

        InvokeRepeating(nameof(ShowAndHideGoalLoop), 4f, 40f);

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
            // executar o código se for a IA a jogar (nas respetivas cenas)
            if (SceneManager.GetActiveScene().name == "SolvePuzzleAI_Train" ||
                SceneManager.GetActiveScene().name == "SolvePuzzleAI_Play")
            {
                if (_puzzleManagerScript.BoardAI.StopPlaying)
                {
                    _puzzleManagerScript.AfterSolvePuzzle(_playerCameraObject, _playerScript);
                }
                return;
            }

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
            ChangePlayerPosition(_positionToChange, _rotationToChange);
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
            _positionToChange = _currentMapActions[0].gameStateInfo.position;
            _rotationToChange = _currentMapActions[0].gameStateInfo.rotation;
            _isChangingPositon = true;
        }

        // configurações específicas na mudança de estado
        switch (nextGameState)
        {
            // mostra a cutscene externa e trata do colisor no script LevelChanger
            case GameState.INTRO_GAME:
                StartCoroutine(OpenInitialCutscene(nextGameState));
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
                _puzzleManagerScript.BeforeSolvePuzzle(_playerCameraObject);
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
        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");
        if (playerPrefab != null)
        {
            PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
            playerAnimations.WalkingSound.Pause();
        }

        if (_backgroundAaudioSource != null && _backgroundAaudioSource.isPlaying)
        {
            _backgroundAaudioSource.Pause();
        }

        // bloquear falas da ação do F ou do objetivo
        if (_currentActionDialogue != null) _currentActionDialogue.Stop();
        if (_currentGoalDialogue != null) _currentGoalDialogue.Stop();
        CancelInvoke(nameof(ShowAndHideGoalLoop));

        Time.timeScale = 0f;

        _mainCanvas.enabled = false;
        videoPlayer.enabled = true;
        videoPlayer.Play();
    }

    private void OnVideoCutsceneEnd(VideoPlayer videoPlayer, GameState nextGameState)
    {
        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");
        if (playerPrefab != null)
        {
            PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
            playerAnimations.WalkingSound.UnPause();
        }

        if (_backgroundAaudioSource != null && !_backgroundAaudioSource.isPlaying)
        {
            _backgroundAaudioSource.UnPause();
        }

        _mainCanvas.enabled = true;
        videoPlayer.enabled = false;

        Time.timeScale = 1f;

        if (_currentMapActions[0].gameStateInfo.hasNewPosition)
        {
            int lastGameStateInfoIndex = GetLastGameStateInfoIndex();
            _positionToChange = _gameStateList[lastGameStateInfoIndex].position;
            _rotationToChange = _gameStateList[lastGameStateInfoIndex].rotation;

            _isChangingPositon = true;
        }

        // ativar novamente a fala do objetivo durante o jogo
        InvokeRepeating(nameof(ShowAndHideGoalLoop), 4f, 40f);

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

        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");
        if (playerPrefab != null)
        {
            PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
            playerAnimations.WalkingSound.Pause();
        }

        if (_backgroundAaudioSource != null && _backgroundAaudioSource.isPlaying)
        {
            _backgroundAaudioSource.Pause();
        }

        // bloquear falas da ação do F ou do objetivo
        if (_currentActionDialogue != null) _currentActionDialogue.Stop();
        if (_currentGoalDialogue != null) _currentGoalDialogue.Stop();
        CancelInvoke(nameof(ShowAndHideGoalLoop));

        _mainCanvas.enabled = false;

        timelineObject.SetActive(true);

        timeline.Play();
    }

    private void OnTimelineCutsceneEnd(PlayableDirector timeline, GameObject timelineObject, GameState nextGameState)
    {
        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");
        if (playerPrefab != null)
        {
            PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
            playerAnimations.WalkingSound.UnPause();
        }

        if (_backgroundAaudioSource != null && !_backgroundAaudioSource.isPlaying)
        {
            _backgroundAaudioSource.UnPause();
        }

        _mainCanvas.enabled = true;
        timelineObject.SetActive(false);

        if (_currentMapActions[0].gameStateInfo.hasNewPosition)
        {
            int lastGameStateInfoIndex = GetLastGameStateInfoIndex();
            _positionToChange = _gameStateList[lastGameStateInfoIndex].position;
            _rotationToChange = _gameStateList[lastGameStateInfoIndex].rotation;

            _isChangingPositon = true;
        }

        // ativar novamente a fala do objetivo durante o jogo
        InvokeRepeating(nameof(ShowAndHideGoalLoop), 4f, 40f);

        if (CurrentGameState.Value == GameState.INTRO_CAMP)
        {
            // não permite que os inimigos venham ter connosco durante a cutscene
            if (_colliderInFrontOfCampCutscene != null)
            {
                Destroy(_colliderInFrontOfCampCutscene.gameObject);
            }
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

    private IEnumerator OpenInitialCutscene(GameState nextGameState)
    {
        yield return new WaitForSeconds(1f);
        ConfigVideoCutscene(nextGameState);
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

        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");
        PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
        playerAnimations.WalkingSound.Pause();

        yield return new WaitForSeconds(dialogueDuration + 1f);

        _currentActionDialogue = null;

        _isLookingToObject = false;
        _targetToLook = null;

        UnFreezePlayer();

        _currentActionPanel.SetActive(false);

        playerAnimations.WalkingSound.UnPause();
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
            if (mapAction.hasProgress && mapAction.dialogue != null && (_currentActionDialogue == null))
            {
                _currentGoalTextMeshPro.text = mapAction.title;

                _currentGoalDialogue = mapAction.dialogue.GetComponent<AudioSource>();
                dialogueDuration = _currentGoalDialogue.clip.length;
                _currentGoalDialogue.Play();

                _currentGoalPanel.SetActive(true);

                break;
            }
        }

        yield return new WaitForSeconds(dialogueDuration + 1f);

        _currentGoalDialogue = null;

        _currentGoalPanel.SetActive(false);
    }

    private IEnumerator OpenChestAndStartCutscene(GameState nextGameState)
    {
        _treasureChestAnimator.SetBool("isOpen", true);
        _treasureChestAudioSource.Play();

        Vector3 initialPosition = _orb.transform.localPosition;
        Vector3 targetPosition = new Vector3(-129.8151f, -809.9969f, 329.599f);

        float duration = 5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            _orb.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        _orb.transform.localPosition = targetPosition;

        yield return new WaitForSeconds(3f);

        ConfigVideoCutscene(nextGameState);
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
            // impedir ação se estivar a saltar
            if (_playerScript.IsJumping)
            {
                return;
            }

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
            // se ação tem diálogo e a fala do objetivo não está a ser falada no momento
            if (mapAction.hasDialogue && _currentGoalDialogue == null)
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
        _playerScript.SetDeathCollider(false);
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
        playerAnimations.StopAllAnimations();

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