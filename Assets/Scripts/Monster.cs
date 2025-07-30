using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    // Stats
    public float health = 40f;
    public float maxHealth = 40f;
    public float movementSpeed = 1.0f;
    public float attacksPerSecond = 1.0f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;

    // Cached refs
    private NavMeshAgent agentNavigation;
    private Animator animator;
    private Player player;

    void Start()
    {
        agentNavigation = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = FindObjectOfType<Player>();

        if (agentNavigation != null)
            agentNavigation.speed = movementSpeed;

        if (player == null)
            Debug.LogWarning("Monster: no Player found in scene");
    }

    void Update()
    {
        UpdateMovement();
        // later you can call HandleAttacks() here
    }

    private void UpdateMovement()
    {
        if (animator != null && agentNavigation != null)
            animator.SetFloat("Speed", agentNavigation.velocity.magnitude);

        if (player != null && agentNavigation != null)
            agentNavigation.SetDestination(player.transform.position);
    }

    // call to apply damage
    public void TakeDamage(float amount)
    {
        health = Mathf.Max(health - amount, 0f);
        // you can play hit anim or check for death here
    }
}
