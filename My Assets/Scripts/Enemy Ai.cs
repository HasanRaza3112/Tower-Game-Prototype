using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public Transform target;
     private NavMeshAgent agent;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        ChaseTarget(); // Chase the target which is player
    }
    void ChaseTarget()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}
