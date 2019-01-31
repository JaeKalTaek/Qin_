using UnityEngine;

public class SC_BaseQinChara : SC_Character {

    [Header("Base Qin Character Variables")]
    [Tooltip("Weapon of this character")]
    public SC_Weapon weapon;

    [Tooltip("Description for that character when you want to create it")]
    public string description;

    [Tooltip("Cost to create this character")]
    public int cost;

    public override void OnStartClient () {

        base.OnStartClient();

        weapon = loadedCharacter.BaseQinChara.weapon;

    }

    public override void TryCheckMovements () {

        if (CanMove) {

            SC_Player.localPlayer.CmdSetQinTurnStarting(false);

            base.TryCheckMovements();

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

    public override Vector2 GetRange (SC_Tile t = null) {

        return weapon.Range(this, t);

    }

}
