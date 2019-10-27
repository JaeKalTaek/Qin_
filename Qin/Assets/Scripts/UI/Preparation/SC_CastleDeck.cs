﻿using UnityEngine;

public class SC_CastleDeck : MonoBehaviour {

    public SC_QinPreparationSlot Castle { get; set; }
    public SC_QinPreparationSlot Trap { get; set; }

    void Awake () {

        Castle = transform.GetChild (0).GetComponent<SC_QinPreparationSlot> ();

        Trap = transform.GetChild (1).GetComponent<SC_QinPreparationSlot> ();

    }

}
