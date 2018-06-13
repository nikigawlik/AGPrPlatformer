using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public class ActivePowerUp
	{
		public float durationLeft;
		public PowerUp powerUp;

		private PlayerController player;

		public ActivePowerUp(PowerUp powerUp, PlayerController player) {
			this.powerUp = powerUp;
			this.durationLeft = powerUp.duration;
			this.player = player;

			powerUp.ApplyPowerUp(player);
		}

		public void UnInit() {
			powerUp.UnapplyPoweUp(player);
		}
	}

	public float moveSpeed = 5f;
	public float airSpeed = 1f;
	public float moveAcc = .1f;
	public float uprightTorque = 10f;
	public float jumpStrength = 10f;
	public float groundRayLength = 0.1f;
	public int numberOfRaycasts = 8;

	public float jumpCooldown = 0.2f;
	private float jumpCounter = 0f;

	// [HideInInspector]
	public float speedModifier = 1f;

	public GameObject display;
	public GameObject powerUpDisplay;

	private Rigidbody2D rb;
	private Animator anim;

	private Vector2 startPos;
	private Vector2 lastGroundNormal = Vector2.up;

	private List<ActivePowerUp> activePowerUps;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponentInChildren<Animator>();
		startPos = transform.position;

		activePowerUps = new List<ActivePowerUp>();
	}
	
	// Update is called once per frame
	void Update () {
		UpdatePowerUps();

		if (Input.GetKeyDown("r")) {
			Respawn();
		}
	}

	void UpdatePowerUps() {
		if(activePowerUps != null) {
			for(int i = activePowerUps.Count - 1; i >= 0; i--) {
				ActivePowerUp apu = activePowerUps[i];
				apu.durationLeft = apu.durationLeft - Time.deltaTime;
				// Debug.Log("Apu " + i + ": Duration left: " + apu.durationLeft);
				if(apu.durationLeft <= 0) {
					apu.UnInit();
					activePowerUps.RemoveAt(i);
				}
			}

			if(activePowerUps.Count != 0 && powerUpDisplay != null) {
				powerUpDisplay.SetActive(true);
			} else {
				powerUpDisplay.SetActive(false);
			}
		}
	}

	void Respawn() {
		transform.position = startPos;
		rb.velocity = Vector2.zero;
	}

	void FixedUpdate() {
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

		// movement
		if(onGround) {
			// calculate the velocity we want to have
			float targetV = GetHorizontalInput() * moveSpeed * speedModifier;
			// calculate velocity in movement direction
			float currentV = Vector2.Dot(display.transform.right, rb.velocity);

			if(targetV != 0) {
				float delta = targetV - currentV;
				float acc = Mathf.Clamp(delta, -moveAcc, moveAcc);

				rb.AddForce(display.transform.right * acc, ForceMode2D.Impulse);
			}

			// if(Mathf.Abs(GetHorizontalInput()) < 0.2) {
			// 	GoUpright();
			// }

			// visuals
			if(targetV != 0) {
				display.transform.localScale = new Vector3(Mathf.Sign(targetV), 1, 1);
			}
			anim.SetFloat("tangentSpeed", Mathf.Abs(currentV));
			// anim.speed = Mathf.Abs(currentV) * animSpeedMod;
		} else {
			
			// calculate the velocity we want to have
			float targetV = GetHorizontalInput() * airSpeed;
			// calculate velocity in movement direction
			float currentV = Vector2.Dot(Vector3.right, rb.velocity);

			if(targetV != 0 && Mathf.Abs(targetV) > Mathf.Abs(currentV)) {
				float delta = targetV - currentV;
				float acc = Mathf.Clamp(delta, -moveAcc, moveAcc);
				rb.AddForce(Vector3.right * acc, ForceMode2D.Impulse);
			}

			// smooth towards flying dir
			Vector2 velNormal = -rb.velocity.normalized;
			lastGroundNormal = Vector2.Lerp(lastGroundNormal, velNormal, 0.03f);
		}

		if(Input.GetButtonDown("Jump") && onGround && jumpCounter <= 0) {
			rb.AddForce(lastGroundNormal * jumpStrength, ForceMode2D.Impulse);
			jumpCounter = jumpCooldown;
			anim.SetTrigger("jump");
		}

		jumpCounter = Mathf.Max(jumpCounter - Time.fixedDeltaTime, 0);

		// animation info
		float rotation = Mathf.Atan2(lastGroundNormal.y, lastGroundNormal.x) * Mathf.Rad2Deg - 90;
		display.transform.rotation = Quaternion.Euler(0, 0, rotation);

		anim.SetBool("onGround", onGround);
	}

	float GetHorizontalInput() {
		// TODO replace this with controller compatible solution
		return (Input.GetKey("d")? 1f : 0f) - (Input.GetKey("a")? 1f : 0f);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if(other.gameObject.tag == "DamageZone") {
			Respawn();
		}

		if(other.GetComponent<PowerUpOnContact>() != null) {
			this.activePowerUps.Add(new ActivePowerUp(other.GetComponent<PowerUpOnContact>().powerUp, this));
			GameObject.Destroy(other.gameObject);
		}

		// Debug.Log("TRIG");
	}
}
