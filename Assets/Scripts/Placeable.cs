using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Placeable : MonoBehaviour, ISelectable
{
    public float placementCost;
    public float placementRadius = 2.5f;
    public bool isBeingPlaced = true;
    public Color validPlacementColor = Color.green;
    public Color invalidPlacementColor = Color.red;
    public Color obstructPlacementColor = Color.yellow;
    public Sprite UIIcon;

    private float yOffsetWhenPlacing = 0.01f;
    private bool previousValidPosition;
    private bool firstPlacementCheck = true;
    private GameObject obstructingPlaceable = null;
    private bool isSelected = false;

    private MaterialUpdater materialUpdater;
    private GameObject selectionVisual;
    void Start()
    {
        materialUpdater = GetComponent<MaterialUpdater>();
        selectionVisual = transform.Find("Selection")?.gameObject;
        if (selectionVisual != null)
        {
            selectionVisual.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
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
        Collider[] colliders = Physics.OverlapSphere(position, placementRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject == gameObject)
            {
                continue;
            }
            if (excludeObstructing && colliders[i].gameObject == obstructingPlaceable)
            {
                continue;
            }
            if (colliders[i].CompareTag("Placeable"))
            {
                if (excludeObstructing)
                {
                    float obstructingDistance = Vector3.Distance(position, obstructingPlaceable.transform.position);
                    float currentIndexDistance = Vector3.Distance(position, colliders[i].transform.position);
                    if (currentIndexDistance < obstructingDistance)
                    {
                        obstructingPlaceable = colliders[i].gameObject;
                    }
                    return false;
                }
                else
                {
                    obstructingPlaceable = colliders[i].gameObject;
                    return true;
                }
            }
            if (colliders[i].CompareTag("Wall") || colliders[i].CompareTag("Enemy"))
            {
                obstructingPlaceable = null;
                return false;
            }
        }
        if (!excludeObstructing && obstructingPlaceable != null)
        {
            obstructingPlaceable = null;
        }
        return true;
    }

    private bool AdjustPlacementOnObstruction(Vector3 position)
    {
        Vector3 obstructionPosition = obstructingPlaceable.transform.position;

        // Calculate direction from obstruction to position
        Vector3 direction = (position - obstructionPosition).normalized;

        // If direction is zero, use a default direction
        if (direction == Vector3.zero)
        {
            direction = Vector3.forward;
        }

        // Calculate the distance needed to place next to the obstruction
        float distance = placementRadius + obstructingPlaceable.GetComponent<Placeable>().placementRadius;

        // Set new position
        Vector3 newPosition = obstructionPosition + direction * distance;
        newPosition.y = 0f + yOffsetWhenPlacing; // Maintain the same height

        transform.position = newPosition;
        if (CheckClearance(newPosition, true))
        {
            return true;
        }
        return false;
    }

    public bool Place()
    {
        if (previousValidPosition)
        {
            isBeingPlaced = false;
            materialUpdater.StopFlash();
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            return true;
        }
        return false;
    }

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

    public Type GetSelectableType()
    {
        return typeof(Placeable);
    }
}
