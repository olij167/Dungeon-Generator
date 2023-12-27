using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : MonoBehaviour
{
    public Item item;
    public Image image;
    public TextMeshProUGUI stackCountText;
    public int numCarried;

    public bool isInUse;

    public float batteryCharge;

    public GameObject physicalItem;


    //private void Start()
    //{
    //    InitialiseItem(item);
    //}

    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.itemIcon;
        numCarried = 1;
        if (item.isStackable)
        {
            stackCountText.text = "[" + numCarried.ToString() + "]";
        }
        else stackCountText.gameObject.SetActive(false);

        if (item.usesBatteries)
        {
            batteryCharge = item.maxBatteryCharge;
        }
        
    }

    //public void InitialiseUsedItem(InventoryItem usedItem)
    //{
    //    item = usedItem.item;
    //    image.sprite = usedItem.item.itemIcon;
    //    physicalItem = usedItem.physicalItem;

    //    numCarried = 1;
    //    if (item.isStackable)
    //    {
    //        stackCountText.text = "[" + numCarried.ToString() + "]";
    //    }
    //    else stackCountText.gameObject.SetActive(false);

    //    if (item.usesBatteries)
    //    {
    //        batteryCharge = usedItem.batteryCharge;
    //    }

    //    isInUse = true;
    //}
}
