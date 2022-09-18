using UnityEngine;

using System.Collections;
public class PhysicsObject : MonoBehaviour
{
    public float minGroundNormalY = 0.65f;
    public float gravityModifier = 1.0f;

    protected Vector2 gravity;
    protected Vector2 targetVelocity;
    protected bool isGrounded;
    protected Vector2 groundNormal;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
        //SetGravity(new Vector2(Physics2D.gravity.y, 0));
        SetGravity(Physics2D.gravity);
    }

    public void SetGravity(Vector2 newGravity)
    {
        gravity = newGravity;
        bool isMoveUpDown = gravity.x == 0;
        if (isMoveUpDown)
            velocity.y = gravity.y;
        else
            velocity.x = gravity.x;
    }

    void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    void Update()
    {
        Debug.Log("Gravity " + gravity);
        Debug.Log("TargetVelocity " + targetVelocity);
        Debug.Log("Velocity " + velocity);
        Debug.Log("isGrounded " + isGrounded);
        Debug.Log("GroundNormal " + groundNormal);
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {
    }

    void FixedUpdate()
    {
        bool isMoveUpDown = gravity.x == 0;
        velocity += gravityModifier * gravity * Time.deltaTime;
        if (isMoveUpDown)
        {
            //velocity.y = Mathf.Clamp(velocity.y + gravity.y, -9.81f, 9.81f);
            velocity.x = targetVelocity.x;
        }
        else
        {
            //velocity.x = Mathf.Clamp(gravity.x, -9.81f, 9.81f);
            velocity.y = targetVelocity.y;
        }
        isGrounded = false;
        Vector2 deltaPosition = velocity * Time.deltaTime;
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);
        Vector2 move = isMoveUpDown ? moveAlongGround * deltaPosition.x : moveAlongGround * deltaPosition.y;
        Movement(move, false);
        move = isMoveUpDown ? Vector2.up * deltaPosition.y : Vector3.right * deltaPosition.x;
        Movement(move, true);
        velocity = new Vector2(Mathf.Clamp(velocity.x, -7, 7), Mathf.Clamp(velocity.y, -7, 7));
    }

    void Movement(Vector2 move, bool isJumpMovement)
    {
        float moveDistance = move.magnitude;
        if (moveDistance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, moveDistance + shellRadius);
            isGrounded = count != 0;
            for (int i = 0; i < count; ++i)
            {
                if (isJumpMovement)
                {
                    Vector2 currentNormal = hitBuffer[i].normal;
                    //gravity = -currentNormal * gravity;
                    groundNormal = currentNormal;
                }
                float modifiedDistance = hitBuffer[i].distance - shellRadius;
                moveDistance = modifiedDistance < moveDistance ? modifiedDistance : moveDistance;
            }
        }
        rb2d.position += move.normalized * moveDistance;
    }
}
