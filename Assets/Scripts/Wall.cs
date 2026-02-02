using UnityEngine;
using System;

public class Wall : Placeable
{
    public override Type GetSelectableType()
    {
        return typeof(Wall);
    }
    public override SelectInfo GetSelectInfo()
    {
        SelectInfo selectInfo = new SelectInfo();
        selectInfo.name = nameof(Wall);
        return selectInfo;
    }
}
