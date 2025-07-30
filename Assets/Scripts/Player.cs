using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    // Player Stats
    public float health = 100f;
    public float maxHealth = 100f;
    public float movementSpeed = 3.5f;
    public float attacksPerSecond = 1.0f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;

    // Cached component refs
    private NavMeshAgent agentNavigation;
    private Animator animator;

    void Start()
    {
        agentNavigation = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // sync NavMeshAgent speed to our stat
        agentNavigation.speed = movementSpeed;
    }

    void Update()
    {
        UpdateMovement();
        // later you can call HandleAttacks() here
    }

    // handle movement and drive the "Speed" parameter
    private void UpdateMovement()
    {
        if (animator != null)
            animator.SetFloat("Speed", agentNavigation.velocity.magnitude);

        if (Input.GetMouseButton(0))
        {
            Vector3 targetPos = Utilities.GetMouseWorldPosition();
            agentNavigation.SetDestination(targetPos);
        }
    }

    // call this from other scripts or events when taking damage
    public void TakeDamage(float amount)
    {
        health = Mathf.Max(health - amount, 0f);
        // update health UI, play hit anim, etc.
    }
}
