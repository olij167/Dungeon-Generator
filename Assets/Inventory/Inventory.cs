using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

public class Inventory : MonoBehaviour
{
    [Header("Item Selection")]
    public InventoryItem selectedInventoryItem;
    public GameObject selectedPhysicalItem;
    public int selectedItemSlot = 1;
    [SerializeField] private Transform handPos;
    [SerializeField] private Color selectedColour;
    private Color originalColour;
    private PlayerInteractionRaycast playerInteractionRaycast;

    [Header("UI Elements")]
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TextMeshProUGUI itemTextPrompts;
    [SerializeField] private TextPopUp textPopUp;
    private Transform[] inventorySlots;

    [Header("Inputs")]
    [SerializeField] private KeyCode dropItemInput = KeyCode.Z;
    [SerializeField] private KeyCode throwItemInput = KeyCode.F;
    [SerializeField] private KeyCode consumeItemInput = KeyCode.C;

    [Header("Inventory Contents")]
    public float inventoryWeight;
    public float inventoryValue;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private TextMeshProUGUI valueText;
    public List<InventoryItem> inventory;
    private PlayerController player;

    [Header("Holding Items")]
    public TwoBoneIKConstraint leftArm;
    public TwoBoneIKConstraint rightArm;
    float currentRightWeight;
    //float currentLeftWeight;
    public float throwCooldown;
    public float throwForce;
    public float throwUpwardForce;
    public bool canThrow;

    private void Awake()
    {
        playerInteractionRaycast = FindObjectOfType<PlayerInteractionRaycast>();
        player = FindObjectOfType<PlayerController>();

        inventorySlots = new Transform[inventoryPanel.transform.childCount];

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventory.Add(null);
        }

        for (int i = 0; i < inventoryPanel.transform.childCount; i++)
        {
            inventorySlots[i] = inventoryPanel.transform.GetChild(i);
        }
        originalColour = inventorySlots[0].GetComponent<Image>().color;

        GetInventoryWeight();
        GetInventoryValue();

        rightArm.weight = 0f;
        leftArm.weight = 0f;
    }

    private void Update()
    {
        SelectInventoryItemWithNumbers();
        SelectInventoryItemWithScroll();

        if (inventory[selectedItemSlot] != null && selectedInventoryItem == null)
        {
            selectedInventoryItem = inventory[selectedItemSlot];

            //EndItemInspection();
            HoldItem();
        }

        if (selectedPhysicalItem != null)
        {
            rightArm.weight = 1f;
        }
        else
            rightArm.weight = 0f;

        if (inventoryPanel.activeSelf)
        {
            

            if (selectedInventoryItem != null)
            {
                SetSelectedItemColour();
                HoldItem();
                itemTextPrompts.gameObject.SetActive(true);
                if (selectedInventoryItem.item.canConsume)
                {
                    itemTextPrompts.text = "Drop [" + dropItemInput.ToString() + "]" + "\nConsume [" + consumeItemInput.ToString() + "]";

                    if (Input.GetKeyDown(consumeItemInput))
                    {
                        ConsumeFood();
                    }
                }
                else itemTextPrompts.text = "Drop [" + dropItemInput.ToString() + "]";

                if (Input.GetKeyDown(dropItemInput))
                {

                    DropItem(selectedInventoryItem);

                }

                if (Input.GetKeyDown(throwItemInput) && canThrow)
                {
                    ThrowItem(selectedInventoryItem);
                }
            }
            else
            {
                itemTextPrompts.text = "";
                itemTextPrompts.gameObject.SetActive(false);


                if (selectedPhysicalItem != null)
                {
                    EndItemInspection();
                }
                selectedInventoryItem = null;
                SetSelectedItemColour();

            }
        }
        else
        {
            itemTextPrompts.text = "";
            itemTextPrompts.gameObject.SetActive(false);

            if (selectedPhysicalItem != null)
            {
                EndItemInspection();
            }
            selectedInventoryItem = null;
            SetSelectedItemColour();
        }
    }

    public void AddItemToInventory(Item item, GameObject itemInWorld)
    {
        if (player.weight + item.weight <= player.maxWeight)
        {
            //if item is already in inventory increase num carried (in 'InventoryItem' scriptable object)
            if (item.isStackable)
            {
                List<InventoryItem> slotsWithItem = new List<InventoryItem>();

                for (int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i] != null && inventory[i].item == item)
                    {
                        slotsWithItem.Add(inventory[i]);
                    }
                }

                if (slotsWithItem.Count > 0)
                {
                    for (int i = 0; i < slotsWithItem.Count; i++)
                    {
                        if (slotsWithItem[i].numCarried < item.maxNumCarried)
                        {

                            slotsWithItem[i].numCarried += 1;
                            slotsWithItem[i].stackCountText.text = "[" + slotsWithItem[i].numCarried + "]";

                            if (itemInWorld.GetComponent<ItemInWorld>())
                            {
                                Destroy(itemInWorld);
                                playerInteractionRaycast.selectedObject = null;
                                //playerInteractionRaycast.interactPromptIndicator.SetActive(false);
                            }
                            GetInventoryWeight();
                            GetInventoryValue();
                            break;
                        }
                        else if (i == slotsWithItem.Count - 1)// if the capacity is full check if it can take a new slot
                        {
                            AddItemToNewInventorySlot(item, itemInWorld);
                            break;
                        }
                    }
                }
                else AddItemToNewInventorySlot(item, itemInWorld);

            }
            else // otherwise add a new item to the inventory
            {
                AddItemToNewInventorySlot(item, itemInWorld);
            }
        }
        else
        {
            //display a pop up if the player can't pick up an item
            textPopUp.SetAndDisplayPopUp("Weight Limit Reached");
        }

    }

    int CheckEmptySlots()
    {
        int emptySlots = 0;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].childCount == 0)
            {
                emptySlots++;
            }
        }

        return emptySlots;
    }

    void AddItemToNewInventorySlot(Item item, GameObject itemInWorld)
    {
        if (CheckEmptySlots() > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].childCount == 0) // for the first empty slot
                {
                    if (playerInteractionRaycast.selectedObject.GetComponent<InventoryItem>()) // check whether it has used variables
                    {
                        SpawnUsedItem(playerInteractionRaycast.selectedObject.GetComponent<InventoryItem>(), i); // add a new inventory item with old variables

                        if (inventory[i].physicalItem.GetComponent<RopeItem>())
                        {
                            inventory[i].physicalItem.GetComponent<RopeItem>().inventoryItem = inventory[i];
                        }

                        playerInteractionRaycast.selectedObject = null;

                    }
                    else
                    {
                        SpawnNewItem(item, i);

                        if (itemInWorld.GetComponent<ItemInWorld>())
                        {
                            Destroy(itemInWorld);
                            playerInteractionRaycast.selectedObject = null;
                        }
                    }

                    if (selectedItemSlot == i)
                    {
                        // add exception for rope to maintain the line renderer component
                        selectedInventoryItem = inventory[selectedItemSlot];
                        EndItemInspection();
                        HoldItem();

                    }

                    break;
                }

                //if (inventory[i].physicalItem == itemInWorld && !inventory[i].isInUse)
                //{
                //    Destroy(itemInWorld);
                //    playerInteractionRaycast.selectedObject = null;
                //}
                //else if (inventory[i].isInUse) playerInteractionRaycast.selectedObject = null;
            }
            //if (itemInWorld.GetComponent<ItemInWorld>() && !inventory[])
            //{
            //    Destroy(itemInWorld);
            //    playerInteractionRaycast.selectedObject = null;
            //}

        }
        else
        {
            //display a pop up if the player can't pick up an item
            textPopUp.SetAndDisplayPopUp("Inventory Capacity Reached");
        }

        GetInventoryWeight();
        GetInventoryValue();
    }
   
    void SpawnNewItem(Item item, int itemSlot)
    {
        GameObject newItemUI = Instantiate(inventoryItemPrefab, inventorySlots[itemSlot].transform);
        InventoryItem inventoryItem = newItemUI.GetComponent<InventoryItem>();

        inventoryItem.InitialiseItem(item);

        inventory[itemSlot] = inventoryItem;

    }
    void SpawnUsedItem(InventoryItem usedItem, int itemSlot)
    {
        GameObject newItemUI = Instantiate(inventoryItemPrefab, inventorySlots[itemSlot].transform);
        InventoryItem inventoryItem = newItemUI.GetComponent<InventoryItem>();

        CopyItemVariables(usedItem, inventoryItem);

        inventoryItem.image.sprite = inventoryItem.item.itemIcon;

        inventory[itemSlot] = inventoryItem;

    }

    InventoryItem CopyItemVariables(InventoryItem originalInventoryItem, InventoryItem copyInventoryItem)
    {
        copyInventoryItem.item = originalInventoryItem.item;
        copyInventoryItem.isInUse = originalInventoryItem.isInUse;
        copyInventoryItem.physicalItem = originalInventoryItem.physicalItem;
        //copyInventoryItem.image.sprite = copyInventoryItem.item.itemIcon;
        copyInventoryItem.batteryCharge = originalInventoryItem.batteryCharge;
        copyInventoryItem.numCarried = originalInventoryItem.numCarried;

        Debug.Log("InventoryItem copied");
        return copyInventoryItem;

        //selectedInventoryItem.GetComponentnsform);
    }

    public void DropItem(InventoryItem item)
    {
        if (item.isInUse || item.batteryCharge < item.item.maxBatteryCharge)
        {
            InventoryItem copyInventoryItem = item.physicalItem.AddComponent<InventoryItem>();
            CopyItemVariables(item, copyInventoryItem);
        }

        item.physicalItem.transform.parent = null;
        item.physicalItem.GetComponent<Rigidbody>().useGravity = true;
        item.physicalItem.GetComponent<Rigidbody>().isKinematic = false;
        selectedPhysicalItem.GetComponent<ItemInWorld>().enabled = true;
        selectedPhysicalItem.GetComponent<Collider>().enabled = true;

        RemoveItemFromInventory(item);

        //if (droppedItem.GetComponent<Breakable>())
        //{
        //    droppedItem.GetComponent<Breakable>().BreakObject();
        //}
    }
    
    public void ThrowItem(InventoryItem item)
    {
        canThrow = false;

        item.physicalItem.transform.parent = null;

        if (selectedInventoryItem.isInUse || item.batteryCharge < item.item.maxBatteryCharge)
        {
            InventoryItem copyInventoryItem = item.physicalItem.AddComponent<InventoryItem>();
            CopyItemVariables(item, copyInventoryItem);
        }

        Vector3 force = Camera.main.transform.forward * throwForce + transform.up * throwUpwardForce;

        item.physicalItem.GetComponent<Rigidbody>().useGravity = true;
        item.physicalItem.GetComponent<Rigidbody>().isKinematic = false;

        selectedPhysicalItem.GetComponent<ItemInWorld>().enabled = true;
        selectedPhysicalItem.GetComponent<Collider>().enabled = true;

        item.physicalItem.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);

        RemoveItemFromInventory(item);

        Invoke(nameof(ResetThrow), throwCooldown);
    }

    private void ResetThrow()
    {
        canThrow = true;
    }

    public void RemoveItemFromInventory(InventoryItem item)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == item)
            {
                inventory[i].numCarried--;
                inventory[i].stackCountText.text = "[" + inventory[i].numCarried + "]";

                if (inventory[i].numCarried <= 0f)
                {
                    if (selectedInventoryItem == inventory[i])
                    {
                        selectedInventoryItem = null;
                        SetSelectedItemColour();
                    }

                    Destroy(inventory[i].gameObject);

                    inventory[i] = null;
                }
                else
                    if (i == selectedItemSlot)
                {
                    selectedPhysicalItem = null;
                    inventory[i].physicalItem = null;
                    HoldItem();
                }
            }
        }

        GetInventoryWeight();
        GetInventoryValue();
    }

    public float GetInventoryWeight()
    {
        float weight = 0;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                weight += inventory[i].item.weight * inventory[i].numCarried;
            }
        }
        player.weight = weight;
        weightText.text = weight.ToString("0.00") + ("KG");
        return inventoryWeight = weight;
    }
    
    public float GetInventoryValue()
    {
        float value = 0;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                value += inventory[i].item.itemValue * inventory[i].numCarried;
            }
        }
        //player.weight = value;
        valueText.text = value.ToString("$" + "0.00");
        return inventoryValue = value;
    }

    public bool CheckInventoryForItem(Item desiredItem)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                if (inventory[i].item == desiredItem)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public InventoryItem GetInventoryItem(Item desiredItem)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                if (inventory[i].item == desiredItem)
                {
                    return inventory[i];
                }
            }
        }
        return null;
    }

    public int CheckItemCount(Item desiredItem)
    {
        foreach(InventoryItem item in inventory)
        {
            if (item == desiredItem)
            {
                return item.numCarried;
            }
        }
        return 0;
    }

    public bool CheckIfConsumableItem(Item desiredItem)
    {
        foreach (InventoryItem item in inventory)
        {
            if (item == desiredItem)
            {
                return item.item.canConsume;
            }
        }
        return false;
    }

    private void HoldItem()
    {
        if (selectedInventoryItem != null)
        {
            if (selectedPhysicalItem == null && selectedInventoryItem.physicalItem == null)
            {
                selectedPhysicalItem = Instantiate(selectedInventoryItem.item.prefab, handPos.position, Quaternion.identity);
                selectedPhysicalItem.transform.parent = handPos;

                selectedInventoryItem.physicalItem = selectedPhysicalItem;

                selectedPhysicalItem.GetComponent<ItemInWorld>().enabled = false;
                selectedPhysicalItem.GetComponent<Collider>().enabled = false;

                if (selectedPhysicalItem.GetComponent<Rigidbody>() && selectedPhysicalItem.GetComponent<Rigidbody>().useGravity != false)
                {
                    selectedPhysicalItem.GetComponent<Rigidbody>().useGravity = false;
                }
            }
            else selectedPhysicalItem = selectedInventoryItem.physicalItem;
        }
    }

    private void EndItemInspection() //Add exception for items which still function when not selected (e.g. torch, rope)
    {
        if (selectedPhysicalItem != null && selectedInventoryItem != null && !selectedInventoryItem.isInUse)
        {
            Destroy(selectedPhysicalItem);
        }

        selectedPhysicalItem = null;
    }

    public void ResetHeldItem()
    {
        EndItemInspection();
        HoldItem();
    }

    public void ConsumeFood()
    {
        if (selectedInventoryItem != null && selectedInventoryItem.item.canConsume)
        {
            //Apply food effects
            RemoveItemFromInventory(selectedInventoryItem);
        }
    }

    public void SelectInventoryItemAsButton(TextMeshProUGUI itemText)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (itemText.text.Contains(inventory[i].item.itemName))
            {
                selectedInventoryItem = inventory[i];
                EndItemInspection();
                HoldItem();
            }
        }
    }

    private void SelectInventoryItemWithNumbers()
    {
        if (inventorySlots.Length >= 1)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (inventorySlots.Length >= 1)
                {
                    selectedItemSlot = 0;
                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[0];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length >= 2)
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (inventorySlots.Length >= 2)
                {
                    selectedItemSlot = 1;
                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[1];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length >= 3)
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (inventorySlots.Length >= 3)
                {
                    selectedItemSlot = 2;
                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[2];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length >= 4)
        {
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (inventorySlots.Length >= 4)
                {
                    selectedItemSlot = 3;
                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[3];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length >= 5)
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                if (inventorySlots.Length >= 5)
                {
                    selectedItemSlot = 4;
                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[4];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length >= 6)
        {
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                if (inventorySlots.Length >= 6)
                {
                    selectedItemSlot = 5;
                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[5];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length >= 7)
        {
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                if (inventorySlots.Length >= 7)
                {
                    selectedItemSlot = 6;
                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[6];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length >= 8)
        {
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                if (inventorySlots.Length >= 8)
                {
                    selectedItemSlot = 7;
                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[7];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length >= 9)
        {
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                if (inventorySlots.Length >= 9)
                {
                    selectedItemSlot = 8;

                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[8];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
        if (inventorySlots.Length == 10)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                if (inventorySlots.Length >= 10)
                {
                    selectedItemSlot = 9;

                    if (inventory[selectedItemSlot] != null)
                    {
                        selectedInventoryItem = inventory[9];
                        EndItemInspection();
                        HoldItem();
                    }
                }
            }
        }
    }
    
    private void SelectInventoryItemWithScroll()
    {
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            selectedItemSlot += Mathf.RoundToInt(Input.mouseScrollDelta.y);
            //selectedItemSlot += Mathf.Clamp(Mathf.RoundToInt(Input.mouseScrollDelta.y), 1, inventorySlots.Length);

            if (selectedItemSlot > inventorySlots.Length - 1)
            {
                selectedItemSlot = 0;
            }
            else if (selectedItemSlot < 0)
            {
                selectedItemSlot = inventorySlots.Length - 1;
            }

            if (inventory[selectedItemSlot] != null)
            {
                EndItemInspection();
                selectedInventoryItem = inventory[selectedItemSlot];

                HoldItem();
            }
            else
            {
                EndItemInspection();
                selectedInventoryItem = null;

            }
        }
    }

    private void SetSelectedItemColour()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i == selectedItemSlot)
            {
                inventorySlots[i].GetComponent<Image>().color = selectedColour;
            }
            else inventorySlots[i].GetComponent<Image>().color = originalColour;
        }
    }
}
