using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SC_Global;
using DG.Tweening;
using System.Collections.Generic;

public abstract class SC_PreparationElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public abstract int ElementType { get; }

    SpriteRenderer draggedElement;

    public Image Renderer { get; set; }

    public Sprite Sprite { get { return Renderer.sprite; } }

    public static Dictionary<int, List<SC_PreparationElement>> preparationElements = null;    

    void Awake () {

        if (preparationElements == null)
            preparationElements = new Dictionary<int, List<SC_PreparationElement>> ();

        if (!preparationElements.ContainsKey (ElementType))
            preparationElements.Add (ElementType, new List<SC_PreparationElement> ());

        GetElementsList (ElementType).Add (this);

        Renderer = GetComponent<Image> ();

    }

    void IBeginDragHandler.OnBeginDrag (PointerEventData eventData) {

        if (Renderer.color.a == 1) {

            draggedElement = new GameObject ("Dragged Element").AddComponent<SpriteRenderer> ();

            draggedElement.transform.position = new Vector3 (WorldMousePos.x, WorldMousePos.y, 0);
           
            draggedElement.sortingOrder = 11;

            draggedElement.sprite = GetComponent<Image> ().sprite;

        }

    }

    void IDragHandler.OnDrag (PointerEventData eventData) {

        if (draggedElement)
            draggedElement.transform.position = new Vector3 (WorldMousePos.x, WorldMousePos.y, 0);

    }

    void IEndDragHandler.OnEndDrag (PointerEventData eventData) {

        if (draggedElement) {

            if (ElementType == (int) EQinPreparationElement.Soldiers && SC_Player.localPlayer.Qin) {

                if ((!SC_Cursor.Tile.infos.heroDeploy) && SC_Cursor.Tile.baseCost < 100 && !SC_Cursor.Tile.Qin)
                    Instantiate (Resources.Load<SC_DeploymentSoldier> ("Prefabs/Characters/Soldiers/P_DeploymentSoldier"), SC_Cursor.Tile.transform.position, Quaternion.identity).SpriteR.sprite = Sprite;

            } else if (IsPrepCastle (ElementType)) {

                SC_Castle c = GetObjectUnderMouse<SC_Castle> ();

                if (c) {

                    if (c.CastleType != "") {

                        GiveBackElement (ElementType, c.CastleType + "Castle");

                        GetPrepCastle (c).Renderer.sprite = draggedElement.sprite;

                    } else
                        SC_UI_Manager.Instance.QinPreparationSlotsCount++;

                    c.SetCastle (name);

                    Renderer.DOFade (.5f, 0);

                    GetPrepCastle (c).Renderer.sprite = draggedElement.sprite;

                }

            } else {

                SC_PreparationSlot slot = null;

                foreach (GameObject g in eventData.hovered)
                    slot = g.GetComponent<SC_PreparationSlot> () ?? slot;

                if ((slot?.ElementType ?? -1) == ElementType) {

                    if (slot.IsDefault) {

                        if (ElementType == (int) EHeroPreparationElement.Weapon && GetType () == typeof (SC_HeroPreparationElement)) {

                            SC_PreparationSlot correctSlot = null;

                            foreach (SC_PreparationSlot w in slot.GetComponentInParent<SC_HeroDeck> ().Weapons)
                                if (!correctSlot && w.IsDefault)
                                    correctSlot = w;

                            correctSlot.Renderer.sprite = draggedElement.sprite;

                        } else
                            slot.Renderer.sprite = draggedElement.sprite;

                        if (SC_Player.localPlayer.Qin)
                            SC_UI_Manager.Instance.QinPreparationSlotsCount++;
                        else
                            SC_UI_Manager.Instance.HeroesPreparationSlotsCount++;

                    } else {

                        GiveBackElement (ElementType, slot.Renderer.sprite.name);

                        slot.Renderer.sprite = draggedElement.sprite;

                    }

                    Renderer.DOFade (.5f, 0);

                }

            }

            Destroy (draggedElement.gameObject);

        }

    }

    static List<SC_PreparationElement> GetElementsList (int type) {

        List<SC_PreparationElement> list;

        preparationElements.TryGetValue (type, out list);

        return list;

    }

    public static void GiveBackElement (int type, string element) {

        foreach (SC_PreparationElement e in GetElementsList (type))
            if (e.Sprite.name == element)
                e.Renderer.DOFade (1, 0);

    }

}
