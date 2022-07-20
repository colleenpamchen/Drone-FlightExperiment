using UnityEngine;
using System.Text;
using System.IO;
using Assets.LSL4Unity.Scripts;

public class SicknessTracker : MonoBehaviour
{
    [SerializeField] private GameObject m_SickCanvas;
    [SerializeField] private GameObject m_ScoreDisplayFollow;

    private GameObject m_SicknessHUD;
    private GameObject m_FPSControllerVRAvatar;
    private GameObject m_MainCamera;
    private TimeSeriesOutlet m_TimeSeriesOutlet;
    private AddNoise2 m_addNoise2;
    private Vector3 v_spawnTransform;
    private Vector3 v_spawnRotation;
    
    private StringBuilder csvBuilder = new StringBuilder();
    private string savePath;
    
    // variables that need to be public in order to write these parameters to file  
    public float time;
    public float time2;
    public bool HUDCallComplete = false;
    public float askTime; 
    public int maxInputs;
    public int inputNumber = -1;
    public int[] sicknessScores = new int[30];
    public int[] totalShoves = new int[500];
    public float[] TimeFB = new float[500];
    public float[] TimeRL = new float[500];
    public int numTimesShoved;              
    public int numTimesShovedNew;
    public float totalTimeFB;
    public float totalTimeRL;
    public int maxShoves;
    public bool finalSpawn = false;

    void Start()
    {
        savePath = DataPath();
        
        m_SicknessHUD = GameObject.Find("SicknessHUD");
        m_FPSControllerVRAvatar = GameObject.Find("FPSControllerVRAvatar");
        m_MainCamera = GameObject.Find("Main Camera");
        m_TimeSeriesOutlet = GameObject.Find("ServerManager").GetComponent<TimeSeriesOutlet>();
        m_addNoise2 = GameObject.Find("FPSControllerVRAvatar").GetComponent<AddNoise2>();

        v_spawnTransform.x = m_FPSControllerVRAvatar.transform.position.x; 
        v_spawnTransform.y = m_FPSControllerVRAvatar.transform.position.y; 
        v_spawnTransform.z = m_FPSControllerVRAvatar.transform.position.z;
        v_spawnRotation.x = 1; 
        v_spawnRotation.y = 177; 
        v_spawnRotation.z = 1;

        m_ScoreDisplayFollow.SetActive(false);

        if (m_FPSControllerVRAvatar.GetComponent<AddNoise2>().testingMode == true)
        {
            askTime = 3f;
            maxInputs = 3;
        }

    }

    void Update()
    {
        // Keep track of time since last SSQ response
        time += Time.deltaTime;
        time2 += Time.deltaTime;
        
        // Experiment termination condition
        maxShoves = m_addNoise2.shoveDirection.Length;
        
        // Trial number 
        numTimesShovedNew = m_FPSControllerVRAvatar.GetComponent<AddNoise2>().shoveNum + 1;
        
        // Tracks player movement in the virtual environment 
        CharacterController controller = m_FPSControllerVRAvatar.GetComponent<CharacterController>();
        if (controller.velocity.z > 0.1f | controller.velocity.z < -0.1f) // .z is the FB 
        {
            totalTimeFB++;
        }
        if (controller.velocity.x > 0.1f | controller.velocity.x < -0.1f) // .x is the RL 
        {
            totalTimeRL++;
        }


        if (inputNumber > -1)
        {
            askTime = 30f;  
        }

        // Show the HUD only when n number of seconds have passed since the last subject response was recorded
        if (inputNumber + 1 < maxInputs)
        {
            if (time2 >= askTime)
            {
                if (!HUDCallComplete)
                {
                    inputNumber++;
                    m_TimeSeriesOutlet.ssqMarker = 0; 
                    
                    float x = m_SicknessHUD.transform.position.x;
                    float y = m_SicknessHUD.transform.position.y;
                    float z = m_SicknessHUD.transform.position.z;
                    GameObject sickCanvasRef = Instantiate(m_SickCanvas, new Vector3(x + .1f, y - .05f, z), m_MainCamera.transform.rotation) as GameObject;
                    sickCanvasRef.tag = "Clone";
                    sickCanvasRef.transform.SetParent(m_SicknessHUD.transform, true);
                    m_SicknessHUD.GetComponent<AudioSource>().Play();

                    HUDCallComplete = true;

                    TimeFB[inputNumber] = totalTimeFB;
                    TimeRL[inputNumber] = totalTimeRL;
                    totalShoves[inputNumber] = numTimesShovedNew;
                }
            }
        }
        if (inputNumber + 1 == maxInputs && !finalSpawn && !HUDCallComplete)
        {
            m_ScoreDisplayFollow.SetActive(true);
            m_ScoreDisplayFollow.GetComponent<AudioSource>().Play();

            finalSpawn = true;
            m_FPSControllerVRAvatar.transform.position = v_spawnTransform;
        } 

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SaveData();
        }
    }

    public static string DataPath()
    {
        return string.Format("{0}/{1}.csv", Application.dataPath, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    void SaveData()
    {
        for (int k = 0; k < sicknessScores.Length; k++)
        {
            string newLine = string.Format("{0},{1},{2},{3}", sicknessScores[k], totalShoves[k], TimeFB[k], TimeRL[k]);
            csvBuilder.AppendLine(newLine);
        }

        File.WriteAllText(savePath, csvBuilder.ToString());
    }
}
