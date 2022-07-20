using UnityEngine;
using System;
using VRCamera.Scripts;

public class HideReticle : MonoBehaviour {

    private GameObject m_MainCamera;
    public bool hide;

	// Use this for initialization
	void Start ()
    {
        m_MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if(hide)
        {
            m_MainCamera.GetComponent<Reticle>().Hide();
        }       
    }

}
