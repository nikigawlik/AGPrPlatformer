/// <summary>
/// Follows the target object smoothly, does not rotate the camera
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow2D : MonoBehaviour {
	public GameObject targetObject;	
	public float lerpFactor = 1f;
	public bool useFixedUpdate = true;
	
	// Update is called once per frame
	void Update () {
		if(!useFixedUpdate){
			UpdateCam();
		}
	}

	private void FixedUpdate() {
		if(useFixedUpdate){
			UpdateCam();
		}
	}

	void UpdateCam() {
		if(targetObject != null) {
			transform.position = Vector3.Lerp(transform.position, new Vector3(
				targetObject.transform.position.x,
				targetObject.transform.position.y,
				transform.position.z
			), lerpFactor * Time.deltaTime);
		}
	}
}
