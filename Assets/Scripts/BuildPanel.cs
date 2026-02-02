using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

public class BuildPanel : MonoBehaviour
{

    public float widthPerIcon = 74f;
    public List<GameObject> iconContainers = new List<GameObject>();
    public GameUIHandler gameUIHandler;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PopulatePanel(List<GameObject> placeables)
    {
        Debug.Assert(iconContainers.Count > 0, "BuildPanel: No icon containers found!");
        Debug.Assert(placeables.Count > 0, "BuildPanel: Invalid placeables list!");
        float totalWidth = placeables.Count * widthPerIcon;
        transform.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth, widthPerIcon);
        for (int i = 0; i < placeables.Count; i++)
        {
            GameObject iconContainer;
            if (iconContainers.Count > i)
            {
                iconContainer = iconContainers[i];
            }
            else
            {
                iconContainer = Instantiate(iconContainers[0], transform);
                iconContainers.Add(iconContainer);
                iconContainer.transform.SetParent(iconContainers[0].transform.parent);
            }
            // Icon
            GameObject icon = iconContainer.transform.Find("Icon").gameObject;
            icon.GetComponent<Image>().sprite = placeables[i].GetComponent<Placeable>().UIIcon;
            // Price Text
            GameObject price = iconContainer.transform.Find("Price Text").gameObject;
            price.GetComponent<TextMeshProUGUI>().text = placeables[i].GetComponent<Placeable>().placementCost.ToString("F0");
            // Prefab
            iconContainer.GetComponent<BuildPanelButton>().linkedPrefab = placeables[i];
            iconContainer.GetComponent<BuildPanelButton>().SetupButton(gameUIHandler);
        }
        if (iconContainers.Count > placeables.Count)
        {
            for (int i = placeables.Count; i < iconContainers.Count; i++)
            {
                Destroy(iconContainers[i]);
            }
            iconContainers.RemoveRange(placeables.Count, iconContainers.Count - placeables.Count);
        }
    }
}
