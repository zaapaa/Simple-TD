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

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void UpdateTargetingButtons(List<ISelectable> selectedObjects)
    {
        TargetingType targetingType = TargetingType.None;
        foreach (ISelectable selectable in selectedObjects)
        {
            if (selectable is Tower tower)
            {
                if (targetingType == TargetingType.None)
                {
                    targetingType = tower.targetingType;
                }
                else if (targetingType != tower.targetingType)
                {
                    targetingType = TargetingType.None;
                    break;
                }
            }
        }
        SetTargetingButtons(targetingType);
    }
    public void SetTargetingButtons(TargetingType targetingType)
    {
        foreach (Button button in buttons)
        {
            button.image.color = defaultButtonColor;
        }
        
        switch (targetingType)
        {
            case TargetingType.Nearest:
                buttons[0].image.color = selectedButtonColor;
                break;
            case TargetingType.Farthest:
                buttons[1].image.color = selectedButtonColor;
                break;
            case TargetingType.Weakest:
                buttons[2].image.color = selectedButtonColor;
                break;
            case TargetingType.Strongest:
                buttons[3].image.color = selectedButtonColor;
                break;
            case TargetingType.First:
                buttons[4].image.color = selectedButtonColor;
                break;
            case TargetingType.Last:
                buttons[5].image.color = selectedButtonColor;
                break;
        }
    }
    
    public void Clicked(TargetingType type)
    {
        onTargetingTypeSelected?.Invoke(type);
    }
    
    // Helper methods for button onClick (these will appear in Inspector)
    public void SetTargetingNearest() => Clicked(TargetingType.Nearest);
    public void SetTargetingFarthest() => Clicked(TargetingType.Farthest);
    public void SetTargetingWeakest() => Clicked(TargetingType.Weakest);
    public void SetTargetingStrongest() => Clicked(TargetingType.Strongest);
    public void SetTargetingFirst() => Clicked(TargetingType.First);
    public void SetTargetingLast() => Clicked(TargetingType.Last);
}
