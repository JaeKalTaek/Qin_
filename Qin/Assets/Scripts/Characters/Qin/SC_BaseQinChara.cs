using UnityEngine;

public class SC_BaseQinChara : SC_Character {

    [Header("Base Qin Character Variables")]
    [Tooltip("Weapon of this character")]
    public SC_Weapon weapon;

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

    public override bool Hit (int damages, bool saving) {

        base.Hit(damages, saving);

        if (Health <= 0)
            DestroyCharacter();
        else
            UpdateHealth();

        return (Health <= 0);

    }

    public override Vector2 GetRange (SC_Tile t = null) {

        return weapon.Range(this, t);

    }

}
