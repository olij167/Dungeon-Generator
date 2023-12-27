using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public enum GeneratorState {  inactive, main, branches, cleanup, done }
public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] startPrefabs;
    [SerializeField] private GameObject[] tilePrefabs;
    [SerializeField] private GameObject[] exitPrefabs;
    [SerializeField] private GameObject[] blockedPrefabs;
    [SerializeField] private GameObject[] doorPrefabs;


    [Header("Debugging Options")]
    [SerializeField] private bool useBoxColliders;
    [SerializeField] private bool useLightsForDebugging;
    [SerializeField] private bool restoreLightsAfterDebugging;

    [Header("Key Bindings")]
    [SerializeField] private KeyCode reloadKey = KeyCode.Backspace;
    //[SerializeField] private KeyCode mapKey = KeyCode.M;

    [Header("Generation Limits")]
    [SerializeField] public Vector3 startPosition;
    [Range(2, 100)] [SerializeField] private int mainLength = 10;
    [Range(0, 50)] [SerializeField] private int branchLength = 5;
    [Range(0, 50)] [SerializeField] private int numBranches = 10;
    [Range(0, 100)] [SerializeField] private int doorPercent = 25;

    [Range(0f, 1f)] [SerializeField] private float constructionDelay;


    [Header("Runtime")]
    public List<Tile> generatedTiles = new List<Tile>();

    [HideInInspector] public GeneratorState generatorState = GeneratorState.inactive;
    [HideInInspector] public List<GameObject> blockedPassages;

    GameObject goPlayer;
    //GameObject goCamera;
    List<Connector> availableConnectors = new List<Connector>();
    Color startLightColour = Color.white;
    Transform tileFrom, tileTo, tileRoot;
    Transform container;
    int attempts;
    int maxAttempts = 50;

    
    private void Start()
    {
        //goCamera = GameObject.Find("Overhead Camera");
        goPlayer = GameObject.FindWithTag("Player");



        StartCoroutine(LevelBuild());
    }

    private void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            SceneManager.LoadScene("Prototype");
        }

        //if (Input.GetKeyDown(mapKey))
        //{
        //    goCamera.SetActive(!goCamera.activeInHierarchy);
        //    goPlayer.SetActive(!goPlayer.activeInHierarchy);
        //}
    }

    IEnumerator LevelBuild()
    {
        //goCamera.SetActive(true);
        //goPlayer.SetActive(false);

        GameObject goContainer = new GameObject("Main Path");
        container = goContainer.transform;
        container.SetParent(transform);
        tileRoot = CreateStartTile();
        DebugRoomLighting(tileRoot, Color.magenta);

        tileTo = tileRoot;

        generatorState = GeneratorState.main;

        while (generatedTiles.Count < mainLength)
        {
            yield return new WaitForSeconds(constructionDelay);

            tileFrom = tileTo;
            if (generatedTiles.Count == mainLength - 1)
            {
                tileTo = CreateExitTile();
                DebugRoomLighting(tileTo, Color.magenta);
            }
            else
            {
                tileTo = CreateTile();
                DebugRoomLighting(tileTo, Color.yellow);
            }

            ConnectTiles();

            CollisionCheck();
        }

        // get all unconnected connectors within the main branch
        foreach (Connector connector in container.GetComponentsInChildren<Connector>())
        {
            if (!connector.isConnected)
            {
                if (!availableConnectors.Contains(connector))
                {
                    availableConnectors.Add(connector);
                }
            }
        }

        //branching
        generatorState = GeneratorState.branches;

        for (int b = 0; b < numBranches; b++)
        {
            if (availableConnectors.Count > 0)
            {
                goContainer = new GameObject("Branch " + (b + 1));
                container = goContainer.transform;
                container.SetParent(transform);
                int availIndex = Random.Range(0, availableConnectors.Count);
                tileRoot = availableConnectors[availIndex].transform.parent.parent;

                availableConnectors.RemoveAt(availIndex);
                tileTo = tileRoot;

                for (int i = 0; i < branchLength - 1; i++)
                {
                    yield return new WaitForSeconds(constructionDelay);

                    tileFrom = tileTo;
                    tileTo = CreateTile();

                    DebugRoomLighting(tileTo, Color.cyan);

                    ConnectTiles();
                    CollisionCheck();
                    if (attempts >= maxAttempts) break;

                }
            }
            else break;
        }

        generatorState = GeneratorState.cleanup;

        LightRestoration();
        CleanupBoxes();

        BlockedPassages();

        SpawnDoors();

        DoorMaster.instance.SpawnInteriorEnteranceDoors();

        generatorState = GeneratorState.done;

        //Recalculate all graphs
        AstarPath.active.Scan();

        FindObjectOfType<SpawnObjects>().SpawnRandomObjects();

        yield return null;

        //goCamera.SetActive(false);
        //goPlayer.SetActive(true);
    }

    void SpawnDoors()
    {
        if (doorPercent > 0)
        {
            Connector[] allConnectors = transform.GetComponentsInChildren<Connector>();
            for (int i = 0; i < allConnectors.Length; i++)
            {
                Connector myConnector = allConnectors[i];
                if (myConnector.isConnected)
                {
                    //random chance of spawning a door here
                    int roll = Random.Range(1, 101);

                    if (roll <= doorPercent)
                    {
                        Vector3 halfExtents = new Vector3(myConnector.size.x, 1f, myConnector.size.x);
                        Vector3 pos = myConnector.transform.position;
                        Vector3 offset = Vector3.up * 0.5f;
                        Collider[] hits = Physics.OverlapBox(pos + offset, halfExtents, Quaternion.identity, LayerMask.GetMask("Door"));

                        if (hits.Length == 0)
                        {
                            int doorIndex = Random.Range(0, doorPrefabs.Length);

                            GameObject goDoor = Instantiate(doorPrefabs[doorIndex], pos, myConnector.transform.rotation, myConnector.transform);
                            goDoor.name = doorPrefabs[doorIndex].name;
                        }
                    }
                }
            }
        }
    }
    void BlockedPassages()
    {
        foreach (Connector connector in transform.GetComponentsInChildren<Connector>())
        {
            if (!connector.isConnected)
            {
                Vector3 pos = connector.transform.position;
                int wallIndex = Random.Range(0, blockedPrefabs.Length);
                GameObject goWall = Instantiate(blockedPrefabs[wallIndex], pos, connector.transform.rotation, connector.transform);
                goWall.name = blockedPrefabs[wallIndex].name;
                blockedPassages.Add(goWall);
            }
        }

        
    }

    void CollisionCheck()
    {
        BoxCollider box = tileTo.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = tileTo.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }
        Vector3 offset = (tileTo.right * box.center.x) + (tileTo.up * box.center.y) + (tileTo.forward * box.center.z);
        Vector3 halfExtents = box.bounds.extents;

        List<Collider> hits = Physics.OverlapBox(tileTo.position + offset, halfExtents, Quaternion.identity, LayerMask.GetMask("Tile")).ToList();

        if (hits.Count > 0)
        {
            if (hits.Exists(x => x.transform != tileFrom && x.transform != tileTo))
            {
                //hit something other than tileFrom or tileTo
                attempts++;
                int toIndex = generatedTiles.FindIndex(x => x.tile == tileTo);
                if (generatedTiles[toIndex].connector != null)
                {
                    generatedTiles[toIndex].connector.isConnected = false;
                }
                generatedTiles.RemoveAt(toIndex);
                DestroyImmediate(tileTo.gameObject);

                //Backtracking
                if (attempts >= maxAttempts)
                {
                    int fromIndex = generatedTiles.FindIndex(x => x.tile == tileFrom);
                    Tile myTileFrom = generatedTiles[fromIndex];

                    if (tileFrom != tileRoot)
                    {
                        if (myTileFrom.connector != null)
                        {
                            myTileFrom.connector.isConnected = false;
                        }
                        availableConnectors.RemoveAll(x => x.transform.parent.parent == tileFrom);
                        generatedTiles.RemoveAt(fromIndex);
                        DestroyImmediate(tileFrom.gameObject);

                        if (myTileFrom.origin != tileRoot)
                        {
                            tileFrom = myTileFrom.origin;
                        }
                        else if (container.name.Contains("Main"))
                        {
                            if (myTileFrom.origin != null)
                            {
                                tileRoot = myTileFrom.origin;
                                tileFrom = tileRoot;
                            }
                        }
                        else if (availableConnectors.Count > 0)
                        {
                            int availIndex = Random.Range(0, availableConnectors.Count);
                            tileRoot = availableConnectors[availIndex].transform.parent.parent;
                            availableConnectors.RemoveAt(availIndex);
                            tileFrom = tileRoot;
                        }
                        else return;
                    }
                    else if (container.name.Contains("Main"))
                    {
                        if (myTileFrom.origin != null)
                        {
                            tileRoot = myTileFrom.origin;
                            tileFrom = tileRoot;
                        }
                    }
                    else if (availableConnectors.Count > 0)
                    {
                        int availIndex = Random.Range(0, availableConnectors.Count);
                        tileRoot = availableConnectors[availIndex].transform.parent.parent;
                        availableConnectors.RemoveAt(availIndex);
                        tileFrom = tileRoot;
                    }
                    else return;
                }

                //Retry
                if (tileFrom != null)
                {
                    if (generatedTiles.Count == mainLength - 1)
                    {
                        tileTo = CreateExitTile();
                        DebugRoomLighting(tileTo, Color.magenta);
                    }
                    else
                    {
                        tileTo = CreateTile();
                        Color retryColour = container.name.Contains("Branch") ? Color.blue : Color.red;
                        DebugRoomLighting(tileTo, retryColour * 2f);
                    }



                    ConnectTiles();
                    CollisionCheck();
                }
            }
            else attempts = 0; // nothing else was hit, restoring attempts to 0

        }
    }


    void LightRestoration()
    {
        if (useLightsForDebugging && restoreLightsAfterDebugging && Application.isPlaying)
        {
            Light[] lights = transform.GetComponentsInChildren<Light>();

            foreach(Light light in lights)
            {
                light.color = startLightColour;
            }
        }
    }

    void DebugRoomLighting(Transform tile, Color lightColour)
    {
        if (useLightsForDebugging && Application.isEditor)
        {
            Light[] lights = tile.GetComponentsInChildren<Light>();

            if (lights.Length > 0)
            {
                if (startLightColour == Color.white)
                {
                    startLightColour = lights[0].color;
                }

                foreach (Light light in lights)
                {
                    light.color = lightColour;
                }
            }
        }
    }

    void CleanupBoxes()
    {
        if (!useBoxColliders)
        {
            foreach(Tile myTile in generatedTiles)
            {
                BoxCollider box = myTile.tile.GetComponent<BoxCollider>();

                if (box != null) Destroy(box);
            }
        }


    }

    void ConnectTiles()
    {
        Transform connectFrom = GetRandomConnector(tileFrom);
        if (connectFrom == null) return;

        Transform connectTo = GetRandomConnector(tileTo);
        if (connectTo == null) return;

        connectTo.SetParent(connectFrom);
        tileTo.SetParent(connectTo);

        connectTo.localPosition = Vector3.zero;
        connectTo.localRotation = Quaternion.identity;
        connectTo.Rotate(0f, 180f, 0f);

        tileTo.SetParent(container);
        connectTo.SetParent(tileTo.Find("Connectors"));

        generatedTiles.Last().connector = connectFrom.GetComponent<Connector>();
    }

    private Transform GetRandomConnector(Transform tile)
    {
        if (tile == null) return null;

        List<Connector> connectorList = tile.GetComponentsInChildren<Connector>().ToList().FindAll(x => x.isConnected == false);
        if (connectorList.Count > 0)
        {
            int connectorIndex = Random.Range(0, connectorList.Count);
            connectorList[connectorIndex].isConnected = true;

            if (tile == tileFrom)
            {
                BoxCollider box = tile.GetComponent<BoxCollider>();

                if (box == null)
                {
                    box = tile.gameObject.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                }
            }

            return connectorList[connectorIndex].transform;
        }

        return null;
    }

    Transform CreateTile()
    {
        int index = Random.Range(0, tilePrefabs.Length);
        GameObject tile = Instantiate(tilePrefabs[index], Vector3.zero, Quaternion.identity, container);

        tile.name = tilePrefabs[index].name;

        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(tile.transform, origin));

        return tile.transform;
    }
     Transform CreateExitTile()
    {
        int index = Random.Range(0, exitPrefabs.Length);
        GameObject exitTile = Instantiate(exitPrefabs[index], Vector3.zero, Quaternion.identity, container);

        exitTile.name = "Exit Room";

        Transform origin = generatedTiles[generatedTiles.FindIndex(x => x.tile == tileFrom)].tile;
        generatedTiles.Add(new Tile(exitTile.transform, origin));

        return exitTile.transform;
    }

    Transform CreateStartTile()
    {
        int index = Random.Range(0, startPrefabs.Length);
        GameObject startTile = Instantiate(startPrefabs[index], startPosition, Quaternion.identity, container);

        startTile.name = "Start Room";

        float yRot = Random.Range(0, 4) * 90f;
        startTile.transform.Rotate(0, yRot, 0);

        Transform target = startTile.GetComponentInChildren<Connector>().transform;
        Vector3 targetPostition = new Vector3(target.position.x, transform.position.y, target.position.z);

        goPlayer.transform.LookAt(-targetPostition);

        generatedTiles.Add(new Tile(startTile.transform, null));

        return startTile.transform;
    }
}
