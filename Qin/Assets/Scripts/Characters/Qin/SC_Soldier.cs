using TMPro;
using UnityEngine;

public class SC_Soldier : SC_BaseQinChara {    

    [Header("Soldiers Variables")]
    [Tooltip("Energy gained by Qin when he sacrifices this unit")]
    public int sacrificeValue;    

    GameObject sacrificeValueText;

    public bool Builder { get { return characterName == "Builder"; } }

    public override void OnStartClient () {

        base.OnStartClient();

        cost = loadedCharacter.Soldier.cost;

        sacrificeValue = loadedCharacter.Soldier.sacrificeValue;

    }

    protected override void Start () {

        base.Start();

        sacrificeValueText = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Components/SacrificeValueText"), transform);

        sacrificeValueText.GetComponent<TextMeshPro>().text = sacrificeValue.ToString();

        transform.parent = uiManager.soldiersT;

    }

    public void SetupNew() {

        CanMove = false;

        Tire();

        SC_Qin.ChangeEnergy(-cost);

    }    

    public void ToggleDisplaySacrificeValue() {

        sacrificeValueText.SetActive(!sacrificeValueText.activeSelf);

    }

    public override void DestroyCharacter() {

        base.DestroyCharacter();

        if(isServer)
		    SC_Player.localPlayer.CmdDestroyGameObject (gameObject);

    }

}
