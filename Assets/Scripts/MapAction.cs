/*
 * Para armazenar as diversas a��es do mapa e o seu estado de jogo correspondente.
*/
using UnityEngine;

[System.Serializable]
public class MapAction
{
    public int id;                          // identificador da a��o
    public GameStateInfo gameStateInfo;     // uma a��o pertence a um estado do jogo
    public string title;                    // para quando for um objetivo, mostra no menu de pausa
    public bool hasProgress;                // quando a a��o � um objetivo concluido, quer dizer que progride no jogo
    public bool hasClick;                   // se a a��o � preciso pressionar 'F'
    public GameObject button;               // guarda o game object para que a��o seja clic�vel
    public bool hasDialogue;                // para determinar se o personagem ir� falar
    public GameObject dialogue;             // guarda o game object para que a personagem fale
    public bool isSingle;                   // se esta a��o s� pode ser feita exclusivamente uma vez
}