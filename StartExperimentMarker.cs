using UnityEngine;
using System.Collections;
using Assets.LSL4Unity.Scripts;

public class startExperimentMarker : MonoBehaviour {

    private TimeSeriesOutlet m_TimeSeriesOutlet;

    // Sends a marker on the first frame of the experiment
    void Start ()
    {
        m_TimeSeriesOutlet = GameObject.Find("ServerManager").GetComponent<TimeSeriesOutlet>();
        m_TimeSeriesOutlet.marker = 11;
    }
}
