using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(NavMeshAgent))]
public class Ghoul : AbstractMultiWorld
{
    private enum State
    {
        Idle,
        Wandering,
        Searching,
        Chasing,
        Attacking,
        Dying
    }

    // Custom Variables
    [Header("AI Properties")]
    [Range(60, 180)]
    public float sightAreaAngle = 120;
    [Range(0, 5)]
    public float searchTime = 3;
    [Range(0, 5)]
    public float idleTime = 3;
    [Range(0, 5)]
    public float wanderDistance = 3;
    [Range(0.5f, 2)]
    public float attackDistance = 0.8f;
    [Range(0, 2)]
    public float attackDelay = 0.1f;
    [Range(0, 5)]
    public float attackCooldown = 1.5f;
    [Header("Entity Properties")]
    [SerializeField]
    private float damage = 1;
    [SerializeField]
    private float health = 3;

    // Reference Variables
    private SphereCollider sightArea
    {
        get { return GetComponent<SphereCollider>(); }
    }
    private NavMeshAgent nav
    {
        get { return GetComponent<NavMeshAgent>(); }
    }
    private BoxCollider attackCol
    {
        get { return GetComponentInChildren<BoxCollider>(); }
    }
    // <Prototype Purposes>
    private MeshRenderer armMesh
    {
        get { return transform.FindChild("Arm").GetComponent<MeshRenderer>(); }
    }
    // </Prototype Purposes>

    // Object Variables
    private State state;
    private Vector3 initialPoint;
    
    private void Awake()
    {
        initialPoint = transform.position;
        StartCoroutine(Idle());
    }

    /// <summary>
    /// Checks if the player is in range and is visible by this entity
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (state < State.Chasing && other.tag == "Player")
        {
            Vector3 direction = other.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);
            if (angle < sightAreaAngle / 2)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up / 2, direction.normalized, out hit, sightArea.radius))
                {
                    if (hit.collider.tag == "Player")
                        StartCoroutine(Chase(hit.transform, hit.collider));
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (state == State.Chasing && other.tag == "Player")
            StartCoroutine(Search(other.transform));
    }

    /// <summary>
    /// Deals the amount of damage to this object
    /// </summary>
    /// <param name="amount">The amount of damage recieved</param>
    private void TakeDamage(int amount)
    {
        if (health >= 0)
        {
            health -= amount;
            if (health == 0)
                StartCoroutine(Die());
;        }
    }

    /// <summary>
    /// Continuously chase the target
    /// </summary>
    /// <param name="target">The target position to chase</param>
    /// <param name="targetCol">The targeted collider to chase</param>
    private IEnumerator Chase(Transform target, Collider targetCol)
    {
        state = State.Chasing;
        while(state == State.Chasing)
        {
            if (attackCol.bounds.Intersects(targetCol.bounds) && Vector3.Distance(transform.position, target.position) < attackDistance)
                StartCoroutine(Attack(target.gameObject));
            else
                nav.destination = target.position;        
            yield return null;
        }
    }

    /// <summary>
    /// After the target exits the sight area, try to reach it for 
    /// 3 seconds, then if fail, get back to the starting position
    /// </summary>
    private IEnumerator Search(Transform target)
    {
        state = State.Searching;
        float time = 0;
        while(state == State.Searching && time < searchTime)
        {
            time += Time.deltaTime;
            nav.destination = target.position;
            yield return null;
        }

        if (state == State.Searching)
        {
            nav.destination = transform.position;
            yield return new WaitForSeconds(searchTime);
            StartCoroutine(Wander(initialPoint));
        }
    }

    /// <summary>
    /// Makes the entity walk toward the given position
    /// </summary>
    private IEnumerator Wander(Vector3 target)
    {
        state = State.Wandering;
        nav.destination = target;
        while(state == State.Wandering && Vector3.Distance(transform.position, target) > 0.1f)
            yield return null;

        if (state == State.Wandering)
            StartCoroutine(Idle());
    }

    /// <summary>
    /// Waits for a period of time, and then starts to walk randomly
    /// </summary>
    private IEnumerator Idle()
    {
        state = State.Idle;
        float time = 0;
        while (state == State.Idle && time < idleTime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (state == State.Idle)
            StartCoroutine(Wander(RandomNavPosition()));
    }

    /// <summary>
    /// Attacks the target
    /// </summary>
    /// <param name="target">The attack target</param>
    private IEnumerator Attack(GameObject target)
    {
        state = State.Attacking;
        nav.destination = transform.position;
        Color initColor = armMesh.material.color; // Prototype Feedback
        Collider targetCol = target.GetComponent<Collider>();
        float time = 0;
        float percentage = 0;
        while(state == State.Attacking && time < attackDelay)
        {
            time += Time.deltaTime;
            percentage = time / attackDelay;
            armMesh.material.color = initColor * (1 - percentage) + Color.red * percentage; // Prototype Feedback
            yield return null;
        }

        if (state == State.Attacking)
        {
            if (attackCol.bounds.Intersects(targetCol.bounds))
                target.SendMessage("TakeDamage", damage);
            yield return new WaitForSeconds(attackCooldown);
            StartCoroutine(Search(target.transform));         
        }
        armMesh.material.color = initColor;
    }

    private IEnumerator Die()
    {
        state = State.Dying;
        //float time = 0;
        Renderer rend = GetComponentInChildren<Renderer>();
        Color color = rend.material.color;
        while(color.a > 0)
        {
            color.a -= Time.deltaTime;
            rend.material.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Generates a random wander position
    /// </summary>
    /// <returns>A random destination</returns>
    private Vector3 RandomNavPosition()
    {
        Vector3 randomPlace = UnityEngine.Random.insideUnitSphere * wanderDistance;
        randomPlace += transform.position;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomPlace, out navHit, wanderDistance, -1);
        return navHit.position;
    }

    protected override void InitToggleWorlds()
    {
        print("Toggling");
    }

    protected override void ToggleWorlds()
    {
        print("Toggled");
    }
}
