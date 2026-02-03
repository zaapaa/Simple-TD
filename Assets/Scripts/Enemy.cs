using UnityEngine;
using UnityEngine.AI;
using System;

public class Enemy : MonoBehaviour, ISelectable
{

    public float maxHealth;
    public float health;
    public float progress;
    public float speed;
    public float waveBudget;
    public float reward;
    public int minSpawnWaveNumber;
    public float waitTime;
    public Sprite UIIcon;
    private float waitTimer;
    public EnemyType enemyType;

    private NavMeshAgent agent;
    private Transform target;
    private Transform spawnPoint;
    private float totalPathLength;
    private EnemyWaveSpawner waveSpawner;
    private GameObject selectionVisual;
    private bool isSelected = false;

    public void Initialize(Transform spawn, Transform end, int spawnPositionInWave, EnemyWaveSpawner spawner)
    {
        spawnPoint = spawn;
        target = end;
        waitTimer = spawnPositionInWave * waitTime;
        waveSpawner = spawner;
        agent = GetComponent<NavMeshAgent>();
        CalculateTotalPathLength();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectionVisual = transform.Find("Selection")?.gameObject;
        health = maxHealth;
        agent.speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                agent.SetDestination(target.position);
            }
            return;
        }
        // Update progress based on remaining distance
        if (target != null && totalPathLength > 0)
        {
            progress = 1f - (agent.remainingDistance / totalPathLength);
        }

        // Check if reached destination
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            ReachEndpoint();
        }
    }

    void CalculateTotalPathLength()
    {
        if (spawnPoint == null || target == null) return;

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(spawnPoint.position, target.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            totalPathLength = GetPathLength(path.corners);
        }
        else
        {
            totalPathLength = Vector3.Distance(spawnPoint.position, target.position);
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

    void ReachEndpoint()
    {
        // Damage player and destroy enemy
        GameManager.instance.DecreaseLives(1);
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Give money to player
        GameManager.instance.IncreaseMoney(reward); // Adjust reward as needed
        Destroy(gameObject);
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
        selectionVisual.SetActive(false);
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
}

public enum EnemyType
{
    Basic,
    Fast,
    Strong,
    Big
}

// Marker types for enemy identification
public class BasicEnemyType {}
public class FastEnemyType {}
public class StrongEnemyType {}
public class BigEnemyType {}

