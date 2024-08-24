using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

public class PuzzleManager : MonoBehaviour
{
    /* ATRIBUTOS */

    [SerializeField] private int[] pieceOrder = { 6, 9, 8, 7, 2, 4, 5, 3, 1 };

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

    private bool _walkStarted = false;
    private bool _lookStarted = false;

    [SerializeField] private AudioSource _pieceDragAudio;


    /* PROPRIEDADES */

    public bool IsSolving
    {
        get { return _isSolving; }
        set { _isSolving = value; }
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

    private void ShufflePuzzle()
    {
        _pieces = _pieces.OrderBy(piece => Array.IndexOf(pieceOrder, piece.position)).ToList();

        for (int i = 0; i < _pieces.Count; i++)
        {
            Vector3 newPosition = _wallPoints[i].localPosition;
            UpdatePosition(i, newPosition);
        }
    }

    public void DoPlay()
    {
        // se uma tecla for pressionada
        if (Input.anyKeyDown)
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
                    return;
                }
                else
                {
                    _secondPiece = ChoosePiece(inputtedNumber);
                    StartCoroutine(MoveSecondPieceThenSwap());
                }
            }
        }
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

    private bool CheckValidPlay(int inputtedNumber)
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
        Vector3 endStartPosition = new Vector3(_secondPiece.piece.transform.position.x, _secondPiece.piece.transform.position.y, _secondPiece.piece.transform.position.z);

        yield return StartCoroutine(MoveToFront(_secondPiece, _secondPieceToFrontDistance));

        yield return StartCoroutine(MoveSecondToFirstPiece());
        yield return StartCoroutine(MoveToBack(_secondPiece, _secondPieceToFrontDistance));
        yield return StartCoroutine(MoveFirstToSecondPiece(endStartPosition));

        int firstIndexOfList = _pieces.IndexOf(_firstPiece);
        int secondIndeOfList = _pieces.IndexOf(_secondPiece);
        SwapPieceInList(firstIndexOfList, secondIndeOfList);

        ResetValues();
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

    IEnumerator MoveToFront(PuzzlePiece currentPiece, float moveUntil)
    {
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

        yield return null;
    }

    IEnumerator MoveToBack(PuzzlePiece currentPiece, float moveUntil)
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

    /*
     * Mover a 2º peça até à 1º peça, mantendo o eixo z da 2º peça.
     */
    IEnumerator MoveSecondToFirstPiece()
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
    * Mover a 1º peça até à 2º peça, mantendo o eixo z da 1º peça.
    */
    IEnumerator MoveFirstToSecondPiece(Vector3 endPosition)
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

    public void BeforeSolvePuzzle(GameObject playerCamera, ThirdPersonMovement playerScript)
    {
        ThirdPersonCam thirdPersonCamera = playerCamera.GetComponent<ThirdPersonCam>();
        thirdPersonCamera.SwitchCameraStyle(ThirdPersonCam.CameraStyle.FocusOnPuzzle);

        _walkStarted = true;
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

        Destroy(this);
    }

    /*
     * Caminhar automaticamente até à posição dos botões que movem o puzzle.
    */
    private void WalkToPuzzle()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject playerPrefab = GameObject.FindGameObjectWithTag("PlayerPrefab");

        PlayerAnimations playerAnimations = playerPrefab.GetComponent<PlayerAnimations>();
        playerAnimations.FreezeAllAnimations = true;
        playerPrefab.GetComponent<Animator>().SetBool(Animations.WALKING, true);

        // calcula a direção para o ponto de destino
        Vector3 direction = _walkToPuzzlePoint.transform.position - playerPrefab.transform.position;
        direction.Normalize();

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