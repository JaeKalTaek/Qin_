using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SC_Global;
using DG.Tweening;
using System.Collections.Generic;

public class SC_HeroPreparationElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [Header ("Preparation element variables")]
    [Tooltip ("Type of this prepration element")]
    public EPreparationElement elementType;

    SpriteRenderer draggedElement;

    public Image Renderer { get; set; }

    public Sprite Sprite { get { return Renderer.sprite; } }

    public static Dictionary<EPreparationElement, List<SC_HeroPreparationElement>> preparationElements = null;    

    void Awake () {

        if (preparationElements == null)
            preparationElements = new Dictionary<EPreparationElement, List<SC_HeroPreparationElement>> ();

        if (!preparationElements.ContainsKey (elementType))
            preparationElements.Add (elementType, new List<SC_HeroPreparationElement> ());

        GetElementsList (elementType).Add (this);

    }

    void Start () {

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

            SC_HeroPreparationSlot slot = null;

            foreach (GameObject g in eventData.hovered)
                slot = g.GetComponent<SC_HeroPreparationSlot> () ?? slot;

            if (slot?.elementType == elementType) {

                if (slot.Sprite == slot.DefaultSprite) {

                    if (elementType == EPreparationElement.Weapon) {

                        SC_HeroPreparationSlot correctSlot = null;

                        foreach (SC_HeroPreparationSlot w in slot.GetComponentInParent<SC_HeroDeck> ().Weapons)
                            if (!correctSlot && w.Sprite == w.DefaultSprite)
                                correctSlot = w;

                        correctSlot.Sprite = draggedElement.sprite;

                    } else
                        slot.Sprite = draggedElement.sprite;

                    SC_UI_Manager.Instance.PreparationSlotsCount++;

                } else {

                    GiveBackElement (elementType, slot.Sprite);

                    slot.Sprite = draggedElement.sprite;

                }

                Renderer.DOFade (.5f, 0);

            }

            Destroy (draggedElement.gameObject);

        }

    }

    static List<SC_HeroPreparationElement> GetElementsList (EPreparationElement type) {

        List<SC_HeroPreparationElement> list;

        preparationElements.TryGetValue (type, out list);

        return list;

    }

    public static void GiveBackElement (EPreparationElement type, Sprite element) {

        foreach (SC_HeroPreparationElement e in GetElementsList (type))
            if (e.Sprite == element)
                e.Renderer.DOFade (1, 0);

    }

}
