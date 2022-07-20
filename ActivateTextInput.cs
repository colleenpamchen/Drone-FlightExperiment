using UnityEngine;
using UnityEngine.UI;


public class ActivateTextInput : MonoBehaviour
{
    public GameObject canvasObject;
    public InputField mainInputField;
    public GameObject m_FPSControllerVRAvatar;
    public string answer;
    public float parameter = 12;
    
    private AddNoise2 m_addnoisescript;

    
    /// <summary>
    /// Activate the main input field when the scene starts.
    /// </summary>
    void Start()
    {
        m_FPSControllerVRAvatar = GameObject.Find("FPSControllerVRAvatar");
        m_addnoisescript = m_FPSControllerVRAvatar.GetComponent<AddNoise2>();
    }

    private void FixedUpdate()
    {
        mainInputField.ActivateInputField();
        
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            answer = mainInputField.text;
            m_addnoisescript.parameter = parameter;
            canvasObject.SetActive(false);
        }

    }

}