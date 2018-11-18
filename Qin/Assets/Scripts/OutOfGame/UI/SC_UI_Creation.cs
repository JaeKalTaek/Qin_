using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SC_UI_Creation : SC_UI_Button {

    public void SetCanClick () {

        Button.onClick.RemoveAllListeners();

        bool b = Condition();

        if (b)
            Button.onClick.AddListener(Listener);

        ColorBlock cB = Button.colors;
        cB.normalColor = b ? Color.white : cB.disabledColor;
        cB.highlightedColor = b ? Color.white : cB.disabledColor;
        Button.colors = cB;

    }

    public override void OnSelect (BaseEventData eventData) {

        base.OnSelect(eventData);

        TooltipUpdate();

        transform.GetChild(0).gameObject.SetActive(true);

    }

    public override void OnDeselect (BaseEventData eventData) {

        base.OnDeselect(eventData);

        transform.GetChild(0).gameObject.SetActive(false);

    }

    protected abstract bool Condition ();

    protected abstract void Listener ();

    protected abstract void TooltipUpdate ();

}
