using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;

public class Enemy : MonoBehaviour, ISelectable
{

    public float maxHealth;
    public float health;
    public float progress;
    public float speed;
    public float waveBudget;
    public float reward;
    public int minSpawnWaveNumber;
    public float rotationSpeed;
    public Sprite UIIcon;
    public EnemyType enemyType;

    private NavMeshAgent agent;
    private Vector3[] currentPath;
    private int currentPathIndex;
    private Transform endPoint;
    private Transform spawnPoint;
    private float distanceMoved;
    private float totalPathLength;
    private EnemyWaveSpawner waveSpawner;
    private GameObject selectionVisual;
    private bool isSelected = false;
    public bool hasSpawnImmunity = true;
    private Image healthBarImage;
    private GameObject model;
    private GameObject targetVisual;
    private GameObject forceTargetVisual;
    private List<Tower> targetedBy = new List<Tower>();
    private List<Tower> forceTargetedBy = new List<Tower>();
    private bool needsPathRecalculation = false;
    private const float k_PathRecalculationInterval = 0.5f;
    private float lastPathRecalculationTime;

    public void Initialize(Transform spawn, Transform end, EnemyWaveSpawner spawner, Color color, float distanceMoved = 0f)
    {
        spawnPoint = spawn;
        endPoint = end;
        waveSpawner = spawner;
        agent = GetComponent<NavMeshAgent>();
        model = transform.Find("Model")?.gameObject;
        model.GetComponent<Renderer>().material.color = color;
        this.distanceMoved = distanceMoved;
        CalculateTotalPathLength();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectionVisual = transform.Find("Selection")?.gameObject;
        targetVisual = transform.Find("Target")?.gameObject;
        forceTargetVisual = transform.Find("ForceTarget")?.gameObject;
        health = maxHealth;

        // Get healthbar image component
        healthBarImage = GetComponentInChildren<Image>();

        agent.updateRotation = false;
        agent.updatePosition = false;
        CalculatePath();

        if (UnityEngine.Random.value < 0.5f)
        {
            rotationSpeed *= -1f;
        }
        StartCoroutine(RemoveSpawnImmunity());
    }

    // Update is called once per frame
    void Update()
    {
        // Check if we need to recalculate path
        if (needsPathRecalculation || Time.time - lastPathRecalculationTime > k_PathRecalculationInterval)
        {
            CalculatePath();
            needsPathRecalculation = false;
            lastPathRecalculationTime = Time.time;
        }

        // Move along the path manually
        if (currentPath != null && currentPath.Length > 0 && currentPathIndex < currentPath.Length)
        {
            MoveAlongPath();
        }

        progress = distanceMoved / totalPathLength;

        // Check if reached destination
        if (Vector3.Distance(transform.position, endPoint.position) < 2f)
        {
            ReachEndpoint();
        }
        model.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        selectionVisual.transform.rotation = model.transform.rotation;
        if (targetedBy.Count > 0)
        {
            targetVisual.SetActive(true);
        }
        else
        {
            targetVisual.SetActive(false);
        }
        if (forceTargetedBy.Count > 0)
        {
            forceTargetVisual.SetActive(true);
        }
        else
        {
            forceTargetVisual.SetActive(false);
        }
    }
    IEnumerator RemoveSpawnImmunity()
    {
        yield return new WaitForSeconds(1f);
        hasSpawnImmunity = false;
    }

    void CalculatePath()
    {
        if (agent == null || endPoint == null) return;

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, endPoint.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            currentPath = path.corners;
            currentPathIndex = 1;
        }
        else
        {
            // Fallback to direct path
            currentPath = new Vector3[] { endPoint.position };
            currentPathIndex = 1;
        }
    }

    void MoveAlongPath()
    {
        if (currentPathIndex >= currentPath.Length) return;

        Vector3 targetPoint = currentPath[currentPathIndex];
        Vector3 direction = (targetPoint - transform.position).normalized;

        // Constant speed movement
        float moveDistance = speed * Time.deltaTime;

        // Check if we'll reach or pass the target point this frame
        float distanceToTarget = Vector3.Distance(transform.position, targetPoint);

        if (distanceToTarget <= moveDistance)
        {
            while (distanceToTarget <= moveDistance)
            {
                moveDistance -= distanceToTarget;
                distanceMoved += distanceToTarget;
                // We've reached this corner, move to next one
                transform.position = targetPoint;
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Length)
                {
                    ReachEndpoint();
                    return;
                }
                targetPoint = currentPath[currentPathIndex];
                direction = (targetPoint - transform.position).normalized;
                distanceToTarget = Vector3.Distance(transform.position, targetPoint);
            }
        } else {
            transform.position += direction * moveDistance;
            distanceMoved += moveDistance;
        }
    }

    void CalculateTotalPathLength()
    {
        if (spawnPoint == null || endPoint == null) return;

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(spawnPoint.position, endPoint.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            totalPathLength = GetPathLength(path.corners);
        }
        else
        {
            totalPathLength = Vector3.Distance(spawnPoint.position, endPoint.position);
        }
    }

    float GetPathLength(Vector3[] corners)
    {
        float length = 0f;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            length += Vector3.Distance(corners[i], corners[i + 1]);
        }
        return length;
    }

    public void OnPlaceablePlaced()
    {
        // Force path recalculation when a new placeable is placed
        needsPathRecalculation = true;
    }

    void ReachEndpoint()
    {
        // Damage player and destroy enemy
        GameManager.instance.DecreaseLives(1);
        GameUIHandler.instance.RemoveSelection(this);
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (hasSpawnImmunity) return;
        health -= damage;

        // Update healthbar
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = health / maxHealth;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Give money to player
        GameManager.instance.IncreaseMoney(reward); // Adjust reward as needed
        Deselect();
        GameUIHandler.instance.RemoveSelection(this);
        if (enemyType == EnemyType.Big)
        {
            Color myColor = model.GetComponent<Renderer>().material.color;
            waveSpawner.SpawnEnemy(waveSpawner.enemyPrefabs[0], transform.position, myColor, distanceMoved);
            waveSpawner.SpawnEnemy(waveSpawner.enemyPrefabs[0], transform.position, myColor, distanceMoved);
        }
        Destroy(gameObject);
    }

    public void Target(Tower tower)
    {
        if (targetedBy.Contains(tower))
        {
            return;
        }
        targetedBy.Add(tower);
    }

    public void Untarget(Tower tower)
    {
        targetedBy.Remove(tower);
    }

    public void ForceTarget(Tower tower)
    {
        if (forceTargetedBy.Contains(tower))
        {
            return;
        }
        forceTargetedBy.Add(tower);
    }

    public void UnForceTarget(Tower tower)
    {
        forceTargetedBy.Remove(tower);
    }

    // ISelectable implementation
    public void Select()
    {
        isSelected = true;
        selectionVisual.SetActive(true);
    }

    public void Deselect()
    {
        isSelected = false;
        if (selectionVisual == null)
        {
            selectionVisual = transform.Find("SelectionVisual")?.gameObject;
        }
        if (selectionVisual != null)
        {
            selectionVisual.SetActive(false);
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public virtual Type GetSelectableType()
    {
        return enemyType switch
        {
            EnemyType.Basic => typeof(BasicEnemyType),
            EnemyType.Fast => typeof(FastEnemyType),
            EnemyType.Strong => typeof(StrongEnemyType),
            EnemyType.Big => typeof(BigEnemyType),
            _ => typeof(Enemy)
        };
    }

    public EnemyType GetEnemyType()
    {
        return enemyType;
    }

    public SelectInfo GetSelectInfo()
    {
        return new SelectInfo
        {
            name = enemyType switch
            {
                EnemyType.Basic => "Basic Enemy",
                EnemyType.Fast => "Fast Enemy",
                EnemyType.Strong => "Strong Enemy",
                EnemyType.Big => "Big Enemy",
                _ => "Enemy"
            },
            health = health,
            maxHealth = maxHealth,
            progress = progress,
            speed = speed,
            reward = reward
        };
    }

    void OnDrawGizmos()
    {
        if (currentPath != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.Length - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawWireSphere(currentPath[i], 0.1f);
            }
        }


    }
}

public enum EnemyType
{
    Basic,
    Fast,
    Strong,
    Big
}

// Marker types for enemy identification
public class BasicEnemyType { }
public class FastEnemyType { }
public class StrongEnemyType { }
public class BigEnemyType { }

