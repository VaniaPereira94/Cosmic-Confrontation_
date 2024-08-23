using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private Light _light;

    [SerializeField] private float _minIntensity = 1.5f;
    [SerializeField] private float _maxIntensity = 2.5f;
    [SerializeField] private float _flickerSpeed = 3f;

    private float _originalIntensity = 2.5f;

    private void Start()
    {
        _originalIntensity = _light.intensity;

        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            // gera um valor aleatório entre minIntensity e maxIntensity
            float randomIntensity = Random.Range(_minIntensity, _maxIntensity);

            // aplica o valor aleatório como a intensidade da luz
            _light.intensity = randomIntensity;

            // aguarda por um curto período de tempo
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f) / _flickerSpeed);

            // restaura a intensidade original
            _light.intensity = _originalIntensity;

            // aguarda por um período de tempo
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f) / _flickerSpeed);
        }
    }
}