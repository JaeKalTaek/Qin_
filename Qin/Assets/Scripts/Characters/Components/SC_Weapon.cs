using UnityEngine;
//using static SC_Global;

public class SC_Weapon : MonoBehaviour {

    [Header("Weapon Variables")]
    [Tooltip("Name of this weapon")]
    public string weaponName;

    [Tooltip("Does this weapon inflict physical or chi damages")]
    public bool physical;    

    [Tooltip("Minimum range of this weapon")]
    public int minRange;    

    [Tooltip("Maxmimum range of this weapon")]
    public int maxRange;    

    public Vector2 Range (SC_Character owner, SC_Tile t = null) {

        if (!t)
            t = owner.Tile;

        return new Vector2(minRange, MaxRange(owner, t));

    }

    public int MaxRange(SC_Character owner, SC_Tile t) {

        return (maxRange == 1) ? maxRange : Mathf.Max(minRange, maxRange + owner.Range(t));

    }

    //public ShiFuMi value;

    //public bool IsBow { get { return value == ShiFuMi.Special; } }

    /*public float ShiFuMiModifier(SC_Weapon opponent) {

		if ((weaponOrQi == opponent.weaponOrQi) && (value != ShiFuMi.Special) && (opponent.value != ShiFuMi.Special) && (value != opponent.value))
            return ((value == ShiFuMi.Rock && opponent.value == ShiFuMi.Scissors) ||
                (value == ShiFuMi.Paper && opponent.value == ShiFuMi.Rock) ||
                (value == ShiFuMi.Scissors && opponent.value == ShiFuMi.Paper)) ?
                SC_Game_Manager.Instance.CommonCharactersVariables.shiFuMiAvantage : SC_Game_Manager.Instance.CommonCharactersVariables.shiFuMiDisavantage;
        else
            return 1;

    }*/

}
