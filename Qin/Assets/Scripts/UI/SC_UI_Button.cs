using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SC_UI_Button : MonoBehaviour, IButtonInterface {

    public Button Button { get { return GetComponent<Button>(); } }

    public virtual void OnDeselect (BaseEventData eventData) {

        Button.OnPointerExit(null);

    }

    public virtual void OnSelect (BaseEventData eventData) { }

    public virtual void OnPointerEnter (PointerEventData eventData) {

        Button.Select();

    }

}

public interface IButtonInterface : ISelectHandler, IDeselectHandler, IPointerEnterHandler { }
