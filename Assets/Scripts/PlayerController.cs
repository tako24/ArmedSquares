using Mirror;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerController : NetworkBehaviour
{
    private Rigidbody2D _rb;
    private Vector2 _gravityVector;
    private Vector2 _moveVector;
    private bool _isGrounded;
    private int _currentSpeed;
    private int _gravityVectorsCount;
    private List<Ray2D> _rays;

    [SerializeField] private int maxSpeed;
    [SerializeField] private int gravityScale;
    [SerializeField] private int jumpForce;

    // Client input
    private bool _isJump = false;
    private int _moveRight = 0;
    private int _moveUp = 0;

    private bool _lastIsJump = false;
    private int _lastMoveRight = 0;
    private int _lastMoveUp = 0;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _gravityVector = -transform.up;
    }

    [Command]
    private void InputCmd(bool jump, int moveX, int moveY, int scopeX, int scopeY)
    {
        moveX = Mathf.Clamp(moveX, -1, 1);
        moveY = Mathf.Clamp(moveY, -1, 1);
        scopeX = Mathf.Clamp(scopeX, -1, 1);
        scopeY = Mathf.Clamp(scopeY, -1, 1);

        if (jump)
        {
            JumpServer();
        }

        if (moveX == 0 || moveY == 0)
        {
            ResetSpeedAndMoveVector();
        }
        if (moveX != 0)
        {
            MoveRightLeftServer(moveX);
        }
        else if (moveY != 0)
        {
            MoveUpDownServer(moveY);
        }
        // TODO scope
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

        _gravityVectorsCount = 0;
        RaycastHit2D hit;
        for (int i = 0; i < _rays.Count; ++i)
        {
            hit = Physics2D.Raycast(_rays[i].origin, _rays[i].direction, 0.55f);
            if (hit)
            {
                _gravityVectorsCount++;
                _gravityVector = _rays[i].direction;
                if (_gravityVectorsCount > 1)
                    ResetSpeedAndMoveVector();
                _isGrounded = true;
            }
        }
    }

    private void Move(Vector2 moveVector)
    {
        if (_gravityVector.Equals(moveVector))
            return;
        var deltaTime = Time.deltaTime;
        _rb.AddForce(moveVector * _currentSpeed);
        Debug.DrawRay(transform.localPosition, moveVector * _currentSpeed, Color.red);
    }

    [Client]
    public void Jump()
    {
        _isJump = true;
    }

    [Server]
    private void JumpServer()
    {
        if (_gravityVectorsCount > 1)
            return;

        if (_isGrounded)
        {
            _isGrounded = false;
            _rb.AddForce((-_gravityVector) * jumpForce, ForceMode2D.Impulse);
        }
    }

    [Server]
    private void MoveRightLeftServer(int multiplier)
    {
        if (_gravityVectorsCount > 1)
        {
            if (!_gravityVector.Equals(multiplier * transform.right))
            {
                _currentSpeed = maxSpeed;
                _moveVector = multiplier * transform.right;
            }
            return;
        }
        if (!_gravityVector.Equals(multiplier * transform.right) && !(-_gravityVector).Equals(multiplier * transform.right))
        {
            _currentSpeed = maxSpeed;
            _moveVector = multiplier * transform.right;
        }
    }

    [Client]
    public void MoveRight()
    {
        _moveRight = 1;
    }

    [Client]
    public void MoveLeft()
    {
        _moveRight = -1;
    }

    [Client]
    public void ResetMoveMoveRightLeft()
    {
        _moveRight = 0;
    }

    [Server]
    private void MoveUpDownServer(int multiplier)
    {
        if (_gravityVectorsCount > 1)
        {
            if (!_gravityVector.Equals(multiplier * transform.up))
            {
                _currentSpeed = maxSpeed;
                _moveVector = multiplier * transform.up;
            }
            return;
        }
        if (!_gravityVector.Equals(multiplier * transform.up) && !(-_gravityVector).Equals(multiplier * transform.up))
        {
            _currentSpeed = maxSpeed;
            _moveVector = multiplier * transform.up;
        }
    }

    [Client]
    public void MoveUp()
    {
        _moveUp = 1;
    }

    [Client]
    public void MoveDown()
    {
        _moveUp = -1;
    }

    [Client]
    public void ResetMoveMoveUpDown()
    {
        _moveUp = 0;
    }

    public void ResetSpeedAndMoveVector()
    {
        _currentSpeed = 0;
        _moveVector = Vector2.zero;
    }

    private void LateUpdate()
    {
        var currentVelocity = _rb.velocity;
        var constantSpeed = 2;
        var smoothingFactor = 1;
        var tvel = currentVelocity.normalized * constantSpeed;
        _rb.velocity = Vector2.Lerp(currentVelocity, tvel, Time.deltaTime * smoothingFactor);
    }

    private void Update()
    {
        if (isServer)
        {
            Debug.Log(_gravityVectorsCount);
            Move(_moveVector);
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            _moveRight = (int)Input.GetAxis("Horizontal");
            _moveUp = (int)Input.GetAxis("Vertical");
            _isJump = Input.GetKeyDown(KeyCode.Space);
            if (_lastIsJump != _isJump || _lastMoveRight != _moveRight || _lastMoveUp != _moveUp)
            {
                InputCmd(_isJump, _moveRight, _moveUp, 0, 0);
                _isJump = false;
                _lastIsJump = _isJump;
                _lastMoveRight = _moveRight;
                _lastMoveUp = _moveUp;
            }
        }

        if (isServer)
        {
            ChangeGravityVector();
            Debug.DrawRay(transform.localPosition, _gravityVector, Color.blue);
            _rb.AddForce(_gravityVector * gravityScale);
        }
    }
}
