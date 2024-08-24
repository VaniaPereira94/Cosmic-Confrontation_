using System.Collections;
using UnityEngine;

public class DoorAnimationManager : MonoBehaviour
{
    [SerializeField] private float _endXPosition = 7.4f;
    [SerializeField] private float _movementSpeed = 0.5f;

    private Vector3 _initialPosition;
    private Vector3 _endPosition;

    private bool _startMoving = false;

    public bool StartMoving
    {
        get { return _startMoving; }
        set { _startMoving = value; }
    }

    void Start()
    {
        _initialPosition = transform.position;
        _endPosition = new Vector3(_endXPosition, _initialPosition.y, _initialPosition.z);
    }

    void Update()
    {
        if (_startMoving)
        {
            StartCoroutine(MoveDoorToRight());
        }
    }

    IEnumerator MoveDoorToRight()
    {
        float elapsedtime = 0f;

        while (elapsedtime < 1f)
        {
            elapsedtime += Time.deltaTime * _movementSpeed;

            transform.position = Vector3.Lerp(_initialPosition, _endPosition, elapsedtime);

            yield return null;
        }

        transform.position = _endPosition;

        _startMoving = false;
    }
}