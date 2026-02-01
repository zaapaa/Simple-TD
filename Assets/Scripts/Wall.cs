using UnityEngine;
using System;

public class Wall : Placeable
{
    public override Type GetSelectableType()
    {
        return typeof(Wall);
    }
}
