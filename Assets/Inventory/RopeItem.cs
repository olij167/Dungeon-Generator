using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeItem : ItemAction
{
    private LineRenderer lineRenderer;

    [field: ReadOnlyField, SerializeField] private float ropeLength;
    [field: ReadOnlyField, SerializeField] private float maxDistanceFromLastAnchor;

    public float maxLength;
    [SerializeField] private List<Vector3> anchorPoints;
    private Inventory inventory;
    private PlayerInteractionRaycast raycast;
    private MeshCollider ropeCollider;
    private TextPopUp popUp;
    private Transform player;
    private PlayerController playerController;
    private Rigidbody rb;
    [SerializeField] private Vector3 lastValidPos;
    [SerializeField] private Vector3 closestValidPos;
    [SerializeField] private float ropeRetractSpeed = 5f;

    public bool isHeld;
    [HideInInspector] public InventoryItem inventoryItem;

    //[SerializeField] private float maxSwingDistance = 25f;
    //[SerializeField] private Vector3 swingPoint;
    //private SpringJoint joint;


    public LayerMask collMask;

    public float minCollisionDistance = 0.5f;

    [SerializeField] private bool isActive;

    public List<Vector3> ropePositions { get; set; } = new List<Vector3>();

    public List<Vector3> ropePos;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        raycast = FindObjectOfType<PlayerInteractionRaycast>();
        playerController = FindObjectOfType<PlayerController>();
        player = playerController.transform;
        inventory = FindObjectOfType<Inventory>();
        ropeCollider = GetComponent<MeshCollider>();
        popUp = FindObjectOfType<TextPopUp>();

        //rb.isKinematic = true;
        BakeMesh();
        //rb.isKinematic = false;
    }

    public override void ItemFunction()
    {
        if (raycast.hitTransform != null && raycast.hitTransform.GetComponent<AnchorItem>() && raycast.hitTransform.GetComponent<AnchorItem>().isAnchored)
        {
            if (!isActive)
            {
                lineRenderer.useWorldSpace = true;
                inventoryItem = inventory.selectedInventoryItem;
                //ropeCollider.enabled = true;
                //rb.isKinematic = true;
                isActive = true;
            }

            if (!inventoryItem.isInUse)
                inventoryItem.isInUse = true;

            if (anchorPoints.Count <= 0 || (anchorPoints.Count > 0 && anchorPoints[anchorPoints.Count - 1] != raycast.hitTransform.position))
            {
                AddPosToRope(raycast.hitTransform.position);

                anchorPoints.Add(raycast.hitTransform.position);
            } 
            else if (anchorPoints[anchorPoints.Count - 1] == raycast.hitTransform.position)
            {
                if (ropePositions.Count > 2 && anchorPoints.Count > 1)
                {
                    for (int i = ropePositions.Count - 1; i > 0; i--)
                    {
                        if (ropePositions[i] == anchorPoints[anchorPoints.Count - 1])
                        {
                            ropePositions.RemoveAt(i);
                            break;
                        }
                    }
                    
                    anchorPoints.RemoveAt(anchorPoints.Count - 1);
                }
                else
                {
                    //ropePositions.RemoveAt(ropePositions.Count - 1);
                    //anchorPoints.RemoveAt(anchorPoints.Count - 1);
                    isActive = false;
                    inventoryItem.isInUse = false;
                    inventory.ResetHeldItem();
                    //ropeCollider.enabled = false; 
                    //rb.isKinematic = false;
                }
            }
        }
        else
        {
            popUp.SetAndDisplayPopUp("Cannot Tie Rope Here");
        }
    }

    private void Update()
    {
        if (isActive)
        {
            ropeLength = GetRopeLength();

            if (inventory.inventory.Contains(inventoryItem))
            {
                isHeld = true;
                if (ropeLength >= maxLength) // restrict player movement to between rope max length and the last anchor point
                {
                    playerController.lastAnchorPoint = anchorPoints[anchorPoints.Count - 1];

                    playerController.maxRopePos = lastValidPos;

                    playerController.characterControllerMovement = false;
                    player.position = Vector3.MoveTowards(player.position, ropePositions[ropePositions.Count - 2], ropeRetractSpeed * Time.deltaTime);

                    if (!player.GetComponent<CharacterController>().isGrounded && playerController.transform.position.y <= anchorPoints[anchorPoints.Count - 1].y)
                    {
                        playerController.isOnRope = true;

                    }
                }
                playerController.characterControllerMovement = true;

                //CalculateMaxPlayerPosFromRope();

                UpdateRopePositions();
                LastSegmentGoToPlayerPos();
            }
            else
            {
                if (isHeld)
                    AddPosToRope(player.position);

                playerController.isOnRope = false;

                isHeld = false;

                if (ropeLength <= maxLength)
                {
                    UpdateRopePositions();
                    BakeMesh();

                }
            }


            DetectCollisionEnter();
            if (ropePositions.Count > 2) DetectCollisionExits();

            ropePos = ropePositions;
        }
    }

    //void CalculateMaxPlayerPosFromRope()
    //{
    //    maxDistanceFromLastAnchor = maxLength - GetAnchoredRopeLength();
    //}

    private void DetectCollisionEnter()
    {
        RaycastHit hit;
        if (Physics.Linecast(ropePositions[ropePositions.Count - 1], lineRenderer.GetPosition(ropePositions.Count - 2), out hit, collMask))
        {
            if (System.Math.Abs(Vector3.Distance(lineRenderer.GetPosition(ropePositions.Count - 2), hit.point)) > minCollisionDistance)
            {
                if (!anchorPoints.Contains(ropePositions[ropePositions.Count - 1]))
                {
                    ropePositions.RemoveAt(ropePositions.Count - 1);
                }
                    AddPosToRope(hit.point);
                
            }
        }
    }

    private void DetectCollisionExits()
    {
        RaycastHit hit;
        if (!Physics.Linecast(player.position, lineRenderer.GetPosition(ropePositions.Count - 3), out hit, collMask))
        {
            if (!anchorPoints.Contains(ropePositions[ropePositions.Count - 2]))
                ropePositions.RemoveAt(ropePositions.Count - 2);
        }
    }

    private void AddPosToRope(Vector3 _pos)
    {
        ropePositions.Add(_pos);
        ropePositions.Add(player.position); //Always the last pos must be the player
    }

    private void UpdateRopePositions()
    {
        lineRenderer.positionCount = ropePositions.Count;
        lineRenderer.SetPositions(ropePositions.ToArray());
        //BakeMesh();
    }

    private void LastSegmentGoToPlayerPos() => lineRenderer.SetPosition(lineRenderer.positionCount - 1, player.position);
    //private void PlayerLastSegmentGoToPos() => lineRenderer.SetPosition(lineRenderer.positionCount - 1, player.position);

    private float GetRopeLength()
    {
        float currentLength = 0f;
        Vector3[] pointsInLine = new Vector3[lineRenderer.positionCount];

        for (int i = 0; i < pointsInLine.Length; i++)
        {
            if (i != 0f)
                currentLength += (lineRenderer.GetPosition(i) - lineRenderer.GetPosition(i - 1)).sqrMagnitude;
        }

        return currentLength;
    }

    private float GetAnchoredRopeLength()
    {
        float currentLength = 0f;
        Vector3[] pointsInLine = new Vector3[anchorPoints.Count];

        for (int i = 0; i < pointsInLine.Length; i++)
        {
            if (i != 0f)
                currentLength += (anchorPoints[i] - anchorPoints[i - 1]).sqrMagnitude;
        }

        return currentLength;
    }

    private void BakeMesh()
    {
        rb.isKinematic = true;
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        ropeCollider.sharedMesh = mesh;
        rb.isKinematic = false;

    }
}
