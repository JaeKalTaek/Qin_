using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static SC_EditorTile;

[CustomEditor(typeof(SC_MapEditorScript))]
public class SC_MapEditor : Editor {

    bool generated;

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        SC_MapEditorScript map = ((SC_MapEditorScript)target);

        generated = map.transform.childCount > 0;

        if (GameObject.Find(target.name)) {            

            if (!generated && GUILayout.Button("Generate map")) {

                generated = true;

                map.GenerateMap();

            }

            if (generated) {

                if (!currentQinTile)
                    EditorGUILayout.HelpBox("Qin is missing from the map", MessageType.Warning);

                if (heroesOnTiles.Count < 6 && !map.prepMap)
                    EditorGUILayout.HelpBox("Not all heroes are on this map", MessageType.Warning);

                if (heroesDeployTilesCount < 6 && map.prepMap)
                    EditorGUILayout.HelpBox ("Not enough deployment tiles for heroes on this map", MessageType.Warning);

                if (regions != null) {

                    int tilesInRegion = 0;

                    int nbr = 0;

                    foreach (List<SC_EditorTile> region in regions) {

                        if (region.Count < 1)
                            EditorGUILayout.HelpBox(((Region)nbr) + " region doesn't have a tile", MessageType.Warning);

                        nbr++;

                        tilesInRegion += region.Count;

                    }

                    if (tilesInRegion < map.SizeMapX * map.SizeMapY)
                        EditorGUILayout.HelpBox("At least one tile has no region", MessageType.Warning);

                } else {

                    EditorGUILayout.HelpBox("Regions are not setup", MessageType.Warning);

                }

            }

        }        

    }

}