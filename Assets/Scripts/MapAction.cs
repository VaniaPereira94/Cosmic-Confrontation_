/*
 * Para armazenar as diversas ações do mapa e o seu estado de jogo correspondente.
*/
using UnityEngine;

[System.Serializable]
public class MapAction
{
    public int id;                          // identificador da ação
    public GameStateInfo gameStateInfo;     // uma ação pertence a um estado do jogo
    public string title;                    // para quando for um objetivo, mostra no menu de pausa
    public bool hasProgress;                // quando a ação é um objetivo concluido, quer dizer que progride no jogo
    public bool hasClick;                   // se a ação é preciso pressionar 'F'
    public GameObject button;               // guarda o game object para que ação seja clicável
    public bool hasDialogue;                // para determinar se o personagem irá falar
    public GameObject dialogue;             // guarda o game object para que a personagem fale
    public bool isSingle;                   // se esta ação só pode ser feita exclusivamente uma vez
}