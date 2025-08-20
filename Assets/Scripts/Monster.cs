using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public float maxHealth = 100f;
    public float movementSpeed = 3.5f;
    public float attacksPerSecond = 1.0f;
    public float attackRange = 2.0f;
    public float attackDamage = 10.0f;
    [Range(0.0f, 1.0f)] public float slashActivationPoint = 0.4f;

    private Player player;
    private float canCastAt;
    private float canMoveAt;

    private const float MOVEMENT_DELAY_AFTER_CASTING = 1.5f;
    private const float TURNING_SPEED = 10.0f;
    private const float TIME_BEFORE_CORPSE_DESTROYED = 5.0f;

    private NavMeshAgent agentNavigation;
    private Animator animator;

    private enum Ability { Slash }
    private Ability? abilityBeingCast = null;
    private float finishAbilityCastAt;

    private void Start()
    {
        agentNavigation = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindObjectOfType<Player>();
        if (agentNavigation != null) agentNavigation.speed = movementSpeed;
        canMoveAt = Time.time + 1.0f;
        if (player != null) transform.LookAt(player.transform);
    }

    private void Update()
    {
        if (player == null || player.isDead) return;
        UpdateMovement();
        UpdateAbilityCasting();
    }

    private void UpdateMovement()
    {
        if (animator != null && agentNavigation != null)
            animator.SetFloat("Speed", agentNavigation.velocity.magnitude);

        if (Time.time > canMoveAt)
            agentNavigation.SetDestination(player.transform.position);
    }

    private void UpdateAbilityCasting()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < attackRange && Time.time > canCastAt)
            StartCastingSlash();

        if (abilityBeingCast != null && Time.time > finishAbilityCastAt)
        {
            if (abilityBeingCast == Ability.Slash)
                FinishCastingSlash();
        }

        if (abilityBeingCast != null)
        {
            Quaternion look = Quaternion.LookRotation(player.transform.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, look, Time.deltaTime * TURNING_SPEED);
        }
    }

    private void StartCastingSlash()
    {
        agentNavigation.SetDestination(transform.position);
        abilityBeingCast = Ability.Slash;

        animator.CrossFadeInFixedTime("Attack", 0.2f);
        animator.SetFloat("AttackSpeed", attacksPerSecond);

        float castTime = 1.0f / attacksPerSecond;
        canCastAt = Time.time + castTime;
        finishAbilityCastAt = Time.time + slashActivationPoint * castTime;
        canMoveAt = finishAbilityCastAt + MOVEMENT_DELAY_AFTER_CASTING;
    }

    private void FinishCastingSlash()
    {
        abilityBeingCast = null;

        Vector3 hitPoint = transform.position + transform.forward * attackRange;
        List<Player> targets = Utilities.GetAllWithinRange<Player>(hitPoint, attackRange);
        foreach (var t in targets)
            t.TakeDamage(attackDamage);
    }

    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f) Kill();
    }

    public virtual void Kill()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
            animator.transform.SetParent(null);
            Destroy(animator.gameObject, TIME_BEFORE_CORPSE_DESTROYED);
        }
        Destroy(gameObject);
    }

    public float GetCurrentHealthPercent()
    {
        return Mathf.Clamp01(health / maxHealth);
    }
}
