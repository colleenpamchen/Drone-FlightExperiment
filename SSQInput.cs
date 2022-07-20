using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using VRCamera.Scripts;
using Assets.LSL4Unity.Scripts;

public class SSQInput : MonoBehaviour
{

    [SerializeField] private VRInteractiveItem m_VRInteractiveItem;
    [SerializeField] private SelectionSlider m_SelectionSlider;
    [SerializeField] private SelectionRadial m_SelectionRadial;

    private TimeSeriesOutlet m_TimeSeriesOutlet;
    private GameObject m_ExperimentController;

    public int SSQValue;
    private int m_inputNumber;

    void Start()
    {
        m_ExperimentController = GameObject.Find("ExperimentController");
        m_TimeSeriesOutlet = GameObject.Find("ServerManager").GetComponent<TimeSeriesOutlet>();
    }

    void OnEnable()
    {
        m_SelectionSlider.OnBarFilled += SickHUDSelected;
        m_SelectionSlider.OnBarFilled += ssqMarkerOn;
        m_SelectionSlider.OnBarFilled -= ssqMarkerOff;
   //     Debug.Log("ssqMarkerOn");
    }

    void OnDisable()
    {
        m_SelectionSlider.OnBarFilled -= SickHUDSelected;
        m_SelectionSlider.OnBarFilled += ssqMarkerOff;
        m_SelectionSlider.OnBarFilled -= ssqMarkerOn;
        Debug.Log("ssqMarkerOff");
    }

    void Update()
    {
        m_inputNumber = m_ExperimentController.GetComponent<SicknessTracker>().inputNumber;

        if (m_SelectionSlider.m_GazeOver)
        {
            this.GetComponent<Image>().color = Color.gray;
        }

        else if(!m_SelectionSlider.m_GazeOver)
        {
            this.GetComponent<Image>().color = Color.white;
        }
    }

    void SickHUDSelected()
    {
        // Hide the selection radial
        m_SelectionRadial.Hide();

        // Record the subject's SSQ response
        m_ExperimentController.GetComponent<SicknessTracker>().sicknessScores[m_inputNumber] = SSQValue;

        // Reset the call boolean
        m_ExperimentController.GetComponent<SicknessTracker>().HUDCallComplete = false;

        // Reset the timer
        m_ExperimentController.GetComponent<SicknessTracker>().time = 0; // this is reseting the time to 0 and this is linked to SickessTracker 
        m_ExperimentController.GetComponent<SicknessTracker>().time2 = 0;

        // Disable showing the Sickness HUD
        //m_ExperimentController.GetComponent<SicknessTracker>().ShowSickHUD(false);
        Destroy(GameObject.FindWithTag("Clone"));


    }

    void ssqMarkerOn()
    {
        // Marker for when the user actually finished entering in their sickness rating
        m_TimeSeriesOutlet.ssqMarker = 1;
    }

    void ssqMarkerOff()
    {
        // Marker for when the user actually finished entering in their sickness rating
        m_TimeSeriesOutlet.ssqMarker = 0;
    }
}

