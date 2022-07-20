using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class ImpactReceiver : MonoBehaviour
{
    float mass = 1.0F; // defines the character mass
    Vector3 impact = Vector3.zero;
    private CharacterController character;
    private FirstPersonController m_FirstPersonController;

    // Use this for initialization
    void Start()
    {
        character = this.GetComponent<CharacterController>();
        m_FirstPersonController = GetComponent<FirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        // apply the impact force:
        m_FirstPersonController.m_WalkSpeed = 3;
        if (impact.magnitude > 0.2F)
        {
            character.Move(impact * Time.deltaTime);
            m_FirstPersonController.m_WalkSpeed = 0;
        }
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 1.25f * Time.deltaTime);
    }

    // call this function to add an impact force:
    public void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force / mass;
    }
}
