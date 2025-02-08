using TopDownShooter;
using UnityEngine;

public class ShooterAI : BaseEnemyAI
{
    public GameObject player; // Reference to the player's transform
    public float moveSpeed = 3f; // Speed at which the enemy moves
    public float attackRange = 20f; // Range within which the enemy can attack
    public int damage = 10; // Damage dealt by the enemy
    public float attackCooldown = 10f; // Cooldown between attacks

    public Animator animator;

    public Transform BulletPoint;

    public WeaponData WeaponData;

    private float lastAttackTime; // Time when the last attack occurred
    private HitPoint playerHealth;

    private readonly int walkparam = Animator.StringToHash("Walk");

    void Update()
    {

        // Calculate the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        transform.LookAt(player.transform);

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

        // Play the walk animation
        animator.SetBool(walkparam, true);
    }

    void AttackPlayer()
    {
        var bullet = Instantiate(WeaponData.Weapons[1].Bullet, BulletPoint.position,
                transform.rotation).GetComponent<Damage>();
            //muzzle effect

            Instantiate(WeaponData.Weapons[1].MuzzleEffect, BulletPoint.position,
                BulletPoint.rotation);

            //add speed and direction to the bullet
            bullet.SetupBullet(BulletPoint.forward * WeaponData.Weapons[1].BulletSpeed,
                damage);
    }

    public override void SetPlayer(GameObject player){
        this.player = player;
    }
}