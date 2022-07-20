using UnityEngine;
using System.Collections;
//using Random = UnityEngine.Random;
using UnityStandardAssets.Characters.FirstPerson;
using Assets.LSL4Unity.Scripts;
using System.Text;
using System.IO;
using System;

public class draft_addNoise2 : MonoBehaviour
{
    private StringBuilder csvBuilder = new StringBuilder(); private StringBuilder csvBuilder2 = new StringBuilder();
    private string savePath; private string savePath2;
    [SerializeField]
    private FirstPersonController m_FirstPersonController;
    public int TrialLength = 456;  // 152 * 3 = 456
    private float[] v_xNoise = new float[152]; // new float[456];
    private float[] v_yNoise = new float[152];
    private float[] v_zNoise = new float[152];
    private float[] v_noiseAmp = new float[152];
    private float[] v_noiseSpeed = new float[152];
    private float[] v_framesUntilChange = new float[152];
    private float[] x_position = new float[152];
    private float[] y_position = new float[152];
    private float[] z_position = new float[152];
    private int secToWait = 999;
    private int frame;
    private int relativeFrame;
    public float xNoise;
    public float yNoise;
    public float zNoise;
    public float noiseSpeed = 12;

    [Range(0, 50)]
    public int framesPerShove; // = 12;     // FRAMES PER SHOVE is public...............This affects the visual stimulus 
                               //  [Range(0, 50)]
                               //  private float[] noiseSpeed = new float[] {1.5f,3.0f,6.0f };   // NOISE speed is also public ................. 


    private int shoveCounter = 0;
    private TimeSeriesOutlet m_TimeSeriesOutlet;
    private GameObject m_ExperimentController;
    public GameObject m_ActivateTextInput;
    public int framesUntilChange;
    public float changeRateMean = 1.0f; // 100.0f;
    public float changeRateSD = 0.5f; //5.0f;
    // ...I think this deals with timing of how long in between the shoves 
    public int minFrameChange = 90;
    public int maxFrameChange = 110;
    public int shoveNum = -1;
    public bool testingMode = false;
    public bool noShovesMode = false;
    public bool xzMode = false; // THIS IS THE UPDOWN FORBACK EXPERIMENT *******************
    public bool xyzMode = false; // this is the experiment with XYZ perturbations  
    public bool whiteNoiseMode = true;
    public bool multiAxisMode = false;
    public bool vertMode = false; // The vertical segment will be randomly selected. 
    public int Condition; // = 0 ;
    public float parameter;

    public int[] shoveDirection;
    private int[] shoveDirX;
    private int[] shoveDirY;
    private int[] shoveDirZ;
    private int[] c1;
    private int[] c2;
    private int[] c3;

    void Awake()
    {
        // Random.seed = 42;  //this is depricated, maybe removing this would allow a unique random vector each time the reshuffle() method is called 
        frame = 0;
        relativeFrame = 0;
        framesUntilChange = secToWait * 50;

    }

    // Sends a marker on the first frame of the experiment
    void Start()
    {
        savePath = DataPath();
        savePath2 = DataPath2(); // THIS WRITES THE SHOVE ORIENTATION IN CSV FILE 
        m_FirstPersonController = GameObject.Find("FPSControllerVRAvatar").GetComponent<FirstPersonController>();
        m_TimeSeriesOutlet = GameObject.Find("ServerManager").GetComponent<TimeSeriesOutlet>();
        m_ExperimentController = GameObject.Find("ExperimentController");
        m_ActivateTextInput = GameObject.Find("Canvas");
        MakeNoise();
    } //end of start()

    public void MakeNoise()
    {
        if (testingMode)
        {
            shoveDirection = new int[12];
            // Fill array for testing trials
            shoveDirection[0] = 1;
            shoveDirection[1] = 1;
            shoveDirection[2] = -1;
            shoveDirection[3] = -1;
            shoveDirection[4] = 2;
            shoveDirection[5] = 2;
            shoveDirection[6] = -2;
            shoveDirection[7] = -2;
            shoveDirection[8] = 3;
            shoveDirection[9] = 3;
            shoveDirection[10] = -3;
            shoveDirection[11] = -3;
        }
        if (noShovesMode)
        {
            shoveDirection = new int[296];
            // Fill the array for shoveless trials
            for (int i = 0; i < 296; i++)
            {
                shoveDirection[i] = 0;
            }
        }
        if (xzMode) // CONVERTED THIS INTO THE randomized UP DOWN SHOVES  ********************
        {
            shoveDirection = new int[296];
            // Fill the array for real xz trials
            for (int i = 0; i < 74; i++)
            { shoveDirection[i] = 1; }
            for (int i = 74; i < 148; i++)
            { shoveDirection[i] = -1; }
            for (int i = 148; i < 222; i++)
            { shoveDirection[i] = 2; }
            for (int i = 222; i < 296; i++)
            { shoveDirection[i] = -2; }
        }
        if (whiteNoiseMode)
        {
            shoveDirection = new int[TrialLength];
            shoveDirX = new int[152]; shoveDirY = new int[152]; shoveDirZ = new int[152];
            for (int i = 0; i < 74; i++) { shoveDirZ[i] = 1; }
            for (int i = 74; i < 152; i++) { shoveDirZ[i] = -1; }

            for (int i = 0; i < 74; i++) { shoveDirX[i] = 2; }
            for (int i = 74; i < 152; i++) { shoveDirX[i] = -2; }

            for (int i = 0; i < 74; i++) { shoveDirY[i] = 3; }
            for (int i = 74; i < 152; i++) { shoveDirY[i] = -3; }
            reshuffle(shoveDirY); reshuffle(shoveDirZ); reshuffle(shoveDirX);

            shoveDirZ.CopyTo(shoveDirection, 0);
            shoveDirY.CopyTo(shoveDirection, shoveDirZ.Length);
            shoveDirX.CopyTo(shoveDirection, 304);

            reshuffle(shoveDirection);


        }
        if (xyzMode)
        {
            shoveDirection = new int[TrialLength];
            shoveDirX = new int[152]; shoveDirY = new int[152]; shoveDirZ = new int[152]; //total of 456 shoves

            // vectorZ [1/-1] for/back  with 21% mix of other orientations 
            for (int i = 0; i < 74; i++) { shoveDirZ[i] = 1; }
            for (int i = 74; i < 152; i++) { shoveDirZ[i] = -1; }

            //  vectorX [2/-2] left/right with 21% mix of other orientations 
            for (int i = 0; i < 74; i++) { shoveDirX[i] = 2; }
            for (int i = 74; i < 152; i++) { shoveDirX[i] = -2; }

            // vectorY up/down with 21% mix of other orientations 
            for (int i = 0; i < 74; i++) { shoveDirY[i] = 3; }
            for (int i = 74; i < 152; i++) { shoveDirY[i] = -3; }


            reshuffle(shoveDirY); reshuffle(shoveDirZ); reshuffle(shoveDirX);

            shoveDirZ.CopyTo(shoveDirection, 0);
            shoveDirY.CopyTo(shoveDirection, shoveDirZ.Length);
            shoveDirX.CopyTo(shoveDirection, 304);

            reshuffle(shoveDirection);

            /* This was for CONTROL EXP 2 with low forward backward amplitudes. 
            for( int i=0; i<(TrialLength/2) ; i++) { shoveDirection[i] = 1; }
            for( int i= (TrialLength / 2); i< TrialLength; i++) { shoveDirection[i] = -1; }
             reshuffle(shoveDirection);

    */
            // shoveDirection = shoveDirZ.Concat(shoveDirY, shoveDirX);

        }
        if (vertMode)  // Experiment is broken down into 3 Epochs. 
        {
            shoveDirection = new int[300];
            shoveDirX = new int[100]; shoveDirY = new int[100]; shoveDirZ = new int[100]; //total of 300 shoves
            c1 = new int[300]; c2 = new int[300]; c3 = new int[300];

            // Z [1/-1] for/back 
            for (int i = 0; i < 22; i++) { shoveDirZ[i] = 1; }
            for (int i = 22; i < 45; i++) { shoveDirZ[i] = -1; }
            for (int i = 45; i < 67; i++) { shoveDirZ[i] = 2; }
            for (int i = 67; i < 90; i++) { shoveDirZ[i] = -2; }
            for (int i = 90; i < 95; i++) { shoveDirZ[i] = 3; }
            for (int i = 95; i < 100; i++) { shoveDirZ[i] = -3; }
            //  X [2/-2] left/right
            for (int i = 0; i < 22; i++) { shoveDirX[i] = 2; }
            for (int i = 22; i < 45; i++) { shoveDirX[i] = -2; }
            for (int i = 45; i < 67; i++) { shoveDirX[i] = 1; }
            for (int i = 67; i < 90; i++) { shoveDirX[i] = -1; }
            for (int i = 90; i < 95; i++) { shoveDirX[i] = 3; }
            for (int i = 95; i < 100; i++) { shoveDirX[i] = -3; }
            // Y up/down  
            for (int i = 0; i < 40; i++) { shoveDirY[i] = 3; }
            for (int i = 40; i < 80; i++) { shoveDirY[i] = -3; }
            for (int i = 80; i < 85; i++) { shoveDirY[i] = 2; }
            for (int i = 85; i < 90; i++) { shoveDirY[i] = -2; }
            for (int i = 90; i < 95; i++) { shoveDirY[i] = 1; }
            for (int i = 95; i < 100; i++) { shoveDirY[i] = -1; }

            reshuffle(shoveDirX);
            reshuffle(shoveDirY);
            reshuffle(shoveDirZ);

            // RANDOMIZE THE UP/DOWN segment here

            // C1
            shoveDirZ.CopyTo(c1, 0);
            shoveDirX.CopyTo(c1, shoveDirZ.Length);
            shoveDirY.CopyTo(c1, 200);
            // C2
            shoveDirX.CopyTo(c2, 0);
            shoveDirY.CopyTo(c2, shoveDirX.Length);
            shoveDirZ.CopyTo(c2, 200);
            // C3 
            shoveDirY.CopyTo(c3, 0);
            shoveDirZ.CopyTo(c3, shoveDirY.Length);
            shoveDirX.CopyTo(c3, 200);

            switch (Condition)
            {
                case 1:
                    shoveDirection = c1;
                    break;
                case 2:
                    shoveDirection = c2;
                    break;
                case 3:
                    shoveDirection = c3;
                    break;
            }

            // end of the Experimental Conditions where there are 3 epochs, one of which is mostly vertical perturbations 
        }
        if (multiAxisMode)
        {
            shoveDirection = new int[296];

            // Fill the array with threes as filler
            for (int i = 0; i < 296; i++)
            {
                shoveDirection[i] = 4;
            }
        }
        SaveData2();

        // Confirm array length
        //  Debug.Log("There are " + shoveDirY.Length + " shoves in this orientation.");
        //     Debug.Log("There are " + shoveDirection.Length + " shoves this run.");

    }

    void reshuffle(int[] a) // this method may need a random seed, because the shuffling isn't random each time it's called. it's ok for now though... I really don't care 
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < a.Length; t++)
        {
            int tmp = a[t];
            int r = UnityEngine.Random.Range(t, a.Length);
            a[t] = a[r];
            a[r] = tmp;
        }
    }

    void FixedUpdate() // physics enginge 50 Hz 
    {
        if (m_ExperimentController.GetComponent<SicknessTracker>().finalSpawn == true)
        {
            m_ExperimentController.GetComponent<SicknessTracker>().finalSpawn = false;
            //     frame = 0;
            //     relativeFrame = 0;
            relativeFrame = -1 * shoveCounter;  //shoveCounter initiallized to 0
            framesUntilChange = secToWait * 50;  //secToWait = 999, but this framesUntilChange changed to 50 when startShoves activated. 
            shoveNum = -1;
            m_ActivateTextInput.SetActive(true);
        }
        frame++; //starts to increment from 0
        relativeFrame++;
        //    Debug.Log("from user input: " + parameter);
        // Adds noise every changeRate-th frame
        if (shoveNum < shoveDirection.Length) //- 1)
        {
            if (relativeFrame >= framesUntilChange)
            {
                // Track how many shoves have occurred
                shoveNum++;
                //      Debug.Log("shove NUMBER: " + shoveNum + " direction:" + shoveDirection[shoveNum]);
                // Sets the number of frames to shove over
                shoveCounter = framesPerShove;

                // Ensures that the number of frames to shove over is odd
                shoveCounter = shoveCounter + (shoveCounter + 1) % 2;

                // Write the event to the change marker stream
                m_TimeSeriesOutlet.noise = 1; /////////////////////// noise = PERTURBATION god damn this is fucking confusing! 

                // Selects how many frames until the next perturbation
                framesUntilChange = Mathf.RoundToInt(RandomFromDistribution.RandomNormalDistribution(changeRateMean, changeRateSD));
                if (framesUntilChange > maxFrameChange)
                {
                    framesUntilChange = maxFrameChange;
                }
                if (framesUntilChange < minFrameChange)
                {
                    framesUntilChange = minFrameChange;
                }

                // Selects a direction for the perturbation
                if (!multiAxisMode)
                {
                    if (shoveDirection[shoveNum] == 1) // Z Forward
                    {
                        // Debug.Log("if shoveDirection==1");
                        xNoise = 0;
                        zNoise = 1f;
                        yNoise = 0;
                    }
                    if (shoveDirection[shoveNum] == -1) // Z Backward
                    {
                        xNoise = 0;
                        zNoise = -1f;
                        yNoise = 0;
                    }
                    if (shoveDirection[shoveNum] == 2) // X Right
                    {
                        xNoise = 1f;
                        zNoise = 0;
                        yNoise = 0;
                    }
                    if (shoveDirection[shoveNum] == -2) // X Left
                    {
                        xNoise = -1f;
                        zNoise = 0;
                        yNoise = 0;
                    }
                    if (shoveDirection[shoveNum] == 3) // Y Up
                    {
                        xNoise = 0;
                        zNoise = 0;
                        yNoise = 1f;
                    }
                    if (shoveDirection[shoveNum] == -3) // Y Down
                    {
                        xNoise = 0;
                        zNoise = 0;
                        yNoise = -1f;
                    }
                }

                if (multiAxisMode)
                {
                    xNoise = RandomFromDistribution.RandomNormalDistribution(0f, 0.5f);
                    zNoise = RandomFromDistribution.RandomNormalDistribution(0f, 0.5f);
                    yNoise = RandomFromDistribution.RandomNormalDistribution(0f, 0.5f);
                }

                // Log which direction is being shoved
                //     Debug.Log("Shove " + shoveNum + "  x: " + xNoise + "  y: " + yNoise + "  z: " + zNoise);

                // Reset the relative frame to 0 since a perturbation just occurred
                relativeFrame = -1 * shoveCounter;

                noiseSpeed = parameter;
                Debug.Log("noiseSpeed= " + noiseSpeed);

                // Record this shove's info to CSV file
                v_xNoise[shoveNum] = xNoise;
                v_yNoise[shoveNum] = yNoise;
                v_zNoise[shoveNum] = zNoise;
                v_noiseAmp[shoveNum] = m_FirstPersonController.noiseAmp;
                v_noiseSpeed[shoveNum] = noiseSpeed;
                v_framesUntilChange[shoveNum] = framesUntilChange;
                x_position[shoveNum] = transform.position.x;
                y_position[shoveNum] = transform.position.y;
                z_position[shoveNum] = transform.position.z;
                //        Debug.Log("shove " + shoveNum + "  x: " + transform.position.x + "  y: " + transform.position.y + "  z: " + transform.position.z);
            }
            if (shoveCounter > 0)
            {
                noiseSpeed = parameter;
                doShove(xNoise, yNoise, zNoise, noiseSpeed);

                //   Debug.Log("time: " + Time.time * 1000);
                // Debug.Log("doing shove");
                shoveCounter--;          // .................... this is the countdown 

                if (shoveCounter == 0)
                {
                    m_FirstPersonController.xNoise = 0;
                    m_FirstPersonController.yNoise = 0;
                    m_FirstPersonController.zNoise = 0;
                    m_FirstPersonController.noiseAmp = 1; // Needs to be 1 so that it does not scale intended speed
                    m_FirstPersonController.noiseSpeed = 1; // Needs to be 1 so that it does not scale intended speed

                    // Write the event to the change marker stream
                    m_TimeSeriesOutlet.noise = 0f;
                }
            }

        }
        //if (shoveCounter > 0)
        //{

        //    doShove(xNoise, yNoise, zNoise, noiseSpeed[0]);

        //    //   Debug.Log("time: " + Time.time * 1000);
        //    // Debug.Log("doing shove");

        //    shoveCounter--;          // this is the countdown 

        //    if (shoveCounter == 0)
        //    {
        //        m_FirstPersonController.xNoise = 0;
        //        m_FirstPersonController.yNoise = 0;
        //        m_FirstPersonController.zNoise = 0;
        //        m_FirstPersonController.noiseAmp = 1; // Needs to be 1 so that it does not scale intended speed
        //        m_FirstPersonController.noiseSpeed = 1; // Needs to be 1 so that it does not scale intended speed

        //        // Write the event to the change marker stream
        //        m_TimeSeriesOutlet.noise = 0f;
        //    }
        //}

    }

    void doShove(float x, float y, float z, float s)
    {
        // Selects a random direction for the perturbation on the unit circle
        m_FirstPersonController.xNoise = x;
        m_FirstPersonController.yNoise = y;
        m_FirstPersonController.zNoise = z;

        m_FirstPersonController.noiseSpeed = s;

        // Magnitude of the show vector
        m_FirstPersonController.noiseAmp = Mathf.Sqrt((m_FirstPersonController.xNoise * m_FirstPersonController.xNoise) + (m_FirstPersonController.yNoise * m_FirstPersonController.yNoise) + (m_FirstPersonController.zNoise * m_FirstPersonController.zNoise));

        if (m_FirstPersonController.noiseAmp != 0)
        {
            // Normalizes if amplitude is not 0
            m_FirstPersonController.xNoise = m_FirstPersonController.xNoise / m_FirstPersonController.noiseAmp;
            m_FirstPersonController.yNoise = m_FirstPersonController.yNoise / m_FirstPersonController.noiseAmp;
            m_FirstPersonController.zNoise = m_FirstPersonController.zNoise / m_FirstPersonController.noiseAmp;
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SaveData();

            //  Debug.Log("Noise CSV file saved...");
            this.GetComponent<AudioSource>().Play();
        }
    }

    public static string DataPath()
    {
        return string.Format("{0}/{1}.csv", Application.dataPath, System.DateTime.Now.ToString("Noise_yyyy-MM-dd_HH-mm-ss"));
    }
    public static string DataPath2()
    {
        return string.Format("{0}/{1}.csv", Application.dataPath, System.DateTime.Now.ToString("Dir_yyyy-MM-dd_HH-mm-ss"));
    }

    void SaveData()
    {

        for (int k = 0; k < v_xNoise.Length; k++)
        {
            string newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", x_position[k], y_position[k], z_position[k], v_noiseSpeed[k], framesPerShove, v_xNoise[k], v_yNoise[k], v_zNoise[k], v_noiseAmp[k]); //v_framesUntilChange[k]
            csvBuilder.AppendLine(newLine);
        }

        // Make array into string
        File.WriteAllText(savePath, csvBuilder.ToString());
    }
    public void SaveData2()
    { // 

        for (int k = 0; k < shoveDirection.Length; k++)
        {
            string newLine = string.Format("{0}", shoveDirection[k]);
            csvBuilder2.AppendLine(newLine);
        }

        // Make array into string
        File.WriteAllText(savePath2, csvBuilder2.ToString());
    }
}
