using UnityEngine;

public class ProjectileLaser : Projectile
{
    public int middlePointCount;
    private LineRenderer lineRenderer;
    private Vector3 endPosition;
    protected override void Start()
    {
        base.Start();
        lineRenderer = GetComponent<LineRenderer>();
        transform.SetParent(owner.transform);
        lineRenderer.positionCount = 0;
    }
    protected override void Update()
    {
        base.Update();
        target = owner.GetTarget()?.transform;
        if (target != null)
        {
            owner.RotateBarrel(target.gameObject);
            SetEndPosition();
            SetLineRenderer();
            if (hitEffect != null)
            {
                hitEffect.transform.position = endPosition;
                hitEffect.transform.rotation = Quaternion.LookRotation(source.position - endPosition);
                hitEffect.SetActive(true);
            }
            if (isAreaEffectDamage)
            {
                AreaEffect(endPosition);
            }
            else
            {
                Enemy enemy = target.GetComponent<Enemy>();
                enemy.TakeDamage(damage * Time.deltaTime);
                owner.DamageDone(damage * Time.deltaTime);
            }
        } else {
            lineRenderer.positionCount = 0;
            hitEffect.SetActive(false);
        }
    }
    protected override void OnTriggerEnter(Collider other)
    {
        return;
    }
    protected override bool HasNoTarget()
    {
        return false;
    }
    protected override void SetDirection()
    {
        return;
    }
    protected override void Move()
    {
        return;
    }
    private void SetLineRenderer()
    {
        lineRenderer.positionCount = middlePointCount + 2;
        lineRenderer.SetPosition(0, source.position);
        lineRenderer.SetPosition(1, endPosition);
        for (int i = 2; i < middlePointCount + 2; i++)
        {
            lineRenderer.SetPosition(i, Vector3.Lerp(source.position, endPosition, (float)i / (middlePointCount + 2)));
        }
    }
    private void SetEndPosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(source.position, target.position - source.position, out hit))
        {
            endPosition = hit.point;
        }
    }
}
