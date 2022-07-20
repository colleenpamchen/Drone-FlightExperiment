using UnityEngine;
using System;
using System.Collections;
using VRCamera.Scripts;
using Assets.LSL4Unity.Scripts;

public class DisolveOnSelect : MonoBehaviour
{

    [SerializeField] private VRInteractiveItem m_VRInteractiveItem;
    [SerializeField] private SelectionSlider m_SelectionSlider;
    [SerializeField] private SelectionRadial m_SelectionRadial;

    private GameObject m_ExperimentController;
    private TimeSeriesOutlet m_TimeSeriesOutlet;
    private MarkerOutlet m_MarkerOutlet;

    public float time;
   // public bool destroyed; 

    void Start()
    {
        m_ExperimentController = GameObject.Find("ExperimentController");
        m_TimeSeriesOutlet = GameObject.Find("ServerManager").GetComponent<TimeSeriesOutlet>();
       // destroyed = false; 
    }

    void OnEnable()
    {
        m_SelectionSlider.OnBarFilled += imSelected;
    }

    void OnDisable()
    {
        m_SelectionSlider.OnBarFilled -= imSelected;
    }

    void imSelected()
    {
        Debug.Log("I've been selected!");

        m_TimeSeriesOutlet.marker = 1;

        // Play the aduio clip
        //GetComponent<AudioSource>().Play();
        
        // Start the dissolve
        StartCoroutine("Dissolve",time);

        // Update the subject's score (number of targets collected)
        m_ExperimentController.GetComponent<ScoreTracker>().score++;

        //destroyed = true; 
    }

    IEnumerator Dissolve(float time)
    {  // remember the start

        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            // Increase the dissolve
            float x = Mathf.Lerp(0, 1, (elapsedTime / time));
            this.GetComponent<Renderer>().material.SetFloat("_DissolveIntensity", x);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        m_SelectionRadial.Hide();
        Destroy(this.gameObject);
    }
}
