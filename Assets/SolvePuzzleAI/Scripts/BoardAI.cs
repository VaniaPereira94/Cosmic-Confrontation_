using System.Threading.Tasks;
using Unity.MLAgents.Policies;
using UnityEngine;

public class BoardAI : MonoBehaviour
{
    /* CAMPOS DA CLASSE */

    [SerializeField] public PuzzleManager PuzzleManager;

    public PlayerAI Player1;
    public PlayerAI Player2;

    public bool AgentsWorking { get; private set; }

    public PlayerTypeAI CurrentPlayer { get; private set; }
    public GameStatusAI CurrentGameStatus { get; set; }
    public GameResultAI CurrentGameResult { get; set; }

    public RewardsAI RewardsAI;

    public bool Training;

    // cada jogada completa é uma tentativa, ou seja, a troca de 2 peças é uma tentativa
    // (usado apenas em treinamento para que este seja mais eficaz)
    public float MaxAttempts = 30;
    public float CurrentAttempts { get; set; }

    public int Turn { get; private set; }

    public int LastFirstChoice { get; set; }
    public int LastChoice { get; set; }

    public bool StopPlaying { get; set; }


    /* MÉTODOS */

    private void Start()
    {
        AgentsWorking = false;
        StopPlaying = false;
    }

    private void FixedUpdate()
    {
        if (StopPlaying)
        {
            return;
        }

        // troca os turno dos agentes, no fim de cada um,
        // usado apenas com os modelos já treinados
        if (!Training)
        {
            if (AgentsWorking && Player1.BehaviourParameters.BehaviorType != BehaviorType.HeuristicOnly)
            {
                if (CurrentPlayer == PlayerTypeAI.player1 && CurrentGameStatus == GameStatusAI.WaitingOnHuman)
                {
                    CurrentGameStatus = GameStatusAI.ReadyToMove;
                }
            }

            if (AgentsWorking && Player2.BehaviourParameters.BehaviorType != BehaviorType.HeuristicOnly)
            {
                if (CurrentPlayer == PlayerTypeAI.player2 && CurrentGameStatus == GameStatusAI.WaitingOnHuman)
                {
                    CurrentGameStatus = GameStatusAI.ReadyToMove;
                }
            }
        }

        // se o agente está pronto para realizar uma jogada,
        // por cada turno o agente toma 2 decisões,
        // 1º move uma peça e 2º verifica se resolveu o puzzle
        if (CurrentGameStatus == GameStatusAI.ReadyToMove && AgentsWorking)
        {
            CurrentGameStatus = GameStatusAI.PerformingMove;

            if (CurrentPlayer == PlayerTypeAI.player1)
            {
                RequestDecision(Player1);
            }
            else
            {
                RequestDecision(Player2);
            }
        }
        // se o agentes tomou a 1º decisão (escolheu um número e moveu a respetiva peça),
        // passa para a 2º decisão (verifica se resolveu o puzzle)
        else if (CurrentGameStatus == GameStatusAI.ObserveMove)
        {
            CurrentGameStatus = GameStatusAI.ObservingMove;

            if (CurrentPlayer == PlayerTypeAI.player1)
            {
                RequestDecision(Player1);
            }
            else
            {
                RequestDecision(Player2);
            }
        }
        // se o agente tomou as 2 decisões no seu turno, pode então partir para a troca do turno
        else if (CurrentGameStatus == GameStatusAI.ChangePlayer)
        {
            CurrentGameStatus = GameStatusAI.ChangingPlayer;
            ChangePlayer();
        }
        // se o 2º agente resolveu o puzzle (porque só verifica no 2º agente que é quando a troca de peças é feita),
        // pode então atribuir as recompensas finais para que o jogo termine
        else if (CurrentGameStatus == GameStatusAI.GiveRewards)
        {
            CurrentGameStatus = GameStatusAI.GiveRewards;
            AgentsWorking = false;

            Player2.PlayerStatusAI = PlayerStatusAI.Resetting;
            Player1.PlayerStatusAI = PlayerStatusAI.Resetting;

            GiveRewards();
        }
        // se o puzzle foi resolvido ou ficou sem tentativas, em vez de trocar de turnos, o jogo termina
        else if (CurrentGameStatus == GameStatusAI.FinalObservation)
        {
            CurrentGameStatus = GameStatusAI.MakingFinalObservation;

            RequestDecision(Player1);
            RequestDecision(Player2);
        }
        // depois de o puzzle estar resolvido ou ficar sem tentativas e os agentes não tenhem mais ações a tomar,
        // se estiver em treinamento o jogo reinicia senão termina por definitivo
        else if (Player1.PlayerStatusAI == PlayerStatusAI.MadeFinalObservation && Player2.PlayerStatusAI == PlayerStatusAI.MadeFinalObservation)
        {
            Player1.PlayerStatusAI = PlayerStatusAI.EndingGame;
            Player2.PlayerStatusAI = PlayerStatusAI.EndingGame;
            CurrentGameStatus = GameStatusAI.EndingGame;

            EndGame();

            if (Training)
            {
                RestartGame();
            }
            else
            {
                StopPlaying = true;
                Destroy(this);
            }
        }
    }

    public void StartGame()
    {
        if (Training)
        {
            StartGameForTraining();
        }
        else
        {
            StartGameWithTrainedModels();
        }
    }

    public void StartGameForTraining()
    {
        Player2.PlayerStatusAI = PlayerStatusAI.Working;
        Player1.PlayerStatusAI = PlayerStatusAI.Working;

        InitialiseGame();
        AgentsWorking = true;
    }

    public void StartGameWithTrainedModels()
    {
        Player2.PlayerStatusAI = PlayerStatusAI.Working;
        Player1.PlayerStatusAI = PlayerStatusAI.Working;

        InitialiseGame();
        AgentsWorking = true;

        if (Player1.BehaviourParameters.BehaviorType == BehaviorType.HeuristicOnly)
        {
            if (Player1.BehaviourParameters.Model != null)
            {
                Player1.BehaviourParameters.BehaviorType = BehaviorType.InferenceOnly;
            }
            else
            {
                Debug.LogError("Nenhum modelo carregado para o player 1.");
            }
        }
        else
        {
            Player1.BehaviourParameters.BehaviorType = BehaviorType.HeuristicOnly;
        }

        if (Player2.BehaviourParameters.BehaviorType == BehaviorType.HeuristicOnly)
        {
            if (Player2.BehaviourParameters.Model != null)
            {
                Player2.BehaviourParameters.BehaviorType = BehaviorType.InferenceOnly;
            }
            else
            {
                Debug.LogError("Nenhum modelo carregado para o player 2.");
            }
        }
        else
        {
            Player2.BehaviourParameters.BehaviorType = BehaviorType.HeuristicOnly;
        }

        CurrentGameStatus = GameStatusAI.WaitingOnHuman;
    }

    private void InitialiseGame()
    {
        CurrentGameResult = GameResultAI.notSolved;

        LastFirstChoice = -1;
        LastChoice = -1;

        if (Training)
        {
            CurrentAttempts = MaxAttempts;
            CurrentGameStatus = GameStatusAI.ReadyToMove;
        }
        else
        {
            CurrentGameStatus = GameStatusAI.WaitingToStart;
        }

        CurrentPlayer = PlayerTypeAI.player1;

        Turn = 0;
    }

    public bool[] GetAvailablePiecesToOrder()
    {
        bool[] availablePiecesToOrder = new bool[10];

        for (int i = 0; i < PuzzleManager.Pieces.Count; i++)
        {
            if (PuzzleManager.Pieces[i].position != i + 1)
            {
                availablePiecesToOrder[i + 1] = true;
            }
        }

        return availablePiecesToOrder;
    }

    public bool CheckCorrectPieceExchange(int firstChoice, int secondChoice)
    {
        int firstPosition = PuzzleManager.Pieces[firstChoice - 1].position;
        int secondPosition = PuzzleManager.Pieces[secondChoice - 1].position;

        if (firstPosition == secondChoice)
        {
            return true;
        }
        else if (secondPosition == firstChoice)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckSameChoice(int firstChoice, int secondChoice)
    {
        if (firstChoice == secondChoice)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> PlacePiece(int chosenNumber)
    {
        if (!PuzzleManager.CheckValidPlay(chosenNumber))
        {
            return false;
        }

        if (CurrentPlayer == PlayerTypeAI.player1)
        {
            await PuzzleManager.MoveFirstPiece(chosenNumber);
        }
        else if (CurrentPlayer == PlayerTypeAI.player2)
        {
            await PuzzleManager.MoveSecondPiece(chosenNumber);
        }

        return true;
    }

    private void ChangePlayer()
    {
        Turn += 1;

        if (CurrentPlayer == PlayerTypeAI.player1)
        {
            CurrentPlayer = PlayerTypeAI.player2;
        }
        else
        {
            CurrentPlayer = PlayerTypeAI.player1;
        }

        if (Training)
        {
            CurrentGameStatus = GameStatusAI.ReadyToMove;
        }
        else
        {
            CurrentGameStatus = GameStatusAI.WaitingOnHuman;
        }
    }

    public GameResultAI CheckGameStatusAI()
    {
        bool isSolved = PuzzleManager.CheckPuzzleSolved();
        bool noMoreAttempts = false;

        if (Training)
        {
            noMoreAttempts = ChecNoMoreAttempts();
        }

        if (isSolved)
        {
            return GameResultAI.solved;
        }
        else if (noMoreAttempts)
        {
            return GameResultAI.noMoreAttempts;
        }
        else
        {
            return GameResultAI.notSolved;
        }
    }

    public bool ChecNoMoreAttempts()
    {
        if (CurrentAttempts == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void RequestDecision(PlayerAI player)
    {
        player.RequestDecision();
    }

    public void GiveRewards()
    {
        if (CurrentGameResult == GameResultAI.solved)
        {
            Player1.AddReward(RewardsAI.HAS_PUZZLE_SOLVED);
            Player2.AddReward(RewardsAI.HAS_PUZZLE_SOLVED);
        }
        else if (CurrentGameResult == GameResultAI.noMoreAttempts)
        {
            Player1.AddReward(RewardsAI.NOT_HAS_PUZZLE_SOLVED);
            Player2.AddReward(RewardsAI.NOT_HAS_PUZZLE_SOLVED);
        }
        else if (CurrentGameResult == GameResultAI.notSolved)
        {
            Player1.AddReward(RewardsAI.NOT_HAS_PUZZLE_SOLVED);
            Player2.AddReward(RewardsAI.NOT_HAS_PUZZLE_SOLVED);
        }

        CurrentGameStatus = GameStatusAI.FinalObservation;
    }

    public void EndGame()
    {
        Player1.EndEpisode();
        Player2.EndEpisode();
    }

    public void RestartGame()
    {
        Player1.PlayerStatusAI = PlayerStatusAI.Working;
        Player2.PlayerStatusAI = PlayerStatusAI.Working;

        PuzzleManager.ShufflePuzzle();

        InitialiseGame();
        AgentsWorking = true;
    }

    public void ResetGame()
    {
        AgentsWorking = false;

        Player2.EpisodeInterrupted();
        Player1.EpisodeInterrupted();
    }
}