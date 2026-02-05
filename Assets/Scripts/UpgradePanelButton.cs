using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanelButton : BuildPanelButton
{
    public Wall wall;

    protected override void Update()
    {
        Placeable placeable = linkedPrefab.GetComponent<Placeable>();
        TextMeshProUGUI priceText = transform.Find("Price Text").GetComponent<TextMeshProUGUI>();
        Button button = GetComponent<Button>();
        float cost = placeable.placementCost - wall.placementCost;
        priceText.text = cost.ToString("F0");
        if (GameManager.instance.HasEnoughMoney(cost))
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
    public override void SetupButton(GameUIHandler gameUIHandler)
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => gameUIHandler.UpgradeWall(linkedPrefab));
    }
}
