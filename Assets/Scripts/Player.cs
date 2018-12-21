using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public System.Action BallThrown;


    //References
    [Header("References")]
    public Rigidbody2D rigibdoy;
    public Transform throwPosition;
    public SpriteRenderer sprite;
    public GameObject ballDetector;
    public Animator animator;

    [Header("Data")]
    public uint number = 0;

    [Header("Movement")]
    public float movementSpeed = 10.0f;
    public float maxVelocityX = 400.0f;
    public float jumpForce = 10.0f;
    public float jumpMomentum = 0.0f;
    public float jumpDumping = 0.5f;
    public float inAirModifier = 0.7f;
    public int maxJumpTimeFrames = 40;
    private int flip = 1;
    private int jumpFrames = 0;
    

    [Header("Throwing")]
    public Vector2 throwForce = new Vector2();
    public float torque = 0.0f;

    [Header("Debug")]
    public bool infiniteBalls = false;


    private bool jumping = false;
    private bool throwing = false;

    private IEnumerator EnableBallPickupCoroutineREF;



    private bool HasBall
    {
        get
        {
            return infiniteBalls || hasBall;
        }

        set
        {
            hasBall = value;
            animator.SetBool("HasBall", value);
        }
    }

    public int Flip
    {
        get
        {
            return flip;
        }

        set
        {
            flip = value;
            sprite.flipX = value == 1 ? false : true;
        }
    }

    public bool Jumping
    {
        get
        {
            return jumping;
        }

        set
        {
            jumping = value;
            animator.SetBool("IsJumping", value);
        }
    }

    private bool hasBall = true;



    // Use this for initialization
    void Start () {
        ballDetector.GetComponent<BallDetetor>().balldetected += () => { HasBall = true; };
	}
	
	// Update is called once per frame
	void Update () {
        UpdateThrowing();

    }

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        float horizontal = GameInput.instance.GetAxis(GameAxis.X_MOVEMENT, (int)number);
        Vector2 moveVector = new Vector2();
        if (horizontal != 0)
        {
            float absXValue = Mathf.Abs(throwPosition.localPosition.x);
            Flip = horizontal > 0 ? 1 : -1;
            throwPosition.localPosition = new Vector3(absXValue * Flip, throwPosition.localPosition.y, throwPosition.localPosition.z);

            if (Jumping)
            {
                moveVector.x = horizontal * movementSpeed * Time.deltaTime * inAirModifier;
            }
            else
            {
                moveVector.x = horizontal * movementSpeed * Time.deltaTime;
            }
            animator.SetBool("IsMoving",true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        if (Jumping)
        {
            moveVector.y = jumpMomentum * Time.deltaTime;
            if(GameInput.instance.GetButton(GameButtons.JUMP, (int)number) && jumpFrames < maxJumpTimeFrames)
            {
                jumpFrames++;
            }
            else
            {
                jumpMomentum -= jumpDumping;
            }

            
        }
        if (!Jumping && GameInput.instance.GetButton(GameButtons.JUMP, (int)number))
        {
            Jumping = true;
            jumpFrames++;
            jumpMomentum = jumpForce;
            //Vector2 forceVector = new Vector2(0.0f, jumpForce);
            //rigibdoy.AddForce(forceVector);
        }
        rigibdoy.MovePosition(rigibdoy.position + moveVector);

    }


    private void UpdateThrowing()
    {
        if (GameInput.instance.GetButtonPressed(GameButtons.THROW, (int)number) && HasBall)
        {
            ThrowBall();
        }
    }

    void ThrowBall()
    {
        Vector2 temp = throwForce;
        temp.x *= Flip;
        UniverseManager.instance.SpawnBall(throwPosition.position, temp, torque * Flip);
        HasBall = false;
        DisableBallDetector();
        if (BallThrown != null)
            BallThrown();
    }

    void DisableBallDetector()
    {
        ballDetector.SetActive(false);
        if (EnableBallPickupCoroutineREF != null)
            StopCoroutine(EnableBallPickupCoroutineREF);
        EnableBallPickupCoroutineREF = EnableBallPickupCoroutine();
        StartCoroutine(EnableBallPickupCoroutineREF);
    }

    IEnumerator EnableBallPickupCoroutine()
    {
        yield return new WaitForSeconds(0.25f);
        ballDetector.SetActive(true);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.LogFormat("Collision: {0}", collision.gameObject.name);
        if (collision.gameObject.CompareTag("Ground"))
        {
            Jumping = false;
            jumpFrames = 0;
        }
    }
}
