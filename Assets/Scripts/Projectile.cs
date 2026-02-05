using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    public float damage;
    public bool isAreaEffectDamage;
    public float areaEffectRadius;
    public AnimationCurve aoeFallOffCurve;
    public float speedInitial;
    public float acceleration;
    public Transform target;
    public Transform source;
    public Tower owner;
    public Vector3 direction;
    protected float speed;
    public GameObject hitEffect;
    protected virtual void Start()
    {
        SetDirection();
        speed = speedInitial;
    }
    protected virtual void Update()
    {
        if (HasNoTarget()) return;
        Move();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Collision with " + other.gameObject.name);
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (isAreaEffectDamage)
            {
                AreaEffect(transform.position);
            }
            else
            {
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                enemy.TakeDamage(damage);
                owner.DamageDone(damage);
            }
            Hit();
        }
        else if (other.gameObject.CompareTag("Ground"))
        {
            if (isAreaEffectDamage)
            {
                AreaEffect(transform.position);
            }
            Hit();
        }
    }

    protected virtual bool HasNoTarget()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    void Hit()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        speed = 0;
        acceleration = 0;


        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation);
        }
        Destroy(gameObject, 5f);
    }

    protected void AreaEffect(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, areaEffectRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Enemy"))
            {
                Enemy enemy = collider.gameObject.GetComponent<Enemy>();
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                float fallOff = aoeFallOffCurve.Evaluate(distance / areaEffectRadius);
                enemy.TakeDamage(damage * fallOff);
                owner.DamageDone(damage * fallOff);
            }
        }
    }

    protected Vector3 GetPredictedTargetPosition()
    {

        NavMeshAgent targetAgent = target.GetComponent<NavMeshAgent>();
        Vector3 targetVelocity = targetAgent.velocity;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float timeToTarget = distanceToTarget / speed;
        float averageSpeed = speed + (acceleration * timeToTarget * 0.5f);
        timeToTarget = distanceToTarget / averageSpeed;
        return target.position + targetVelocity * timeToTarget;
    }

    protected virtual void SetDirection()
    {
        direction = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    protected virtual void Move()
    {
        speed += acceleration * Time.deltaTime;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaEffectRadius);
    }


}
