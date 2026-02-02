using UnityEngine;

public class TowerLaser : Tower
{
    public override SelectInfo GetSelectInfo()
    {
        SelectInfo selectInfo = base.GetSelectInfo();
        selectInfo.name = "Laser Tower";
        return selectInfo;
    }

    public override System.Type GetSelectableType()
    {
        return typeof(TowerLaser);
    }
}
