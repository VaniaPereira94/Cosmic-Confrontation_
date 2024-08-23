/*
 * Para definir os diferentes tipos de estado do jogo.
*/
public enum GameState
{
    INTRO_GAME,     // quando passa do menu para a cutscene inicial
    HIDE_SHIP,      // quando est� � procura da nave
    GO_TO_FOREST,   // quando est� � procura do caminho para a floresta
    INTRO_FOREST,   // quando passa da praia para a floresta
    GO_TO_CAMP,     // quando segue o caminho da floresta at� ao acampamento
    INTRO_CAMP,     // quando passa de antes do acampamento para depois da cutscene do acampamento
    GO_TO_CAVE,     // quando segue o caminho depois do acampamento at� � entrada da caverna
    INTRO_CAVE,     // quando passa da floresta para a caverna
    GO_TO_MAZE,     // quando segue o caminho da caverna at� ao labirinto
    GO_TO_PYRAMID,  // quando segue o caminho depois do labirinto at� � pir�mide
    SOLVE_PUZZLE,   // quando inicia o puzzle para entrar na pir�mide
    INTRO_PYRAMID,  // quando passa do caminho armadilhado para a pir�mide
    PICK_TREASURE,  // quando tem de derrotar os inimigos e pegar no tesouro
    FINISH_GAME,    // quando passa de pegar o tesouro para a cutscene final
}