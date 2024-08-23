using System.Collections;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    protected bool _isDead = false;
    protected bool _isShooting;

    public bool IsDead
    {
        get { return _isDead; }
        set { _isDead = value; }
    }

    public bool IsShooting
    {
        get { return _isShooting; }
    }


}