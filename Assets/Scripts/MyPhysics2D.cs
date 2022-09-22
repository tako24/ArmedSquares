using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPhysics2D : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minGroundNormalY = 0.65f;
    [SerializeField] private float gravityScale;
    
    private ContactFilter2D _contactFilter;
    
    private List<Ray2D> _rays;
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D> (16);
    
    private Rigidbody2D _rb;
    private Vector2 _targetVelocity;
    private Vector2 _velocity;
    private Vector2 _gravityVector;
    private Vector2 _groundNormal;
    private bool _isGrounded;
    private int _currentSpeed;
    private int _gravityVectorsCount;
    
    
    private const float MinMoveDistance = 0.001f;
    private const float ShellRadius = 0.01f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D> ();
        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask (Physics2D.GetLayerCollisionMask (gameObject.layer));
        _contactFilter.useLayerMask = true;
        _gravityVector = -transform.up;
    }

    private void Update()
    {
        _targetVelocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
    }

    private void FixedUpdate()
    {
        _velocity += _gravityVector * (gravityScale * Time.deltaTime);
        _velocity = _targetVelocity;
        Vector2 deltaPosition = _velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2 (_groundNormal.y, -_groundNormal.x);

        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement (move, false);

        move = Vector2.up * deltaPosition.y;

        Movement (move, true);
        
    }
    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > MinMoveDistance) 
        {
            int count = _rb.Cast (move, _contactFilter, _hitBuffer, distance + ShellRadius);
            _hitBufferList.Clear ();
            for (int i = 0; i < count; i++) {
                _hitBufferList.Add (_hitBuffer [i]);
            }

            for (int i = 0; i < _hitBufferList.Count; i++) 
            {
                Vector2 currentNormal = _hitBufferList [i].normal;
                var temp = currentNormal.normalized;
                Debug.Log("x= "+temp.x + " y= " + temp.y);
                if (currentNormal.y > minGroundNormalY) 
                {
                    _isGrounded = true;
                    if (yMovement) 
                    {
                        _groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot (_velocity, currentNormal);
                if (projection < 0) 
                {
                    _velocity = _velocity - projection * currentNormal;
                }

                float modifiedDistance = _hitBufferList [i].distance - ShellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }


        }

        _rb.position = _rb.position + move.normalized * distance;
    }
}
