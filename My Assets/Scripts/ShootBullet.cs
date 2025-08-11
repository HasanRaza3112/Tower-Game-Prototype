using Unity.Mathematics;
using UnityEngine;

public class ShootBullet : MonoBehaviour
{
    public GameObject projectile;
    public Transform firepoint;
    public float fireRate = 100f;
    public float bulletForce = 1000f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EnemyHealth targetEnemy = this.FindClosestEnemy();
            if (targetEnemy != null)
            {
                GameObject proj = Instantiate(projectile, firepoint.position, quaternion.identity);
                Vector3 direction = (targetEnemy.transform.position - firepoint.position).normalized;
                proj.GetComponent<Rigidbody>().AddForce(direction * bulletForce);
                Destroy(proj, 2f);
            }
        }
    }

    // This method finds the closest enemy to the firepoint
    EnemyHealth FindClosestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        EnemyHealth closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = firepoint.position;

        foreach (EnemyHealth enemy in enemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, currentPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy;
            }
        }
        return closest;
    }
}