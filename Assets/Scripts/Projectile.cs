using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public Transform target;
    public float speed;
    public Tower owner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        var direction = (target.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(damage);
            owner.DamageDone(damage);
            Destroy(gameObject);
        }
    }
}
