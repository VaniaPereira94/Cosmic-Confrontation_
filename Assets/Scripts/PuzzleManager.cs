using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Utils;

public class PuzzleManager : MonoBehaviour
{
    /* ATRIBUTOS */

    [SerializeField] private List<Transform> _wallPoints;
    [SerializeField] private List<PuzzlePiece> _pieces;

    private PuzzlePiece _firstPiece = null;
    private PuzzlePiece _secondPiece = null;

    private float _firstPieceToFrontDistance = 0.81f;
    private float _secondPieceToFrontDistance = 0.45f;

    private bool _isSolving = false;

    [SerializeField] private float _moveDuration = 1f;

    [SerializeField] private GameObject _door;
    private DoorAnimationManager _doorScript;

    [SerializeField] private GameObject _pyramidEntranceCollider;

    [SerializeField] private GameObject _walkToPuzzlePoint;
    [SerializeField] private GameObject _lookToPuzzlePoint;

    [SerializeField] private Animator animator;

    private bool _walkStarted = false;
    private bool _lookStarted = false;

    private bool _isMovingFirstPiece = false;
    private bool _isMovingSecondPiece = false;

    [SerializeField] private AudioSource _pieceDragAudio;

    [SerializeField] private BoardAI _boardAI;


    /* PROPRIEDADES */

    public List<PuzzlePiece> Pieces
    {
        get { return _pieces; }
        set { _pieces = value; }
    }

    public bool IsSolving
    {
        get { return _isSolving; }
        set { _isSolving = value; }
    }

    public BoardAI BoardAI
    {
        get { return _boardAI; }
        set { _boardAI = value; }
    }


    /* MÉTODOS */

    /*
     * Embaralha a lista das peças do puzzle, de acordo com a ordem dada. 
     * E atualiza as posições das peças na parede.
    */
    private void Start()
    {
        _doorScript = _door.GetComponent<DoorAnimationManager>();
        _pyramidEntranceCollider.SetActive(false);

        ShufflePuzzle();
    }

    private void FixedUpdate()
    {
        if (_walkStarted)
        {
            WalkToPuzzle();
        }
        if (_lookStarted)
        {
            LookToPuzzle();
        }
    }

    public void ShufflePuzzle()
    {
        int[] pieceOrder = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        ShuffleNumbers(pieceOrder);

        _pieces = _pieces.OrderBy(piece => Array.IndexOf(pieceOrder, piece.position)).ToList();

        for (int i = 0; i < _pieces.Count; i++)
        {
            Vector3 newPosition = _wallPoints[i].localPosition;
            UpdatePosition(i, newPosition);
        }
    }

    private void ShuffleNumbers(int[] array)
    {
        System.Random random = new System.Random();

        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);

            // troca os elementos de i e j
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    public void DoPlay()
    {
        // se uma tecla for pressionada e não existe nenhuma peça a mover-se
        if (Input.anyKeyDown && !_isMovingFirstPiece && !_isMovingSecondPiece)
        {
            // obtém o número da tecla
            int inputtedNumber = GetNumericKeyValue();

            if (CheckValidPlay(inputtedNumber))
            {
                // se AINDA NÃO escolheu a primeira peça, escolhe essa
                // se JÁ escolheu a primeira, agora escolhe a segunda
                if (_firstPiece == null)
                {
                    _firstPiece = ChoosePiece(inputtedNumber);
                    StartCoroutine(MoveToFront(_firstPiece, _firstPieceToFrontDistance));
                }
                else
                {
                    _secondPiece = ChoosePiece(inputtedNumber);
                    StartCoroutine(MoveSecondPieceThenSwap());
                }
            }
        }
    }

    public async Task MoveFirstPiece(int chosenNumber)
    {
        _firstPiece = ChoosePiece(chosenNumber);
        await MoveToFront2(_firstPiece, _firstPieceToFrontDistance);
    }

    public async Task MoveSecondPiece(int chosenNumber)
    {
        _secondPiece = ChoosePiece(chosenNumber);

        _isMovingSecondPiece = true;

        Vector3 endStartPosition = new Vector3(_secondPiece.piece.transform.position.x, _secondPiece.piece.transform.position.y, _secondPiece.piece.transform.position.z);

        if (IsSamePiece())
        {
            await MoveToBack2(_firstPiece, _firstPieceToFrontDistance);
        }
        else
        {
            await MoveToFront2(_secondPiece, _secondPieceToFrontDistance);
            await MoveSecondToFirstPiece2();
            await MoveToBack2(_secondPiece, _secondPieceToFrontDistance);
            await MoveFirstToSecondPiece2(endStartPosition);

            int firstIndexOfList = _pieces.IndexOf(_firstPiece);
            int secondIndeOfList = _pieces.IndexOf(_secondPiece);
            SwapPieceInList(firstIndexOfList, secondIndeOfList);
        }

        ResetValues();

        _isMovingSecondPiece = false;
    }

    /*
     * Recebe a tecla pressionada e converte o valor.
     * 0 a 9 - se a tecla for um número.
     * -1 - se não for.
    */
    private int GetNumericKeyValue()
    {
        string key = Input.inputString;
        int result;

        if (int.TryParse(key, out result))
        {
            return result;
        }
        else
        {
            return -1;
        }
    }

    public bool CheckValidPlay(int inputtedNumber)
    {
        if (inputtedNumber >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsSamePiece()
    {
        if (_firstPiece == _secondPiece)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private PuzzlePiece ChoosePiece(int inputtedNumber)
    {
        PuzzlePiece piece = _pieces[inputtedNumber - 1];

        if (piece != null)
        {
            return piece;
        }
        else
        {
            return null;
        }
    }

    private IEnumerator MoveSecondPieceThenSwap()
    {
        _isMovingSecondPiece = true;

        Vector3 endStartPosition = new Vector3(_secondPiece.piece.transform.position.x, _secondPiece.piece.transform.position.y, _secondPiece.piece.transform.position.z);

        if (IsSamePiece())
        {
            yield return StartCoroutine(MoveToBack(_firstPiece, _firstPieceToFrontDistance));
        }
        else
        {
            yield return StartCoroutine(MoveToFront(_secondPiece, _secondPieceToFrontDistance));
            yield return StartCoroutine(MoveSecondToFirstPiece());
            yield return StartCoroutine(MoveToBack(_secondPiece, _secondPieceToFrontDistance));
            yield return StartCoroutine(MoveFirstToSecondPiece(endStartPosition));

            int firstIndexOfList = _pieces.IndexOf(_firstPiece);
            int secondIndeOfList = _pieces.IndexOf(_secondPiece);
            SwapPieceInList(firstIndexOfList, secondIndeOfList);
        }

        ResetValues();

        _isMovingSecondPiece = false;
    }

    private void SwapPieceInList(int firstIndexOfList, int secondIndeOfList)
    {
        PuzzlePiece tempPiece = _pieces[firstIndexOfList];
        _pieces[firstIndexOfList] = _pieces[secondIndeOfList];
        _pieces[secondIndeOfList] = tempPiece;
    }

    private void UpdatePosition(int listIndex, Vector3 wallPosition)
    {
        _pieces[listIndex].piece.transform.localPosition = wallPosition;
    }

    private IEnumerator MoveToFront(PuzzlePiece currentPiece, float moveUntil)
    {
        _isMovingFirstPiece = true;

        _pieceDragAudio.Play();

        float startTime = Time.time;
        float elapsedTime = 0f;

        Vector3 startPosition = currentPiece.piece.transform.position;
        Vector3 endPosition = startPosition - Vector3.forward * moveUntil;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);

            currentPiece.piece.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        currentPiece.piece.transform.position = endPosition;

        _isMovingFirstPiece = false;

        yield return null;
    }

    private async Task MoveToFront2(PuzzlePiece currentPiece, float moveUntil)
    {
        _isMovingFirstPiece = true;

        _pieceDragAudio.Play();

        float startTime = Time.time;
        float elapsedTime = 0f;

        Vector3 startPosition = currentPiece.piece.transform.position;
        Vector3 endPosition = startPosition - Vector3.forward * moveUntil;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);

            currentPiece.piece.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            await Task.Yield();
        }

        currentPiece.piece.transform.position = endPosition;

        _isMovingFirstPiece = false;
    }

    private IEnumerator MoveToBack(PuzzlePiece currentPiece, float moveUntil)
    {
        _pieceDragAudio.Play();

        float startTime = Time.time;
        float elapsedTime = 0f;

        Vector3 startPosition = currentPiece.piece.transform.position;
        Vector3 endPosition = startPosition - Vector3.back * moveUntil;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);

            currentPiece.piece.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        currentPiece.piece.transform.position = endPosition;

        yield return null;
    }

    private async Task MoveToBack2(PuzzlePiece currentPiece, float moveUntil)
    {
        _pieceDragAudio.Play();

        float startTime = Time.time;
        float elapsedTime = 0f;

        Vector3 startPosition = currentPiece.piece.transform.position;
        Vector3 endPosition = startPosition - Vector3.back * moveUntil;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);

            currentPiece.piece.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            await Task.Yield();
        }

        currentPiece.piece.transform.position = endPosition;
    }

    /*
     * Mover a 2º peça até à 1º peça, mantendo o eixo z da 2º peça.
     */
    private IEnumerator MoveSecondToFirstPiece()
    {
        _pieceDragAudio.Play();

        float startTime = Time.time;
        float elapsedTime = 0f;

        Vector3 startPosition = _secondPiece.piece.transform.position;
        Vector3 endPosition = new Vector3(_firstPiece.piece.transform.position.x, _firstPiece.piece.transform.position.y, _secondPiece.piece.transform.position.z);

        while (elapsedTime < _moveDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);

            _secondPiece.piece.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        // garante que a 1º peça fique alinhada na 2º peça
        _secondPiece.piece.transform.position = endPosition;

        yield return null;
    }

    /*
     * Mover a 2º peça até à 1º peça, mantendo o eixo z da 2º peça.
     */
    private async Task MoveSecondToFirstPiece2()
    {
        _pieceDragAudio.Play();

        float startTime = Time.time;
        float elapsedTime = 0f;

        Vector3 startPosition = _secondPiece.piece.transform.position;
        Vector3 endPosition = new Vector3(_firstPiece.piece.transform.position.x, _firstPiece.piece.transform.position.y, _secondPiece.piece.transform.position.z);

        while (elapsedTime < _moveDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);

            _secondPiece.piece.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            await Task.Yield();
        }

        // garante que a 1º peça fique alinhada na 2º peça
        _secondPiece.piece.transform.position = endPosition;
    }

    /*
    * Mover a 1º peça até à 2º peça, mantendo o eixo z da 1º peça.
    */
    private IEnumerator MoveFirstToSecondPiece(Vector3 endPosition)
    {
        _pieceDragAudio.Play();

        float startTime = Time.time;
        float elapsedTime = 0f;

        Vector3 startPosition = _firstPiece.piece.transform.position;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);

            _firstPiece.piece.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        // garante que a 1º peça fique alinhada na 2º peça
        _firstPiece.piece.transform.position = endPosition;

        yield return null;
    }

    /*
    * Mover a 1º peça até à 2º peça, mantendo o eixo z da 1º peça.
    */
    private async Task MoveFirstToSecondPiece2(Vector3 endPosition)
    {
        _pieceDragAudio.Play();

        float startTime = Time.time;
        float elapsedTime = 0f;

        Vector3 startPosition = _firstPiece.piece.transform.position;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);

            _firstPiece.piece.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            await Task.Yield();
        }

        // garante que a 1º peça fique alinhada na 2º peça
        _firstPiece.piece.transform.position = endPosition;
    }

    /*
     * Limpar os valores do turno anterior (troca de 2 peças).
    */
    private void ResetValues()
    {
        _firstPiece = null;
        _secondPiece = null;
    }

    public bool CheckPuzzleSolved()
    {
        bool isSolved = true;

        for (int i = 0; i < _pieces.Count; i++)
        {
            if (_pieces[i].position != i + 1)
            {
                isSolved = false;
                break;
            }
        }

        return isSolved;
    }

    public void BeforeSolvePuzzle(GameObject playerCamera)
    {
        ThirdPersonCam thirdPersonCamera = playerCamera.GetComponent<ThirdPersonCam>();
        thirdPersonCamera.SwitchCameraStyle(ThirdPersonCam.CameraStyle.FocusOnPuzzle);

        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");
        PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
        playerAnimations.FreezeAllAnimations = true;
        playerAnimations.StopAllAnimations();

        _walkStarted = true;

        // executar o código se for a IA a jogar (apenas nas respetivas cenas)
        if (SceneManager.GetActiveScene().name == "SolvePuzzleAI_Train" ||
            SceneManager.GetActiveScene().name == "SolvePuzzleAI_Play")
        {
            _boardAI.StartGame();
        }
    }

    public void AfterSolvePuzzle(GameObject playerCamera, ThirdPersonMovement playerScript)
    {
        _isSolving = false;

        ThirdPersonCam thirdPersonCamera = playerCamera.GetComponent<ThirdPersonCam>();
        thirdPersonCamera.SwitchCameraStyle(ThirdPersonCam.CameraStyle.Basic);

        _doorScript.StartMoving = true;
        _pyramidEntranceCollider.SetActive(true);

        playerScript.freeze = false;

        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");
        PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
        playerAnimations.FreezeAllAnimations = false;
        playerAnimations.StopAllAnimations();

        animator.SetTrigger("isMoving");

        Destroy(this);
    }

    /*
     * Caminhar automaticamente até à posição dos botões que movem o puzzle.
    */
    private void WalkToPuzzle()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");

        // animação de andar
        playerPrefab.GetComponent<Animator>().SetBool(Animations.WALKING, true);

        // calcula a direção para o ponto de destino
        Vector3 direction = _walkToPuzzlePoint.transform.position - playerPrefab.transform.position;
        direction.Normalize();

        // muda a rotação
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        playerPrefab.transform.rotation = lookRotation;

        // obtém o game object do player prefab para move-lo até aos botões do puzzle
        playerPrefab.transform.position += direction * 1.5f * Time.deltaTime;

        // verifica se o jogador chegou ao ponto de destino
        if (Vector3.Distance(playerPrefab.transform.position, _walkToPuzzlePoint.transform.position) < 0.1f)
        {
            _walkStarted = false;
            _lookStarted = true;

            playerPrefab.GetComponent<Animator>().SetBool(Animations.WALKING, false);
            player.GetComponent<ThirdPersonMovement>().freeze = true;
        }
    }

    /*
     * Olhar até à posição do puzzle na parede.
    */
    private void LookToPuzzle()
    {
        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");

        Vector3 direction = _lookToPuzzlePoint.transform.position - playerPrefab.transform.position;
        direction.Normalize();

        // Calcula a rotação desejada
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));

        // suaviza a transição de rotação
        float rotationSpeed = 2f;
        playerPrefab.transform.rotation = Quaternion.Slerp(playerPrefab.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        Vector3 forwardDirection = playerPrefab.transform.forward;

        float dotProduct = Vector3.Dot(direction, forwardDirection);

        // verifica se o jogador está a olhar para o puzzle
        float angleThreshold = 0.9f;
        if (dotProduct > angleThreshold)
        {
            _isSolving = true;
        }
    }
}