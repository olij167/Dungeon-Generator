using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Pathfinding;

// requires a nav mesh in the scene
public class SpawnObjects : MonoBehaviour
{
    public Transform parent;
    public List<GameObject> prefabs;
    Vector3 spawnPos;
    public int spawnNumPerPrefab, totalSpawnNum;

    public bool randomPrefabs;

    void Awake()
    {
       
    }

    public void SpawnRandomObjects()
    {
        if (!randomPrefabs)
        {
            for (int i = 0; i < prefabs.Count; i++)
            {
                SpawnGameObjects(prefabs[i], spawnNumPerPrefab);
            }

        }
        else
        {
            for (int i = 0; i < totalSpawnNum; i++)
            {
                SpawnGameObjects(prefabs[Random.Range(0, prefabs.Count)], spawnNumPerPrefab);
            }
        }
    }

    public void SpawnGameObjects(GameObject prefab, int numToSpawn)
    {
        for (int i = 0; i < numToSpawn; i++)
        {
            spawnPos = GetRandomPointOnGraph();
            GameObject newObject = Instantiate(prefab, spawnPos, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
            newObject.transform.parent = parent;
        }
    }

    public Vector3 GenerateRandomWayPoint()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        int maxIndices = navMeshData.indices.Length - 3;

        // pick the first indice of a random triangle in the nav mesh
        int firstVertexSelected = UnityEngine.Random.Range(0, maxIndices);
        int secondVertexSelected = UnityEngine.Random.Range(0, maxIndices);

        // spawn on verticies
        Vector3 point = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];

        Vector3 firstVertexPosition = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
        Vector3 secondVertexPosition = navMeshData.vertices[navMeshData.indices[secondVertexSelected]];

        // eliminate points that share a similar X or Z position to stop spawining in square grid line formations
        if ((int)firstVertexPosition.x == (int)secondVertexPosition.x || (int)firstVertexPosition.z == (int)secondVertexPosition.z)
        {
            point = GenerateRandomWayPoint(); // re-roll a position - I'm not happy with this recursion it could be better
        }
        else
        {
            // select a random point on it
            point = Vector3.Lerp(firstVertexPosition, secondVertexPosition, UnityEngine.Random.Range(0.05f, 0.95f));
        }

        return point;
    }

    public Vector3 GetRandomPointOnGraph()
    {
        // Works for ANY graph type, however this is much slower
        var graph = AstarPath.active.data.graphs[0];
        // Add all nodes in the graph to a list
        List<GraphNode> nodes = new List<GraphNode>();
        graph.GetNodes((System.Action<GraphNode>)nodes.Add);
        GraphNode randomNode = nodes[Random.Range(0, nodes.Count)];

        // Use the center of the node as the destination for example
        var destination1 = (Vector3)randomNode.position;
        // Or use a random point on the surface of the node as the destination.
        // This is useful for navmesh-based graphs where the nodes are large.
        return randomNode.RandomPointOnSurface();
    }
}
