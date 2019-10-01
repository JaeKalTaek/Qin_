using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static SC_Global;
using System.Collections.Generic;

public class SC_HeroPreparationSlot : MonoBehaviour, IPointerClickHandler {

    [Header ("Preparation slot variables")]
    [Tooltip ("Type of this prepration slot")]
    public EPreparationElement elementType;

    public Sprite DefaultSprite { get; set; }

    public Sprite Sprite {

        get { return Renderer.sprite; }

        set { Renderer.sprite = value; }

    }

    public Image Renderer { get; set; }

    void Awake () {

        Renderer = GetComponent<Image> ();

        DefaultSprite = Renderer.sprite;

    }

    void IPointerClickHandler.OnPointerClick (PointerEventData eventData) {

        if (Sprite != DefaultSprite && eventData.button == PointerEventData.InputButton.Right && SC_UI_Manager.Instance.preparationPhase == elementType) {

            SC_HeroPreparationElement.GiveBackElement (elementType, Sprite);

            Sprite = DefaultSprite;            

            SC_UI_Manager.Instance.PreparationSlotsCount--;

            if (elementType == EPreparationElement.Weapon) {

                List<Sprite> sprites = new List<Sprite> ();

                foreach (SC_HeroPreparationSlot w in GetComponentInParent<SC_HeroDeck> ().Weapons) {

                    if (w.Sprite != w.DefaultSprite) {

                        sprites.Add (w.Sprite);

                        w.Sprite = w.DefaultSprite;

                    }

                }

                for (int i = 0; i < sprites.Count; i++)
                    GetComponentInParent<SC_HeroDeck> ().Weapons[i].Sprite = sprites[i];

            }

        }

    }

}
