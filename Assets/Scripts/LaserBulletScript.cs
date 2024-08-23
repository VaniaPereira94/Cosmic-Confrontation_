using UnityEngine;

public class LaserBulletScript : MonoBehaviour
{
    private CapsuleCollider capsuleCollider;

    [SerializeField]
    private float _damage = 20f;

    [SerializeField]
    private GameObject muzzlePrefab;

    [SerializeField]
    private GameObject hitPrefab;

    [SerializeField]
    private GameObject holePrefab;

    public float Damage { get { return _damage; } }

    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();

        Destroy(gameObject, 1);
    }


    private void OnCollisionEnter(Collision collision)
    {


        RaycastHit hit;

        ContactPoint[] contactPoints = collision.contacts;

        Physics.Raycast(transform.position, transform.forward, out hit);

        if (hitPrefab != null)
        {
            Instantiate(hitPrefab, transform.position, transform.rotation);
        }

        if (collision.collider.CompareTag("LaserHole"))
        {
            // Do something (ignore the collision, for example)
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider, true);
            Destroy(gameObject);
            return;
        }

        if (holePrefab != null)
        {
            Vector3 colliderPosition = transform.TransformPoint(capsuleCollider.center);
            GameObject holeInstance = Instantiate(holePrefab, colliderPosition, Quaternion.Euler(0, 0, 0));

            //holeInstance.transform.SetParent(collision.gameObject.transform);
        }

        Destroy(gameObject);
    }
}