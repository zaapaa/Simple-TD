using UnityEngine;

public class TowerMissile : Tower
{
    public override SelectInfo GetSelectInfo()
    {
        SelectInfo selectInfo = base.GetSelectInfo();
        selectInfo.name = "Missile Tower";
        return selectInfo;
    }

    public override System.Type GetSelectableType()
    {
        return typeof(TowerMissile);
    }
}
