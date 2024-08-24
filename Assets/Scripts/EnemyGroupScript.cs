using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

public class EnemyGroupScript : MonoBehaviour
{
    private List<GameObject> enemies;

    
    bool detectionActivated = true;
    // Start is called before the first frame update
    void Start()
    {
        enemies = transform.Cast<Transform>().Select(t => t.gameObject).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        bool isPlayerDetected = false;
        if (!detectionActivated) return;

        foreach (GameObject child in enemies)
        {
            EnemyScript enemyScript = child.GetComponent<EnemyScript>();

            isPlayerDetected = EnemyStates.ATTACK_IDLE.Equals(enemyScript.CurrentStateName) || EnemyStates.ATTACK_CHASE.Equals(enemyScript.CurrentStateName);

            if (isPlayerDetected) break;
        }

        

        if (!isPlayerDetected) return;

        foreach (GameObject child in enemies)
        {
            EnemyScript enemyScript = child.GetComponent<EnemyScript>();

            enemyScript.SetGroupState(Utils.EnemyStates.ATTACK_CHASE);
        }

        detectionActivated = false;
        ReactivateDetection();
    }

    private IEnumerator ReactivateDetection()
    {
        yield return new WaitForSeconds(30f);
        detectionActivated = true;
    }
}
