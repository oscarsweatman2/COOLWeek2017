using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
public class Minion : MonoBehaviour
{
    public static List<Minion> AllMinions = new List<Minion>();

    public enum Allegiance
    {
        RED,BLUE,NEUTRAL 
    }

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
    public AudioSource m_minionhittower;
    public bool miniononplayerteam;
    private bool m_Jump;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private bool m_Jumping;

    public Transform Target;
    
    public bool enableDeathTimer = false;
    public float totalLifeTime = 20.0f;

	void Start ()
    {
        AllMinions.Add(this);

        m_CharacterController = GetComponent<CharacterController>();
        m_Jumping = false;

        PickTarget();
	}

    private void OnDestroy()
    {
        AllMinions.Remove(this);
    }

    void PickTarget()
    {
        towerScript closestTower = get_tower();
        if (closestTower != null)
        {
            Target = closestTower.transform;
        }
    }

    // targets tower
    towerScript get_tower()
    {
        towerScript result = null;
        float closest_distance = 50000000;
        Vector3 myposition = transform.position;
        
        foreach(towerScript tower in towerScript.AllTowers)
        {
            bool opponettower = true;
            bool toweronplayerteam = tower.m_teamAllegiance == Allegiance.BLUE;
            bool toweronenemyteam = tower.m_teamAllegiance == Allegiance.RED;
            //tells mionion if on tower team and targets that tower
            if (toweronplayerteam)
            {
                if (miniononplayerteam)
                {
                    opponettower = false;
                }
                else
                {
                    opponettower = true;
                }
            }
            else if (toweronenemyteam)
            {
                if (miniononplayerteam)
                {
                    opponettower = true;
                }
                else
                {
                    opponettower = false;
                }

            }
            else
            {
                opponettower = true;
            }


            if (opponettower == true)
            {
                Vector3 towerpos = tower.transform.position;
               float distance = Vector3.Distance(myposition, towerpos);
                if(distance < closest_distance)
                {
                    closest_distance = distance;
                    result = tower;
                }
            }
        }
        return result;
    }
	void Update ()
    {
        PickTarget();

        //RotateView(); 
        Vector3 targetPos = Target != null ? Target.position : Vector3.zero;
        TurnTowardTarget(targetPos);

        towerScript closesttower = get_tower();

        if (closesttower != null)
        {
            //distance to tower that would destroy sheep
            float closesttower_distance = (closesttower.transform.position - this.transform.position).magnitude;

            if (closesttower_distance < 3)
            {
                GameObject.Destroy(this.gameObject);
                closesttower.towerHasBeenAttacked(this);
            }
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

        // Optionally kill minions if enough time has passed
        if (enableDeathTimer)
        {
            totalLifeTime -= Time.deltaTime;

            if (totalLifeTime <= 0.0f)
            {
                GameObject.Destroy(this.gameObject);
                return;
            }
        }

        // Hack fix for minions falling out of the world, kill them
        if(this.transform.position.y < 0)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        GetInput();
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        //RaycastHit hitInfo;
        //Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
        //                   m_CharacterController.height / 2f, ~0, QueryTriggerInteraction.Ignore);
        //desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

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

    }

    private void TurnTowardTarget(Vector3 target)
    {
        target.y = transform.position.y;

        Vector3 right = transform.right;

        Vector3 toTarget = target - transform.position;
        if (toTarget.magnitude <= 3.0f)
            return;

        float rightDotToTarget = Vector3.Dot(right, toTarget.normalized);
        float fwdDotToTarget = Vector3.Dot(transform.forward, toTarget);

        // If the target is in front of us
        if (fwdDotToTarget > 0.0f)
        {
            // If the target is in front of us.... we want a small threshold
            // where we don't turn at all.  Makes for smoother forward movement.
            if (rightDotToTarget > 0.01f)
                transform.Rotate(0, m_TurnSpeed * Time.deltaTime, 0);
            else if (rightDotToTarget < -0.01f)
                transform.Rotate(0, -m_TurnSpeed * Time.deltaTime, 0);
        }
        else
        {
            // Target is behind us
            // Now we need to turn left or right, no place for a small threshold of no turning here.
            // That behavior would make us 'stick' going forward if the target were directly behind us.
            if (rightDotToTarget > 0.0f)
                transform.Rotate(0, m_TurnSpeed * Time.deltaTime, 0);
            else
                transform.Rotate(0, -m_TurnSpeed * Time.deltaTime, 0);
        }
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

       
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Make enemy minions do damage to blocks
        if(!miniononplayerteam && hit.collider != null && hit.collider is BoxCollider)
        {
            Voxel voxel = VoxelWorld.Inst.GetVoxelFromCollider(hit.collider as BoxCollider);

            if (voxel != null)
            {
                if (voxel.TypeDef.Type == VoxelType.Weak || voxel.TypeDef.Type == VoxelType.Strong)
                {
                    voxel.TakeDamage(1);
                    GameObject.Destroy(this.gameObject);
                }
            }
        }

        /*
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
        */
    }
}