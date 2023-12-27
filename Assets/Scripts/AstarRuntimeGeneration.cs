//from: https://www.arongranberg.com/astar/docs/runtimegraphs.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AstarRuntimeGeneration : MonoBehaviour
{
    // Setup a grid graph with some values
    [SerializeField] private int width = 50;
    [SerializeField] private int depth = 50;
    [SerializeField] private float nodeSize = 1;

    private void Start()
    {
        GenerateGraph();
    }
    public void GenerateGraph()
    {
        // This holds all graph data
        AstarData data = AstarPath.active.data;

        // This creates a Grid Graph
        GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;        

        gg.center = new Vector3(10, 0, 0);

        // Updates internal size from the above values
        gg.SetDimensions(width, depth, nodeSize);

        // Scans all graphs
        AstarPath.active.Scan();
    }
}
