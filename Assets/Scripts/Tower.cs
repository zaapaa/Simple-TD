using UnityEngine;
using System;
using System.Linq;

public class Tower : Placeable, ISelectable
{
    public float targetingRange;
    public float attackCooldown;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float lastAttackTime;
    public TargetingType targetingType = TargetingType.Nearest;
    public float damageDone;
    public int killCount;
    public TowerType towerType;
    private GameObject targetingRangeVisual;

    protected override void Start()
    {
        base.Start();
        targetingRangeVisual = transform.Find("TargetingRange")?.gameObject;
        SetTargetingRangeVisualScale();
        targetingRangeVisual.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();
        if (!isBeingPlaced)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                GameObject target = GetTarget();
                if (target != null)
                {
                    Attack(target);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    private GameObject GetTarget()
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
            TargetingType.First => enemiesInRange.OrderBy(e => e.progress).FirstOrDefault()?.gameObject,
            TargetingType.Last => enemiesInRange.OrderByDescending(e => e.progress).FirstOrDefault()?.gameObject,
            _ => null
        };
    }

    private void Attack(GameObject target)
    {
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            projectile.GetComponent<Projectile>().target = target.transform;
            projectile.GetComponent<Projectile>().owner = this;
        }
        // TODO: add attacking logic stuff
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

    public override bool Place()
    {
        bool placed = base.Place();
        if (placed)
        {
            targetingRangeVisual.SetActive(false);
        }
        return placed;
    }

    public override void Select()
    {
        base.Select();
        targetingRangeVisual.SetActive(true);
    }
    
    public override void Deselect()
    {
        base.Deselect();
        targetingRangeVisual.SetActive(false);
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
public class BasicTowerType {}
public class MissileTowerType {}
public class LaserTowerType {}

