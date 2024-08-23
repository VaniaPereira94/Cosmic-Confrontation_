using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public class PlayerAI : Agent
{
    /* ATRIBUTOS */

    public PlayerTypeAI Player;
    public PlayerStatusAI PlayerStatusAI { get; set; }

    public BehaviorParameters BehaviourParameters { get; private set; }
    public VectorSensorComponent VectorSensorComponent { get; set; }

    public PuzzleManager PuzzleManager;
    public BoardAI BoardAI;


    /* M�TODOS */

    private void Start()
    {
        VectorSensorComponent = GetComponent<VectorSensorComponent>();
        BehaviourParameters = GetComponent<BehaviorParameters>();
        PlayerStatusAI = PlayerStatusAI.WakingUp;
    }

    public override void Initialize()
    {
        PlayerStatusAI = PlayerStatusAI.Ready;
    }

    public override void OnEpisodeBegin()
    {
        PlayerStatusAI = PlayerStatusAI.Ready;
    }

    /*
     * Define informa��es que o agente recebe sobre o ambiente e o estado do jogo atual, antes de tomar uma decis�o.
     */
    public override void CollectObservations(VectorSensor sensor)
    {
        // observa��es para as posi��es atuais das pe�as no puzzle
        foreach (PuzzlePiece piece in PuzzleManager.Pieces)
        {
            VectorSensorComponent.GetSensor().AddObservation(piece.piece.transform.position);
            VectorSensorComponent.GetSensor().AddObservation(piece.position);
        }

        // observa��es para o saber o jogador atual
        if (BoardAI.CurrentPlayer == PlayerTypeAI.player1)
        {
            VectorSensorComponent.GetSensor().AddObservation(new float[] { 1, 0 });
        }
        else if (BoardAI.CurrentPlayer == PlayerTypeAI.player2)
        {
            VectorSensorComponent.GetSensor().AddObservation(new float[] { 0, 1 });
        }

        // observa��es para o saber a �ltima escolha de um n�mero
        // (-1 se escolha ainda n�o foi definida)
        if (BoardAI.LastChoice != -1)
        {
            float[] oneHotChoice = new float[PuzzleManager.Pieces.Count];
            oneHotChoice[BoardAI.LastChoice - 1] = 1;
            VectorSensorComponent.GetSensor().AddObservation(oneHotChoice);
        }
        else
        {
            VectorSensorComponent.GetSensor().AddObservation(new float[PuzzleManager.Pieces.Count]);
        }
    }

    /*
     * Cada agente toma 2 decis�es por turno.
     * Cada decis�o � uma a��o (n�mero poss�vel que a IA pode escolher).
     * 1� decis�o: escolhe o n�mero da pe�a (a��o entre 1 a 9),
     * mas usamos "availablePiecesToOrder" pois s� � preciso escolher as pe�as que ainda n�o est�o no s�tio correto.
     * 2� decis�o: verifica se resolveu o puzzle, etc. (representado pela a��o 10).
     * Portanto na 1� decis�o s� s�o aceites n�meros entre 1 a 9, e na 2� decis�o apenas o 10.
    */
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        bool[] availablePiecesToOrder = BoardAI.GetAvailablePiecesToOrder();

        if (BoardAI.CurrentGameStatus == GameStatusAI.PerformingMove)
        {
            for (int i = 1; i < availablePiecesToOrder.Length; i++)
            {
                actionMask.SetActionEnabled(0, 0, false);
                actionMask.SetActionEnabled(0, i, availablePiecesToOrder[i]);
                actionMask.SetActionEnabled(0, 10, false);
            }
        }
        else if (BoardAI.CurrentGameStatus == GameStatusAI.ObservingMove || BoardAI.CurrentGameStatus == GameStatusAI.MakingFinalObservation)
        {
            for (int i = 0; i <= PuzzleManager.Pieces.Count; i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }

            actionMask.SetActionEnabled(0, 10, true);
        }
    }

    /*
     * Determina as a��es a serem tomadas com base nas entradas fornecidas.
     * A fun��o da heuristica s� � chamada quando o treino � realizado no ambiente Python, apenas em fase de desenvolvimento.
     */
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;

        if (BoardAI.CurrentGameStatus == GameStatusAI.PerformingMove)
        {
            if (BoardAI.Training)
            {
                bool[] availableActions = BoardAI.GetAvailablePiecesToOrder();

                List<int> trueIndices = new List<int>();

                for (int i = 1; i < availableActions.Length; i++)
                {
                    if (availableActions[i])
                    {
                        trueIndices.Add(i);
                    }
                }

                int randomPiece = trueIndices[UnityEngine.Random.Range(0, trueIndices.Count)];

                discreteActionsOut[0] = randomPiece;
            }
        }
        else if (BoardAI.CurrentGameStatus == GameStatusAI.ObservingMove || BoardAI.CurrentGameStatus == GameStatusAI.MakingFinalObservation)
        {
            discreteActionsOut[0] = 10;
        }
    }

    /*
     * Executa de facto as a��es, ou seja, na 1� decis�o move uma pe�a e na 2� verifica se resolveu o puzzle.
     */
    public override async void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        if (action > 0 && action < 10)
        {
            bool correctPieceExchange = false;
            bool samePiece = false;

            BoardAI.LastChoice = action;

            if (Player == PlayerTypeAI.player1)
            {
                BoardAI.LastFirstChoice = action;
            }
            else if (Player == PlayerTypeAI.player2)
            {
                correctPieceExchange = BoardAI.CheckCorrectPieceExchange(BoardAI.LastFirstChoice, action);
                samePiece = BoardAI.CheckSameChoice(BoardAI.LastFirstChoice, action);
            }

            bool placedPiece = await BoardAI.PlacePiece(action);

            // se a jogada n�o � v�lida (n�mero inv�lido)
            if (!placedPiece)
            {
                BoardAI.ResetGame();
            }
            else
            {
                // atualiza o n�mero de tentativas para resolver o puzzle
                if (BoardAI.Training && Player == PlayerTypeAI.player2)
                {
                    BoardAI.CurrentAttempts -= 1;
                }

                BoardAI.CurrentGameResult = BoardAI.CheckGameStatusAI();

                // se n�o resolveu, continua a tentar resolver
                if (BoardAI.CurrentGameResult == GameResultAI.notSolved)
                {
                    if (Player == PlayerTypeAI.player2)
                    {
                        // atribui recompensas se a troca das pe�as tem alguma que fica na posi��o correta
                        if (correctPieceExchange)
                        {
                            BoardAI.Player1.AddReward(BoardAI.RewardsAI.CORRECT_PIECE_EXCHANGE);
                            BoardAI.Player2.AddReward(BoardAI.RewardsAI.CORRECT_PIECE_EXCHANGE);
                        }
                        else
                        {
                            BoardAI.Player1.AddReward(BoardAI.RewardsAI.WRONG_PIECE_EXCHANGE);
                            BoardAI.Player2.AddReward(BoardAI.RewardsAI.WRONG_PIECE_EXCHANGE);
                        }

                        // atribui recompensas se as pe�as s�o iguais ou diferentes
                        if (samePiece)
                        {
                            BoardAI.Player1.AddReward(BoardAI.RewardsAI.SAME_PIECE);
                            BoardAI.Player2.AddReward(BoardAI.RewardsAI.SAME_PIECE);
                        }
                        else
                        {
                            BoardAI.Player1.AddReward(BoardAI.RewardsAI.DIFFERENT_PIECE);
                            BoardAI.Player2.AddReward(BoardAI.RewardsAI.DIFFERENT_PIECE);
                        }
                    }

                    BoardAI.CurrentGameStatus = GameStatusAI.ObserveMove;
                }
                // se ficou sem tentativas, termina o puzzle
                else if (BoardAI.CurrentGameResult == GameResultAI.noMoreAttempts)
                {
                    BoardAI.CurrentGameStatus = GameStatusAI.GiveRewards;
                }
                // se resolveu, termina o puzzle
                else if (BoardAI.CurrentGameResult == GameResultAI.solved)
                {
                    BoardAI.CurrentGameStatus = GameStatusAI.GiveRewards;
                }
            }
        }
        else if (action == 10)
        {
            if (BoardAI.CurrentGameStatus == GameStatusAI.ObservingMove)
            {
                BoardAI.CurrentGameStatus = GameStatusAI.ChangePlayer;
            }
            else if (BoardAI.CurrentGameStatus == GameStatusAI.MakingFinalObservation)
            {
                PlayerStatusAI = PlayerStatusAI.MadeFinalObservation;
            }
        }
        else
        {
            BoardAI.ResetGame();
        }
    }
}