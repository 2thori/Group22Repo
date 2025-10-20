using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadNotes : MonoBehaviour
{
    public GameObject player;
    public GameObject noteUI;
    public GameObject hud;
    public GameObject inv;

    public GameObject pickUpText;

    public AudioSource pickUpSound;

    public bool inReach;

    private void Start()
    {
        noteUI.SetActive(false);
        hud.SetActive(true);
        inv.SetActive(true);
        pickUpText.SetActive(false);

        inReach = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if( other.gameObject.tag == "Reach")
        {
            inReach = true;
            pickUpText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Reach")
        {
            inReach = false;
            pickUpText.SetActive(false);

        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact") && inReach)
        {
            noteUI.SetActive(true);
            pickUpSound.Play();
            hud.SetActive(false);
            inv.SetActive(false);
            player.GetComponent<FirstPersonController>().enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ExitButton()
    {
        noteUI.SetActive(false);
        hud.SetActive(true);
        inv.SetActive(true);
        player.GetComponent<FirstPersonController>().enabled = true;
    }
}
