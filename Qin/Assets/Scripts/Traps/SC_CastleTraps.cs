using UnityEngine;

public class SC_CastleTraps : MonoBehaviour {

    [Header ("Castle traps variables")]
    [Tooltip ("Retribution damage")]
    public int retributionDamage;

    public void Retribution () {

        SC_Character.activeCharacter?.Hit (retributionDamage);

    }

}
