using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector2 _gravityVector;
    private Vector2 _moveVector;
    private bool _isGrounded;
    private int _currentSpeed;


    private List<Ray2D> _rays;
    private List<Vector2> _gravityVectors;


    [SerializeField]
    private int maxSpeed ;
    [SerializeField]
    private int gravityScale ;
    [SerializeField] 
    private int jumpForce;


    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _gravityVector = -transform.up;
        _gravityVectors = new List<Vector2>();
    }
    private void ChangeGravityVector()
    {
        _isGrounded = false;
        
        _rays = new List<Ray2D>{
            new Ray2D(transform.localPosition, transform.up),
            new Ray2D(transform.localPosition, -transform.up),
            new Ray2D(transform.localPosition, transform.right),
            new Ray2D(transform.localPosition, -transform.right)
        };
        _gravityVectors.Clear();

        RaycastHit2D hit ;
        
        for (int i = 0; i < _rays.Count; ++i)
        {
            hit = Physics2D.Raycast(_rays[i].origin, _rays[i].direction,0.55f);
            if(hit)
            {
                
                _gravityVector = _rays[i].direction;
                _gravityVectors.Add(_rays[i].direction);
                if (_gravityVectors.Count>1)
                    ResetSpeedAndMoveVector();
                _isGrounded = true;
               // Debug.DrawRay(_rays[i].origin, _gravityVector , Color.blue);
            }
        }
    }
    
    private void Move(float x, float y)
    {
        var deltaTime = Time.deltaTime;
        transform.position += new Vector3(
            x * deltaTime * _currentSpeed,
            y * deltaTime * _currentSpeed,
            0
        );
        //_rb.AddForce(new Vector2(x,y)*_maxSpeed);
    }
    private void Move(Vector2 moveVector)
    {

        if(_gravityVector.Equals(moveVector))
            return;
        var deltaTime = Time.deltaTime;
        transform.position += new Vector3(
            moveVector.x * deltaTime * _currentSpeed,
            moveVector.y * deltaTime * _currentSpeed,
            0 
        );

        //_rb.AddForce(moveVector * _currentSpeed);
        Debug.DrawRay ( transform.localPosition, moveVector*_currentSpeed ,Color.red );
    }

    public void Jump()
    {
        if (_gravityVectors.Count >1)
            return;
        
        if (_isGrounded)
        {
            _isGrounded = false;
            _rb.AddForce((-_gravityVector + _moveVector) * jumpForce, ForceMode2D.Impulse);
        }
    }

    public void MoveRight()
    {
        if(_gravityVectors.Count>1)
        {
            foreach (var gravityVector in _gravityVectors)
            {
                if (gravityVector.Equals(transform.right))
                    return;
            }
            _currentSpeed = maxSpeed;
            _moveVector = transform.right;
        }
        
        if (_gravityVector.Equals(transform.right) || (-_gravityVector).Equals(transform.right) ) return;
        _currentSpeed = maxSpeed;
        _moveVector = transform.right;
    }
    
    public void MoveLeft()
    {
        if(_gravityVectors.Count>1)
        {
            foreach (var gravityVector in _gravityVectors)
            {
                if (gravityVector.Equals(-transform.right))
                    return;
            }
            _currentSpeed = maxSpeed;
            _moveVector = -transform.right;
        }
        
        if (_gravityVector.Equals(-transform.right) || (-_gravityVector).Equals(-transform.right) ) return;
        _currentSpeed = maxSpeed;
        _moveVector = -transform.right;
    }
    
    public void MoveUp()
    {
        if(_gravityVectors.Count>1)
        {
            foreach (var gravityVector in _gravityVectors)
            {
                if (gravityVector.Equals(transform.up))
                    return;
            }
            _currentSpeed = maxSpeed;
            _moveVector = transform.up;
        }
        
        if (_gravityVector.Equals(transform.up) || (-_gravityVector).Equals(transform.up) ) return;
        _currentSpeed = maxSpeed;
        _moveVector = transform.up;
    }
    
    public void MoveDown()
    {
        if(_gravityVectors.Count>1)
        {
            foreach (var gravityVector in _gravityVectors)
            {
                if (gravityVector.Equals(-transform.up))
                    return;
            }
            _currentSpeed = maxSpeed;
            _moveVector = -transform.up;
        }
        
        if (_gravityVector.Equals(-transform.up) || (-_gravityVector).Equals(-transform.up) ) return;
        _currentSpeed = maxSpeed;
        _moveVector = -transform.up;
    }
    
    public void ResetSpeedAndMoveVector()
    {
        _currentSpeed = 0;
        _moveVector = Vector2.zero;
    } 

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        Move(_moveVector);
    }

    private void FixedUpdate()
    {
        ChangeGravityVector();
        Debug.DrawRay(transform.localPosition, _gravityVector , Color.blue);
        _rb.AddForce(_gravityVector*gravityScale);
    }
}