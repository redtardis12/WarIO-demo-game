using TopDownShooter;
using UnityEngine;

public class EnemyAI : BaseEnemyAI
{
    public GameObject player; // Reference to the player's transform
    public float moveSpeed = 3f; // Speed at which the enemy moves
    public float attackRange = 2f; // Range within which the enemy can attack
    public int damage = 10; // Damage dealt by the enemy
    public float attackCooldown = 2f; // Cooldown between attacks

    public Animator animator;

    private float lastAttackTime; // Time when the last attack occurred
    private HitPoint playerHealth;

    private readonly int walkparam = Animator.StringToHash("Walk");

    void Update()
    {
        // Calculate the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Move towards the player if not in attack range
        if (distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            animator.SetBool(walkparam, false);
            // Attack the player if cooldown has passed
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
    }

    void MoveTowardsPlayer()
    {
        // Move the enemy towards the player
        Vector3 direction = (player.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Optionally, rotate the enemy to face the player
        transform.LookAt(player.transform);

        // Play the walk animation
        animator.SetBool(walkparam, true);
    }

    void AttackPlayer()
    {
        playerHealth = player.GetComponent<HitPoint>();
        if (playerHealth.CurrentHitPoint != 0)
        {
            playerHealth.ApplyDamage(damage);
            Debug.Log("Enemy attacked player for " + damage + " damage.");
        }
        else
        {
            Debug.LogWarning("PlayerHealth component not found on player.");
        }
    }

    public override void SetPlayer(GameObject player){
        this.player = player;
    }
}