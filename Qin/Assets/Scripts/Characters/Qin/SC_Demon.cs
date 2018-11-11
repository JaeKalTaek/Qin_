using UnityEngine;
using static SC_Global;

public class SC_Demon : SC_BaseQinChara {

    [Header("Demon Variables")]
    [Tooltip("Range of the aura of this demon")]
    public int auraRange;

    [Tooltip("Color of the aura of this demon")]
    public Color auraColor;

    [Tooltip("Modifiers applied by the aura of this demon")]
    public SC_CombatModifiers auraModifiers;

    [Tooltip("Number of turns for this demon to respawn at its castle after being killed")]
    public int respawnTime;

    public int Alive { get; set; }

    SC_Tile spawnTile;

    public int Region { get { return spawnTile.Region; } }

    public static SC_Demon[] demons;

    public override void OnStartClient () {

        base.OnStartClient();

        auraRange = loadedCharacter.Demon.auraRange;

        auraColor = loadedCharacter.Demon.auraColor;

        auraModifiers = loadedCharacter.Demon.auraModifiers;

        respawnTime = loadedCharacter.Demon.respawnTime;

        Alive = -1;

    }

    protected override void Start () {

        base.Start();

        for (int i = -auraRange; i <= auraRange; i++) {

            for (int j = -auraRange; j <= auraRange; j++) {

                if(SC_Tile_Manager.TileDistance(transform.position, transform.position + (new Vector3(i, j, 0) * SC_Game_Manager.TileSize)) <= auraRange) {

                    SpriteRenderer sr = Instantiate(Resources.Load<SpriteRenderer>("Prefabs/Characters/Components/DemonAura"), transform);

                    sr.transform.localPosition = new Vector3(i, j, transform.position.z);

                    sr.color = auraColor;

                }

            }

        }

        AddAura(true);

        spawnTile = Tile;

        demons[Region] = this;

        transform.parent = uiManager.demonsT;

    }

    delegate void Action (SC_Tile tile);

    void PerformAction (Action action, SC_Tile center = null) {

        foreach (SC_Tile tile in tileManager.GetRange(center ? center.transform.position : transform.position, auraRange))
            action(tile);

    }

    public void AddAura(bool refreshInfos) {

        PerformAction((SC_Tile tile) => {

            tile.TryAddAura(characterName, auraModifiers);

            if (refreshInfos && tile.Character)
                uiManager.TryRefreshInfos(tile.Character.gameObject, tile.Character.GetType());

        });

    }

    public void RemoveAura(bool refreshInfos, SC_Tile center = null) {

        PerformAction((SC_Tile tile) => {

            tile.DemonAuras.Remove(new DemonAura(characterName, auraModifiers));

            if (refreshInfos && tile.Character)
                uiManager.TryRefreshInfos(tile.Character.gameObject, tile.Character.GetType());

        }, center);

    }
    
    public void TryRespawn() {

        Alive++;

        if (Alive > respawnTime) {

            SC_Tile respawnTile = CanCharacterSetOn(spawnTile) ? spawnTile : tileManager.GetUnoccupiedNeighbor(this);

            if (respawnTile) {

                Alive = -1;

                Health = maxHealth;

                UpdateHealth();

                Lifebar.lifebar.SetActive(uiManager.LifeBarsOn);

                CriticalAmount = 0;

                DodgeAmount = 0;

                transform.SetPos(respawnTile.transform);

                respawnTile.Character = this;

                LastPos = respawnTile;

                AddAura(true);

                gameObject.SetActive(true);

            }

        }

    }

    public override void DestroyCharacter () {

        RemoveAura(true);

        base.DestroyCharacter();

        Alive = SC_Castle.castles[Region] ? 0 : -1;

        if (isServer && !SC_Castle.castles[Region])
            SC_Player.localPlayer.CmdDestroyGameObject(gameObject);
        else
            gameObject.SetActive(false);

    }

    public override bool CanCharacterGoThrough (SC_Tile t) {

        return base.CanCharacterGoThrough(t) && (t.Region == Region);

    }

    public override bool CanCharacterSetOn (SC_Tile t) {

        return base.CanCharacterSetOn(t) && (t.Region == Region);

    }

}
