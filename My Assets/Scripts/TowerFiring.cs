using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerFiring : MonoBehaviour
{
    [Header("Firing Settings")]
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float range = 10f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private LayerMask enemyLayer = -1;
    
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 15f;
    
    [Header("Targeting")]
    [SerializeField] private TargetingMode targetingMode = TargetingMode.Closest;
    
    [Header("Visual")]
    [SerializeField] private Transform towerHead; // The part that rotates
    [SerializeField] private float rotationSpeed = 90f;
    
    [Header("Manual Firing")]
    [SerializeField] private KeyCode fireKey = KeyCode.Space;
    [SerializeField] private bool showFireInstructions = true;
    
    private float lastFireTime;
    private GameObject currentTarget;
    private List<GameObject> enemiesInRange = new List<GameObject>();
    private SphereCollider rangeCollider;
    
    public enum TargetingMode
    {
        Closest,
        Furthest,
        Strongest,
        Weakest
    }
    
    private void Awake()
    {
        SetupRangeCollider();
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }
    
    private void SetupRangeCollider()
    {
        rangeCollider = gameObject.AddComponent<SphereCollider>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = range;
    }
    
    private void Update()
    {
        UpdateTargeting();
        RotateTowardsTarget();
        
        // Check for manual fire input
        if (Input.GetKeyDown(fireKey))
        {
            if (CanFire() && currentTarget != null)
            {
                Fire();
            }
            else if (currentTarget == null)
            {
                // Optional: Show message when no target available
                Debug.Log("No enemies in range to fire at!");
            }
        }
    }
    
    private void UpdateTargeting()
    {
        // Clean up null references
        enemiesInRange.RemoveAll(enemy => enemy == null);
        
        if (enemiesInRange.Count == 0)
        {
            currentTarget = null;
            return;
        }
        
        currentTarget = GetBestTarget();
    }
    
    private GameObject GetBestTarget()
    {
        if (enemiesInRange.Count == 0) return null;
        
        GameObject bestTarget = null;
        float bestValue = 0f;
        
        foreach (GameObject enemy in enemiesInRange)
        {
            if (enemy == null) continue;
            
            float value = EvaluateTarget(enemy);
            
            if (bestTarget == null || 
                (targetingMode == TargetingMode.Closest && value < bestValue) ||
                (targetingMode == TargetingMode.Furthest && value > bestValue) ||
                (targetingMode == TargetingMode.Strongest && value > bestValue) ||
                (targetingMode == TargetingMode.Weakest && value < bestValue))
            {
                bestTarget = enemy;
                bestValue = value;
            }
        }
        
        return bestTarget;
    }
    
    private float EvaluateTarget(GameObject enemy)
    {
        switch (targetingMode)
        {
            case TargetingMode.Closest:
            case TargetingMode.Furthest:
                return Vector3.Distance(transform.position, enemy.transform.position);
            
            case TargetingMode.Strongest:
            case TargetingMode.Weakest:
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                return enemyHealth != null ? enemyHealth.GetHealthPercentage() : 0f;
            
            default:
                return 0f;
        }
    }
    
    private void RotateTowardsTarget()
    {
        if (currentTarget == null || towerHead == null) return;
        
        Vector3 direction = (currentTarget.transform.position - towerHead.position).normalized;
        direction.y = 0; // Keep rotation only on Y-axis for medieval towers
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            towerHead.rotation = Quaternion.RotateTowards(towerHead.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private bool CanFire()
    {
        return Time.time >= lastFireTime + (1f / fireRate);
    }
    
    private void Fire()
    {
        lastFireTime = Time.time;
        
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            
            // Setup projectile
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(currentTarget, damage, projectileSpeed);
            }
            else
            {
                // If no Projectile script, just move towards target
                StartCoroutine(MoveProjectileToTarget(projectile));
            }
        }
        else
        {
            // Instant hit
            DealDamageToTarget();
        }
    }
    
    private IEnumerator MoveProjectileToTarget(GameObject projectile)
    {
        Vector3 startPos = projectile.transform.position;
        Vector3 targetPos = currentTarget.transform.position;
        
        float journeyTime = Vector3.Distance(startPos, targetPos) / projectileSpeed;
        float elapsedTime = 0;
        
        while (elapsedTime < journeyTime && projectile != null && currentTarget != null)
        {
            elapsedTime += Time.deltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;
            
            projectile.transform.position = Vector3.Lerp(startPos, currentTarget.transform.position, fractionOfJourney);
            
            yield return null;
        }
        
        if (projectile != null)
        {
            DealDamageToTarget();
            Destroy(projectile);
        }
    }
    
    private void DealDamageToTarget()
    {
        if (currentTarget == null) return;
        
        EnemyHealth enemyHealth = currentTarget.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            if (!enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Add(other.gameObject);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (enemiesInRange.Contains(other.gameObject))
        {
            enemiesInRange.Remove(other.gameObject);
            
            if (currentTarget == other.gameObject)
            {
                currentTarget = null;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
    
    // Visual feedback for manual firing
   /* private void OnGUI()
    {
        if (!showFireInstructions) return;
        
        // Get screen position of the tower
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        
        if (screenPos.z > 0) // Only show if tower is in front of camera
        {
            // Convert to screen coordinates
            float x = screenPos.x;
            float y = Screen.height - screenPos.y;
            
            // Show firing instructions
            string instructionText = currentTarget != null ? 
                $"Press {fireKey} to fire at enemy!" : 
                "No enemies in range";
            
            // Style for the instruction text
            GUIStyle style = new GUIStyle();
            style.normal.textColor = currentTarget != null ? Color.green : Color.red;
            style.fontSize = 14;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            
            // Draw background box
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(new Rect(x - 100, y - 30, 200, 60), "");
            
            // Draw instruction text
            GUI.color = Color.white;
            GUI.Label(new Rect(x - 100, y - 20, 200, 40), instructionText, style);
            
            // Draw key hint
            if (currentTarget != null)
            {
                style.fontSize = 12;
                style.normal.textColor = Color.yellow;
                GUI.Label(new Rect(x - 100, y + 10, 200, 20), $"Target: {currentTarget.name}", style);
            }
        }
    }*/
}
