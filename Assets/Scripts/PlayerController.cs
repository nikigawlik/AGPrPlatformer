/// <summary>
/// Implements a physics based player movement and power up system.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {

	// class representing a power up while it's active for the player
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

	
	[Tooltip("maximum speed of player")]
	public float moveSpeed = 5f;
	[Tooltip("maximum speed while in the air")]
	public float airSpeed = 1f;
	[Tooltip("acceleration of player")]
	public float moveAcc = .1f;
	public float jumpStrength = 10f;
	[Tooltip("extra distance for raycasts checking for ground. Accumulative with the size of the circle collider.")]
	public float groundRayLength = 0.1f;
	[Tooltip("Number of raycasts looking for ground in a circle.")]
	public int numberOfRaycasts = 8;

	[Tooltip("Time the player is unable to jump, right after having jumped, regardless of ground contact.")]
	public float jumpCooldown = 0.2f;
	private float jumpCounter = 0f;

	[Tooltip("Object containing most visual things for the player. Usually child object.")]
    public GameObject display;
	[Tooltip("Object to activate while power ups are active.")]
	public GameObject powerUpDisplay;

	private Rigidbody2D rb;
	private Animator anim;

	private Vector2 lastGroundNormal = Vector2.up;

	private List<ActivePowerUp> activePowerUps;

	// used by speed powerup to change the speed of the player
    private float speedModifier = 1f;

    public float SpeedModifier
    {
        get
        {
            return speedModifier;
        }

        set
        {
            speedModifier = value;
        }
    }

    // Use this for initialization
    void Start () {
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponentInChildren<Animator>();

		activePowerUps = new List<ActivePowerUp>();
	}
	
	// Update is called once per frame
	void Update () {
		UpdatePowerUps();

		// If player gets stuck
		if (Input.GetKeyDown("r")) {
			Respawn();
		}
	}

	// applies all power up modifications
	void UpdatePowerUps() {
		if(activePowerUps != null) {
			for(int i = activePowerUps.Count - 1; i >= 0; i--) {
				// update cooldown of powerup
				ActivePowerUp apu = activePowerUps[i];
				apu.durationLeft = apu.durationLeft - Time.deltaTime;
				if(apu.durationLeft <= 0) {
					// remove powerup and undo effects
					apu.UnInit();
					activePowerUps.RemoveAt(i);
				}
			}

			// toggle power up display
			if(activePowerUps.Count != 0 && powerUpDisplay != null) {
				powerUpDisplay.SetActive(true);
			} else {
				powerUpDisplay.SetActive(false);
			}
		}
	}

	// respawns the player
	void Respawn() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	void FixedUpdate() {
		// jumping
		CircleCollider2D cc = GetComponent<CircleCollider2D>();
		bool onGround = false;
		
		// Look for ground with raycast, starting from below the player and going up
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
				// we found "ground". Set the normal and boolean value
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
			// ground movement

			// calculate the velocity we want to have
			float targetV = GetHorizontalInput() * moveSpeed * SpeedModifier;
			// calculate current velocity in movement direction
			float currentV = Vector2.Dot(display.transform.right, rb.velocity);

			// calculate the delta and apply the neccesary force
			if(targetV != 0) {
				float delta = targetV - currentV;
				float acc = Mathf.Clamp(delta, -moveAcc, moveAcc);

				rb.AddForce(display.transform.right * acc, ForceMode2D.Impulse);
			}

			// visuals

			// mirror the display when walking to the left
			if(targetV != 0) {
				display.transform.localScale = new Vector3(Mathf.Sign(targetV), 1, 1);
			}
			// pass speed to animation controller
			anim.SetFloat("tangentSpeed", Mathf.Abs(currentV));
		} else {
			// air movement

			// calculate the velocity we want to have
			float targetV = GetHorizontalInput() * airSpeed;
			// calculate velocity in movement direction
			float currentV = Vector2.Dot(Vector3.right, rb.velocity);

			// calculate the delta and apply the neccesary force
			if(targetV != 0 && Mathf.Abs(targetV) > Mathf.Abs(currentV)) {
				float delta = targetV - currentV;
				float acc = Mathf.Clamp(delta, -moveAcc, moveAcc);
				rb.AddForce(Vector3.right * acc, ForceMode2D.Impulse);
			}

			// smooth the ground normal towards flying direction
			// this ensures a "smoother landing"
			Vector2 velNormal = -rb.velocity.normalized;
			lastGroundNormal = Vector2.Lerp(lastGroundNormal, velNormal, 0.03f);
		}

		// jumping

		// jump if grounded and not in cooldown
		if(Input.GetButtonDown("Jump") && onGround && jumpCounter <= 0) {
			rb.AddForce(lastGroundNormal * jumpStrength, ForceMode2D.Impulse);
			jumpCounter = jumpCooldown;

			anim.SetTrigger("jump");
		}

		// count down jump cooldown
		jumpCounter = Mathf.Max(jumpCounter - Time.fixedDeltaTime, 0);

		// additional animation info

		// set display rotation according to ground normal
		float rotation = Mathf.Atan2(lastGroundNormal.y, lastGroundNormal.x) * Mathf.Rad2Deg - 90;
		display.transform.rotation = Quaternion.Euler(0, 0, rotation);

		anim.SetBool("onGround", onGround);
	}

	float GetHorizontalInput() {
		// TODO replace this with controller compatible solution
		return (Input.GetKey("d")? 1f : 0f) - (Input.GetKey("a")? 1f : 0f);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		// handle kill zones
		if(other.gameObject.tag == "DamageZone") {
			Respawn();
		}

		// handle power ups
		if(other.GetComponent<PowerUpOnContact>() != null) {
			this.activePowerUps.Add(new ActivePowerUp(other.GetComponent<PowerUpOnContact>().powerUp, this));
			GameObject.Destroy(other.gameObject);
		}
	}
}
