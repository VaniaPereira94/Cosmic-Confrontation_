using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController character;
    private Animator animator;
    private Vector3 inputs;

    private float speed = 2f;

    private float shootWeight = 0.0f;

    void Start()
    {
        character = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        inputs.Set(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        character.Move(inputs * Time.deltaTime * speed);
        character.Move(Vector3.down * Time.deltaTime);

        //Debug.Log("DIst: " + transform.position);
        //Debug.Log("Forward: " + transform.forward);

        if (inputs != Vector3.zero)
        {
            animator.SetBool("isWalking", true);
            transform.forward = Vector3.Slerp(transform.forward, inputs, Time.deltaTime * 10);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        float fadeTime = Input.GetButton("Fire1") ? 1.0f : 0.0f;

        shootWeight = Mathf.Lerp(shootWeight, fadeTime, 0.05f);
        animator.SetLayerWeight(animator.GetLayerIndex("Shoot"), shootWeight);
    }
}