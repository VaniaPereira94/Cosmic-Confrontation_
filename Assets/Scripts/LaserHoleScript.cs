using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserHoleScript : MonoBehaviour
{
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play(Utils.Animations.HOLE_CLOSING);
    }

    // Update is called once per frame
    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f)
        {
            // The animation has finished
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collider's GameObject has the specified tag
        if (collision.collider.CompareTag("LaserHole"))
        {
            // Do something (ignore the collision, for example)
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider, true);
        }
    }
}
