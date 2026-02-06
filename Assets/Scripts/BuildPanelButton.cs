using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BuildPanelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject linkedPrefab;
    private GameObject tooltip;

    void Start()
    {
        tooltip = transform.Find("Tooltip").gameObject;
        tooltip.SetActive(false);
        var tooltipText = tooltip.transform.Find("TooltipText").GetComponent<TextMeshProUGUI>();
        if (linkedPrefab && linkedPrefab.GetComponent<Tower>() != null)
        {
            tooltipText.text = linkedPrefab.GetComponent<Tower>().GetInfoString();
        }
        else
        {
            tooltipText.text = "Wall that can be upgraded to a tower";
        }
    }

    protected virtual void Update()
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
    public virtual void SetupButton(GameUIHandler gameUIHandler)
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            gameUIHandler.SelectBuildPlaceable(linkedPrefab);
            tooltip.SetActive(false);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.SetActive(false);
    }
    

}
