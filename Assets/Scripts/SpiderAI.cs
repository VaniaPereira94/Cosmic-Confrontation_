using UnityEngine;
using UnityEngine.AI;

public class SpiderAI : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool movingForward = true;

    public float moveSpeed;
    public float maxDistance;



    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
        SetRandomTarget();
    }

    void Update()
    {
        if (navMeshAgent.remainingDistance < 0.1f)
        {
            SetRandomTarget();
        }
    }
    void SetRandomTarget()
    {
        if (movingForward)
        {
            targetPosition = initialPosition + Random.insideUnitSphere * maxDistance;
        }
        else
        {
            targetPosition = initialPosition;
        }

        navMeshAgent.SetDestination(targetPosition);
        movingForward = !movingForward;
        animator.SetBool("isWalking", true);
    }
}

