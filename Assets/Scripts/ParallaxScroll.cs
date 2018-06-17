/// <summary>
/// Creates a textured quad in the background which displays several
/// background layers with parallax scrolling.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroll : MonoBehaviour {
	[Tooltip("textures of the parallax layers")]
	public Texture2D[] textures;
	public float minScroll = 0f;
	public float maxScroll = 0.5f;
	public Vector2 scrollMultiplier = new Vector2(1, 1);
	[Tooltip("Material of the background layers")]
	public Material material;
	public Camera cam;

	private GameObject[] planes; // now with 100% less snakes
	private float orthoHeight;
	private float orthoWidth;	

	// Use this for initialization
	void Start () {
		// calculate the dimensions of the camera and screen
		orthoHeight = cam.orthographicSize * 2;
		orthoWidth = orthoHeight * Screen.width/ Screen.height;

		// initialize array for the quads
		planes = new GameObject[textures.Length];

		// create several quads and assign textures, material and transformation
		for(int i = 0; i < textures.Length; i++) {
			GameObject g = GameObject.CreatePrimitive(PrimitiveType.Quad);
			planes[i] = g;
			g.transform.parent = transform;
			g.transform.position = transform.position;
			g.transform.localPosition = new Vector3(0, 0, 20 + (textures.Length - 1 - i));
			g.transform.localScale = new Vector3(orthoWidth, orthoHeight, 1);

			MeshRenderer mr = g.GetComponent<MeshRenderer>();
			mr.material = material;

			mr.material.SetTexture("_MainTex", textures[i]);
		}
	}
	
    void LateUpdate() 
    {
		// for every plane set the texture offset based on the calculated parallax scroll
		for(int i = 0; i < planes.Length; i++) {
			float scroll = minScroll + ((float)i / (planes.Length - 1)) * (maxScroll - minScroll);

			Vector2 uvOffset = new Vector2(
				cam.transform.position.x * scroll / orthoWidth, 
				cam.transform.position.y * scroll / orthoHeight
			) * scrollMultiplier;

			MeshRenderer mr = planes[i].GetComponent<MeshRenderer>();

			if(mr.enabled)
			{
				mr.materials[0].SetTextureOffset("_MainTex", uvOffset);
			}
		}
    }
}
