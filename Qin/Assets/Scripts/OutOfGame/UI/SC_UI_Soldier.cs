using UnityEngine;
using static SC_Global;

public class SC_UI_Soldier : SC_UI_Creation {

    protected override bool Condition () {

        return CanCreateSoldier(name);

    }

    protected override void Listener () {

        SC_UI_Manager.Instance.PitCreateSoldier(name);

    }

    protected override void TooltipUpdate () {

        SC_Soldier soldier = Resources.Load<SC_Soldier>("Prefabs/Characters/Soldiers/Basic/P_" + name);

        SC_UI_Manager.Instance.UpdateCreationTooltip(

            new string[3] { soldier.characterName, soldier.cost.ToString(), soldier.description },
            false

        );

    }

}
