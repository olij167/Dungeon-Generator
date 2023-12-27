using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[RequireComponent(typeof(ItemInWorld))]
public abstract class ItemAction : MonoBehaviour
{
    public abstract void ItemFunction();
}
