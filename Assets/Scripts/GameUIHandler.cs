using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;

public class GameUIHandler : MonoBehaviour
{

    public List<GameObject> placeablePrefabs;
    private GameObject currentPlacement = null;
    private float placementAngle = 0f;
    private float placementAngleStep = 15f;
    private List<ISelectable> selectedObjects = new List<ISelectable>();
    public GameObject selectionPanel;
    public GameObject selectionHeaderText;
    public IconManager iconManager;
    public List<GameObject> placedObjects = new List<GameObject>();
    public TargetPriorityUIManager targetPriorityUIManager;
    public GameObject buildPanel;
    public GameObject upgradePanel;
    public TextMeshProUGUI selectInfoText;
    private GameObject selectedBuildPlaceable = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectInfoText.text = "";
        selectionPanel.SetActive(false);
        buildPanel.SetActive(false);
        upgradePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Build placeable selected from build panel, begin placement
        if (selectedBuildPlaceable != null)
        {
            StartPlacement();
        }
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (currentPlacement != null)
            {
                Destroy(currentPlacement);
                currentPlacement = null;
                selectedBuildPlaceable = null;
            }
            // TODO: if tower(s) selected and enemy right clicked, override towers' target to that enemy. then return to prevent build panel
            ClearSelection();
            ShowBuildPanel();
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HideBuildPanel();
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
                    else
                    {
                        selectedBuildPlaceable = null;
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
            ShowSelectedInformation();
        }

    }

    void ShowBuildPanel()
    {
        buildPanel.GetComponent<BuildPanel>().PopulatePanel(placeablePrefabs);
        buildPanel.SetActive(true);
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        RectTransform panelRect = buildPanel.GetComponent<RectTransform>();
        RectTransform canvasRect = buildPanel.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        Vector2 panelSize = panelRect.sizeDelta;

        // Convert mouse position to local canvas coordinates
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePosition, null, out Vector2 localPoint);

        // Determine if mouse is on right side of screen
        bool isOnRightSide = mousePosition.x > Screen.width / 2;
        bool isOnTopSide = mousePosition.y > Screen.height / 2;

        // Calculate position based on mouse position
        float xOffset, yOffset;

        // X-axis positioning
        if (isOnRightSide)
        {
            // Position on right of mouse
            xOffset = -panelSize.x * (1 - panelRect.pivot.x);
        }
        else
        {
            // Position on left of mouse
            xOffset = panelSize.x * panelRect.pivot.x;
        }

        // Y-axis positioning
        if (isOnTopSide)
        {
            // Position below mouse
            yOffset = -panelSize.y * (1 - panelRect.pivot.y);
        }
        else
        {
            // Position above mouse
            yOffset = panelSize.y * panelRect.pivot.y;
        }

        panelRect.anchoredPosition = localPoint + new Vector2(xOffset, yOffset);
    }

    public void SelectBuildPlaceable(GameObject gameObject)
    {
        selectedBuildPlaceable = gameObject;
        HideBuildPanel();
    }

    void HideBuildPanel()
    {
        StartCoroutine(HideBuildPanelDelayed());
    }

    System.Collections.IEnumerator HideBuildPanelDelayed()
    {
        yield return new WaitForSeconds(0.1f); // Wait 0.1 seconds
        buildPanel.SetActive(false);
    }

    void StartPlacement()
    {

        if (currentPlacement == null)
        {
            var mousePosition = Mouse.current.position.ReadValue();
            var ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject placementObject = Instantiate(selectedBuildPlaceable, hit.point, Quaternion.Euler(0, placementAngle, 0));
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

        // Update selection display
        ShowSelectedInformation();
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
            }
        }
        else
        {
            // Clicked on empty space - clear selection
            ClearSelection();
        }
        ShowSelectedInformation();
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
        if (selectedObjects.Count == 0)
        {
            selectionPanel.SetActive(false);
            return;
        }
        
        iconManager?.RefreshIcons(selectedObjects);
        targetPriorityUIManager.UpdateTargetingButtons(selectedObjects);

        SelectInfo combinedInfo = selectedObjects[0].GetSelectInfo();
        for (int i = 1; i < selectedObjects.Count; i++)
        {
            ISelectable selectable = selectedObjects[i];
            combinedInfo += selectable.GetSelectInfo();
        }
        selectInfoText.text = combinedInfo.ToString();
        selectionHeaderText.GetComponent<TMPro.TextMeshProUGUI>().text = selectedObjects.Count == 1 ? "Selection" : $"Selection ({selectedObjects.Count})";
        selectionPanel.SetActive(true);
    }
}
