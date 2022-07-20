using UnityEngine;
using System.Collections;
using Assets.LSL4Unity.Scripts;

public class TriggerTimer : MonoBehaviour {

    public float waitTime = 2f;
    public float force = 5f;
    public float markerValue = 1;
    public Vector3 direction;

    private ImpactReceiver m_ImpactReceiver;
    private TimeSeriesOutlet m_TimeSeriesOutlet;
    private MarkerOutlet m_MarkerOutlet;
    private bool timerRunning;
    private float timer;
    private bool inTrigger;

    private SicknessTracker m_SicknessTracker;

    // Use this for initialization
    void Start ()
    {
        m_ImpactReceiver = GameObject.FindGameObjectWithTag("Player").GetComponent<ImpactReceiver>();
        m_TimeSeriesOutlet = GameObject.Find("ServerManager").GetComponent<TimeSeriesOutlet>();
        m_MarkerOutlet = GameObject.Find("ServerManager").GetComponent<MarkerOutlet>();
        m_SicknessTracker = GameObject.Find("ExperimentController").GetComponent<SicknessTracker>();
    }

    void Update()
    {
        if (timerRunning)
        {
            timer += Time.deltaTime;

            if (timer > waitTime)
            {

                // Shove the player
                m_ImpactReceiver.AddImpact(direction, force);

                // Record that the shove has occurred
                m_SicknessTracker.numTimesShoved++;

                // Kill the timer and destroy the script
                timer = 0f;
                timerRunning = false;
                StartCoroutine("stopMarker");         
            }
        }
    }

    // When an object enters the trigger
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object entered trigger at " + Time.time);
        timerRunning = true;
        Debug.Log("Marker set to " + markerValue);
        m_TimeSeriesOutlet.marker = markerValue;
        m_MarkerOutlet.Write(markerValue.ToString());
    }

    // Waits for 1 frame and then sets the marker to 0 and destroys the object
    IEnumerator stopMarker()
    {
        yield return 0;
        if(gameObject.tag == "InvisibleObject")
        {
            Destroy(this.transform.parent.gameObject, 3);
            Debug.Log("Parent destroyed.");
        }
        if(gameObject.tag == "Perturbation")
        {
            Destroy(gameObject);
            Debug.Log("Object destroyed.");
        }
    }
}
