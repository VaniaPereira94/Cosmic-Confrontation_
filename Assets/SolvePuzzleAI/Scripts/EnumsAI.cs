/*
 * Define os 2 agentes utilizados para resolver o puzzle.
 */
public enum PlayerTypeAI
{
    player1,    // escolhe a 1º peça
    player2     // escolhe a 2º peça
}

/*
 * O estado atual dos agentes.
 */
public enum PlayerStatusAI
{
    WakingUp,
    Ready,
    Working,
    MadeFinalObservation,
    EndingGame,
    Resetting
}

/*
 * O estado atual do jogo (resolver o puzzle).
 */
public enum GameStatusAI
{
    WaitingToStart,
    WaitingOnHuman,
    ReadyToMove,
    PerformingMove,
    ObserveMove,
    ObservingMove,
    ChangePlayer,
    ChangingPlayer,
    GiveRewards,
    GivingRewards,
    FinalObservation,
    MakingFinalObservation,
    EndingGame
}

/*
 * O resultado atual do jogo (resolver o puzzle).
 */
public enum GameResultAI
{
    notSolved,          // quando ainda não resolveu o puzzle
    noMoreAttempts,     // quando esgotam-se as tentivas e perde
    solved              // quando resolve o puzzle
}