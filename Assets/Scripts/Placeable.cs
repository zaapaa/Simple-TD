using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.AI;

public class Placeable : MonoBehaviour, ISelectable
{
    public string description;
    public Sprite UIIcon;
    public float placementCost;
    public float placementRadius = 1.3f;
    public float placementRadiusEnemy = 2f;
    public bool isBeingPlaced = true;
    public Color validPlacementColor = Color.green;
    public Color invalidPlacementColor = Color.red;
    public Color obstructPlacementColor = Color.yellow;

    private float yOffsetWhenPlacing = 0.01f;
    private bool previousValidPosition;
    private bool firstPlacementCheck = true;
    private GameObject obstructingPlaceable = null;
    private bool isSelected = false;

    private MaterialUpdater materialUpdater;
    private GameObject selectionVisual;
    protected virtual void Start()
    {
        materialUpdater = GetComponent<MaterialUpdater>();
        selectionVisual = transform.Find("Selection")?.gameObject;
        if (selectionVisual != null)
        {
            selectionVisual.SetActive(false);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isBeingPlaced)
        {
            Vector3 mousePosition = GetMousePositionInWorld();
            if (obstructingPlaceable == null)
            {
                transform.position = new Vector3(mousePosition.x, 0f + yOffsetWhenPlacing, mousePosition.z);
            }
            bool validPosition = CheckClearance(mousePosition);
            bool shouldFlash = false;

            // Try to adjust position if obstructed by another placeable
            if (obstructingPlaceable != null)
            {
                validPosition = AdjustPlacementOnObstruction(mousePosition);
                // Debug.Log($"Valid position after adjustment: {validPosition}");
            }
            if (firstPlacementCheck)
            {
                firstPlacementCheck = false;
                shouldFlash = true;
            }
            else if (validPosition != previousValidPosition)
            {
                shouldFlash = true;
            }


            if (shouldFlash)
            {
                if (validPosition)
                {
                    materialUpdater.StartFlash(validPlacementColor, validPlacementColor);
                    if (obstructingPlaceable != null)
                    {
                        materialUpdater.StartFlash(obstructPlacementColor, obstructPlacementColor);
                    }
                }
                else
                {
                    materialUpdater.StartFlash(invalidPlacementColor, invalidPlacementColor);
                }
                previousValidPosition = validPosition;
            }
        }
    }

    Vector3 GetMousePositionInWorld()
    {
        var mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        int layerMask = 1 << 6; //layer 6
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private bool CheckClearance(Vector3 position, bool excludeObstructing = false)
    {
        Collider[] allColliders = Physics.OverlapSphere(position, placementRadius * 2);

        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i].gameObject == gameObject)
            {
                continue;
            }

            if (excludeObstructing && allColliders[i].gameObject == obstructingPlaceable)
            {
                continue;
            }

            // Handle different object types with appropriate collision detection
            if (allColliders[i].CompareTag("Placeable"))
            {
                // Use spherical distance for placeables
                float distance = Vector3.Distance(position, allColliders[i].transform.position);
                var otherPlaceable = allColliders[i].GetComponent<Placeable>();
                float combinedRadius = placementRadius + (otherPlaceable?.placementRadius ?? placementRadius);

                if (distance < combinedRadius)
                {
                    if (excludeObstructing)
                    {
                        float obstructingDistance = Vector3.Distance(position, obstructingPlaceable.transform.position);
                        float currentIndexDistance = Vector3.Distance(position, allColliders[i].transform.position);
                        if (currentIndexDistance < obstructingDistance)
                        {
                            obstructingPlaceable = allColliders[i].gameObject;
                        }
                        return false;
                    }
                    else
                    {
                        obstructingPlaceable = allColliders[i].gameObject;
                        return true;
                    }
                }
            }
            else if (allColliders[i].CompareTag("Wall"))
            {
                // Use actual collider bounds for walls (handles non-equilateral shapes)
                if (IsCollidingWithWall(position, allColliders[i]))
                {
                    obstructingPlaceable = null;
                    return false;
                }
            }
            else if (allColliders[i].CompareTag("Enemy"))
            {
                // Use spherical distance for enemies (assuming they're roughly circular)
                float distance = Vector3.Distance(position, allColliders[i].transform.position);
                if (distance < placementRadiusEnemy)
                {
                    obstructingPlaceable = null;
                    return false;
                }
            }
        }

        if (!excludeObstructing && obstructingPlaceable != null)
        {
            obstructingPlaceable = null;
        }
        return true;
    }

    private bool IsCollidingWithWall(Vector3 position, Collider wallCollider)
    {
        // Use the wall's actual collider bounds for accurate detection
        Bounds wallBounds = wallCollider.bounds;

        // Create a bounds for the placeable at the given position
        Vector3 placeableSize = Vector3.one * (placementRadius * 2);
        Bounds placeableBounds = new Bounds(position, placeableSize);

        // Check if the bounds intersect
        return wallBounds.Intersects(placeableBounds);
    }

    private bool AdjustPlacementOnObstruction(Vector3 position)
    {
        Vector3 obstructionPosition = obstructingPlaceable.transform.position;

        position.y = 0f;
        obstructionPosition.y = 0f;

        // Calculate direction from obstruction to position
        Vector3 direction = (position - obstructionPosition).normalized;

        // If direction is zero, use a default direction
        if (direction == Vector3.zero)
        {
            return false;
        }

        // Calculate the distance needed to place next to the obstruction
        float distance = placementRadius + obstructingPlaceable.GetComponent<Placeable>().placementRadius;

        // Set new position
        Vector3 newPosition = obstructionPosition + direction * distance;
        newPosition.y = yOffsetWhenPlacing; // Maintain the same height

        transform.position = newPosition;
        if (CheckClearance(newPosition, true))
        {
            return true;
        }
        return false;
    }

    public virtual bool Place(bool force = false)
    {
        if (previousValidPosition || force)
        {
            isBeingPlaced = false;
            materialUpdater?.StopFlash();
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            NavMeshObstacle navObstacle = GetComponent<NavMeshObstacle>();
            navObstacle.enabled = true;
            
            // Notify all enemies to recalculate their paths
            NotifyEnemiesOfPlacement();
            
            return true;
        }
        return false;
    }

    private void NotifyEnemiesOfPlacement()
    {
        // Find all active enemies and notify them
        foreach (Transform child in GameUIHandler.instance.waveSpawner.transform)
        {
            child.GetComponent<Enemy>().OnPlaceablePlaced();
        }
    }

    public virtual void Select()
    {
        isSelected = true;
        selectionVisual.SetActive(true);
    }

    public virtual void Deselect()
    {
        isSelected = false;
        selectionVisual?.SetActive(false);
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public virtual Type GetSelectableType()
    {
        return typeof(Placeable);
    }
    public virtual SelectInfo GetSelectInfo()
    {
        SelectInfo selectInfo = new SelectInfo();
        selectInfo.name = nameof(Placeable);
        return selectInfo;
    }
}
