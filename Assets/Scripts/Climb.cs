using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Toolbelt_OJ;

public class Climb : MonoBehaviour
{
    public List<AudioClip> climbingSounds;
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().moveDirection.y = Input.GetAxisRaw("Vertical") * (other.GetComponent<PlayerController>().speed / 2);

            //if (audioSource.isPlaying)
            //{
            //    audioSource.Stop();
            //}
            if (other.GetComponent<CharacterController>().velocity.magnitude > 2f && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(climbingSounds[Random.Range(0, climbingSounds.Count)]);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioSource.Pause();
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        other.GetComponent<PlayerController>().moveDirection.y = Input.GetAxisRaw("Vertical");
    //    }
    //}
}
