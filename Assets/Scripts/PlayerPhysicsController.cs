using UnityEngine;

public class PlayerPhysicsController : PhysicsObject
{
    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;

    //private SpriteRenderer spriteRenderer;
    //private Animator animator;

    void Awake()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
        //animator = GetComponent<Animator>();
        debugLastG = isGrounded;
    }

    bool debugLastG;
    protected override void ComputeVelocity()
    {
        if (debugLastG != isGrounded)
        {
            Debug.Log("isGrounded " + isGrounded);
            debugLastG = isGrounded;
        }
        if(Input.GetKey(KeyCode.LeftArrow))
            SetGravity(new Vector2(Physics2D.gravity.y, 0));
        else if (Input.GetKey(KeyCode.DownArrow))
            SetGravity(Physics2D.gravity);
        else if (Input.GetKey(KeyCode.RightArrow))
            SetGravity(new Vector2(-Physics2D.gravity.y, 0));
        else if (Input.GetKey(KeyCode.UpArrow))
            SetGravity(-Physics2D.gravity);


        Vector2 move = new Vector2(
            gravity.y * -Input.GetAxis("Horizontal"),
            gravity.x * Input.GetAxis("Vertical"));
        move.Normalize();
        float jumpSign = isMoveUpDown ? Mathf.Sign(-gravity.y) : Mathf.Sign(-gravity.x);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (isMoveUpDown)
                velocity.y = jumpTakeOffSpeed * jumpSign;
            else
                velocity.x = jumpTakeOffSpeed * jumpSign;
        }
        
        
        //bool flipSprite = (spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < 0.01f));
        //if (flipSprite)
        //{
        //    spriteRenderer.flipX = !spriteRenderer.flipX;
        //}

        //animator.SetBool("grounded", grounded);
        //animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

        targetVelocity = move * maxSpeed;
    }

}
