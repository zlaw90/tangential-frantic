using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public GameObject player;

    private NavMeshAgent navMeshAgent;

    public EnemyState state = EnemyState.Idle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == EnemyState.Idle)
        {
            BehaviorIdle();
        }
        if (state == EnemyState.Seek)
        {
            BehaviorSeek();
        }
        if (state == EnemyState.Attack)
        {
            navMeshAgent.SetDestination(player.transform.position);
        }
    }


    internal void BehaviorIdle()
    {
        // TODO check if enemy can detect player
        bool canDetectPlayer = true;
        if (canDetectPlayer)
        {
            state = EnemyState.Seek;
        }
    }

    internal void BehaviorSeek()
    {
        navMeshAgent.SetDestination(player.transform.position);
        var distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < 1)
        {
            state = EnemyState.Attack;
        }
        if (distance > 5)
        {
            state = EnemyState.Idle;
        }
    }

}

public enum EnemyState
{
    Idle,
    Seek,
    Attack
}