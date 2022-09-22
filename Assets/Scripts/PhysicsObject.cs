using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    public float minGroundNormalY = .65f;
    public float gravityModifier = 1f;

    protected Vector2 targetVelocity;
    protected bool grounded;
    protected Vector2 groundNormal;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D> (16);


    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;
    
    private List<Ray2D> _rays;
    private int _gravityVectorsCount;
    private Vector2 _gravityVector;
    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D> ();
    }

    void Start () 
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask (Physics2D.GetLayerCollisionMask (gameObject.layer));
        contactFilter.useLayerMask = true;
        _gravityVector = -transform.up;
    }

    void Update () 
    {
        targetVelocity = new Vector2(Input.GetAxis ("Horizontal"),0);

        if (Input.GetButtonDown ("Jump") && grounded) 
            velocity.y = 5;  
    }

    protected virtual void ComputeVelocity()
    {

    }
    
    private void ChangeGravityVector()
    {
        grounded = false;
        
        _rays = new List<Ray2D>{
            new Ray2D(transform.localPosition, transform.up),
            new Ray2D(transform.localPosition, -transform.up),
            new Ray2D(transform.localPosition, transform.right),
            new Ray2D(transform.localPosition, -transform.right)
        };
        
        _gravityVectorsCount = 0;
        
        RaycastHit2D hit ;
        
        for (int i = 0; i < _rays.Count; ++i)
        {
            hit = Physics2D.Raycast(_rays[i].origin, _rays[i].direction,0.55f);
            if(hit)
            {
                
                _gravityVectorsCount++;
                _gravityVector = _rays[i].direction;
                // if (_gravityVectorsCount>1)
                //     ResetSpeedAndMoveVector();
                grounded = true;
                // Debug.DrawRay(_rays[i].origin, _gravityVector , Color.blue);
            }
        }
    }

    void FixedUpdate()
    {
        //ChangeGravityVector();
        velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
        velocity.x = targetVelocity.x;

        grounded = false;

        Vector2 deltaPosition = velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2 (groundNormal.y, -groundNormal.x);

        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement (move, false);

        move = Vector2.up * deltaPosition.y;

        Movement (move, true);
    }

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance) 
        {
            int count = rb2d.Cast (move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear ();
            for (int i = 0; i < count; i++) {
                hitBufferList.Add (hitBuffer [i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++) 
            {
                Vector2 currentNormal = hitBufferList [i].normal;
                if (currentNormal.y > minGroundNormalY) 
                {
                    grounded = true;
                    if (yMovement) 
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot (velocity, currentNormal);
                if (projection < 0) 
                {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hitBufferList [i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }


        }

        rb2d.position = rb2d.position + move.normalized * distance;
    }

}