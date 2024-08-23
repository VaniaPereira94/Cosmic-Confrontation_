using UnityEngine;
using UnityEngine.Animations.Rigging;
using static Utils;

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField]
    private GameObject playerMainComp;

    private AudioSource walkingSound;

    private Animator animator;
    private Vector3 inputs;

    private float shootWeight = 0.0f;

    private ThirdPersonMovement thirdPersonMovement;

    private int layerShootIdx;
    private bool _isShooting;

    private bool _freezeAllAnimations = false;

    public bool IsShooting
    {
        get { return _isShooting; }
    }

    public bool FreezeAllAnimations
    {
        get { return _freezeAllAnimations; }
        set { _freezeAllAnimations = value; }
    }

    public AudioSource WalkingSound
    {
        get { return walkingSound; }
    }


    void Start()
    {
        thirdPersonMovement = playerMainComp.GetComponent<ThirdPersonMovement>();
        animator = GetComponent<Animator>();
        layerShootIdx = animator.GetLayerIndex(Constants.SHOOT);
        walkingSound = GetComponent<AudioSource>();
    }

    void Update()
    {
        // impedir outras animações quando está a resolver o puzzle
        if (_freezeAllAnimations)
        {
            return;
        }

        // ---------- MORRER ----------
        if (thirdPersonMovement.IsDead)
        {
            PlayAnimation(animator, Animations.DYING);
            return;
        }
        else if (thirdPersonMovement.IsPicking)
        {
            PlayAnimation(animator, Animations.PICKING);
            return;
        }
        else if (thirdPersonMovement.IsGrabing)
        {
            PlayAnimation(animator, Animations.GRABING);
            return;
        }

        if (Cursor.lockState == CursorLockMode.None)
        {
            return;
        }

        animator.SetBool(Animations.DYING, false);
        animator.SetBool(Animations.PICKING, false);
        animator.SetBool(Animations.GRABING, false);

        // ---------- SALTAR ----------
        animator.SetBool(Animations.JUMPING, thirdPersonMovement.IsJumping);

        // ---------- Correr ----------
        animator.SetBool(Animations.RUNNING, thirdPersonMovement.IsRunning);

        // ---------- ANDAR ----------
        inputs.Set(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        bool isWalking = inputs != Vector3.zero;

        animator.SetBool(Animations.WALKING, isWalking);
        if (walkingSound != null) walkingSound.enabled = isWalking && !thirdPersonMovement.IsJumping;

        bool isAiming = Input.GetButton(Constants.AIM_KEY) || Input.GetButton(Constants.SHOOT_KEY);

        float fadeTime = isAiming ? 1.0f : 0.0f;

        shootWeight = Mathf.Lerp(shootWeight, fadeTime, 0.5f);
        animator.SetLayerWeight(layerShootIdx, shootWeight);

        animator.SetBool(Animations.SHOOTING, thirdPersonMovement.IsShooting);

        GameObject spineRotation = GameObject.FindGameObjectWithTag(Constants.SPINE_ROTATION);
        spineRotation.GetComponent<OverrideTransform>().weight = thirdPersonMovement.IsShooting ? 1f : 0f;

        Vector3 playerPosition = playerMainComp.transform.position;

        transform.position = playerPosition;
    }

    public void StopAllAnimations()
    {
        // parar animação de mirar
        shootWeight = 0;
        animator.SetLayerWeight(layerShootIdx, shootWeight);

        animator.SetBool(Animations.WALKING, false);
        animator.SetBool(Animations.DYING, false);
        animator.SetBool(Animations.PICKING, false);
        animator.SetBool(Animations.SHOOTING, false);
        animator.SetBool(Animations.JUMPING, false);
        animator.SetBool(Animations.GRABING, false);
        animator.SetBool(Animations.RUNNING, false);
        animator.SetBool(Animations.HOLE_CLOSING, false);
    }
}