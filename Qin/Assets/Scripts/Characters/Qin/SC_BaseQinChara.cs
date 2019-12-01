using System.Collections.Generic;
using UnityEngine;

public class SC_BaseQinChara : SC_Character {

    [Header("Base Qin Character Variables")]
    [Tooltip("Description for that character when you want to create it")]
    public string description;

    [Tooltip("Cost to create this character")]
    public int cost;

    public override void OnStartClient () {

        base.OnStartClient ();

        weapons = new List<SC_Weapon> (loadedCharacter.weapons);

    }

    public override void TrySelecting () {

        if (CanBeSelected) {

            if(gameManager.QinTurnStarting)
                SC_Player.localPlayer.CmdSetQinTurnStarting(false);

            base.TrySelecting();

        }

    }

    /*public override bool Hit (int damages, bool saving) {

        base.Hit(damages, saving);

        if (Health <= 0)
            DestroyCharacter();
        else
            UpdateHealth();

        return (Health <= 0);

    }*/

}
