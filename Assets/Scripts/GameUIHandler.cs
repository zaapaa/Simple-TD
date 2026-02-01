using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameUIHandler : MonoBehaviour
{

    public List<GameObject> placeablePrefabs;
    private GameObject currentPlacement = null;
    private float placementAngle = 0f;
    private float placementAngleStep = 15f;
    private List<ISelectable> selectedObjects = new List<ISelectable>();
    public IconManager iconManager;
    public List<GameObject> placedObjects = new List<GameObject>();
    public TargetPriorityUIManager targetPriorityUIManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            ClearSelection();
            StartPlacement();
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentPlacement == null)
            {
                StartSelect();
            }
            else
            {
                bool wasPlacementSuccessful = currentPlacement.GetComponent<Placeable>().Place();
                if (wasPlacementSuccessful)
                {
                    placedObjects.Add(currentPlacement);
                    currentPlacement = null;
                    if (Keyboard.current.shiftKey.isPressed)
                    {
                        StartPlacement();
                    }
                }
            }
        }

        if (currentPlacement != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll > 0)
            {
                placementAngle += placementAngleStep;
                currentPlacement.transform.Rotate(Vector3.up, placementAngleStep);
            }
            else if (scroll < 0)
            {
                placementAngle -= placementAngleStep;
                currentPlacement.transform.Rotate(Vector3.up, -placementAngleStep);
            }
        }
        
        if (Keyboard.current.aKey.wasPressedThisFrame && Keyboard.current.ctrlKey.isPressed)
        {
            ClearSelection();
            foreach (var placedObject in placedObjects)
            {
                if (placedObject.TryGetComponent<ISelectable>(out ISelectable selectable))
                {
                    selectedObjects.Add(selectable);
                    selectable.Select();
                }
            }
        }

    }

    void StartPlacement()
    {
        if (currentPlacement == null)
        {
            var mousePosition = Mouse.current.position.ReadValue();
            var ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject placementObject = Instantiate(placeablePrefabs[1], hit.point, Quaternion.Euler(0, placementAngle, 0));
                placementObject.GetComponent<Placeable>().isBeingPlaced = true;
                currentPlacement = placementObject;
                Debug.Log($"Placed object {placementObject.name} at {hit.point}");
            }
        }
    }
    GameObject GetObjectAtMousePosition()
    {
        var mousePosition = Mouse.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    void ClearSelection()
    {
        foreach (ISelectable selectable in selectedObjects)
        {
            selectable.Deselect();
        }
        selectedObjects.Clear();
        
        // Update icon display
        if (iconManager != null)
        {
            iconManager.RefreshIcons(selectedObjects);
        }
    }

    void StartSelect()
    {
        GameObject objectAtMouse = GetObjectAtMousePosition();
        if (objectAtMouse != null)
        {
            if (objectAtMouse.CompareTag("Ground"))
            {
                ClearSelection();
                return;
            }

            ISelectable selectable = objectAtMouse.GetComponent<ISelectable>();
            if (selectable != null)
            {
                if (Keyboard.current.shiftKey.isPressed || Keyboard.current.ctrlKey.isPressed)
                {
                    // Multi-select: add to selection if not already selected
                    if (!selectable.IsSelected())
                    {
                        selectable.Select();
                        selectedObjects.Add(selectable);
                    }
                    else
                    {
                        selectable.Deselect();
                        selectedObjects.Remove(selectable);
                    }
                }
                else
                {
                    // Single-select: clear current selection and select new object
                    ClearSelection();
                    selectable.Select();
                    selectedObjects.Add(selectable);
                }
                // Update icon display
                if (iconManager != null)
                {
                    iconManager.RefreshIcons(selectedObjects);
                }
            }
        }
        else
        {
            // Clicked on empty space - clear selection
            ClearSelection();
            // Update icon display
            if (iconManager != null)
            {
                iconManager.RefreshIcons(selectedObjects);
            }
        }
        if (IsOnlyTowersSelected())
        {
            targetPriorityUIManager.UpdateTargetingButtons(selectedObjects);
        }
    }

    private bool IsOnlyTowersSelected()
    {
        foreach (ISelectable selectable in selectedObjects)
        {
            if (selectable is not Tower)
            {
                return false;
            }
        }
        return true;
    }

    public void OnTowerTargetingTypeSelected(TargetingType type)
    {
        if (IsOnlyTowersSelected())
        {
            foreach (ISelectable selectable in selectedObjects)
            {
                if (selectable is Tower tower)
                {
                    tower.targetingType = type;
                }
            }
        }
    }
    
    void ShowSelectedInformation()
    {
        //TODO: show information about selected objects
    }
}
