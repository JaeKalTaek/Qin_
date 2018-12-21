using UnityEditor;
using UnityEngine;
using static SC_EditorTile;

[CustomEditor(typeof(SC_EditorTile)), CanEditMultipleObjects]
public class SC_EditorTileEditor : Editor {

    public override void OnInspectorGUI () {

        DrawDefaultInspector();

        if (GameObject.Find(target.name)) {

            Object[] tiles = targets;

            foreach (Object o in tiles) {

                SC_EditorTile tile = o as SC_EditorTile;

                string s = (tile.IsChanging ? "Changing" : (tile.IsRiver ? "River/" + tile.riverSprite : tile.tileType + "/0"));

                Debug.Log("Sprites/Tiles/" + s);

                tile.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Tiles/" + s);

                tile.SetSprite(0, tile.construction == ConstructionType.None ? "" : (tile.construction == ConstructionType.Castle ? ("Sprites/Constructions/Castles/" + tile.castleType) : ("Sprites/Constructions/" + tile.construction)));

                /*if (tile.PrevRegion != tile.region)
                    ChangeTileRegion(tile);*/

                if (tile.Hero != HeroType.None) {

                    SC_EditorTile t = GetHeroTile(tile.Hero);

                    if (t && (t != tile)) {

                        t.Hero = HeroType.None;
                        t.PrevHero = HeroType.None;

                        t.SetSprite(1, "");

                        heroesOnTiles.Remove(new HeroTile(tile.Hero, t));

                    }

                    if (tile.PrevHero == HeroType.None) {

                        tile.soldier = SoldierType.None;
                        tile.PrevSoldier = SoldierType.None;

                        tile.Qin = false;
                        tile.PrevQin = false;

                        heroesOnTiles.Add(new HeroTile(tile.Hero, tile));

                    } else if (tile.PrevHero != tile.Hero) {

                        heroesOnTiles.Remove(new HeroTile(tile.PrevHero, tile));

                        heroesOnTiles.Add(new HeroTile(tile.Hero, tile));

                    }

                    tile.PrevHero = tile.Hero;

                    tile.SetSprite(1, "Sprites/Characters/Heroes/" + tile.Hero);

                } else if (tile.PrevHero != HeroType.None) {

                    heroesOnTiles.Remove(new HeroTile(tile.PrevHero, tile));

                    tile.PrevHero = HeroType.None;

                }

                if (tile.soldier != SoldierType.None) {

                    if (tile.PrevSoldier == SoldierType.None) {

                        if (tile.Hero != HeroType.None)
                            heroesOnTiles.Remove(new HeroTile(tile.Hero, tile));

                        tile.Hero = HeroType.None;
                        tile.PrevHero = HeroType.None;

                        tile.Qin = false;
                        tile.PrevQin = false;

                    }

                    tile.PrevSoldier = tile.soldier;

                    tile.SetSprite(1, "Sprites/Characters/Soldiers/" + tile.soldier);

                }

                if (tile.Qin) {

                    if (currentQinTile && (currentQinTile != tile)) {

                        currentQinTile.Qin = false;
                        currentQinTile.SetSprite(2, "");

                    }

                    if (!tile.PrevQin) {

                        tile.soldier = SoldierType.None;
                        tile.PrevSoldier = SoldierType.None;

                        if (tile.Hero != HeroType.None)
                            heroesOnTiles.Remove(new HeroTile(tile.Hero, tile));

                        tile.Hero = HeroType.None;
                        tile.PrevHero = HeroType.None;

                    }

                    tile.PrevQin = true;

                    tile.SetSprite(1, "Sprites/Characters/Qin");

                    currentQinTile = tile;

                } else if (tile.PrevQin) {

                    tile.PrevQin = false;

                    currentQinTile = null;

                }

                if ((tile.Hero == HeroType.None) && (tile.soldier == SoldierType.None) && !tile.Qin)
                    tile.SetSprite(1, "");

            }

        }

    }

}
