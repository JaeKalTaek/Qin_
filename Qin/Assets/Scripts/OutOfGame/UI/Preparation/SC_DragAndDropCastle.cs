using UnityEngine;
using UnityEngine.EventSystems;
using static SC_Global;

public class SC_DragAndDropCastle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    GameObject draggedCastle;

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

        SC_Tile_Manager.Instance.GetTileAt (WorldMousePos)?.Castle?.SetCastle (draggedCastle.name);

        Destroy (draggedCastle);

    }

}
