using UnityEngine;
using UnityEngine.EventSystems;
using static SC_Global;
using DG.Tweening;

public class SC_DragAndDropCastle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    GameObject draggedCastle;

    SpriteRenderer spriteRenderer;

    void Awake () {

        spriteRenderer = GetComponent<SpriteRenderer> ();

    }

    void IBeginDragHandler.OnBeginDrag (PointerEventData eventData) {

        draggedCastle = Instantiate (Resources.Load<GameObject> ("Prefabs/UI/P_Drag&DropCastle"), new Vector3 (WorldMousePos.x, WorldMousePos.y, 0), Quaternion.identity);

        draggedCastle.transform.GetChild (0).GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("Sprites/Constructions/Castle/Roofs/" + eventData.rawPointerPress.name);

        draggedCastle.name = eventData.rawPointerPress.name;

    }

    void IDragHandler.OnDrag (PointerEventData eventData) {

        if (draggedCastle)        
            draggedCastle.transform.position = new Vector3 (WorldMousePos.x, WorldMousePos.y, 0);

    }

    void IEndDragHandler.OnEndDrag (PointerEventData eventData) {

        SC_Castle c = GetObjectUnderMouse<SC_Castle> ();

        if (c) {

            if (c.CastleType == "")
                SC_UI_Manager.Instance.QinPreparationSlotsCount++;

            c.SetCastle (draggedCastle.name);       
            
            spriteRenderer.DOFade (.5f, 0);

        }

        Destroy (draggedCastle);

    }

}
