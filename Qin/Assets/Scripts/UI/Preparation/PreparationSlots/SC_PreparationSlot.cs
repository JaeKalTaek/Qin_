using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static SC_Global;
using System.Collections.Generic;

public abstract class SC_PreparationSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public abstract int ElementType { get; }

    public Sprite DefaultSprite { get; set; }

    public Image Renderer { get; set; }

    public bool IsDefault { get { return Renderer.sprite == DefaultSprite; } }

    void Awake () {

        Renderer = GetComponent<Image> ();

        DefaultSprite = Renderer.sprite;

    }

    void IPointerClickHandler.OnPointerClick (PointerEventData eventData) {

        if ((!IsDefault) && eventData.button == PointerEventData.InputButton.Right && SC_UI_Manager.Instance.PreparationPhase == ElementType && !IsPrepCastle (ElementType)) {

            SC_PreparationElement.GiveBackElement (ElementType, Renderer.sprite.name);

            Renderer.sprite = DefaultSprite;

            if (SC_Player.localPlayer.Qin)
                SC_UI_Manager.Instance.QinPreparationSlotsCount--;
            else
                SC_UI_Manager.Instance.HeroesPreparationSlotsCount--;

            if (ElementType == (int) EHeroPreparationElement.Weapon && GetType () == typeof (SC_HeroPreparationSlot)) {

                List<Sprite> sprites = new List<Sprite> ();

                foreach (SC_PreparationSlot w in GetComponentInParent<SC_HeroDeck> ().Weapons) {

                    if (!w.IsDefault) {

                        sprites.Add (w.Renderer.sprite);

                        w.Renderer.sprite = w.DefaultSprite;

                    }

                }

                for (int i = 0; i < sprites.Count; i++)
                    GetComponentInParent<SC_HeroDeck> ().Weapons[i].Renderer.sprite = sprites[i];

            }

        }

    }

    bool hovering;

    void IPointerEnterHandler.OnPointerEnter (PointerEventData eventData) {

        hovering = true;        

    }

    void LateUpdate () {

        if (hovering)
            SC_UI_Manager.Instance.ShowTooltip (!IsDefault, Renderer.sprite.name);

    }

    void IPointerExitHandler.OnPointerExit (PointerEventData eventData) {

        hovering = false;

        SC_UI_Manager.Instance.ShowTooltip (false);

    }

}
