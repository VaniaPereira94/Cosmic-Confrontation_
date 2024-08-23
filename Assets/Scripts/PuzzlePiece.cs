/*
 * Para armazenar as peças do puzzle na parede, da entrada da pirâmide.
*/
using UnityEngine;

[System.Serializable]
public class PuzzlePiece
{
    public int position;        // posição da peça
    public GameObject piece;    // guarda o game object correspondente à peça na parede
}