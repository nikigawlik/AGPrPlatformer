/// <summary>
/// Object representing a unique power up effect. 
/// Contains code for different effects, which are stored in the effect enum.
/// The duration of the effect can be adjusted.
/// </summary>
using UnityEngine;

public enum PowerUpEffect {
	SPEED,
}

[CreateAssetMenu(fileName = "PowerUp", menuName = "AGPrPlatformer/PowerUp", order = 0)]
public class PowerUp : ScriptableObject {
	public PowerUpEffect effect = PowerUpEffect.SPEED;
	public float duration = 5f;

	// called when the effect is added to the player
	public void ApplyPowerUp(PlayerController player) {
		switch(effect) {
			case PowerUpEffect.SPEED:
				player.SpeedModifier *= 2f;
			break;
		}
	}

	// called when the effect is removed from the player
	public void UnapplyPoweUp(PlayerController player) {
		switch(effect) {
			case PowerUpEffect.SPEED:
				player.SpeedModifier /= 2f;
			break;
		}
	}
}