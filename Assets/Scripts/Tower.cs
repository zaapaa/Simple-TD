using UnityEngine;
using System;
using System.Linq;

public class Tower : Placeable, ISelectable
{
    public float targetingRange;
    public float attackCooldown;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform barrelPivot;
    public bool canBarrelTilt;
    public float lastAttackTime;
    public TargetingType targetingType = TargetingType.Nearest;
    public float damageDone;
    public int killCount;
    public TowerType towerType;
    private GameObject targetingRangeVisual;
    private float attackTimer;
    private GameObject target;
    private GameObject oldTarget;
    public GameObject forcedTarget;

    protected override void Start()
    {
        base.Start();
        targetingRangeVisual = transform.Find("TargetingRange")?.gameObject;
        SetTargetingRangeVisualScale();
        targetingRangeVisual.SetActive(true);
        attackTimer = attackCooldown;
    }

    protected override void Update()
    {
        base.Update();
        if (!isBeingPlaced)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                target = GetTarget();
                if(ForcedTargetInRange())
                {
                    target = forcedTarget;
                }
                if (target == null)
                {
                    // There was a target but not anymore -> untarget old target (e.g. old target moved out of range)
                    if (oldTarget != null)
                    {
                        oldTarget.GetComponent<Enemy>().Untarget(this);
                    }
                    return;
                }
                // Tower has new target -> untarget old target
                if (IsSelected() && target != oldTarget)
                {
                    if (oldTarget != null)
                    {
                        oldTarget.GetComponent<Enemy>().Untarget(this);
                    }
                    target.GetComponent<Enemy>().Target(this);
                }
                Attack(target);
                RotateBarrel(target);
                // tower attacks only once if attackcooldown is negative (laser tower)
                attackTimer = attackCooldown < 0 ? float.NegativeInfinity : attackTimer - attackCooldown;
                if (IsSelected())
                {
                    oldTarget = target;
                }
            }
        }
    }

    public GameObject GetTarget()
    {
        var enemiesInRange = FindObjectsByType<Enemy>(FindObjectsSortMode.None)
            .Where(e => Vector3.Distance(e.transform.position, transform.position) < targetingRange)
            .Where(e => e.health > 0);

        if (!enemiesInRange.Any()) return null;

        return targetingType switch
        {
            TargetingType.Nearest => enemiesInRange.OrderBy(e => Vector3.Distance(e.transform.position, transform.position)).FirstOrDefault()?.gameObject,
            TargetingType.Farthest => enemiesInRange.OrderByDescending(e => Vector3.Distance(e.transform.position, transform.position)).FirstOrDefault()?.gameObject,
            TargetingType.Weakest => enemiesInRange.OrderBy(e => e.health).FirstOrDefault()?.gameObject,
            TargetingType.Strongest => enemiesInRange.OrderByDescending(e => e.health).FirstOrDefault()?.gameObject,
            TargetingType.First => enemiesInRange.OrderByDescending(e => e.progress).FirstOrDefault()?.gameObject,
            TargetingType.Last => enemiesInRange.OrderBy(e => e.progress).FirstOrDefault()?.gameObject,
            _ => null
        };
    }

    private void Attack(GameObject target)
    {
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            projectile.GetComponent<Projectile>().target = target.transform;
            projectile.GetComponent<Projectile>().source = firePoint;
            projectile.GetComponent<Projectile>().owner = this;
        }
    }
    public void RotateBarrel(GameObject target)
    {
        Vector3 direction = (target.transform.position - barrelPivot.position).normalized;
        if (!canBarrelTilt)
        {
            direction.y = 0;
        }
        barrelPivot.rotation = Quaternion.LookRotation(direction);
    }
    public void DamageDone(float damage)
    {
        damageDone += damage;
    }

    private void SetTargetingRangeVisualScale()
    {
        float ratio = 0.128f;
        targetingRangeVisual.transform.localScale = new Vector3(targetingRange * ratio, targetingRange * ratio, 1f);
    }

    public override bool Place(bool force = false)
    {
        bool placed = base.Place(force);
        if (placed)
        {
            targetingRangeVisual?.SetActive(false);
        }
        return placed;
    }
    public void ForceTarget(GameObject forcetarget)
    {
        if (forcedTarget != null)
        {
            forcedTarget.GetComponent<Enemy>()?.UnForceTarget(this);
        }
        forcedTarget = forcetarget;
        forcedTarget?.GetComponent<Enemy>()?.ForceTarget(this);
    }
    private bool ForcedTargetInRange()
    {
        if (forcedTarget == null) return false;
        return Vector3.Distance(transform.position, forcedTarget.transform.position) <= targetingRange;
    }

    public override void Select()
    {
        base.Select();
        targetingRangeVisual.SetActive(true);
        target?.GetComponent<Enemy>()?.Target(this);
    }

    public override void Deselect()
    {
        base.Deselect();
        targetingRangeVisual.SetActive(false);
        if (target != null)
        {
            target.GetComponent<Enemy>()?.Untarget(this);
        }
        if (oldTarget != null)
        {
            oldTarget?.GetComponent<Enemy>()?.Untarget(this);
            oldTarget = null;
        }
        if (forcedTarget != null)
        {
            forcedTarget.GetComponent<Enemy>()?.UnForceTarget(this);
        }
    }

    public override Type GetSelectableType()
    {
        return towerType switch
        {
            TowerType.Basic => typeof(BasicTowerType),
            TowerType.Missile => typeof(MissileTowerType),
            TowerType.Laser => typeof(LaserTowerType),
            _ => typeof(Tower)
        };
    }

    public override SelectInfo GetSelectInfo()
    {
        SelectInfo selectInfo = new SelectInfo();
        selectInfo.name = towerType switch
        {
            TowerType.Basic => "Basic Tower",
            TowerType.Missile => "Missile Tower",
            TowerType.Laser => "Laser Tower",
            _ => "Tower"
        };
        selectInfo.damage = projectilePrefab.GetComponent<Projectile>().damage;
        selectInfo.attackRange = targetingRange;
        selectInfo.damageDone = damageDone;
        selectInfo.killCount = killCount;

        return selectInfo;
    }
}

public enum TargetingType
{
    Nearest,
    Farthest,
    Weakest,
    Strongest,
    First,
    Last,
    None
}

public enum TowerType
{
    Basic,
    Missile,
    Laser
}

// Marker types for tower identification
public class BasicTowerType { }
public class MissileTowerType { }
public class LaserTowerType { }

