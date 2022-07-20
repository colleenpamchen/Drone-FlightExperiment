using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VRCamera.Scripts;

public class ActivateTarget : MonoBehaviour
{
  //  public GameObject Drone;
    public GameObject[] Drones;
    private Boolean m_HasBeenSelected;     
    public int CurrentDrone;
    public int NextDrone;
    private SelectionSlider m_SelectionSlider;
    private int[] DroneNumber = new int[100];

    // Activate the next when the previous target is destroyed.
    void Start()
    {     
        Drones = GameObject.FindGameObjectsWithTag("Drone"); 
        foreach (GameObject Drone in Drones)
        {
            Drone.SetActive(false);
        }   
        CurrentDrone = 1;
    }
    private void FixedUpdate()
    {
        m_HasBeenSelected = m_SelectionSlider.hasBeenSelected;

        m_SelectionSlider = GameObject.Find("First").GetComponent<SelectionSlider>();

        for (int i=0; i<Drones.Length; i++) 
        {
            string DroneName = Drones[i].name; 
            int nDroneIDX = Convert.ToInt32(DroneName) ; 
            DroneNumber[i] = nDroneIDX; 
        }
        Array.Sort(DroneNumber);  
      //  Debug.Log("first drone in the array:");
       // Debug.Log(DroneNumber[0]); 
        if (m_HasBeenSelected == true)
        {
            Debug.Log("current Drone: " + CurrentDrone);
            NextDrone = CurrentDrone + 1;
            Debug.Log("Next Drone: " + NextDrone);
            for (int i = 0; i < Drones.Length; i++)
            {
                string DroneName = Drones[i].name;
                int DroneNumber = Convert.ToInt32(DroneName);
                if ( DroneNumber == NextDrone )
                {
                    Drones[i].SetActive(true); //activate NextDrone 
   
                    m_SelectionSlider = Drones[i+1].GetComponent<SelectionSlider>();
                    Debug.Log("did it reset automatically?? ");
                    Debug.Log(m_SelectionSlider.hasBeenSelected);

                  //  m_SelectionSlider.hasBeenSelected = false;
                  //  Debug.Log("manual reset");
                  //  Debug.Log(m_SelectionSlider.hasBeenSelected);

                    // Drones = GameObject.FindGameObjectsWithTag("Drone");
                }
            }
            Drones = GameObject.FindGameObjectsWithTag("Drone");

        }
      

    }

}