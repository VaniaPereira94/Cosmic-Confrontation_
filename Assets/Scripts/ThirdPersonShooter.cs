using Cinemachine;
using UnityEngine;

public class ThirdPersonShooter : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera aimVirtualCamera;

    void Update()
    {
        Debug.Log(Input.GetButton("Fire3") ? "I'm the aim camera!" : "sd");
        aimVirtualCamera.gameObject.SetActive(Input.GetButton("Fire3"));
    }
}