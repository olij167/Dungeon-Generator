//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ChangeSpeedFromWeight : MonoBehaviour
//{
//    PlayerController player;
//    Inventory inventory;

//    public float maxWeight = 50f;
//    void Start()
//    {
//        player = FindObjectOfType<PlayerController>();
//        inventory = FindObjectOfType<Inventory>();


//    }
//    void Update()
//    {
//        SetSpeedFromWeight();
//    }

//    public void SetSpeedFromWeight()
//    {
//        player.affectedSpeed = player.speed * (maxWeight - inventory.inventoryWeight) / maxWeight;

//        Debug.Log("Speed = " + player.speed + " * " + "(" + maxWeight + " - " + inventory.inventoryWeight + " / " + maxWeight + "\n = " + player.affectedSpeed);
//    }
//}
