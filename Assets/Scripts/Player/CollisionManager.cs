using UnityEngine;

public class CollisionManager : MonoBehaviour
{
	private Collider2D					MyCollider;				//!< Collider of the object.
	public bool							IsWallInRight;			//!< Indicates if the wall is in the right.
	[SerializeField] private float		GroundedRadius	= 0.4f;	//!< Radius used to calculate if the game object is grounded.
	[SerializeField] private float		WallRadius		= 0.2f;	//!< Radius used to calculate if the game object is touching the wall.
	[SerializeField] private LayerMask	GroundMask		=	0;	//!< Layer that indicates that the object will be detected as ground.

	/**
	* Function called when the game starts.
	*/
	private void Awake()
	{
		MyCollider = gameObject.GetComponent<Collider2D>();
		IsWallInRight = false;
	}

	/**
	* Indicates if the player is grounded.
	* @return true if grounded.
	*/
	public bool IsGrounded()
	{
		Vector2 BottomPoint = transform.position - (MyCollider.bounds.extents.y * transform.up);
		return Physics2D.OverlapCircle((Vector2)BottomPoint, GroundedRadius, GroundMask);
	}

	/**
	* Indicates if the player is on the wall.
	* @return true if on wall.
	*/
	public bool IsOnWall()
	{
		Vector3 MyPosition = transform.position;
		MyPosition += 0.1f * transform.up;

		Vector2 LeftPoint = MyPosition - (MyCollider.bounds.extents.x * transform.right);
		Vector2 RightPoint = MyPosition + (MyCollider.bounds.extents.x * transform.right);
		IsWallInRight = Physics2D.OverlapCircle((Vector2)RightPoint, WallRadius, GroundMask);

		return Physics2D.OverlapCircle((Vector2)LeftPoint, WallRadius, GroundMask) || IsWallInRight;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Vector3 MyPosition = transform.position;
		Vector2 BottomPoint = transform.position - (gameObject.GetComponent<Collider2D>().bounds.extents.y * transform.up);
		Vector2 LeftPoint = MyPosition + (gameObject.GetComponent<Collider2D>().bounds.extents.x * transform.right);
		Vector2 RightPoint = MyPosition - (gameObject.GetComponent<Collider2D>().bounds.extents.x * transform.right);

		Gizmos.DrawWireSphere(LeftPoint, WallRadius);
		Gizmos.DrawWireSphere(RightPoint, WallRadius);
		Gizmos.DrawWireSphere(BottomPoint, GroundedRadius);
	}
}
