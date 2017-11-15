using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animal : MonoBehaviour
{
    private Island island;

    private Animator animator;

    private NavMeshAgent agent;

    public enum SpiritType
    {
        NONE,
        RED,

        COUNT,
    };
    public SpiritType spiritType;

    private enum State
    {
        IDLE,
        WALKING,
    };
    private State state;

    public float idleTime;
    private float currentIdleTime;

    public List<SpiritOrb> orbsDropped;

    public void Initialize()
    {
        island = transform.parent.GetComponent<Island>();

        animator = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();

        SetNewDestination();
    }

    private void Update()
    {
        switch(state)
        {
            case State.IDLE:
                {
                    currentIdleTime += Time.deltaTime;
                    if(currentIdleTime >= idleTime)
                    {
                        currentIdleTime = 0.0f;

                        SetNewDestination();
                    }
                    break;
                }
            case State.WALKING:
                {
                    if (agent.isStopped || agent.isPathStale || (agent.destination - transform.position).magnitude < agent.stoppingDistance)
                    {
                        state = State.IDLE;
                    }

                    break;
                }
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    private void SetNewDestination()
    {
        state = State.WALKING;

        agent.SetDestination(island.FindNavMeshPoint());
    }

    public void OnHit()
    {
        OnDeath();
    }

    private void OnDeath()
    {
        //Instantiate(orbsDropped[Random.Range(0, orbsDropped.Count)], transform.position, Quaternion.identity, transform.parent);

        Destroy(gameObject);
    }

    static public int GetSpiritTypeMask(SpiritType type)
    {
        int returningMask = -1;

        switch (type)
        {
            case Animal.SpiritType.NONE:
                {
                    returningMask = LayerMask.GetMask("Spirit_None");
                    break;
                }
            case Animal.SpiritType.RED:
                {
                    returningMask = LayerMask.GetMask("Spirit_Red");
                    break;
                }
        }

        return returningMask;
    }
}