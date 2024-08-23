using UnityEngine;
using UnityEngine.Video;

/*
 * Para armazenar as informações dos estados do jogo.
*/
[System.Serializable]
public class GameStateInfo
{
    public GameState gameState;             // o identificador do estado do jogo (deve ser único)
    public bool hasCutscene;                // se o estado do jogo tem cutscene
    public CutsceneType cutsceneType;       // o tipo de cutscene
    public VideoPlayer videoCutscene;       // a referência do objeto na cena para o vídeo da cutscene (externo: .mp4)
    public GameObject timelineCutscene;     // a referência do objeto (pai) na cena para o vídeo da cutscene (dentro do unity com a timeline)
    public bool hasNewPosition;             // se o jogador precisa de estar numa nova posição no mapa
    public Vector3 position;                // a posição inicial do jogador nesse estado
    public Vector3 rotation;                // a rotação inicial do jogador nesse estado
}