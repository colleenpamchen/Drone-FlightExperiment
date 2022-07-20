using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TriggerPerturbation : MonoBehaviour {

    public float Force;
    public float waitTime = 2f;

    private CharacterController m_PlayerRB;
    private float timer;
    private bool timerRunning;
    private bool inTrigger;

    void Start()
    {
        m_PlayerRB = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
    }

    void Update()
    {
        if (timerRunning)
        {
            ForcePush();
        }
    }

    void ForcePush()
    {
        timer += Time.deltaTime;
        if (timer > waitTime)
        {
            Debug.Log("Player pushed at " + Time.time);
            timer = 0f;
            timerRunning = false;
            m_PlayerRB.Move(Vector3.forward * Force * Time.deltaTime);
            //m_PlayerRB.velocity = new Vector3(0, 0, 1);
            //m_PlayerRB.AddForce(Vector3.back * Force, ForceMode.Impulse);
        }    
    }

    // When an object enters the trigger
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object entered trigger at " + Time.time);
        timerRunning = true;
    }
}
