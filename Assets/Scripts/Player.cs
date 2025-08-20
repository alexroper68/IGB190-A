using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, IDamageable
{
    public float health = 500f;
    public float maxHealth = 500f;
    public float movementSpeed = 3.5f;
    public float attacksPerSecond = 1.0f;
    public float attackRange = 2.0f;
    public float attackDamage = 40.0f;
    public GameObject slashEffect;
    [HideInInspector] public bool isDead;

    private float canCastAt;
    private float canMoveAt;

    private const float MOVEMENT_DELAY_AFTER_CASTING = 0.2f;
    private const float TURNING_SPEED = 10.0f;

    private NavMeshAgent agentNavigation;
    private Animator animator;

    private enum Ability { Cleave, TwoHandSlash, SlideAttack }
    private Ability? abilityBeingCast = null;
    private float finishAbilityCastAt;
    private Vector3 abilityTargetLocation;

    [Range(0.0f, 1.0f)] public float cleaveActivationPoint = 0.4f;

    private HurtBox hurtBox;

    private void Start()
    {
        agentNavigation = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        hurtBox = GetComponentInChildren<HurtBox>();
        if (agentNavigation != null) agentNavigation.speed = movementSpeed;
    }

    private void Update()
    {
        if (isDead) return;
        UpdateMovement();
        UpdateAbilityCasting();
    }

    private void UpdateMovement()
    {
        if (abilityBeingCast != null) return;

        if (animator != null && agentNavigation != null)
            animator.SetFloat("Speed", agentNavigation.velocity.magnitude);

        if (Input.GetMouseButton(0) && Time.time > canMoveAt)
            agentNavigation.SetDestination(Utilities.GetMouseWorldPosition());
    }

    private void UpdateAbilityCasting()
    {
        if (Input.GetMouseButton(1) && Time.time > canCastAt)
            StartCastingCleave();

        if (Input.GetKeyDown(KeyCode.F) && Time.time > canCastAt)
            StartCastingTwoHandSlash();

        if (Input.GetKeyDown(KeyCode.C) && Time.time > canCastAt)
            StartCastingSlideAttack();

        if (abilityBeingCast != null && Time.time > finishAbilityCastAt)
        {
            switch (abilityBeingCast)
            {
                case Ability.Cleave:
                    FinishCastingCleave();
                    break;
                case Ability.TwoHandSlash:
                    FinishCastingTwoHandSlash();
                    break;
                case Ability.SlideAttack:
                    FinishCastingSlideAttack();
                    break;
            }
        }

        if (abilityBeingCast != null)
        {
            Quaternion look = Quaternion.LookRotation((abilityTargetLocation - transform.position).normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, look, Time.deltaTime * TURNING_SPEED);
        }
    }

    private void StartCastingCleave()
    {
        agentNavigation.SetDestination(transform.position);
        abilityBeingCast = Ability.Cleave;

        animator.CrossFadeInFixedTime("Attack", 0.2f);
        animator.SetFloat("AttackSpeed", attacksPerSecond);

        float castTime = 1.0f / attacksPerSecond;
        canCastAt = Time.time + castTime;
        finishAbilityCastAt = Time.time + cleaveActivationPoint * castTime;
        canMoveAt = finishAbilityCastAt + MOVEMENT_DELAY_AFTER_CASTING;
        abilityTargetLocation = Utilities.GetMouseWorldPosition();
    }

    private void FinishCastingCleave()
    {
        abilityBeingCast = null;

        if (slashEffect != null)
        {
            var slashVisual = Instantiate(slashEffect, transform.position, transform.rotation);
            Destroy(slashVisual, 1.0f);
        }

        Vector3 hitPoint = transform.position + transform.forward * attackRange;
        List<Monster> targets = Utilities.GetAllWithinRange<Monster>(hitPoint, attackRange);
        foreach (var t in targets)
            t.TakeDamage(attackDamage);
    }

    private void StartCastingTwoHandSlash()
    {
        agentNavigation.SetDestination(transform.position);
        agentNavigation.enabled = false;

        abilityBeingCast = Ability.TwoHandSlash;

        animator.CrossFadeInFixedTime("TwoHandSlash", 0.2f);
        animator.SetFloat("AttackSpeed", attacksPerSecond);
        agentNavigation.enabled = true;

        float castTime = 1.0f / attacksPerSecond;
        canCastAt = Time.time + castTime;
        finishAbilityCastAt = Time.time + 3.25f * castTime;
        canMoveAt = finishAbilityCastAt + MOVEMENT_DELAY_AFTER_CASTING;
        abilityTargetLocation = Utilities.GetMouseWorldPosition();
    }

    private void FinishCastingTwoHandSlash()
    {
        abilityBeingCast = null;
        agentNavigation.SetDestination(transform.position);
    }

    // --------------------
    // New Slide Attack
    // --------------------
    private void StartCastingSlideAttack()
    {
        agentNavigation.SetDestination(transform.position);
        agentNavigation.enabled = false;

        abilityBeingCast = Ability.SlideAttack;

        animator.CrossFadeInFixedTime("SlideAttack", 0.2f);
        animator.SetFloat("AttackSpeed", attacksPerSecond);
        agentNavigation.enabled = true;

        float castTime = 1.0f / attacksPerSecond;
        canCastAt = Time.time + castTime;
        finishAbilityCastAt = Time.time + 2.5f * castTime;   // adjust multiplier based on anim length
        canMoveAt = finishAbilityCastAt + MOVEMENT_DELAY_AFTER_CASTING;
        abilityTargetLocation = Utilities.GetMouseWorldPosition();
    }

    private void FinishCastingSlideAttack()
    {
        abilityBeingCast = null;
        agentNavigation.SetDestination(transform.position);
    }

    // --------------------
    // Hurtbox events
    // --------------------
    public void HurtBoxActivate(float strength)
    {
        if (slashEffect != null)
        {
            var slashVisual = Instantiate(slashEffect, transform.position, transform.rotation);
            slashVisual.transform.localScale += new Vector3(strength / 2f, strength / 2f, strength / 2f);
            Destroy(slashVisual, 1.5f);
        }

        if (hurtBox != null)
        {
            hurtBox.colliderControl(true);
            hurtBox.sizeAddition(strength * 1.5f, strength * 1.5f, 0.5f);
        }
    }

    public void HurtBoxDeactivate(float damage)
    {
        if (hurtBox != null)
        {
            hurtBox.dealDamage(damage * attackDamage);
            hurtBox.colliderControl(false);
            hurtBox.sizeReset(0.5f, 0.5f, 1.0f);
        }
    }

    // --------------------
    // Health / death
    // --------------------
    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f) Kill();
    }

    public virtual void Kill()
    {
        isDead = true;
        agentNavigation.SetDestination(transform.position);
        animator.SetTrigger("Die");
        StartCoroutine(RestartLevel());
    }

    public float GetCurrentHealthPercent()
    {
        return Mathf.Clamp01(health / maxHealth);
    }

    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
