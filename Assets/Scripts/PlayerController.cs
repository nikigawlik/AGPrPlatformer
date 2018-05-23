using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	public float moveSpeed = 5f;
	public float moveAcc = .1f;
	public float uprightTorque = 10f;
	public float jumpStrength = 10f;
	public float groundRayLength = 0.1f;
	public int numberOfRaycasts = 8;

	private Rigidbody2D rb;
	private Animator anim;

	private Vector2 startPos;
	private Vector2 lastGroundNormal = Vector2.up;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("r")) {
			Respawn();
		}
	}

	void Respawn() {
		transform.position = startPos;
		rb.velocity = Vector2.zero;
	}

	void FixedUpdate() {
		// calculate the velocity we want to have
		float targetV = GetHorizontalInput() * moveSpeed;
		// calculate velocity in movement direction
		float currentV = Vector2.Dot(Vector2.right, rb.velocity);

		if(targetV != 0) {
			float delta = targetV - currentV;
			float acc = Mathf.Clamp(delta, -moveAcc, moveAcc);
			// Debug.Log("targetV: " + targetV);
			// Debug.Log("currentV: " + currentV);
			// Debug.Log("targetV: " + targetV);	

			rb.AddForce(Vector2.right * acc, ForceMode2D.Impulse);
		}

		if(Mathf.Abs(GetHorizontalInput()) < 0.2) {
			GoUpright();
		}

		// jumping
		CircleCollider2D cc = GetComponent<CircleCollider2D>();
		
		bool onGround = false;
		
		// we skip 0 because 1 also evaluates to zero offset, and we don't need to do this twice
		for(int i = 1; i <= numberOfRaycasts; i++) {
			// special iteration: start down, iterate sides
			float flip = i % 2 == 0? 1 : -1;
			float offset = i / 2;

			// calculate the angle at which to perform the raycast
			float angle = 
				rb.rotation * Mathf.Deg2Rad - Mathf.PI/2 
				+ flip * ((float)offset / numberOfRaycasts * 2f) * Mathf.PI;

			float rayDistance = cc.radius + groundRayLength;

			Debug.DrawLine(
				transform.position, 
				transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * rayDistance
			);

			RaycastHit2D hit = Physics2D.Raycast(
				transform.position, 
				new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)),
				rayDistance
			);

		
			if(hit.collider != null) {
				// Debug.Log("hit distance" + hit.distance);
				onGround = true;
				lastGroundNormal = hit.normal;
				
				Debug.DrawLine(
					transform.position, 
					transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0)
					* rayDistance * 2f
				);
				break;
			}
		}

		if(Input.GetButtonDown("Jump") && onGround) {
			rb.AddForce(lastGroundNormal * jumpStrength, ForceMode2D.Impulse);
		}

		// animation info
		anim.SetBool("armsRaised", GetHorizontalInput() != 0);
	}

	/// <summary>
	/// Applies torque so that the character stands upright (rotation == 0)
	/// </summary>
	void GoUpright() {
		float torque = Mathf.DeltaAngle(rb.rotation, Vector2.SignedAngle(Vector2.up, lastGroundNormal)) * Mathf.Deg2Rad;
		torque = Mathf.Clamp(torque, -uprightTorque, uprightTorque);
		rb.AddTorque(torque, ForceMode2D.Impulse);
	}

	float GetHorizontalInput() {
		// TODO replace this with controller compatible solution
		return (Input.GetKey("d")? 1f : 0f) - (Input.GetKey("a")? 1f : 0f);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if(other.gameObject.tag == "DamageZone") {
			Respawn();
		}
		Debug.Log("TRIG");
	}
}
