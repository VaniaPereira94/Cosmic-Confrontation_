using UnityEngine;

public class BoobyTrappedFloorManager : MonoBehaviour
{
    [SerializeField] private bool _isTrap;

    /*
     * Verifica se o chão é armadilha ou não.
     * Se sim, o jogador morre.
    */
    void OnCollisionEnter(Collision other)
    {
        if (_isTrap && other.gameObject.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
        }
    }
}