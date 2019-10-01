using UnityEngine.EventSystems;

public class SC_QinCancelConstruButton : SC_UI_Creation {

    public bool CanCancel { get; set; }

    protected override bool Condition () {

        return CanCancel;

    }

    public void SetCanClick(bool can) {

        CanCancel = can;
        SetCanClick();

    }

    protected override void Listener () {

        SC_Game_Manager.Instance.CancelLastConstruction();

    }

    public override void OnSelect (BaseEventData eventData) { }

    public override void OnDeselect (BaseEventData eventData) {

        Button.OnPointerExit(null);

    }

    protected override void TooltipUpdate () { }

}
