using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum AttackType
{
    Shoot,
    Bash
}

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private AttackType attackType;
    [Min(0)]
    public float viewRadius = 12;
    [Range(0f, 180f)]
    public float viewAngle;
    [Min(0)]
    public float shootingDistance = 8;
    [Min(0), Header("How long the enemy will keep chasing after losing line of sight")]
    [SerializeField] private float attentionSpan = 0.8f;
    [Min(0), Header("Distance from which the target is being chased, regardless of line of sight")]
    [SerializeField] private float hearingDistance = 4;
    [Min(0), Header("Time in seconds between firing")]
    [SerializeField] private float shootInterval = 2f;
    [Min(0), Header("Amount of projectiles shot at once")]
    [SerializeField] private float projCount = 3;
    [Min(0), Header("Angle in degrees between each projectile")]
    [SerializeField] private int shootAngle = 10;
    [Min(0), Header("Average offset from the calculated angle (randomized)")]
    [SerializeField] private float projOffset = 0.8f;
    [Range(0, 2), Header("Multiplier for how where predicts the player's position"), Tooltip("For aiming ahead or behind; 1 is default aim.")]
    [SerializeField] private float predictionOvershoot = 1;
    [Range(0, 2), Header("Rotation speed when attempting to shoot the player"), Tooltip("Doesn't affect aim")]
    [SerializeField] private float shootingRotationSpeed = 1;
    [SerializeField] private int bashDamage = 1;
    [Range(0.1f, 2), Header("How much further than the player distance to move with a bash"), Tooltip("Distance is a multiplier of the player distance")]
    [SerializeField] private float bashDistanceMultiplier = 1;
    [Min(0), Header("Time it takes to rotate towards the player")]
    [SerializeField] private float bashDelay = 2;
    [Min(0), Header("How long the bash lasts"), Tooltip("Lower time means faster bash speed")]
    [SerializeField] private float bashTime = 3;
    [Min(0), Header("How long it takes to recover after charging and missing")]
    [SerializeField] private float bashMissRecoveryTime = 0.5f;
    [Min(0), Header("How long it takes to recover from being dazed from hitting a wall or the player")]
    [SerializeField] private float bashHitRecoveryTime = 3;
    [Min(0), Header("Distance knocked back after hitting a wall or the player")]
    [SerializeField] private float bashTurnBackTime = 1.5f;
    [SerializeField] private float bashKnockback = 3;
    [Min(0), Header("How long the knockback takes")]
    [SerializeField] private float knockbackTime = 0.8f;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletOrigin;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private FOVisual visual;
    [SerializeField] private Color defaultFovColor;
    [SerializeField] private GameObject boosters;
    [SerializeField] private Color trackingFovColor;
    [SerializeField] private AnimationCurve bashCurve;
    [SerializeField] private AnimationCurve knockbackCurve;
    [SerializeField] private GameObject sparks;
    [SerializeField] private float sparkOffset;
    private Material fovMaterial;
    private Transform player;
    private EnemyBehaviour enemyBehaviour;
    private Rigidbody playerRigbod;
    private PlayerHealth playerHealth;
    private float chaseTimer = 0f;
    private bool isChasing = false;
    private bool isAttacking = false;
    private float shootTimer;
    private Vector3 gizmoPos;
    private float posHeight; 
    private Coroutine bash;
    private float colliderRadius;
    private bool isCharging = false;


    private void Awake()
    {
        player = FindObjectOfType<PlayerMovement>().transform;
        enemyBehaviour = GetComponent<EnemyBehaviour>();
        playerRigbod = player.GetComponent<Rigidbody>();
        playerHealth = player.GetComponent<PlayerHealth>();
        //material = GetComponent<Renderer>().material;
        fovMaterial = visual.GetComponent<Renderer>().material;
        fovMaterial.color = defaultFovColor;
        visual.fov = viewAngle;
        visual.viewDistance = viewRadius;
        colliderRadius = GetComponent<CapsuleCollider>().radius;
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo);
        posHeight = hitInfo.point.y + 0.1f;
        StartCoroutine(FieldOfViewRoutine());
    }

    private void Update()
    {
        Vector3 newOrigin = transform.position;
        newOrigin.y = posHeight;
        visual.SetOrigin(newOrigin);
        visual.SetDirection(transform.forward);
    }

    private void FixedUpdate()
    {
        if (!isChasing) return;

        if (chaseTimer > 0 && bash == null) // timer for giving up on chasing the player after losing line of sight
        {
            chaseTimer -= Time.fixedDeltaTime;
            if (chaseTimer <= 0)
            {
                isChasing = false;
                enemyBehaviour.ContinueRoutine(); // go back to waypoint routine
                fovMaterial.color = defaultFovColor;
            }
        }


        if (!isAttacking)
        {
            enemyBehaviour.SetNextTarget(player.position); // move towards the player
            return;
        }

        SelectAttack();
    }


    /// <summary>
    /// Handles player detection
    /// </summary>
    /// <returns></returns>
    private IEnumerator FieldOfViewRoutine()
    {
        while (true)
        {
            if (FieldOfViewCheck() && bash == null)
            {
                if (isChasing == false) // if the player wasn't being chased yet last frame
                { fovMaterial.color = trackingFovColor; }

                isChasing = true;
                chaseTimer = attentionSpan; // (re)set the timer for chasing the player

                if (Vector3.Distance(transform.position, player.position) < shootingDistance) // if able to shoot the player
                {
                    isAttacking = true;     // start attacking
                    enemyBehaviour._agent.isStopped = true;
                    enemyBehaviour.trackingWaypoints = false;
                }
                else // stop attacking and continue chasing
                {
                    isAttacking = false;
                    enemyBehaviour._agent.isStopped = false;
                    enemyBehaviour.trackingWaypoints = true;
                }
            }
            else if (isAttacking && attackType != AttackType.Bash)
            {
                isAttacking = false;
                enemyBehaviour._agent.isStopped = false;
                enemyBehaviour.trackingWaypoints = true;
            }
            yield return new WaitForSeconds(0.2f); // execute only once every 0.2 seconds to preserve resources, as the FieldOfViewCheck() function is fairly intensive
        }
    }

    /// <summary>
    /// Checks whether the player is within the field of view
    /// </summary>
    /// <returns></returns>
    private bool FieldOfViewCheck()
    {
        Vector3 playerDirection = (player.position - transform.position).normalized;
        float playerDistance = Vector3.Distance(transform.position, player.position);
        Vector3 playerRight = Quaternion.Euler(0, 90, 0) * playerDirection;
        Debug.DrawRay(transform.position, playerDirection);
        bool hasLineOfSight = !Physics.Raycast(transform.position, playerDirection, playerDistance, obstructionMask); // check for walls in a straight line to the player
        bool canBash = (Physics.Raycast(transform.position + playerRight * colliderRadius, playerDirection, playerDistance, obstructionMask) &&  // check for walls with a raycast on each side
                        Physics.Raycast(transform.position - playerRight * colliderRadius, playerDirection, playerDistance, obstructionMask));

        // check if there's a wall in a straight line to the player, or if there's enough space to bash
        if (!hasLineOfSight && (attackType != AttackType.Bash || !isChasing || canBash)) return false;

        Collider[] closebyColliders = Physics.OverlapSphere(transform.position, hearingDistance, targetMask); // check if the player is within the hearing range
        if (closebyColliders.Length != 0) return true;


        Collider[] colliders = Physics.OverlapSphere(transform.position, viewRadius, targetMask); // check if the player is within vision range
        if (colliders.Length == 0) return false;

        Transform target = colliders[0].transform;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2) return true;  // check if the player's position is within the view angle
        
        return false;
    }

    private void SelectAttack()
    {
        switch (attackType) // select whether to shoot or bash
        {
            case AttackType.Shoot:
                {
                    Vector3 lookDirection = (player.position - transform.position).normalized; // look towards the player (doesn't affect aim)
                    Quaternion rotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * shootingRotationSpeed);
                    Shoot();
                    break;
                }
            case AttackType.Bash:
                {
                    if (bash == null)       // only execute if not currently performing a bash
                    { bash = StartCoroutine(Bash()); }
                    break;
                }
        }
    }

    private void Shoot()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer > 0) return;

        float travelTime = Vector3.Distance(bulletOrigin.position, player.position) / bullet.GetComponent<Bullet>().movementSpeed; // get time until a bullet would hit
        Vector3 predictedPos = player.position + (playerRigbod.velocity * travelTime * predictionOvershoot); // apply that time to the movement per second of the player
        predictedPos.y += 0.5f;
        Vector3 direction = (predictedPos - bulletOrigin.position).normalized; // get direction to that point
        Quaternion shootDirection = Quaternion.LookRotation(direction); // face that way
        gizmoPos = predictedPos; // set an orb gizmo there

        for (float curAngle = -shootAngle * ((projCount - 1) / 2); curAngle <= shootAngle * ((projCount - 1) / 2); curAngle += shootAngle)
        {
            float offset = Random.Range(0, projOffset);
            Instantiate(bullet, bulletOrigin.position, shootDirection * Quaternion.Euler(Vector3.up * (curAngle - projOffset / 2 + offset)));
            if (shootAngle == 0) break; // failsafe
        }
        AudioManager.Instance.Play("Shoot");
        shootTimer = shootInterval;
    }

    private IEnumerator Bash()
    {
        enemyBehaviour.trackingWaypoints = false; // stop other movements
        enemyBehaviour._agent.isStopped = true;
        boosters.SetActive(true);
        Quaternion startRotation = transform.rotation;
        Vector3 endDirection = (player.position - transform.position).normalized; // get the direction towards the player
        Quaternion endRotation = Quaternion.LookRotation(endDirection);
        float distance = Vector3.Distance(transform.position, player.position);
        float progress = 0;
        fovMaterial.color = trackingFovColor;
        gizmoPos = transform.position + (bashDistanceMultiplier) * distance * endDirection; // set the attack pos gizmo
        while (progress < 1) // rotate towards the player
        {
            progress += Time.deltaTime / bashDelay;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, bashCurve.Evaluate(progress));
            yield return null;
        }

        isCharging = true;
        progress = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (bashDistanceMultiplier) * distance * endDirection; // get the position with the distance multiplier applied
        endPos.y = transform.position.y;
        while (progress < 1) // charge towards the player
        {
            progress += Time.deltaTime / bashTime;
            transform.position = Vector3.Lerp(startPos, endPos, bashCurve.Evaluate(progress));
            yield return null;
        }
        
        boosters.SetActive(false);
        isCharging = false;

        //rotate to the player again
        startRotation = transform.rotation;
        endDirection = (player.position - transform.position).normalized; // get the direction towards the player
        endRotation = Quaternion.LookRotation(endDirection);
        progress = 0;
        while (progress < 1) // rotate towards the player
        {
            progress += Time.deltaTime / bashTurnBackTime;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, bashCurve.Evaluate(progress));
            yield return null;
        }

        // reset everything, continue chasing
        yield return new WaitForSeconds(bashMissRecoveryTime);
        isChasing = true;
        chaseTimer = attentionSpan;
        isAttacking = false;
        enemyBehaviour.StartTracking();
        bash = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isCharging) return;

        int i = Mathf.RoundToInt((collision.contacts.Length - 1) / 2); // find the middle one (idk if it even gets the middle one positionally or if they're scrambled but whatever lol)
        if (collision.contacts.Length == 1) // probably not needed, but just in case
        { i = 0; } 

        if (obstructionMask == (obstructionMask | (1 << collision.gameObject.layer)))
        {
            StopCoroutine(bash);
            bash = StartCoroutine(BashHitStun(collision.contacts[i].point));
        }

        if (collision.gameObject.layer == player.gameObject.layer)
        {
            playerHealth.ChangeHealth(-bashDamage);
            StopCoroutine(bash);
            bash = StartCoroutine(BashHitStun(collision.contacts[i].point));
        }
    }

    /// <summary>
    /// Cancels the ongoing bash, applies knockback and spawns sparks
    /// </summary>
    /// <param name="hitPos"></param>
    /// <returns></returns>
    private IEnumerator BashHitStun(Vector3 hitPos)
    {
        AudioManager.Instance.Play("Bash");
        boosters.SetActive(false);
        isCharging = false;
        isChasing = false;          ///////////// make the spark spawn with raycast for better accuracy?? ////////////////////////////////////////////////
        Vector3 newPos = hitPos + ((hitPos - transform.position).normalized * sparkOffset); // slightly janky code for moving the position a bit closer (the collision point would be inside of the wall as the enemy is being moved by the transform.position)
        GameObject sparksObj =  Instantiate(sparks, newPos, Quaternion.Euler(Vector3.forward));
        float progress = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position - transform.forward * bashKnockback;    
        while (progress < 1) // take knockback
        {
            progress += Time.deltaTime / knockbackTime;
            transform.position = Vector3.Lerp(startPos, endPos, knockbackCurve.Evaluate(progress));
            yield return new WaitForFixedUpdate();
        }

        // stay stunned for some time, then reset everything
        yield return new WaitForSeconds(bashHitRecoveryTime);

        //rotate to the player again
        Quaternion startRotation = transform.rotation;
        Vector3 endDirection = (player.position - transform.position).normalized; // get the direction towards the player
        Quaternion endRotation = Quaternion.LookRotation(endDirection);
        progress = 0;
        while (progress < 1) // rotate towards the player
        {
            progress += Time.deltaTime / bashTurnBackTime;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, bashCurve.Evaluate(progress));
            yield return null;
        }

        //yield return enemyBehaviour.RotateToTarget(player.position);
        isChasing = true;
        chaseTimer = attentionSpan;
        isAttacking = false;
        enemyBehaviour.StartTracking();
        bash = null;
    }

    public void StartChasing()
    {
        Vector3 playerDirection = (player.position - transform.position).normalized;
        float playerDistance = Vector3.Distance(transform.position, player.position);
        bool hasLineOfSight = !Physics.Raycast(transform.position, playerDirection, playerDistance, obstructionMask);
        if (!hasLineOfSight) return;
        fovMaterial.color = trackingFovColor;
        isChasing = true;
        chaseTimer = attentionSpan;
        isAttacking = false;
        enemyBehaviour.StartTracking();
    }


    private void OnDrawGizmosSelected()
    {
        if (gizmoPos != null) // draw the orb for the shooting or bash position
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(gizmoPos, 0.2f);
        }

        if (isChasing && attackType == AttackType.Bash) // draw the bash vision lines
        {
            Gizmos.color = new Color32(255, 150, 0, 255);

            Vector3 playerDirection = (player.position - transform.position).normalized;
            float playerDistance = Vector3.Distance(transform.position, player.position);
            Vector3 playerRight = Quaternion.Euler(0, 90, 0) * playerDirection;

            Gizmos.DrawRay(transform.position + playerRight * colliderRadius, playerDirection * playerDistance);
            Gizmos.DrawRay(transform.position - playerRight * colliderRadius, playerDirection * playerDistance);
        }
    }
}