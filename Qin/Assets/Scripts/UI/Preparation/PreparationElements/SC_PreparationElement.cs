using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SC_Global;
using DG.Tweening;
using System.Collections.Generic;

public abstract class SC_PreparationElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public abstract int ElementType { get; }

    SpriteRenderer draggedElement;

    public Image Renderer { get; set; }

    public Sprite Sprite { get { return Renderer.sprite; } }

    public static Dictionary<int, List<SC_PreparationElement>> preparationElements = null;    

    protected SC_UI_Manager UIManager { get { return SC_UI_Manager.Instance; } }

    void Awake () {

        if (preparationElements == null)
            preparationElements = new Dictionary<int, List<SC_PreparationElement>> ();

        if (!preparationElements.ContainsKey (ElementType))
            preparationElements.Add (ElementType, new List<SC_PreparationElement> ());

        GetElementsList (ElementType).Add (this);

        Renderer = GetComponent<Image> ();

    }

    void IBeginDragHandler.OnBeginDrag (PointerEventData eventData) {

        SpawnDraggedElement ();

    }

    void IDragHandler.OnDrag (PointerEventData eventData) {

        if (draggedElement)
            draggedElement.transform.position = new Vector3 (WorldMousePos.x, WorldMousePos.y, 0);

    }

    void SpawnDraggedElement () {

        if (Renderer.color.a == 1 && !draggedElement) {

            draggedElement = new GameObject ("Dragged Element").AddComponent<SpriteRenderer> ();

            draggedElement.transform.position = new Vector3 (WorldMousePos.x, WorldMousePos.y, 0);

            draggedElement.sortingOrder = 11;

            draggedElement.sprite = Sprite;

        }

    }

    void IEndDragHandler.OnEndDrag (PointerEventData eventData) {

        SC_PreparationSlot slot = null;

        foreach (GameObject g in eventData.hovered)
            slot = g.GetComponent<SC_PreparationSlot> () ?? slot;

        TryPlaceElement (slot, GetObjectUnderMouse<SC_Castle> ());

    }

    void TryPlaceElement (SC_PreparationSlot slot, SC_Castle c) {

        if (draggedElement) {

            if (ElementType == (int) EQinPreparationElement.Soldiers && SC_Player.localPlayer.Qin) {

                Instantiate (Resources.Load<SC_DeploymentSoldier> ("Prefabs/Characters/Soldiers/P_DeploymentSoldier"), TileUnderMouse.transform.position, Quaternion.identity).SpriteR.sprite = Sprite;

            } else if (IsPrepCastle (ElementType)) {

                if (c) {

                    if (c.CastleType != "") {

                        GiveBackElement (ElementType, c.CastleType + "Castle");

                        GetPrepCastle (c).Renderer.sprite = draggedElement.sprite;

                    } else
                        UIManager.QinPreparationSlotsCount++;

                    c.SetCastle (name);

                    Renderer.DOFade (.5f, 0);

                    GetPrepCastle (c).Renderer.sprite = draggedElement.sprite;

                }

            } else if ((slot?.ElementType ?? -1) == ElementType) {

                if (slot.IsDefault) {

                    if (ElementType == (int) EHeroPreparationElement.Weapon && GetType () == typeof (SC_HeroPreparationElement)) {

                        SC_PreparationSlot correctSlot = null;

                        foreach (SC_PreparationSlot w in slot.GetComponentInParent<SC_HeroDeck> ().Weapons)
                            if (!correctSlot && w.IsDefault)
                                correctSlot = w;

                        correctSlot.Renderer.sprite = draggedElement.sprite;

                    } else
                        PlaceElement (slot);

                    if (SC_Player.localPlayer.Qin)
                        UIManager.QinPreparationSlotsCount++;
                    else
                        UIManager.HeroesPreparationSlotsCount++;

                } else {

                    GiveBackElement (ElementType, slot.Renderer.sprite.name);

                    PlaceElement (slot);

                }

                Renderer.DOFade (.5f, 0);

            }            

            Destroy (draggedElement.gameObject);

        }

    }

    void PlaceElement (SC_PreparationSlot slot) {

        if (ElementType == (int) EHeroPreparationElement.Hero && GetType () == typeof (SC_HeroPreparationElement)) {

            foreach (SC_Tile t in SC_Tile_Manager.Instance.DeploymentTiles) {

                if (!t.DeployedHero) {

                    Instantiate (Resources.Load<SC_DeploymentHero> ("Prefabs/Characters/Heroes/P_DeploymentHero"), t.transform.position, Quaternion.identity).SpriteR.sprite = draggedElement.sprite;

                    break;

                }

            }
        }

        slot.Renderer.sprite = draggedElement.sprite;

    }

    static List<SC_PreparationElement> GetElementsList (int type) {

        List<SC_PreparationElement> list;

        preparationElements.TryGetValue (type, out list);

        return list;

    }

    public static void GiveBackElement (int type, string element) {

        if (type == (int) EHeroPreparationElement.Hero && !SC_Player.localPlayer.Qin) {

            foreach (SC_Tile t in SC_Tile_Manager.Instance.DeploymentTiles) {

                if (t.DeployedHero?.SpriteR.sprite.name == element) {

                    Destroy (t.DeployedHero.gameObject);

                    t.DeployedHero = null;

                }

            }

        }

        foreach (SC_PreparationElement e in GetElementsList (type))
            if (e.Sprite.name == element)
                e.Renderer.DOFade (1, 0);

    }

    void IPointerEnterHandler.OnPointerEnter (PointerEventData eventData) {

        SC_UI_Manager.Instance.ShowTooltip (true, Sprite.name);

    }

    void IPointerExitHandler.OnPointerExit (PointerEventData eventData) {

        SC_UI_Manager.Instance.ShowTooltip (false);

    }

    float doubleClickTime = .17f;
    float lastClickTime;

    void IPointerClickHandler.OnPointerClick (PointerEventData eventData) {        

        if (Time.time - lastClickTime <= doubleClickTime && Renderer.color.a == 1) {

            SpawnDraggedElement ();

            SC_PreparationSlot slot = null;

            if (SC_Player.localPlayer.Qin) {

                if (ElementType == (int) EQinPreparationElement.Curse && UIManager.qinPreprationUI.curseSlot.IsDefault)
                    slot = UIManager.qinPreprationUI.curseSlot;

                foreach (SC_CastleDeck cd in UIManager.qinPreprationUI.castleDecks)
                    foreach (SC_PreparationSlot s in cd.GetComponentsInChildren<SC_PreparationSlot> ())
                        slot = slot ?? (s.ElementType == ElementType && s.IsDefault ? s : null);

            } else if (ElementType == (int) EHeroPreparationElement.Weapon) {

                List<SC_PreparationSlot> weaponSlots = new List<SC_PreparationSlot> ();

                for (int i = 0; i < 3; i++)
                    foreach (SC_HeroDeck hd in UIManager.heroPreparationUI.heroDecks)
                        weaponSlots.Add (hd.Weapons[i]);

                foreach (SC_PreparationSlot s in weaponSlots)
                    slot = slot ?? (s.ElementType == ElementType && s.IsDefault ? s : null);

            } else {

                foreach (SC_HeroDeck hd in UIManager.heroPreparationUI.heroDecks)
                    foreach (SC_PreparationSlot s in hd.GetComponentsInChildren<SC_PreparationSlot> ())
                        slot = slot ?? (s.ElementType == ElementType && s.IsDefault ? s : null);

            }            

            SC_Castle castle = null;

            foreach (SC_Castle c in FindObjectsOfType<SC_Castle> ())
                castle = castle ?? (c.CastleType == "" ? c : null);

            TryPlaceElement (slot, castle); 

        }

        lastClickTime = Time.time;

    }

}
