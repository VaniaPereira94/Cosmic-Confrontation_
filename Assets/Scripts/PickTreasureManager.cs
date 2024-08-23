using System.Collections.Generic;
using UnityEngine;

public class PickTreasureManager : MonoBehaviour
{
    private GameManager _gameManager;

    [SerializeField] private List<GameObject> _enemiesInPyramid = new();

    private void Start()
    {
        _gameManager = GameManager.Instance;
    
    }

    /*
     * Permite saber quando morrem todos os inimigos na pirâmide (inclusive o comandante), para permitir apanhar o tesouro.
    */
    private void Update()
    {
        if (_gameManager.CurrentGameState.Value == GameState.PICK_TREASURE)
        {
            if (_enemiesInPyramid.Count == 0)
            {

                _gameManager.CurrentMapActions[0].hasClick = true;
                _gameManager.CurrentMapActions[0].button.SetActive(true);
                Destroy(this);
                return;
            }

            foreach (GameObject enemyInPyramid in _enemiesInPyramid)
            {
                if (enemyInPyramid.GetComponent<EnemyScript>().IsDead)
                {
                    _enemiesInPyramid.Remove(enemyInPyramid);
                    return;
                }
            }
        }
    }
}