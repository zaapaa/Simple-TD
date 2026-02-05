using UnityEngine;
using UnityEngine.AI;

public class ProjectileHoming : Projectile
{
    public float homingStrength;
    public float speedEffectOnHoming;
    public float distanceEffectOnHoming;
    public float homingStartTime;
    public AnimationCurve homingStartCurve;
    public bool isPredictive;
    public bool canRedirect;
    public float rotationSpeed;
    private Vector3 targetPosition;
    private float homingTimer;
    private float currentRoll;
    protected override void Start()
    {
        direction = transform.forward;
        base.Start();
    }
    protected override void SetDirection()
    {
        if (target == null) return;
        targetPosition = target.position;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (isPredictive)
        {
            NavMeshAgent targetAgent = target.GetComponent<NavMeshAgent>();
            Vector3 targetVelocity = targetAgent.velocity;
            float timeToTarget = distanceToTarget / speed;
            float averageSpeed = speed + (acceleration * timeToTarget * 0.5f);
            timeToTarget = distanceToTarget / averageSpeed;
            targetPosition = target.position + targetVelocity * timeToTarget;
        }
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float homingStartFactor = homingStartCurve.Evaluate(homingTimer / homingStartTime);
        float homingFactor = homingStrength + (speedEffectOnHoming * speed) + (distanceEffectOnHoming * (1f / distanceToTarget));
        direction = Vector3.Slerp(direction, directionToTarget, homingStartFactor * homingFactor * Time.deltaTime);
        
        // Accumulate roll over time
        currentRoll += rotationSpeed * Time.deltaTime;
        
        // Apply both direction and roll
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.AngleAxis(currentRoll, Vector3.forward);
    }
    protected override void Move()
    {
        homingTimer += Time.deltaTime;
        SetDirection();
        base.Move();
    }

    protected override bool HasNoTarget()
    {
        if (canRedirect && target == null)
        {
            var targetGameobject = owner.GetTarget();
            if (targetGameobject != null) target = targetGameobject.transform;
            return false;
        }
        return base.HasNoTarget();
    }
}