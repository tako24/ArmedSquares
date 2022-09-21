using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    BoxCollider2D _collider;
    private Rigidbody2D _rb;
    private int _gravityVectorsCount;
    private Vector2[] _gravityVectors = new Vector2[4];
    private Vector2 _moveVector;
    private int _currentSpeed;
    private RaycastHit2D[] _rayCasts = new RaycastHit2D[8];

    [SerializeField] private int maxSpeed;
    [SerializeField] private int gravityScale;
    [SerializeField] private int jumpForce;
    [SerializeField] private float _raycastDistance = 0.1f;

    // Client input
    private bool _isJump = false;
    private int _moveRight = 0;
    private int _moveUp = 0;

    private bool _lastIsJump = false;
    private int _lastMoveRight = 0;
    private int _lastMoveUp = 0;

    void Start()
    {
        if (isServer == false) return;
        _collider = GetComponent<BoxCollider2D>();
        _rb = GetComponent<Rigidbody2D>();
        _gravityVectors[0] = -transform.up;
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
        else
        {
            ResetSpeedAndMoveVector();
        }

        if (moveX != 0)
        {
            MoveRightLeftServer(moveX);
        }
        if (moveY != 0)
        {
            MoveUpDownServer(moveY);
        }
        // TODO scope
    }

    private void ChangeGravityVector()
    {
        _gravityVectorsCount = 0;
        var half = 0.45f;
        var m1 = 0.95f;
        _rayCasts[0] = Physics2D.Raycast(_collider.bounds.center + (transform.up + transform.right * m1) * half, transform.up, _raycastDistance);
        _rayCasts[1] = Physics2D.Raycast(_collider.bounds.center + (transform.up - transform.right * m1) * half, transform.up, _raycastDistance);
        _rayCasts[2] = Physics2D.Raycast(_collider.bounds.center - (transform.up + transform.right * m1) * half, -transform.up, _raycastDistance);
        _rayCasts[3] = Physics2D.Raycast(_collider.bounds.center - (transform.up - transform.right * m1) * half, -transform.up, _raycastDistance);
        _rayCasts[4] = Physics2D.Raycast(_collider.bounds.center + (transform.right + transform.up * m1) * half, transform.right, _raycastDistance);
        _rayCasts[5] = Physics2D.Raycast(_collider.bounds.center + (transform.right - transform.up * m1) * half, transform.right, _raycastDistance);
        _rayCasts[6] = Physics2D.Raycast(_collider.bounds.center - (transform.right + transform.up * m1) * half, -transform.right, _raycastDistance);
        _rayCasts[7] = Physics2D.Raycast(_collider.bounds.center - (transform.right - transform.up * m1) * half, -transform.right, _raycastDistance);
        Debug.DrawRay(_collider.bounds.center + (transform.up + transform.right * m1) * half, transform.up * _raycastDistance, Color.red);
        Debug.DrawRay(_collider.bounds.center + (transform.up - transform.right * m1) * half, transform.up * _raycastDistance, Color.red);
        Debug.DrawRay(_collider.bounds.center - (transform.up + transform.right * m1) * half, -transform.up * _raycastDistance, Color.green);
        Debug.DrawRay(_collider.bounds.center - (transform.up - transform.right * m1) * half, -transform.up * _raycastDistance, Color.green);
        Debug.DrawRay(_collider.bounds.center + (transform.right + transform.up * m1) * half, transform.right * _raycastDistance, Color.blue);
        Debug.DrawRay(_collider.bounds.center + (transform.right - transform.up * m1) * half, transform.right * _raycastDistance, Color.blue);
        Debug.DrawRay(_collider.bounds.center - (transform.right + transform.up * m1) * half, -transform.right * _raycastDistance, Color.magenta);
        Debug.DrawRay(_collider.bounds.center - (transform.right - transform.up * m1) * half, -transform.right * _raycastDistance, Color.magenta);

        for (int gravityIndex = 0, i = 0; i < _rayCasts.Length; i += 2, ++gravityIndex)
        {
            if (_rayCasts[i] || _rayCasts[i + 1])
            {
                //if (_gravityVectorsCount == 0)
                if (_rayCasts[i].normal.x != 0 || _rayCasts[i].normal.y != 0)
                    _gravityVectors[gravityIndex] = -_rayCasts[i].normal;
                else
                    _gravityVectors[gravityIndex] = -_rayCasts[i + 1].normal;
                ++_gravityVectorsCount;
            }
        }

        {
            Debug.Log("up    " + ((bool)_rayCasts[0] || (bool)_rayCasts[1]).ToString());
            Debug.Log("down  " + ((bool)_rayCasts[2] || (bool)_rayCasts[3]).ToString());
            Debug.Log("right " + ((bool)_rayCasts[4] || (bool)_rayCasts[5]).ToString());
            Debug.Log("left  " + ((bool)_rayCasts[6] || (bool)_rayCasts[7]).ToString());
            Debug.Log("gravity count " + _gravityVectorsCount.ToString());
            for(int i = 0; i < _gravityVectorsCount; ++i)
                Debug.Log("gravity " + i.ToString() + " " + _gravityVectors[i].ToString());
        }
    }

    private bool HasGravity(Vector2 gravity)
    {
        for (int i = 0; i < _gravityVectorsCount; ++i)
            if (_gravityVectors[i] == gravity)
                return true;
        return false;
    }

    private void Move()
    {
        //if (_gravityVector == _moveVector) return;
        if(HasGravity(_moveVector)) return;
        var deltaTime = Time.deltaTime;
        transform.position += new Vector3(
            _moveVector.x * deltaTime * _currentSpeed,
            _moveVector.y * deltaTime * _currentSpeed,
            0
        );

        //Vector3 targetVelocity = new Vector2(_moveVector.x * _currentSpeed + _rb.velocity.x, _moveVector.y * _currentSpeed + _rb.velocity.y);
        //var m_Velocity = Vector2.zero;
        //var m_MovementSmoothing = Time.deltaTime;
        //_rb.velocity = Vector2.SmoothDamp(_rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
    }

    [Client]
    public void Jump()
    {
        _isJump = true;
    }

    [Server]
    private void JumpServer()
    {
        if (_gravityVectorsCount != 1) return;
        _rb.AddForce(-_gravityVectors[0] * jumpForce, ForceMode2D.Impulse);
    }

    [Server]
    private void MoveRightLeftServer(int multiplier) => ChangeMoveVector(multiplier * transform.right);

    [Client]
    public void MoveRight() => _moveRight = 1;

    [Client]
    public void MoveLeft() => _moveRight = -1;

    [Client]
    public void ResetMoveMoveRightLeft() => _moveRight = 0;

    [Server]
    private void MoveUpDownServer(int multiplier) => ChangeMoveVector(multiplier * transform.up);

    [Server]
    private void ChangeMoveVector(Vector2 direction)
    {
        //if (_gravityVectorsCount > 1 || _gravityVectors[0].Abs() != direction.Abs())
        {
            _currentSpeed = maxSpeed;
            _moveVector = direction;
        }
    }

    [Client]
    public void MoveUp() => _moveUp = 1;

    [Client]
    public void MoveDown() => _moveUp = -1;

    [Client]
    public void ResetMoveMoveUpDown() => _moveUp = 0;

    public void ResetSpeedAndMoveVector()
    {
        if (_gravityVectorsCount != 0)
            _rb.velocity = new Vector2(0, 0);
        _currentSpeed = 0;
        _moveVector = Vector2.zero;
    }

    private void LateUpdate()
    {
        _rb.velocity = _rb.velocity.normalized * maxSpeed;
    }

    private void Update()
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
    }

    private void FixedUpdate()
    {
        if (isServer)
        {
            ChangeGravityVector();
            Move();
            _rb.AddForce(_gravityVectors[0] * gravityScale);
        }
    }
}

