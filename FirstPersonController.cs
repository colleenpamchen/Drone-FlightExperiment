using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;


namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        public float m_WalkSpeed;

        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpAmp; 
        [SerializeField] private float m_StickToGroundForce; 
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick; 
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector3 m_Input; 
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle; 
        private float m_NextStep;
        private bool m_Jumping;

        public bool HmdRotatesY = false;

        public float xNoise;
        public float yNoise;
        public float zNoise;
        
        // Parameters for the stimulus 
        public float noiseSpeed; 
        public float noiseAmp; 

        public float horizontalSpeed;
        public float verticalSpeed;
        public float overallSpeed;
        public Vector3 horizontalVelocity; 
        
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);

            // step cycle updates the next step .... 
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;

            m_Jumping = false;
			m_MouseLook.Init(transform , m_Camera.transform);

            // HmdRotatesY parent object
            GameObject cameraParent = new GameObject("Camera Parent");
            cameraParent.transform.SetParent(m_Camera.transform.parent, false);
            m_Camera.transform.SetParent(cameraParent.transform, false);
        }


        private void Update()
        {
            RotateView();
            Vector3 horizontalVelocity = m_CharacterController.velocity;
            horizontalVelocity = new Vector3(m_CharacterController.velocity.x, 0, m_CharacterController.velocity.z);
            
            float horizontalSpeed = horizontalVelocity.magnitude;
            float verticalSpeed = m_CharacterController.velocity.y;
            float overallSpeed = m_CharacterController.velocity.magnitude;
        }
        
        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);

            // Create the noise vector relative to the camera direction
            Vector3 noiseVec = (transform.forward * zNoise) + (transform.up * yNoise)  + (transform.right * xNoise); 
            noiseVec = noiseVec * noiseAmp * noiseSpeed;

            // always move along the camera forward as it is the direction that it being aimed at
            // m_Input.z is Unity Z (FB) direction
            // m_Input.x is Unity X (RL) direction
            // m_input.y is Unity Z (Up/Down) direction 
            Vector3 desiredMove = (transform.forward * m_Input.y) + (transform.right * m_Input.x) + (transform.up * Input.GetAxis("YAxis")); 

            // Scales the speed without noise
            desiredMove.x = desiredMove.x * speed;
            desiredMove.z = desiredMove.z * speed;
            desiredMove.y = desiredMove.y * speed;

            // Adds the noise to the desired movement
            desiredMove = desiredMove + noiseVec;

            // get a normal vector for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.forward, out hitInfo,
                               m_CharacterController.height/2f, ~0, QueryTriggerInteraction.Ignore);

            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal);
            // desired movement that contains force information, noise information, speed .... 
            // this vector should be applied as a force to move the avatar camera 

            m_MoveDir.x = desiredMove.x;
            m_MoveDir.z = desiredMove.z;
            m_MoveDir.y = desiredMove.y;
            
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            m_MouseLook.UpdateCursorLock();
        }
        

        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }

            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed) 
        {
 
            float horizontal = CrossPlatformInputManager.GetAxis("XAxis");
            float vertical = CrossPlatformInputManager.GetAxis("ZAxis");
            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            // HMD rotates yaw, rotating about the Y-axis 
            if (HmdRotatesY) 
            {
                Transform root = m_Camera.transform.parent;
                Transform centerEye = m_Camera.transform;

                Vector3 prevPos = root.position;
                Quaternion prevRot = root.rotation;

                transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

                root.position = prevPos;
                root.rotation = prevRot;

                // Reset the mouse look to our new orientation
                m_MouseLook.Init(transform, m_Camera.transform);
            }

            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
        
        private void ProgressStepCycle(float speed) 
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                    Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;
        }
    }
}
