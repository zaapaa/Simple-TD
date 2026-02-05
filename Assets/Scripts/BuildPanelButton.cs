using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildPanelButton : MonoBehaviour
{
    public GameObject linkedPrefab;

    void Update()
    {
        Placeable placeable = linkedPrefab.GetComponent<Placeable>();
        TextMeshProUGUI priceText = transform.Find("Price Text").GetComponent<TextMeshProUGUI>();
        Button button = GetComponent<Button>();
        if (GameManager.instance.HasEnoughMoney(placeable.placementCost))
        {
            priceText.color = Color.green;
            button.interactable = true;
        }
        else
        {
            priceText.color = Color.red;
            button.interactable = false;
        }
    }
    public void SetupButton(GameUIHandler gameUIHandler)
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => gameUIHandler.SelectBuildPlaceable(linkedPrefab));
    }
}
