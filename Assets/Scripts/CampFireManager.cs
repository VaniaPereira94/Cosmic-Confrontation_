using UnityEngine;

public class BonfireController : MonoBehaviour
{
    private AudioSource audioSource;
    private Transform player;

    public float maxVolume = 0.6f;
    public float minVolume = 0.1f;
    public float proximityRadius = 5f;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        player = Camera.main.transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        float volume = Mathf.Lerp(minVolume, maxVolume, 1 - (distance / proximityRadius));
        volume = Mathf.Clamp01(volume);

        audioSource.volume = volume;

        if (distance <= proximityRadius)
        {
            // se está dentro do range
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // se está fora do range
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}