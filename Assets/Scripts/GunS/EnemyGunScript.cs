using UnityEngine;
using UnityEngine.Serialization;

public class EnemyGunScript : MonoBehaviour
{
    [FormerlySerializedAs("Spawn Point")]
    [SerializeField]
    protected Transform spawnPoint;

    [SerializeField]
    protected GameObject laser;

    [SerializeField]
    protected GameObject player;

    [SerializeField]
    protected Animator animator;

    [SerializeField]
    private GameObject muzzlePrefab;

    protected float speed = 10f;

    [SerializeField]
    protected float fireRate = 0;

    [SerializeField]
    private AudioClip laserShootSound;

    private AudioSource audioSource;

    protected float time = 0;

    protected CharacterBase character;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        character = player.GetComponentInParent<CharacterBase>();

        if (character.IsShooting)
        {
            ShootBullet();
        }
    }

    protected void ShootBullet()
    {
        time += Time.deltaTime;
        float nextTimeToFire = 1 / fireRate;

        if (time >= nextTimeToFire)
        {
            audioSource.PlayOneShot(laserShootSound);

            if (muzzlePrefab != null)
            {
                var muzzleVFX = Instantiate(muzzlePrefab, transform.position, transform.rotation);
                muzzleVFX.transform.SetParent(spawnPoint.transform);
                muzzleVFX.transform.forward = transform.forward;
            }

            GameObject cb = Instantiate(laser, spawnPoint.position, spawnPoint.transform.rotation);
            Rigidbody rb = cb.GetComponent<Rigidbody>();

            rb.AddForce(new Vector3(transform.forward.x, 0, transform.forward.z) * speed, ForceMode.Impulse);
            time = 0;
        }
    }
}