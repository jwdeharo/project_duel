using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private PlayerInputActions			InputActions;					//!< Input actions manager.
	private CollisionManager			CollisionManager;				//!< Manager of the collisions.
	private Rigidbody2D					MyRigidBody;					//!< Rigidbody component.
	private Animator					MyAnimator;						//!< Animator component.
	private SpriteRenderer				MyRender;						//!< Render component.
	private bool						JumpButtonHasBeenPressed;		//!< Indicates that the jump button has been pressed.
	private bool						IsJumping;						//!< Indicates if the object is jumping.
	private bool						HasJumpedInWall;				//!< Indicates if the object has jumped while in the wall.
	private bool						CanMove;						//!< Indicates if the object has jumped while in the wall.
	private bool						HasDashed;						//!< Indicates if the object has dashed.
	private Collider2D					MyCollider;						//!< Collider of the object.
	private Vector2						MyInput;						//!< Container that contains the input x and y values.
	private float						OriginalGravityScale;			//!< Original gravity scale of the rigid body.
	[SerializeField] private LayerMask	GroundMask			= 0;		//!< Layer that indicates that the object will be detected as ground.
	[SerializeField] private float		FallMultiplier		= 0.0f;		//!< Value used when you are holding the jump button.
	[SerializeField] private float		LowJumpMultiplier	= 0.0f;		//!< Value used when you just pressed once the jump button.
	[SerializeField] private float		JumpSpeed			= 0.0f;		//!< Jump speed.
	[SerializeField] private float		WallJumpSpeed		= 0.0f;		//!< Jump speed.
	[SerializeField] private float		MovementSpeed		= 0.0f;		//!< Movement Speed.
	[SerializeField] private float		SlideSpeed			= 0.0f;		//!< Movement Speed.
	[SerializeField] private float		DashSpeed			= 0.0f;		//!< Movement Speed.

	/**
	* Start is called before the first frame update
	*/
	public void Start()
	{
		InputActions.Land.Jump.started		+= _ => SetJumpHasBeenPressed(true);
		InputActions.Land.Jump.canceled		+= _ => SetJumpHasBeenPressed(false);
		InputActions.Land.Jump.performed	+= _ => Jump();
		InputActions.Land.Dash.performed	+= _ => Dash();
	}

	/**
	* Disables the movement during a certain time.
	* @param aTime: time that the movement will be disabled.
	*/
	IEnumerator DisableMovement(float aTime)
	{
		CanMove = false;
		yield return new WaitForSeconds(aTime);
		CanMove = true;
	}

	/**
	* Update is called once per frame
	*/
	public void Update()
	{
		MyInput.x = InputActions.Land.Move.ReadValue<float>();
		MyInput.y = InputActions.Land.MoveY.ReadValue<float>();

		if (MyInput.x != 0.0f)
		{
			MyRender.flipX = MyInput.x < 0.0f;
		}

		MyAnimator.SetBool("is_walking", MyInput.x != 0.0f);

		if (CanMove && !HasDashed)
		{
			if (HasJumpedInWall)
			{
				MovePlayerInWall(MyInput.x);
			}
			else
			{
				MovePlayer(new Vector2(MyInput.x * MovementSpeed, MyRigidBody.velocity.y));

				if (!CollisionManager.IsGrounded() && CollisionManager.IsOnWall())
				{
					MovePlayer(new Vector2(MyRigidBody.velocity.x, -SlideSpeed));
					MyAnimator.SetBool("is_sliding", true);
				}
				else
				{
					MyAnimator.SetBool("is_sliding", false);
				}
			}
		}

		if (!HasDashed)
		{
			ManageJump();
		}
	}

	/**
	 * Function that moves the player.
	 * @param aInputValue: input value.
	 */
	private void MovePlayer(Vector2 aVelocity)
	{
		MyRigidBody.velocity = aVelocity;
	}

	/**
	* Moves the player while he's in the wall.
	* @param aMovementInput: input axis.
	*/
	private void MovePlayerInWall(float aMovementInput)
	{
		MovePlayer(Vector2.Lerp((new Vector2(aMovementInput * MovementSpeed, MyRigidBody.velocity.y)), MyRigidBody.velocity, 10.0f * Time.deltaTime));
		if (CollisionManager.IsGrounded() || CollisionManager.IsOnWall())
		{
			HasJumpedInWall = false;
		}
	}

	/**
	* Sets if the jump button has been pressed or released.
	* @param aJumpHasBeenPressed: true if pressed, false if released.
	*/
	private void SetJumpHasBeenPressed(bool aJumpHasBeenPressed)
	{
		JumpButtonHasBeenPressed = aJumpHasBeenPressed;
	}

	/**
	* If grounded, it modifies the Y velocity of the rigid body.
	*/
	private void Jump()
	{
		if (CollisionManager.IsGrounded())
		{
			MyRigidBody.velocity = Vector2.up * JumpSpeed;
		}
		else if (CollisionManager.IsOnWall())
		{
			StopCoroutine(DisableMovement(0.0f));
			StartCoroutine(DisableMovement(0.1f));

			Vector2 WallDir = CollisionManager.IsWallInRight ? Vector2.left : Vector2.right;

			MyRigidBody.velocity = (Vector2.up + WallDir) * WallJumpSpeed;
			HasJumpedInWall = true;
			MyAnimator.SetBool("is_sliding", false);
		}

		IsJumping = true;
		MyAnimator.SetBool("is_jumping", true);
	}

	/**
	* Dash using the forward direction of the player.
	*/
	private void Dash()
	{
		if (!HasDashed && (MyInput.x != 0.0f || MyInput.y != 0.0f))
		{
			
			MyRigidBody.velocity = Vector2.zero;
			MyRigidBody.velocity += MyInput.normalized * DashSpeed;
			StartCoroutine(DashWait());
			HasDashed				= true;
		}
	}

	/**
	* Makes the dash waits until the next dash.
	*/
	IEnumerator DashWait()
	{
		MyRigidBody.gravityScale = 0;
		yield return new WaitForSeconds(.3f);
		MyRigidBody.gravityScale = OriginalGravityScale;
		HasDashed = false;
	}

	/**
	* Manages the jump velocity.
	*/
	private void ManageJump()
	{
		if (!CollisionManager.IsGrounded())
		{
			if (MyRigidBody.velocity.y < 0.0f)
			{
				MyRigidBody.velocity += Vector2.up * Physics2D.gravity.y * (FallMultiplier - 1.0f) * Time.deltaTime;
				MyAnimator.SetBool("is_falling", true);
				MyAnimator.SetBool("is_jumping", false);
			}
			else if (MyRigidBody.velocity.y > 0.0f && !JumpButtonHasBeenPressed)
			{
				MyRigidBody.velocity += Vector2.up * Physics2D.gravity.y * (LowJumpMultiplier - 1.0f) * Time.deltaTime;
			}
		}
		else
		{
			MyAnimator.SetBool("is_falling", false);
		}
	}

	/**
	* Awake is called before start
	*/
	private void Awake()
	{
		InputActions				= new PlayerInputActions();
		MyInput						= new Vector2();
		MyRigidBody					= gameObject.GetComponent<Rigidbody2D>();
		MyCollider					= gameObject.GetComponent<Collider2D>();
		CollisionManager			= gameObject.GetComponent<CollisionManager>();
		MyAnimator					= gameObject.GetComponentInChildren<Animator>();
		MyRender					= gameObject.GetComponentInChildren<SpriteRenderer>();
		JumpButtonHasBeenPressed	= false;
		HasJumpedInWall				= false;
		CanMove						= true;
		HasDashed					= false;
		OriginalGravityScale		= MyRigidBody.gravityScale;
	}

	/**
	* Function called when the controller enables itself.
	*/
	private void OnEnable()
	{
		InputActions.Enable();
	}

	/**
	* Function called when the controller disables itself.
	*/
	private void OnDisable()
	{
		InputActions.Disable();
	}
}
