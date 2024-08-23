/*
 * Para armazenar as pe�as do puzzle na parede, da entrada da pir�mide.
*/
using UnityEngine;

[System.Serializable]
public class PuzzlePiece
{
    public int position;        // posi��o da pe�a
    public GameObject piece;    // guarda o game object correspondente � pe�a na parede
}