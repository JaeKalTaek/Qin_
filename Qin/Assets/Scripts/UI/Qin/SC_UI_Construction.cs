using static SC_Global;

public class SC_UI_Construction : SC_UI_Creation {

    public bool qinConstru;

    protected override bool Condition () {

        return CanCreateConstruct(name);

    }

    protected override void Listener () {

        if (qinConstru)
            SC_UI_Manager.Instance.DisplayConstructableTiles(name);
        else
            SC_Game_Manager.Instance.SoldierConstruct(name);

    }

    protected override void TooltipUpdate () {

        SC_Construction constru = SC_Game_Manager.Instance.TryLoadConstruction(name).GetComponent<SC_Construction>();

        SC_UI_Manager.Instance.UpdateCreationTooltip(
            
            new string[3] { constru.Name, constru.cost.ToString(), constru.description},
            true
            
        );

    }

}
