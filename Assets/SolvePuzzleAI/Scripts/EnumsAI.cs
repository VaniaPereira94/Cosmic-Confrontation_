/*
 * Define os 2 agentes utilizados para resolver o puzzle.
 */
public enum PlayerTypeAI
{
    player1,    // escolhe a 1� pe�a
    player2     // escolhe a 2� pe�a
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
    notSolved,          // quando ainda n�o resolveu o puzzle
    noMoreAttempts,     // quando esgotam-se as tentivas e perde
    solved              // quando resolve o puzzle
}