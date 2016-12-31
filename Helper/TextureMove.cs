using UnityEngine;
using System.Collections;

public class TextureMove : MonoBehaviour {

	public float dir_x = 0;
	public float dir_y = 0;

	private float timeWentX = 0;
	private float timeWentY = 0;

	private Material material;

	void Start() {
		material = GetComponent<Renderer>().materials[0];
	}

	void FixedUpdate () {
		timeWentX += Time.deltaTime * dir_x;
		timeWentY += Time.deltaTime * dir_y;
		material.SetTextureOffset("_MainTex", new Vector2(timeWentX, timeWentY));
	}
}