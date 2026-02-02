using UnityEngine;
using UnityEngine.UI;

public class BuildPanelButton : MonoBehaviour
{
    public GameObject linkedPrefab;

    public void SetupButton(GameUIHandler gameUIHandler)
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => gameUIHandler.SelectBuildPlaceable(linkedPrefab));
    }
}
