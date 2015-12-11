using UnityEngine;
using System.Collections;

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
    private float damage = 10;
    [SerializeField]
    private float health = 3;
    public AudioClip atkSwing;

    // Reference Variables
    private SphereCollider sightArea
    {
        get { return GetComponentInChildren<SphereCollider>(); }
    }
    private NavMeshAgent nav
    {
        get { return GetComponent<NavMeshAgent>(); }
    }
    private BoxCollider attackCol
    {
        get { return GetComponentInChildren<BoxCollider>(); }
    }
    private AudioSource source
    {
        get { return GetComponent<AudioSource>(); }
    }
    private MeshRenderer bodyMesh
    {
        get { return transform.FindChild("Body").GetComponent<MeshRenderer>(); }
    }

    // Object Variables
    private State state;
    private Coroutine hitRoutine;
    private Vector3 initialPoint;

    private void Awake()
    {
        if (spirit)
            ToggleWorlds();
        initialPoint = transform.position;
        StartCoroutine(Idle());
    }

    /// <summary>
    /// Checks if the player is in range and is visible by this entity
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (spiritRealm && state < State.Chasing && other.tag == "Player")
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
        if (spiritRealm && state == State.Chasing && other.tag == "Player")
            StartCoroutine(Search(other.transform));
    }

    /// <summary>
    /// Deals damage to this object and tells where the damage came from
    /// </summary>
    /// <param name="location">The location where the damage were inflicted</param>
    private void TakeDamage(Vector3 location)
    {
        if (health >= 0)
        {
            health --;
            if (health == 0)
            {
                StopAllCoroutines();
                StartCoroutine(Die());
            }
            else
            {
                if (hitRoutine != null)
                    StopCoroutine(hitRoutine);
                hitRoutine = StartCoroutine(HitFeedback());

                if (state < State.Searching)
                {
                    state = State.Wandering;
                    nav.destination = location;
                }
            }
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
            {
                if (!nav.isOnOffMeshLink)
                    nav.SetDestination(target.position);
            } 
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
            if (!nav.isOnOffMeshLink)
                nav.SetDestination(target.position);
            yield return null;
        }

        if (state == State.Searching)
        {
            nav.SetDestination(transform.position);
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
        nav.SetDestination(target);
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
        Collider targetCol = target.GetComponent<Collider>();

        yield return new WaitForSeconds(attackDelay);

        if (state == State.Attacking)
        {
            source.pitch = Random.Range(0.95f, 1.05f);
            source.PlayOneShot(atkSwing);

            if (attackCol.bounds.Intersects(targetCol.bounds))
                target.SendMessage("TakeDamage", damage);
            yield return new WaitForSeconds(attackCooldown);
            StartCoroutine(Search(target.transform));         
        }
    }

    /// <summary>
    /// Controls the dying phase of the entity
    /// </summary>
    private IEnumerator Die()
    {
        state = State.Dying;
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
    /// Paints the body renderer as red to give a hit feedback
    /// </summary>
    private IEnumerator HitFeedback()
    {
        bodyMesh.material.color = Color.red;
        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime;
            bodyMesh.material.color = Color.Lerp(bodyMesh.material.color, Color.white, Time.deltaTime);
            yield return null;
        }
        bodyMesh.material.color = Color.white;
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

    // Multi-World

    protected override void InitToggleWorlds()
    {
        base.InitToggleWorlds();
        StartCoroutine("OnToggleWorlds");
    }

    protected override void AbortToggleWorlds()
    {
        base.AbortToggleWorlds();
        StopCoroutine("OnToggleWorlds");
    }

    protected override void ToggleWorlds()
    {
        base.ToggleWorlds();
        bodyMesh.enabled = spiritRealm;
    }

    private IEnumerator OnToggleWorlds()
    {
        float time = 0;
        while(onTransition && time < transitionTime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (onTransition)
            ToggleWorlds();
    }
}
