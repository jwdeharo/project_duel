using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private PlayerInputActions			InputActions;					//!< Input actions manager.
	private Rigidbody2D					MyRigidBody;					//!< Rigidbody component.
	private bool						JumpButtonHasBeenPressed;		//!< Indicates that the jump button has been pressed.
	private bool						IsJumping;						//!< Indicates if the object is jumping.
	private Collider2D					MyCollider;						//!< Collider of the object.
	[SerializeField] private LayerMask	GroundMask			= ~0;		//!< Layer that indicates that the object will be detected as ground.
	[SerializeField] private float		FallMultiplier		= 0.0f;		//!< Value used when you are holding the jump button.
	[SerializeField] private float		LowJumpMultiplier	= 0.0f;		//!< Value used when you just pressed once the jump button.
	[SerializeField] private float		JumpSpeed			= 0.0f;		//!< Jump speed.
	[SerializeField] private float		MovementSpeed		= 0.0f;		//!< Movement Speed.

	/**
	* Start is called before the first frame update
	*/
	public void Start()
	{
		InputActions.Land.Jump.started += _ => SetJumpHasBeenPressed(true);
		InputActions.Land.Jump.canceled += _ => SetJumpHasBeenPressed(false);
		InputActions.Land.Jump.performed += _ => Jump();
	}

	/**
	* Update is called once per frame
	*/
	public void Update()
	{
		MovePlayer(InputActions.Land.Move.ReadValue<float>());
		ManageJump();
	}

	/**
	 * Function that moves the player.
	 * @param aInputValue: input value.
	 */
	private void MovePlayer(float aInputValue)
	{
		MyRigidBody.velocity = new Vector2(aInputValue * MovementSpeed, MyRigidBody.velocity.y);
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
		if (IsGrounded())
		{
			MyRigidBody.velocity = Vector2.up * JumpSpeed;
		}
	}

	/**
	* Indicates if the object is grouned.
	* @return true if grounded.
	*/
	private bool IsGrounded()
	{
		Vector2 TopLeftPoint = transform.position;
		Vector2 BottomRightPoint = transform.position;

		TopLeftPoint.x -= MyCollider.bounds.extents.x;
		TopLeftPoint.y += MyCollider.bounds.extents.y;

		BottomRightPoint.x += MyCollider.bounds.extents.x;
		BottomRightPoint.y -= MyCollider.bounds.extents.y;

		return Physics2D.OverlapArea(TopLeftPoint, BottomRightPoint, GroundMask);
	}

	/**
	* Manages the jump velocity.
	*/
	private void ManageJump()
	{
		if (MyRigidBody.velocity.y < 0.0f)
		{
			MyRigidBody.velocity += Vector2.up * Physics2D.gravity.y * (FallMultiplier - 1.0f) * Time.deltaTime;
		}
		else if (MyRigidBody.velocity.y > 0.0f && !JumpButtonHasBeenPressed)
		{
			MyRigidBody.velocity += Vector2.up * Physics2D.gravity.y * (LowJumpMultiplier - 1.0f) * Time.deltaTime;
		}
	}

	/**
	* Awake is called before start
	*/
	private void Awake()
	{
		InputActions = new PlayerInputActions();
		MyRigidBody = gameObject.GetComponent<Rigidbody2D>();
		MyCollider = gameObject.GetComponent<Collider2D>();
		JumpButtonHasBeenPressed = false;
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
