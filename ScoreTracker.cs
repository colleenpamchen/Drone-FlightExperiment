using UnityEngine;
using UnityEngine.UI;
using VRCamera.Scripts;

public class ScoreTracker : MonoBehaviour {

    public float score = 0f;
    public float maxScore = 80;

    public string m_TargetText;
    private GameObject m_ExperimentController;
    private GameObject[] targets; 

    void Start()
    {
        maxScore = GameObject.Find("Targets").transform.childCount;
        m_ExperimentController = GameObject.Find("ExperimentController");
    }

    void Update()
    {
        if(m_ExperimentController.GetComponent<SicknessTracker>().finalSpawn == true)
        {
            targets = GameObject.FindGameObjectsWithTag("Drones");
            m_TargetText = targets[0].name; //GameObject.Find("TargetText");
            // m_TargetText.GetComponent<Text>().text = score + " / " + maxScore;
            //m_TargetText
        }  
    }
}
