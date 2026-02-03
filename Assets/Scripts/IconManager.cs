using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    public List<IconHandler> iconHandlers = new List<IconHandler>();
    public float iconSize1 = 150f;
    public float iconSize2 = 110f;
    public float iconSize3 = 69f;
    public float iconSize7 = 52f;

    private GridLayoutGroup gridLayout;

    void Start()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
    }

    public void RefreshIcons(List<ISelectable> selectedObjects)
    {
        // Reset all counts first
        foreach (var iconHandler in iconHandlers)
        {
            iconHandler.SetCount(0);
            iconHandler.selectableType = null;
            iconHandler.Hide();
        }

        int iconCount = 0;

        // Count selected objects by type and get their sprites
        Dictionary<Type, int> typeCounts = new Dictionary<Type, int>();
        Dictionary<Type, Sprite> typeSprites = new Dictionary<Type, Sprite>();
        foreach (var selectedObject in selectedObjects)
        {
            Type type = selectedObject.GetSelectableType();
            
            if (!typeCounts.ContainsKey(type))
            {
                typeCounts[type] = 1;
                
                // Get sprite based on selectable type
                if (selectedObject is Placeable placeable)
                {
                    typeSprites[type] = placeable.UIIcon;
                }
                else if (selectedObject is Enemy enemy)
                {
                    typeSprites[type] = enemy.UIIcon;
                }
            }
            else
            {
                typeCounts[type]++;
            }
        }

        //sort typecounts by count
        var sortedTypeCounts = new List<KeyValuePair<Type, int>>(typeCounts);
        sortedTypeCounts.Sort((x, y) => y.Value.CompareTo(x.Value));

        // Set selectable types and sprites for icon handlers with no duplicates
        List<Type> assignedTypes = new List<Type>();
        foreach (var iconHandler in iconHandlers)
        {
            if (iconHandler.selectableType == null)
            {
                // Find first unassigned type from typeCounts
                foreach (var kvp in sortedTypeCounts)
                {
                    if (!assignedTypes.Contains(kvp.Key))
                    {
                        iconHandler.SetType(kvp.Key);
                        
                        // Set the sprite if available
                        if (typeSprites.ContainsKey(kvp.Key))
                        {
                            iconHandler.SetIcon(typeSprites[kvp.Key]);
                        }
                        
                        assignedTypes.Add(kvp.Key);
                        break;
                    }
                }
            }
        }

        // Update icon handlers with counts
        foreach (var iconHandler in iconHandlers)
        {
            if (iconHandler.selectableType == null) continue;
            if (typeCounts.ContainsKey(iconHandler.selectableType))
            {
                iconHandler.SetCount(typeCounts[iconHandler.selectableType]);
                iconHandler.Show();
                iconCount++;
            }
            else
            {
                iconHandler.Hide();
            }
        }

        // Adjust grid layout based on number of icons
        if (iconCount == 1)
        {
            gridLayout.cellSize = new Vector2(iconSize1, iconSize1);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 1;
        }
        else if (iconCount == 2)
        {
            gridLayout.cellSize = new Vector2(iconSize2, iconSize2);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
        }
        else if (iconCount >= 3 && iconCount <= 6)
        {
            gridLayout.cellSize = new Vector2(iconSize3, iconSize3);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
        }
        else if (iconCount >= 7)
        {
            gridLayout.cellSize = new Vector2(iconSize7, iconSize7);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
        }
    }
}
