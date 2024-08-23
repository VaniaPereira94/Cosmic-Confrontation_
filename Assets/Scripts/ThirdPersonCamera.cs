using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public GameObject player;
    public GameObject aimTarget;
    public Transform playerObj;
    public float rotationSpeed;

    public Transform combatLookAt;

    public GameObject thirdPersonCam;
    public GameObject combatCam;
    public GameObject focusOnPuzzleCam;

    [SerializeField]
    private GameObject crossHair;

    private CameraStyle currentStyle;

    public CameraStyle CurrentStye { get { return currentStyle; } }
    public enum CameraStyle
    {
        Basic,
        Combat,
        FocusOnPuzzle
    }

    private void Start()
    {
        currentStyle = CameraStyle.Basic;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        ThirdPersonMovement playerScript = player.GetComponent<ThirdPersonMovement>();

        if (playerScript.freeze)
        {
            return;
        }

        if (playerScript.IsDead || playerScript.IsPicking || Cursor.lockState == CursorLockMode.None)
        {
            return;
        }

        // switch styles
        if (focusOnPuzzleCam != null)
        {
            if (focusOnPuzzleCam.active)
            {
                return;
            }

            currentStyle = Input.GetButton(Utils.Constants.AIM_KEY) ? CameraStyle.Combat : CameraStyle.Basic;
            SwitchCameraStyle(currentStyle);
        }
        else
        {
            currentStyle = Input.GetButton(Utils.Constants.AIM_KEY) ? CameraStyle.Combat : CameraStyle.Basic;
            SwitchCameraStyle(currentStyle);
        }

        Transform playerTransform = player.transform;

        // rotate orientation
        Vector3 viewDir = playerTransform.position - new Vector3(transform.position.x, playerTransform.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        // roate player object
        if (currentStyle == CameraStyle.Basic)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }

        else if (currentStyle == CameraStyle.Combat)
        {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;

            playerObj.forward = dirToCombatLookAt.normalized;
        }
    }

    public void SwitchCameraStyle(CameraStyle newStyle)
    {
        combatCam.SetActive(false);
        thirdPersonCam.SetActive(false);
        crossHair.SetActive(false);

        if (focusOnPuzzleCam != null) focusOnPuzzleCam.SetActive(false);

        if (newStyle == CameraStyle.Basic) thirdPersonCam.SetActive(true);
        else if (newStyle == CameraStyle.Combat)
        {
            combatCam.SetActive(true);
            crossHair.SetActive(true);
        }
        else if (newStyle == CameraStyle.FocusOnPuzzle)
        {
            if (focusOnPuzzleCam != null) focusOnPuzzleCam.SetActive(true);
        }

        currentStyle = newStyle;
    }
}