using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Utils;

public class EnemyScript : CharacterBase
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private GameObject droppableItem;

    [SerializeField]
    private float movingRadius = 5f;

    private float _minDistance = 3f;
    private float _maxDistance = 10f;

    private Vector3 inputs;
    private Vector3 destPoint;

    private bool hasDropped = false;
    private bool _isGroupAttack = false;
    private ThirdPersonMovement playerData;

    [Header("Health Manager")]
    [SerializeField]
    private HealthManager _healthManager;

    private NavMeshAgent _agent;
    private Vector3 _initialPosition;
    private EnemyStateMachine _stateManager;
    private string _currentSateName;

    private float timeWalking = 7f;
    private float currentTimeWalking = 0f;

    private GameManager gameManager;

    public NavMeshAgent Agent { 
        get { return _agent; } 
    }

    public string CurrentStateName { 
        get { return _currentSateName; }
    }


    void Start()
    {
        _animator = GetComponent<Animator>();
        playerData = player.GetComponent<ThirdPersonMovement>();
        _agent = GetComponent<NavMeshAgent>();
        _initialPosition = transform.position;
        _agent.speed = 5f;
        _stateManager = new EnemyStateMachine(this);
        _stateManager.Start();

        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (_isDead)
        {
            _stateManager.Update();
            return;
        } else if (_isGroupAttack)
        {
            _stateManager.Update();
            return;
        }

        Transform plTransform = player.transform;
        float playerDistance = Vector3.Distance(transform.position, plTransform.position);
        bool playerIsDead = playerData.IsDead;

        if (playerDistance > _minDistance && playerDistance < _maxDistance && !playerIsDead)
        {
            _currentSateName = EnemyStates.ATTACK_CHASE;
            SetState();

            if (_isGroupAttack) OnGroupAttack();
        }
        else if (playerDistance <= _minDistance && !playerIsDead)
        {
            _currentSateName = EnemyStates.ATTACK_IDLE;
            SetState();

            if (_isGroupAttack) OnGroupAttack();
        } 
        else if (!_isGroupAttack) 
        {
            if (currentTimeWalking < 0 || _agent.remainingDistance < 0.2f)
            {
                currentTimeWalking = timeWalking;
                _currentSateName = EnemyStates.IDLE;
                SetState();
                Invoke(nameof(OnStartPatrol), 2);
            }
            else
            {
                currentTimeWalking -= Time.deltaTime;
            }
        }

        _stateManager.Update();
    }

    public void SetRandomWalking()
    {
        destPoint = AIMovHelpers.GetDestinationPoint(_initialPosition, movingRadius);

        _agent.SetDestination(destPoint);
        transform.LookAt(destPoint);
    }

    private void OnStartPatrol()
    {
        _currentSateName = EnemyStates.PATROL;
        SetState();
        CancelInvoke(nameof(OnStartPatrol));
    }

    
    public void SetShootingAnimation(float fadeTime)
    {
        shootWeight = Mathf.Lerp(shootWeight, fadeTime, 0.1f);
        _animator.SetLayerWeight(_animator.GetLayerIndex(Utils.Constants.SHOOT), shootWeight);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isDead) return;

        Utils.CheckIfIsDead(collision, _healthManager, Utils.Constants.LAZER_BULLET_PLAYER, ref _isDead);

        if (_isDead ) _stateManager.TakeTransition(EnemyStates.DEATH);
        
    }

    private void DropItem()
    {
        GameObject medicineInstantiated = Instantiate(droppableItem, new Vector3(
            transform.position.x,
            transform.position.y + 0.2f,
            transform.position.z), Quaternion.identity);

        gameManager.addMedicine(medicineInstantiated);

        hasDropped = true;
    }

    public void StopShooting()
    {
        _animator.SetBool(Animations.SHOOTING, false);
        _isShooting = false;
    }

    public void FaceTarget()
    {
        Vector3 direction = (player.transform.position - transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Quaternion shootRotation = Quaternion.AngleAxis(20, transform.up) * lookRotation;

        transform.rotation = Quaternion.Slerp(transform.rotation, shootRotation, Time.deltaTime * 100);

        transform.rotation = Quaternion.AngleAxis(15, transform.up) * lookRotation;
        _agent.SetDestination(player.transform.position);
    }

    public void OnDead()
    {
        _agent.isStopped = true;
        StopShooting();
        PlayAnimation(_animator, Animations.DYING);
        GetComponent<CapsuleCollider>().enabled = false;
        if (!hasDropped) DropItem();
    }

    public void SetGroupState(string newStateName)
    {
        _stateManager.TakeTransition(newStateName);
        _isGroupAttack = EnemyStates.ATTACK_IDLE.Equals(newStateName) || EnemyStates.ATTACK_CHASE.Equals(newStateName);

        if (_isGroupAttack) currentTimeWalking = timeWalking;
    }

    public void SetState()
    {
        _stateManager.TakeTransition(_currentSateName);
    }

    private void OnGroupAttack()
    {
        currentTimeWalking -= Time.deltaTime;

        if (currentTimeWalking < 0)
        {
            _isGroupAttack = false;
        }
    }
}