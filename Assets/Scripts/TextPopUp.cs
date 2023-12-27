using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextPopUp : MonoBehaviour
{

    [HideInInspector] public TextMeshProUGUI popUpText;

    [SerializeField] private float popUpDisplayTime = 7.5f;

    private float popUpDisplayTimeReset;

    public bool popUpIndicator;

    //[SerializeField] private AudioSource audioSource;

    private void Start()
    {
        popUpDisplayTimeReset = popUpDisplayTime;
        popUpText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (popUpIndicator)
        {
            DisplayPopUp();
        }
        else
        {
            popUpText.alpha = 0f;
        }
    }

    public void DisplayPopUp()
    {
        
            popUpText.enabled = true;
            popUpDisplayTime -= Time.deltaTime;

            if (popUpDisplayTime >= popUpDisplayTimeReset - 2f)
            {
                popUpText.alpha = Mathf.Lerp(0f, 1f, popUpDisplayTimeReset - popUpDisplayTime);

            }

            if (popUpDisplayTime <= 2f)
            {
                popUpText.alpha = Mathf.Lerp(0f, 1f, popUpDisplayTime);
            }

            if (popUpDisplayTime <= 0f)
            {

                popUpText.enabled = false;
                popUpDisplayTime = popUpDisplayTimeReset;

                popUpIndicator = false;
            }
    }

    public void SetAndDisplayPopUp(string popUpString)
    {
        popUpText.text = popUpString;

        popUpIndicator = true;
    }
}
