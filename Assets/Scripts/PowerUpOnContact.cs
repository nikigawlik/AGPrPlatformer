/// <summary>
/// Game object that applies a power up when picked up. 
/// Power up logic is implemented in the game object picking this
/// up, usually the player.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpOnContact : MonoBehaviour {
	public PowerUp powerUp;
}
