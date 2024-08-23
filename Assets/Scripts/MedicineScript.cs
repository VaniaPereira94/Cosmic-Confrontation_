using UnityEngine;

public class MedicineScript : MonoBehaviour
{
    [SerializeField]
    private float health;

    public float Health
    {
        get { return health; }
    }
}