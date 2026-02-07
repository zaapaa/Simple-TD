using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class TargetPriorityUIManager : MonoBehaviour
{
    [System.Serializable]
    public class TargetingEvent : UnityEvent<TargetingType> { }

    public TargetingEvent onTargetingTypeSelected;
    public List<Button> buttons = new List<Button>();

    public Color selectedButtonColor = Color.green;
    public Color defaultButtonColor = Color.white;

    public void UpdateTargetingButtons(List<ISelectable> selectedObjects)
    {
        List<TargetingType> targetingTypes = new List<TargetingType>();
        foreach (ISelectable selectable in selectedObjects)
        {
            if (selectable is Tower tower)
            {
                if (!targetingTypes.Contains(tower.targetingType))
                {
                    targetingTypes.Add(tower.targetingType);
                }
            }
            else
            {
                break;
            }
        }
        SetTargetingButtons(targetingTypes);
    }
    public void SetTargetingButtons(List<TargetingType> targetingTypes)
    {
        foreach (Button button in buttons)
        {
            button.image.color = defaultButtonColor;
        }

        foreach (TargetingType targetingType in targetingTypes)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].GetComponent<TargetingButton>().targetingType == targetingType)
                {
                    buttons[i].image.color = selectedButtonColor;
                }
            }
        }

        gameObject.SetActive(targetingTypes.Count > 0);
    }

    public void Clicked(TargetingType type)
    {
        onTargetingTypeSelected?.Invoke(type);
        SetTargetingButtons(new List<TargetingType> { type });
    }

    // Helper methods for button onClick (these will appear in Inspector)
    public void SetTargetingNearest() => Clicked(TargetingType.Nearest);
    public void SetTargetingFarthest() => Clicked(TargetingType.Farthest);
    public void SetTargetingStrongest() => Clicked(TargetingType.Strongest);
    public void SetTargetingWeakest() => Clicked(TargetingType.Weakest);
    public void SetTargetingFirst() => Clicked(TargetingType.First);
    public void SetTargetingLast() => Clicked(TargetingType.Last);
}
