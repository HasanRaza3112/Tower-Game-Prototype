using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Death Settings")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private float deathEffectDuration = 2f;
    
    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;
    
    private float currentHealth;
    private bool isDead = false;
    
    private Camera mainCamera;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Spawn death effect
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, transform.rotation);
            Destroy(effect, deathEffectDuration);
        }
        
        OnDeath?.Invoke();
        
        // Remove from enemy spawner's active list
        EnemySpawner.Instance?.RemoveEnemy(gameObject);
        
        Destroy(gameObject);
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}
