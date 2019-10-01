using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_HeroDeck : MonoBehaviour {

    public SC_HeroPreparationSlot Hero { get; set; }
    public List<SC_HeroPreparationSlot> Weapons { get; set; }
    public SC_HeroPreparationSlot Trap { get; set; }

    void Awake () {

        Hero = transform.GetChild (0).GetComponent<SC_HeroPreparationSlot> ();

        Weapons = new List<SC_HeroPreparationSlot> ();

        for (int i = 1; i <= 3; i++)
            Weapons.Add (transform.GetChild (i).GetComponent<SC_HeroPreparationSlot> ());

        Trap = transform.GetChild (4).GetComponent<SC_HeroPreparationSlot> ();

    }

}
