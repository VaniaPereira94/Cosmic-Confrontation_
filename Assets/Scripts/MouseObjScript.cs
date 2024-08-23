using UnityEngine;

public class MouseObjScript : MonoBehaviour
{
    [SerializeField]
    private Camera m_Camera;

    [SerializeField]
    private GameObject crossHair;

    [SerializeField]
    private GameObject aimTarget;

    void Update()
    {
        if (aimTarget.active)
        {
            Vector3 screenCenter = new Vector3((Screen.width + Screen.width / 8) / 2f, (Screen.height - Screen.height / 8) / 2f, 100f);
            Vector3 worldCenter = m_Camera.ScreenToWorldPoint(screenCenter);
            aimTarget.transform.position = worldCenter;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnTriggerStay(Collider other)
    {

    }

    private void AvoidPlayer(Collider other)
    {
        GameObject.FindGameObjectWithTag("Player");
    }
}