using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Minion : MonoBehaviour
{
    [SerializeField]
    private float m_WalkSpeed;
    [SerializeField]
    private float m_TurnSpeed;
    [SerializeField]
    private float m_JumpSpeed;
    [SerializeField]
    private float m_StickToGroundForce;
    [SerializeField]
    private float m_GravityMultiplier;

    private bool m_Jump;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private bool m_Jumping;

    public Transform Target;

	void Start ()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Jumping = false;
        Target = get_tower().transform;
	}

    towerScript get_tower()
    {
        towerScript result = null;
        float closest_distance = 50000000;
        Vector3 myposition = transform.position;

        towerScript[] towerlist = FindObjectsOfType(typeof(towerScript)) as towerScript[];
        foreach(towerScript obj in towerlist)
        {
           if (obj.m_completecontroll == false)
            {
                Vector3 towerpos = obj.transform.position;
               float distance = Vector3.Distance(myposition, towerpos);
                if(distance < closest_distance)
                {
                    closest_distance = distance;
                    result = obj;
                }
            }
        }
        return result;
    }
	void Update ()
    {
        //RotateView(); 
        Vector3 targetPos = Target != null ? Target.position : Vector3.zero;
        TurnTowardTarget(targetPos);

        towerScript closesttower = get_tower();



        float closesttower_distance = (closesttower.transform.position - this.transform.position).magnitude;

        if (closesttower_distance < 2)
        {
            GameObject.Destroy(this.gameObject);

        }










        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump)
        {
            m_Jump = true;
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            //StartCoroutine(m_JumpBob.DoBobCycle());
            //PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
	}

    private void FixedUpdate()
    {
        GetInput();
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, ~0, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * m_WalkSpeed;
        m_MoveDir.z = desiredMove.z * m_WalkSpeed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                //PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        //ProgressStepCycle(speed);
        //UpdateCameraPosition(speed);

        //m_MouseLook.UpdateCursorLock();
    }

    private void TurnTowardTarget(Vector3 target)
    {
        target.y = transform.position.y;

        Vector3 right = transform.right;

        Vector3 toTarget = target - transform.position;
        if (toTarget.magnitude <= 3.0f)
            return;

        float rightDotToTarget = Vector3.Dot(right, toTarget.normalized);

        if (rightDotToTarget > 0.01f)
            transform.Rotate(0, m_TurnSpeed * Time.deltaTime, 0);
        else if (rightDotToTarget < -0.01f)
            transform.Rotate(0, -m_TurnSpeed * Time.deltaTime, 0);
    }

    private void GetInput()
    {
        // Read input
        float horizontal = 0.0f;// CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = 1.0f; // CrossPlatformInputManager.GetAxis("Vertical");

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        // !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        //if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        //{
        //    StopAllCoroutines();
        //    StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        //}
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
    //private void ProgressStepCycle(float speed)
    //{
    //    if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
    //    {
    //        m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
    //                     Time.fixedDeltaTime;
    //    }

    //    if (!(m_StepCycle > m_NextStep))
    //    {
    //        return;
    //    }

    //    m_NextStep = m_StepCycle + m_StepInterval;

    //    PlayFootStepAudio();
    //}
}
