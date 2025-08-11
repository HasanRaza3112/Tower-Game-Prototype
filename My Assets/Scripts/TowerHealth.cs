using UnityEngine;
using UnityEngine.Events;

public class TowerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private bool canBeDestroyed = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private MeshRenderer towerRenderer;
    [SerializeField] private Color damagedColor = Color.red;
    
    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDestroyed;
    
    private float currentHealth;
    private Color originalColor;
    private bool isDestroyed = false;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        
        if (towerRenderer != null)
        {
            originalColor = towerRenderer.material.color;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDestroyed) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Visual feedback
        ShowDamageEffect();
        UpdateTowerAppearance();
        
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        
        if (currentHealth <= 0 && canBeDestroyed)
        {
            DestroyTower();
        }
    }
    
    public void Repair(float repairAmount)
    {
        if (isDestroyed) return;
        
        currentHealth += repairAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        UpdateTowerAppearance();
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }
    
    private void ShowDamageEffect()
    {
        if (damageEffect != null)
        {
            GameObject effect = Instantiate(damageEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }
    }
    
    private void UpdateTowerAppearance()
    {
        if (towerRenderer == null) return;
        
        float healthPercentage = currentHealth / maxHealth;
        
        if (healthPercentage < 0.3f)
        {
            // Tower is heavily damaged
            towerRenderer.material.color = Color.Lerp(damagedColor, originalColor, healthPercentage / 0.3f);
        }
        else
        {
            towerRenderer.material.color = originalColor;
        }
    }
    
    private void DestroyTower()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        
        // Spawn destroy effect
        if (destroyEffect != null)
        {
            GameObject effect = Instantiate(destroyEffect, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }
        
        OnDestroyed?.Invoke();
        
        // Disable tower components
        TowerFiring towerFiring = GetComponent<TowerFiring>();
        if (towerFiring != null)
        {
            towerFiring.enabled = false;
        }
        
        // Optionally destroy the tower or just disable it
        Destroy(gameObject, 1f);
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public bool IsDestroyed()
    {
        return isDestroyed;
    }
}
