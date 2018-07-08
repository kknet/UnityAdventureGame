using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackObstacles : MonoBehaviour
{

    public void Init()
    {
        markTreesAsFull();
    }

    void markTreesAsFull()
    {
        Terrain ter = GetComponent<Terrain>();
        TerrainData data = ter.terrainData;
        TreeInstance[] allTrees = data.treeInstances;
        foreach (TreeInstance tree in allTrees)
        {
            GetComponent<MapPathfind>().containingCell(tree.position + transform.position).setFull(-2);
        }
    }
}