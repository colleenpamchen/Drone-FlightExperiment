using UnityEngine;
using System.Collections;

public class LockDoor : MonoBehaviour
{

    [SerializeField]
    private GameObject m_DoorBarrier;

    void OnTriggerExit(Collider other)
    {
        // Shuts off the parent door
        transform.GetComponentInParent<SphereCollider>().enabled = false;
        m_DoorBarrier.SetActive(true);
    }
}
