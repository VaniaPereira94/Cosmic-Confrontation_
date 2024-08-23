using UnityEngine;

public class AimTargetScript : MonoBehaviour
{
    [SerializeField]
    private Camera m_Camera;

    void Update()
    {
        /*
        Ray ray =m_Camera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            transform.position = raycastHit.point;
        }*/
        /*Vector3 mousePos = Input.mousePosition;
        mousePos.z = 100f;
        mousePos = m_Camera.WorldToScreenPoint(mousePos);
        Debug.DrawRay(transform.position, mousePos - transform.position, Color.blue);*/
    }
}