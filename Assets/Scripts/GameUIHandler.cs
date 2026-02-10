using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.EventSystems;

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
    public EnemyWaveSpawner waveSpawner = null;
    public GameObject enemyDirectionArrow;
    public GameObject cameraPerspective;
    public GameObject cameraOrthographic;
    public static GameUIHandler instance = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        selectInfoText.text = "";
        selectionPanel.SetActive(false);
        buildPanel.SetActive(false);
        upgradePanel.SetActive(false);
    }

    bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.isGameOver)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
            return;
        }
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
            GameObject forceTargetEnemy;
            if (IsOnlyTowersSelected() && IsEnemyUnderMouse(out forceTargetEnemy))
            {
                foreach (Tower tower in selectedObjects)
                {
                    tower.ForceTarget(forceTargetEnemy);
                }
            }
            else
            {
                ClearSelection();
                ShowPanel(buildPanel, placeablePrefabs);
            }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HidePanel(buildPanel);
            if (currentPlacement == null)
            {
                // Don't clear selection if clicking on UI
                if (!IsMouseOverUI())
                {
                    StartSelect();
                }
            }
            else
            {
                // Start path validation coroutine
                StartCoroutine(ValidatePlacementAndPlace(currentPlacement));
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
            if (IsOnlyWallsSelected())
            {
                ShowPanel(upgradePanel, GetUpgradePrefabs());
            }
        }
        
        if (Keyboard.current.digit1Key.wasReleasedThisFrame)
        {
            if(GameManager.instance.HasEnoughMoney(placeablePrefabs[0].GetComponent<Placeable>().placementCost))
            {
                SelectBuildPlaceable(placeablePrefabs[0]);
                StartPlacement();
            }
        }
        if (Keyboard.current.digit2Key.wasReleasedThisFrame)
        {
            if(GameManager.instance.HasEnoughMoney(placeablePrefabs[1].GetComponent<Placeable>().placementCost))
            {
                SelectBuildPlaceable(placeablePrefabs[1]);
                StartPlacement();
            }
        }
        if (Keyboard.current.digit3Key.wasReleasedThisFrame)
        {
            if(GameManager.instance.HasEnoughMoney(placeablePrefabs[2].GetComponent<Placeable>().placementCost))
            {
                SelectBuildPlaceable(placeablePrefabs[2]);
                StartPlacement();
            }
        }
        if (Keyboard.current.digit4Key.wasReleasedThisFrame)
        {
            if(GameManager.instance.HasEnoughMoney(placeablePrefabs[3].GetComponent<Placeable>().placementCost))
            {
                SelectBuildPlaceable(placeablePrefabs[3]);
                StartPlacement();
            }
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GameManager.instance.TogglePause();
        }
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            GameManager.instance.SetGameSpeed(1f);
        }
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            GameManager.instance.SetGameSpeed(2f);
        }
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            GameManager.instance.SetGameSpeed(4f);
        }
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            cameraOrthographic.SetActive(!cameraOrthographic.activeSelf);
            cameraPerspective.SetActive(!cameraPerspective.activeSelf);
        }
        ShowSelectedInformation();
    }

    void ShowPanel(GameObject panel, List<GameObject> prefabs)
    {
        enemyDirectionArrow.SetActive(false);
        panel.GetComponent<BuildPanel>().PopulatePanel(prefabs);
        panel.SetActive(true);
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        RectTransform canvasRect = panel.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
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
        HidePanel(buildPanel);
    }

    void HidePanel(GameObject panel)
    {
        StartCoroutine(HidePanelDelayed(panel));
    }

    System.Collections.IEnumerator HidePanelDelayed(GameObject panel)
    {
        yield return new WaitForSeconds(0.1f * Time.timeScale); // Wait 0.1 seconds
        panel.SetActive(false);
    }

    public void UpgradeWall(GameObject upgradePrefab)
    {
        Debug.Log("Upgrading wall with prefab: " + upgradePrefab.name + ", selected objects: " + string.Join(", ", selectedObjects.Select(x => (x as Placeable).name)));

        if (IsOnlyWallsSelected())
        {
            List<ISelectable> itemsToRemove = new List<ISelectable>();
            foreach (var selectedObject in selectedObjects)
            {
                float cost = upgradePrefab.GetComponent<Placeable>().placementCost - (selectedObject as Wall).placementCost;
                if (!GameManager.instance.HasEnoughMoney(cost)) break;
                GameManager.instance.DecreaseMoney(cost);
                selectedObject.Deselect();
                itemsToRemove.Add(selectedObject);
                (selectedObject as Wall).Upgrade(upgradePrefab);
            }
            foreach (var item in itemsToRemove)
            {
                selectedObjects.Remove(item);
            }
        }
        HidePanel(upgradePanel);
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
                // Debug.Log($"Placed object {placementObject.name} at {hit.point}");
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

    bool IsEnemyUnderMouse(out GameObject enemy)
    {
        enemy = null;
        GameObject objectAtMouse = GetObjectAtMousePosition();
        if (objectAtMouse != null)
        {
            if (objectAtMouse.CompareTag("Enemy"))
            {
                enemy = objectAtMouse;
                return true;
            }
            while (objectAtMouse.transform.parent != null)
            {
                objectAtMouse = objectAtMouse.transform.parent.gameObject;
                if (objectAtMouse.CompareTag("Enemy"))
                {
                    enemy = objectAtMouse;
                    return true;
                }
            }
        }
        return false;
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
                if (IsOnlyWallsSelected())
                {
                    ShowPanel(upgradePanel, GetUpgradePrefabs());
                }
            }
        }
        else
        {
            HidePanel(upgradePanel);
            // Clicked on empty space - clear selection
            ClearSelection();
        }
        ShowSelectedInformation();
    }

    List<GameObject> GetUpgradePrefabs()
    {
        return placeablePrefabs.Where(x => x.GetComponent<Wall>() == null).ToList();
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
    private bool IsOnlyWallsSelected()
    {
        foreach (ISelectable selectable in selectedObjects)
        {
            if (selectable is not Wall)
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
        selectedObjects.RemoveAll(x => x == null);
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
    public void RemoveSelection(ISelectable selectable)
    {
        selectable.Deselect();
        selectedObjects.Remove(selectable);
        ShowSelectedInformation();
    }
    public void StartNextWaveButtonPressed()
    {
        if (waveSpawner != null)
        {
            waveSpawner.StartNextWave();
            enemyDirectionArrow.SetActive(false);
        }
    }
    System.Collections.IEnumerator ValidatePlacementAndPlace(GameObject placementObject)
    {
        // Get spawn and end points from EnemyWaveSpawner
        if (waveSpawner == null || waveSpawner.spawnPoint == null || waveSpawner.endpoint == null)
        {
            // Allow placement if spawner not set up yet
            CompletePlacement(placementObject);
            yield break;
        }

        // Get existing NavMeshObstacle component
        NavMeshObstacle navObstacle = placementObject.GetComponent<NavMeshObstacle>();
        if (navObstacle == null)
        {
            // No NavMeshObstacle, allow placement
            CompletePlacement(placementObject);
            yield break;
        }

        // Store original enabled state
        bool wasEnabled = navObstacle.enabled;

        // Temporarily enable the obstacle for path validation
        navObstacle.enabled = true;

        // Wait a frame for NavMesh to update
        yield return null;

        // Calculate path with obstacle present
        NavMeshPath path = new NavMeshPath();
        bool pathExists = NavMesh.CalculatePath(waveSpawner.spawnPoint.position, waveSpawner.endpoint.position, NavMesh.AllAreas, path);

        // Restore original state
        navObstacle.enabled = wasEnabled;

        // Wait another frame for NavMesh to restore
        yield return null;

        // Complete placement only if path is still valid
        if (pathExists && path.status == NavMeshPathStatus.PathComplete)
        {
            CompletePlacement(placementObject);
        }
        else
        {
            Debug.Log("Placement blocked: would obstruct enemy path");
        }
    }

    void CompletePlacement(GameObject placementObject)
    {
        if (!GameManager.instance.HasEnoughMoney(placementObject.GetComponent<Placeable>().placementCost))
        {
            return;
        }
        bool wasPlacementSuccessful = placementObject.GetComponent<Placeable>().Place();
        if (wasPlacementSuccessful)
        {
            placedObjects.Add(placementObject);
            currentPlacement = null;
            GameManager.instance.DecreaseMoney(placementObject.GetComponent<Placeable>().placementCost);
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
