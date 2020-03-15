using System.Collections.Generic;
using UnityEngine;
using static SC_EditorTile;

public class SC_MapEditorScript : MonoBehaviour {

    [Header("Editor Map Variables")]
    [Tooltip("Width of the map (in number of tiles)")]
    public int SizeMapX;

    [Tooltip("Height of the map (in number of tiles)")]
    public int SizeMapY;

    public bool prepMap;

    public SC_EditorTile[] Tiles { get; set; }

    public void GenerateMap() {              

        for (int x = 0; x < SizeMapX; x++)
            for (int y = 0; y < SizeMapY; y++)
                Instantiate(Resources.Load<GameObject>("Prefabs/Tiles/P_EditorTile"), new Vector3(x, y, 0), Quaternion.identity, transform);

        SetupMap();

    }

    public void SetupMap() {

        regions = new List<SC_EditorTile>[6];

        for (int i = 0; i < regions.Length; i++)
            regions[i] = new List<SC_EditorTile>();

        Tiles = FindObjectsOfType<SC_EditorTile>();

    }

    /*[InitializeOnLoad]
    internal class PrefabExtension {

        static PrefabExtension () {

            PrefabUtility.prefabInstanceUpdated += (GameObject instance) => {

                GameObject prefab = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(instance);

                foreach (Transform t in prefab.transform)
                    print(t.GetComponent<SC_EditorTile>().river + ", " + t.GetComponent<SC_EditorTile>().riverSprite);

            };

        }

    }*/

}