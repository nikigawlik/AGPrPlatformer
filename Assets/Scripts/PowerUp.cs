using UnityEngine;

public enum PowerUpEffect {
	SPEED,
}

[CreateAssetMenu(fileName = "PowerUp", menuName = "AGPrPlatformer/PowerUp", order = 0)]
public class PowerUp : ScriptableObject {
	public PowerUpEffect effect = PowerUpEffect.SPEED;
	public float duration = 5f;

	public void ApplyPowerUp(PlayerController player) {
		switch(effect) {
			case PowerUpEffect.SPEED:
				player.speedModifier *= 2f;
			break;
		}
	}

	public void UnapplyPoweUp(PlayerController player) {
		switch(effect) {
			case PowerUpEffect.SPEED:
				player.speedModifier /= 2f;
			break;
		}
	}
}