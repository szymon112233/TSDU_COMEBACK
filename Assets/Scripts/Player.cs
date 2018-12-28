using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public System.Action BallThrown;

    [Header("Debug")]
    public bool infiniteBalls = false;

    [Header("References")]
    public Rigidbody2D rigibdoy;
    public Transform throwPosition;
    public SpriteRenderer sprite;
    public GameObject ballDetector;
    public GameObject handHitCollider;
    public GameObject handHitAirCollider;
    public Animator animator;
    public UnityEngine.UI.Image powerBar;

    [Header("Data")]
    public uint number = 0;

    [Header("Movement")]
    public float movementSpeed = 10.0f;
    public float jumpForce = 10.0f;
    public float jumpMomentum = 0.0f;
    public float jumpDumping = 0.5f;
    public float inAirModifier = 0.7f;
    public int maxJumpTimeFrames = 40;
    private int flip = 1;
    private int jumpFrames = 0;
    private bool jumping = false;

    [Header("Throwing")]
    public AnimationCurve throwForceCurveX;
    public AnimationCurve throwForceCurveY;
    public AnimationCurve torqueCurve;
    public float lerpTime = 1.0f;
    private Vector2 throwForce = new Vector2();
    private float lerpTimer = 0.0f;
    private float torque = 0.0f;
    private bool throwing = false;
    private bool lerpDir = true;
    private bool hasBall = true;

    private bool hitting = false;

    private IEnumerator EnableBallPickupCoroutineREF;
    private IEnumerator DisableHitStateCoroutineREF;

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

    public bool Throwing
    {
        get
        {
            return throwing;
        }

        set
        {
            animator.SetBool("IsThrowing", value);
            throwing = value;
        }
    }

    public bool Hitting
    {
        get
        {
            return hitting;
        }

        set
        {
            hitting = value;
            animator.SetBool("IsHitting", value);
        }
    }

    // Use this for initialization
    void Start () {
        ballDetector.GetComponent<BallDetetor>().balldetected += () => { HasBall = true; };
	}
	
	// Update is called once per frame
	void Update () {
        UpdateAction();
        UpdateClothMask();
    }

    private void UpdateClothMask()
    {

    }

    public void GetHit(int side)
    {
        if (HasBall)
        {
            HasBall = false;
            Throwing = false;
            powerBar.gameObject.SetActive(false);
            DisableBallDetector();
            UniverseManager.instance.SpawnBall(gameObject.transform.position,new Vector2(150 * side, 150));
        }
    }


    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        float horizontal = GameInput.instance.GetAxis(GameAxis.X_MOVEMENT, (int)number);
        Vector2 moveVector = new Vector2();
        if (horizontal != 0 && !Hitting && !Throwing)
        {
            float absXValue = Mathf.Abs(throwPosition.localPosition.x);
            Flip = horizontal > 0 ? 1 : -1;
            throwPosition.localPosition = new Vector3(absXValue * Flip, throwPosition.localPosition.y, throwPosition.localPosition.z);
            handHitCollider.transform.localScale = new Vector3(Flip, 1, 1);
            handHitAirCollider.transform.localScale = new Vector3(Flip, 1, 1);

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

    private void UpdateAction()
    {
        if (HasBall)
        {
            if (GameInput.instance.GetButtonPressed(GameButtons.ACTION, (int)number))
            {
                powerBar.fillAmount = 0;
                powerBar.gameObject.SetActive(true);
                animator.ResetTrigger("BallThrown");
                Throwing = true;
            }
            else if (GameInput.instance.GetButton(GameButtons.ACTION, (int)number))
            {
                throwForce.x = throwForceCurveX.Evaluate(lerpTimer / lerpTime * 100.0f);
                throwForce.y = throwForceCurveY.Evaluate(lerpTimer / lerpTime * 100.0f);
                torque = torqueCurve.Evaluate(lerpTimer / lerpTime * 100.0f);
                powerBar.fillAmount = lerpTimer / lerpTime;
                if (lerpDir)
                {
                    lerpTimer += Time.deltaTime;
                    if (lerpTimer > lerpTime)
                        lerpDir = !lerpDir;
                }
                else
                {
                    lerpTimer -= Time.deltaTime;
                    if (lerpTimer < 0)
                        lerpDir = !lerpDir;
                }

            }
            else if (GameInput.instance.GetButtonReleased(GameButtons.ACTION, (int)number))
            {
                powerBar.gameObject.SetActive(false);
                Throwing = false;
                ThrowBall();
            }
        }
        else
        {
            if (GameInput.instance.GetButtonPressed(GameButtons.ACTION, (int)number) && !Hitting)
            {
                if (Jumping)
                {
                    handHitAirCollider.SetActive(true);
                }
                else
                {
                    handHitCollider.SetActive(true);
                }
                Hitting = true;
                if (DisableHitStateCoroutineREF != null)
                    StopCoroutine(DisableHitStateCoroutineREF);
                DisableHitStateCoroutineREF = DisableHitStateCoroutine();
                StartCoroutine(DisableHitStateCoroutineREF);
            }
        }
        
    }

    void ThrowBall()
    {
        Debug.LogFormat("Throw Force: [{0}|{1}]", throwForce.x, throwForce.y);
        Vector2 temp = throwForce;
        temp.x *= Flip;
        UniverseManager.instance.SpawnBall(throwPosition.position, temp, torque * Flip);
        HasBall = false;
        DisableBallDetector();
        lerpTimer = 0;
        animator.SetTrigger("BallThrown");
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

    IEnumerator DisableHitStateCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        Hitting = false;
        handHitAirCollider.SetActive(false);
        handHitCollider.SetActive(false);
        DisableHitStateCoroutineREF = null;
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
