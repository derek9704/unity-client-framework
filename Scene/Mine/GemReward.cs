using UnityEngine;
using System.Collections;

public class GemReward : MonoBehaviour {

	public GemType type;
	
	void Start(){
		this.transform.Translate(Vector3.up * Random.Range(-1f, 1f));
		iTween.MoveAdd(this.gameObject, iTween.Hash("x", Random.Range(-1.5f, 1.5f), "time", 0.5f, "islocal", true));
	}
	
	public void Collect ()
	{
		iTween.MoveTo(this.gameObject, iTween.Hash("position", new Vector3(-400, 170, 0), "speed", 500, "oncomplete", "Clear", "islocal", true));
	}
	
	public void Clear(){
		Destroy(this.gameObject);
	}

}
