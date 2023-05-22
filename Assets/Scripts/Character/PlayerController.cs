using UnityEngine;

// FPSWalkerEnhanced
// From Unify Community Wiki

// https://wiki.unity3d.com/index.php/FPSWalkerEnhanced#FPSWalkerEnhanced.cs

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("How fast the player moves when walking (default move speed).")]
    [SerializeField]
    private float m_WalkSpeed = 6.0f;

    [Tooltip("How fast the player moves when running.")]
    [SerializeField]
    private float m_RunSpeed = 11.0f;

    [Tooltip("If true, diagonal speed (when strafing + moving forward or back) can't exceed normal move speed; otherwise it's about 1.4 times faster.")]
    [SerializeField]
    public bool m_LimitDiagonalSpeed = true;

    [Tooltip("If checked, the run key toggles between running and walking. Otherwise player runs if the key is held down.")]
    [SerializeField]
    private bool m_ToggleRun = false;

    [Tooltip("How high the player jumps when hitting the jump button.")]
    [SerializeField]
    private float m_JumpSpeed = 8.0f;

    [Tooltip("How fast the player falls when not standing on anything.")]
    [SerializeField]
    private float m_Gravity = 20.0f;

    [Tooltip("Units that player can fall before a falling function is run. To disable, type \"infinity\" in the inspector.")]
    [SerializeField]
    private float m_FallingThreshold = 10.0f;

    [Tooltip("If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down.")]
    [SerializeField]
    private bool m_SlideWhenOverSlopeLimit = false;

    [Tooltip("If checked and the player is on an object tagged \"Slide\", he will slide down it regardless of the slope limit.")]
    [SerializeField]
    private bool m_SlideOnTaggedObjects = false;

    [Tooltip("How fast the player slides when on slopes as defined above.")]
    [SerializeField]
    private float m_SlideSpeed = 12.0f;

    [Tooltip("If checked, then the player can change direction while in the air.")]
    [SerializeField]
    private bool m_AirControl = false;

    [Tooltip("Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast.")]
    [SerializeField]
    private float m_AntiBumpFactor = .75f;

    [Tooltip("Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping.")]
    [SerializeField]
    private int m_AntiBunnyHopFactor = 1;

    //[Tooltip("How hard does objects move horizontally when pushed around by player")]
    //[SerializeField]
    //private float m_PushForceHorizontal = 1f;

    //[Tooltip("How hard does objects lift, when pushed around by player")]
    //[SerializeField]
    //private float m_PushForceLift = 1f;

    private Vector3 m_MoveDirection = Vector3.zero;
    private bool m_Grounded = false;
    private CharacterController m_Controller;
    [HideInInspector] public Transform m_Transform;
    private float m_Speed;
    private RaycastHit m_Hit;
    private float m_FallStartLevel;
    private bool m_Falling;
    private float m_SlideLimit;
    private float m_RayDistance;
    private Vector3 m_ContactPoint;
    private bool m_PlayerControl = false;
    private int m_JumpTimer;


    private void Awake()
    {
        // Saving component references to improve performance.
        m_Transform = GetComponent<Transform>();
        m_Controller = GetComponent<CharacterController>();

        // Setting initial values.
        m_Speed = m_WalkSpeed;
        m_RayDistance = m_Controller.height * .5f + m_Controller.radius;
        m_SlideLimit = m_Controller.slopeLimit - .1f;
        m_JumpTimer = m_AntiBunnyHopFactor;
    }


    private void Update()
    {
        // If the run button is set to toggle, then switch between walk/run speed. (We use Update for this...
        // FixedUpdate is a poor place to use GetButtonDown, since it doesn't necessarily run every frame and can miss the event)

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        if (m_ToggleRun && m_Grounded && Input.GetButtonDown("Run"))
        {
            m_Speed = (m_Speed == m_WalkSpeed ? m_RunSpeed : m_WalkSpeed);
        }

        // If both horizontal and vertical are used simultaneously, limit speed (if allowed), so the total doesn't exceed normal move speed
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && m_LimitDiagonalSpeed) ? .7071f : 1.0f;

        if (m_Grounded)
        {
            bool sliding = false;
            // See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
            // because that interferes with step climbing amongst other annoyances
            if (Physics.Raycast(m_Transform.position, -Vector3.up, out m_Hit, m_RayDistance))
            {
                if (Vector3.Angle(m_Hit.normal, Vector3.up) > m_SlideLimit)
                {
                    sliding = true;
                }
            }
            // However, just raycasting straight down from the center can fail when on steep slopes
            // So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
            else
            {
                Physics.Raycast(m_ContactPoint + Vector3.up, -Vector3.up, out m_Hit);
                if (Vector3.Angle(m_Hit.normal, Vector3.up) > m_SlideLimit)
                {
                    sliding = true;
                }
            }

            // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
            if (m_Falling)
            {
                m_Falling = false;
                if (m_Transform.position.y < m_FallStartLevel - m_FallingThreshold)
                {
                    OnFell(m_FallStartLevel - m_Transform.position.y);
                }
            }

            // If running isn't on a toggle, then use the appropriate speed depending on whether the run button is down
            if (!m_ToggleRun)
            {
                m_Speed = Input.GetKey(KeyCode.LeftShift) ? m_RunSpeed : m_WalkSpeed;
            }

            // If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on
            if ((sliding && m_SlideWhenOverSlopeLimit) || (m_SlideOnTaggedObjects && m_Hit.collider.tag == "Slide"))
            {
                Vector3 hitNormal = m_Hit.normal;
                m_MoveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                Vector3.OrthoNormalize(ref hitNormal, ref m_MoveDirection);
                m_MoveDirection *= m_SlideSpeed;
                m_PlayerControl = false;
            }
            // Otherwise recalculate moveDirection directly from axes, adding a bit of -y to avoid bumping down inclines
            else
            {
                m_MoveDirection = new Vector3(inputX * inputModifyFactor, -m_AntiBumpFactor, inputY * inputModifyFactor);
                m_MoveDirection = m_Transform.TransformDirection(m_MoveDirection) * m_Speed;
                m_PlayerControl = true;
            }

            // Jump! But only if the jump button has been released and player has been grounded for a given number of frames
            if (!Input.GetButton("Jump"))
            {
                m_JumpTimer++;
            }
            else if (m_JumpTimer >= m_AntiBunnyHopFactor)
            {
                m_MoveDirection.y = m_JumpSpeed;
                m_JumpTimer = 0;
            }
        }
        else
        {
            // If we stepped over a cliff or something, set the height at which we started falling
            if (!m_Falling)
            {
                m_Falling = true;
                m_FallStartLevel = m_Transform.position.y;
            }

            // If air control is allowed, check movement but don't touch the y component
            if (m_AirControl && m_PlayerControl)
            {
                m_MoveDirection.x = inputX * m_Speed * inputModifyFactor;
                m_MoveDirection.z = inputY * m_Speed * inputModifyFactor;
                m_MoveDirection = m_Transform.TransformDirection(m_MoveDirection);
            }
        }

        // Apply gravity
        m_MoveDirection.y -= m_Gravity * Time.deltaTime;

        // Move the controller, and set grounded true or false depending on whether we're standing on something
        m_Grounded = (m_Controller.Move(m_MoveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }


    // Store point that we're in contact with for use in FixedUpdate if needed
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //if (hit.gameObject.layer == Consts.interactableLayer)
        //{
        //    print("hit");
        //    Rigidbody rb = hit.gameObject.GetComponent<Rigidbody>();
        //    if (rb != null)
        //    {
        //        Vector3 force = hit.point - transform.position;
        //        force.y = 0f;
        //        force.Normalize();
        //        force *= m_PushForceHorizontal;
        //        force.y = m_PushForceLift;
        //        rb.AddForce(force, ForceMode.VelocityChange);
        //    }
        //}
        m_ContactPoint = hit.point;
    }


    // This is the place to apply things like fall damage. You can give the player hitpoints and remove some
    // of them based on the distance fallen, play sound effects, etc.
    private void OnFell(float fallDistance)
    {
        print("Ouch! Fell " + fallDistance + " units!");
    }

    #region Serialization

    public WorldPlacementData GetData()
    {
        return new WorldPlacementData(m_Transform);
    }

    public void SetData(WorldPlacementData data)
    {
        print("POS " + data.position[0] + " " + data.position[1] + " " + data.position[2]);
        m_Transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
        m_Transform.rotation = Quaternion.Euler(data.rotation[0], data.rotation[1], data.rotation[2]);
        m_Transform.localScale = new Vector3(data.scale[0], data.scale[1], data.scale[2]);
    }

    #endregion
}