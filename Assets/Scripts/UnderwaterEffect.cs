using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UnderwaterEffect : MonoBehaviour
{
    private PlayerController player;
    [SerializeField] private Volume standardPostProcessingVolume;
    [SerializeField] private Volume underwaterPostProcessingVolume;
    //[SerializeField] private VolumeProfile standardPostProcessing;
    //[SerializeField] private VolumeProfile underwaterPostProcessing;
    //[SerializeField] private VolumeProfile poisonWaterPostProcessing;


    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (player.isUnderwater)
        {
            EnableEffects("Underwater");
        }
        else EnableEffects("Standard");
    }

    void EnableEffects(string effectName)
    {
        if (effectName == "Underwater")
        {
            RenderSettings.fog = true;
            //standardPostProcessingVolume.profile = underwaterPostProcessing;
            standardPostProcessingVolume.weight = 0;
            underwaterPostProcessingVolume.weight = 1;
            return;
        }

        if (effectName == "Standard")
        {
            RenderSettings.fog = false;

            standardPostProcessingVolume.weight = 1;
            underwaterPostProcessingVolume.weight = 0;
            //standardPostProcessingVolume.profile = standardPostProcessing;
        }
    }
}
