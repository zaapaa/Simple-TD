using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class IconHandler : MonoBehaviour
{
    public Type selectableType;
    public int count;
    public GameObject countText;
    void Start()
    {
        if (countText == null)
        {
            countText = transform.Find("Count Text").gameObject;
        }
    }

    public void SetCount(int newCount)
    {
        count = newCount;
        UpdateCountDisplay();
    }

    public void SetIcon(Sprite newIcon)
    {
        GameObject iconObject = transform.Find("Icon")?.gameObject;
        if (iconObject != null)
        {
            Image iconImage = iconObject.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = newIcon;
            }
        }
    }

    private void UpdateCountDisplay()
    {
        if (countText != null)
        {
            if (count > 1)
            {
                countText.GetComponent<TextMeshProUGUI>().text = $"x{count}";
            }
            else
            {
                countText.GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetType(Type type)
    {
        selectableType = type;
    }
}
