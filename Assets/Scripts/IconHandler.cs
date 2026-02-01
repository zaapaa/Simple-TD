using UnityEngine;
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
        UpdateCountDisplay();
        Hide();
    }

    public void SetCount(int newCount)
    {
        count = newCount;
        UpdateCountDisplay();
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
