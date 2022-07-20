using UnityEngine;
using System;
using System.Collections;
using VRCamera.Scripts;
//using Assets.LSL4Unity.Scripts;

public class EnableNextDrone : MonoBehaviour
{

    private Boolean m_HasBeenSelected;
    public int currentDroneTarget = 0;
    public int maxTargets = 80;
    public GameObject target; 

    void Start()
    {
        // Disable all drones mesh renderers
        GameObject[] Drones = GameObject.FindGameObjectsWithTag("Drone");
        foreach (GameObject Drone in Drones)
        {
            Drone.GetComponent<Renderer>().enabled = false;
        }
    }

    void Update()
    {
        if (currentDroneTarget < maxTargets)
        {
            //Find current target and render it
            target = GameObject.Find("Drone (" + currentDroneTarget + ")");
            target.GetComponent<Renderer>().enabled = true;

            // Check if target has been selected
            m_HasBeenSelected = target.GetComponent<SelectionSlider>().hasBeenSelected;
            if (m_HasBeenSelected == true)
            {
                // Turn off current target renderer
                target.GetComponent<Renderer>().enabled = false;
                // Increase index only until out of total targets
                currentDroneTarget++;
            }
        }
    }
}