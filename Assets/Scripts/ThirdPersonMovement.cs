using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ThirdPersonMovement : CharacterBase
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    public float walkSpeed;
    public float airMinSpeed;
    public float runSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    private bool isItemToPick;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState currentState;

    public enum MovementState
    {
        freeze,
        unlimited,
        walking,
        air
    }

    public bool freeze;
    public bool unlimited;

    public bool restricted;

    bool keepMomentum;

    private Animator animator;

    private bool _isPicking = false;

    public bool IsPicking
    {
        get { return _isPicking; }
    }

    private bool _isGrabing = false;

    public bool IsGrabing
    {
        get { return _isGrabing; }
    }

    [Header("Health")]
    [SerializeField]
    private GameObject healthObject;
    private HealthManager _healthManager;

    public HealthManager HealthManager
    {
        get { return _healthManager; }
        private set { _healthManager = value; }
    }

    private bool _isJumping = false;

    public bool IsJumping
    {
        get { return _isJumping; }
    }

    private bool _isRunning = false;

    public bool IsRunning
    {
        get { return _isRunning; }
    }

    private string _currentEnvironment;

    public string CurrentEnvironment { get { return _currentEnvironment; } }

    private GameManager gameManager;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        _healthManager = healthObject.GetComponent<HealthManager>();
        HealthManager = _healthManager;

        animator = GetComponent<Animator>();

        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        if (freeze)
        {
            return;
        }

        if (_isDead)
        {
            _isShooting = false;
            return;
        };

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (currentState == MovementState.walking)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        if (freeze)
        {
            return;
        }

        if (_isDead || _isPicking)
        {
            return;
        };

        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        _isRunning = Input.GetKey(KeyCode.LeftShift);

        // when to jump
        if (Input.GetKeyDown(jumpKey) && grounded && !_isJumping)
        {
            _isJumping = true;
            Invoke(nameof(Jump), 0.4f);
        }

        _isShooting = Input.GetButton(Utils.Constants.SHOOT_KEY);
    }

    private void StateHandler()
    {
        // Mode - Freeze
        if (freeze)
        {
            currentState = MovementState.freeze;
            rb.velocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }

        // Mode - Unlimited
        else if (unlimited)
        {
            currentState = MovementState.unlimited;
            desiredMoveSpeed = 999f;
        }

        // Mode - Walking
        else if (grounded)
        {
            currentState = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            currentState = MovementState.air;

            if (moveSpeed < airMinSpeed)
                desiredMoveSpeed = airMinSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        if (grounded && _isRunning)
        {
            currentState = MovementState.walking;
            desiredMoveSpeed = runSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

        // deactivate keepMomentum
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (restricted) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        moveDirection.Normalize();

        float speedMultiplier = _isRunning ? runSpeed : walkSpeed;

        rb.AddForce(moveDirection.normalized * speedMultiplier, ForceMode.Acceleration);

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // on ground
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJump), jumpCooldown);
        CancelInvoke(nameof(Jump));
    }

    private void ResetJump()
    {
        _isJumping = false;

        exitingSlope = false;

        CancelInvoke(nameof(ResetJump));
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckMedicineCollision(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Utils.CheckIfIsDead(collision, _healthManager, Utils.Constants.LAZER_BULLET_ENEMY, ref _isDead);

        if (_isDead)
        {
            SetDeathCollider(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        CheckMedicineCollision(other);
    }

    private void OnCollisionStay(Collision collision)
    {
        var tag = collision.gameObject.tag;
        bool isTerrain = Utils.Environments.GetValues().Contains(tag);

        if (isTerrain)
        {
            _currentEnvironment = tag;
        }
    }

    private void CheckMedicineCollision(Collider collision)
    {
        if (Input.GetButton(Utils.Constants.PICK))
        {
            if (collision.gameObject.CompareTag(Utils.Constants.MEDICINE) && !_isPicking)
            {
                OnItemIter(collision, PickUpMedicine, out _isPicking);
            }

            if (collision.gameObject.CompareTag(Utils.Constants.TREASURE) && !_isGrabing)
            {
                OnItemIter(collision, GrabTreasure, out _isGrabing);
            }
        }

    }

    private void OnItemIter(Collider collision, Func<GameObject, IEnumerator> IterFunc, out bool var)
    {
        var = true;

        GameObject item = collision.gameObject.CloneViaSerialization();

        StartCoroutine(IterFunc(item));
        StartCoroutine(StopPickingAnimation());
    }

    IEnumerator PickUpMedicine(GameObject medicineAction)
    {
        yield return new WaitForSeconds(1f);

        Transform medicine = medicineAction.transform.GetChild(0);
        MedicineScript medicineScript = medicine.GetComponent<MedicineScript>();
        _healthManager.UpdateHealth(medicineScript.Health);

        gameManager.removeMedicine(medicineAction);

        Destroy(medicineAction);
    }

    IEnumerator GrabTreasure(GameObject treasure)
    {
        yield return new WaitForSeconds(1f);

        Destroy(treasure);
    }

    IEnumerator StopPickingAnimation()
    {
        yield return new WaitForSeconds(2f);
        _isPicking = false;
        _isGrabing = false;
    }

    public void SetDeathCollider(bool activate)
    {
        GameObject kachujinObj = GameObject.Find("Kachujin");
        kachujinObj.GetComponent<CapsuleCollider>().enabled = activate;
    }
}