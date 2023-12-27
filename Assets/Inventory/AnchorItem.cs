using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorItem : ItemAction
{
    public Vector3 anchorPoint;
    public bool isAnchored;
    public PlayerInteractionRaycast interactionRaycast;
    public Inventory inventory;
    public InventoryItem anchorItem;
    public TextPopUp popUp;

    [HideInInspector] public Rigidbody rb;

    private void OnEnable()
    {
        interactionRaycast = FindObjectOfType<PlayerInteractionRaycast>();
        inventory = FindObjectOfType<Inventory>();

        if (inventory.selectedPhysicalItem == gameObject)
        {
            anchorItem = inventory.selectedInventoryItem;
        }

        popUp = FindObjectOfType<TextPopUp>();

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (anchorItem == null && inventory.selectedPhysicalItem == gameObject)
        {
            anchorItem = inventory.inventory[inventory.selectedItemSlot];
        }
    }

    public override void ItemFunction()
    {
        if (interactionRaycast.hitPoint != Vector3.zero)
        {
            //Debug.Log("SelectedItem = " + inventory.selectedInventoryItem);
            anchorItem = inventory.inventory[inventory.selectedItemSlot];

            anchorPoint = interactionRaycast.hitPoint;

            SetAnchor(anchorPoint);
        }
        else popUp.SetAndDisplayPopUp("Cannot Anchor Here");
    }

    public void SetAnchor(Vector3 hitPoint)
    {
        GameObject newAnchor = Instantiate(anchorItem.item.prefab, hitPoint, interactionRaycast.transform.rotation);
        //GameObject newAnchor = inventory.selectedPhysicalItem;
        //newAnchor.transform.parent = null;
        //newAnchor.transform.position = hitPoint;
        //newAnchor.transform.rotation = interactionRaycast.transform.rotation;
        //newAnchor.GetComponent<Collider>().enabled = true;

        AnchorItem newAnchorItem = newAnchor.GetComponent<AnchorItem>();
        newAnchorItem.anchorPoint = hitPoint;
        newAnchorItem.isAnchored = true;
        newAnchorItem.rb.useGravity = false;
        newAnchorItem.rb.isKinematic = true;

        Destroy(inventory.selectedPhysicalItem);
        inventory.RemoveItemFromInventory(inventory.selectedInventoryItem);

    }
}
