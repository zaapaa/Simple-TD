using UnityEngine;
using System;

public interface ISelectable
{
    void Select();
    void Deselect();
    bool IsSelected();
    Type GetSelectableType();
    SelectInfo GetSelectInfo();
}
