using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radio : MonoBehaviour
{
    public float range;
    public bool isClear;
    public GameObject ui;

    public void Start()
    {
        ui.SetActive(false);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ui.SetActive(true);
            oon();
            Time.timeScale = 0;
        }
        Vector3 player = GameObject.FindGameObjectWithTag("Player").transform.position;
        if (Vector3.Distance(player, transform.position) < range)
        {
            isClear = true;
            ui.SetActive(true);
            oon();
            Time.timeScale = 0;
        }
    }

    private void oon()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        Transform pos = FindObjectOfType<ExitUI>().pos;
        player.GetComponent<CharacterController>().enabled = false;
        player.GetComponent<PlayerMove>().enabled = false;
        player.GetComponent<Gun>().enabled = false;
        player.position = pos.position;
        player.rotation = pos.rotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
