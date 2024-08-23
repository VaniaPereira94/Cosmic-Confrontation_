using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [SerializeField]
    private Image _currentHealthImage;

    [SerializeField]
    private float _initialHealth = 100f;

    private float _currentHealth;

    public float Health { get { return _currentHealth; } }

    void Start()
    {
        _currentHealth = _initialHealth;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        UpdateBar();
    }

    public void TakeAllDamage()
    {
        _currentHealth = 0f;

        UpdateBar();
    }

    public void UpdateHealth(float healingAmount)
    {
        _currentHealth += healingAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _initialHealth);

        UpdateBar();
    }

    public void UpdateBar()
    {
        if (_currentHealthImage == null)
        {
            return;
        }

        _currentHealthImage.fillAmount = _currentHealth / _initialHealth;
    }

    public void restoreHealth() {
        _currentHealth = _initialHealth;
        UpdateBar();
    }
}