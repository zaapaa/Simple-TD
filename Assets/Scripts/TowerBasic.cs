using UnityEngine;

public class TowerBasic : Tower
{
    public override SelectInfo GetSelectInfo()
    {
        SelectInfo selectInfo = base.GetSelectInfo();
        selectInfo.name = "Basic Tower";
        return selectInfo;
    }

    public override System.Type GetSelectableType()
    {
        return typeof(TowerBasic);
    }
}
