using System.Collections;
using UnityEngine;
using static Utils;

public class CharacterBase : MonoBehaviour
{
    protected bool _isDead = false;
    protected bool _isShooting;
    protected Animator _animator;
    protected float shootWeight = 0.0f;

    public bool IsDead
    {
        get { return _isDead; }
        set { _isDead = value; }
    }

    public bool IsShooting
    {
        get { return _isShooting; }
        set { _isShooting = value;}
    }

    public Animator Animator { 
        get { return _animator; } 
    }
}