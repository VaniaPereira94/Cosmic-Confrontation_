using UnityEngine;

public class WaveSoundManager : MonoBehaviour
{
    private Transform _player;

    private AudioSource _audioSource;
    public float _maxVolume = 0.45f;
    public float _minVolume = 0.01f;
    public float _proximityRadius = 20f;

    void Start()
    {
        _player = Camera.main.transform;
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        UpdateVolume();
    }

    private void UpdateVolume()
    {
        float distance = Vector3.Distance(transform.position, _player.position);

        float volume = Mathf.Lerp(_minVolume, _maxVolume, 1 - (distance / _proximityRadius));
        volume = Mathf.Clamp01(volume);

        _audioSource.volume = volume;

        if (distance <= _proximityRadius)
        {
            // se está dentro do range
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
        }
        else
        {
            // se está fora do range
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }
    }
}