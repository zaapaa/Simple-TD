using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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
    private Transform endPoint;
    private Transform spawnPoint;
    private float totalPathLength;
    private EnemyWaveSpawner waveSpawner;
    private GameObject selectionVisual;
    private bool isSelected = false;
    private Image healthBarImage;
    private GameObject model;
    private GameObject targetVisual;
    private List<Tower> targetedBy = new List<Tower>();

    public void Initialize(Transform spawn, Transform end, EnemyWaveSpawner spawner)
    {
        spawnPoint = spawn;
        endPoint = end;
        waveSpawner = spawner;
        agent = GetComponent<NavMeshAgent>();
        CalculateTotalPathLength();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectionVisual = transform.Find("Selection")?.gameObject;
        model = transform.Find("Model")?.gameObject;
        targetVisual = transform.Find("Target")?.gameObject;
        health = maxHealth;
        agent.speed = speed;

        // Get healthbar image component
        healthBarImage = GetComponentInChildren<Image>();
        agent.SetDestination(endPoint.position);
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Update progress based on remaining distance
        if (endPoint != null && totalPathLength > 0)
        {
            progress = 1f - (agent.remainingDistance / totalPathLength);
        }

        // Check if reached destination
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            ReachEndpoint();
        }
        model.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        if (targetedBy.Count > 0)
        {
            targetVisual.SetActive(true);
        }
        else
        {
            targetVisual.SetActive(false);
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

    void ReachEndpoint()
    {
        // Damage player and destroy enemy
        GameManager.instance.DecreaseLives(1);
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
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
        if (enemyType == EnemyType.Big)
        {
            waveSpawner.SpawnEnemy(waveSpawner.enemyPrefabs[0], transform.position);
            waveSpawner.SpawnEnemy(waveSpawner.enemyPrefabs[0], transform.position);
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
public class BasicEnemyType { }
public class FastEnemyType { }
public class StrongEnemyType { }
public class BigEnemyType { }

