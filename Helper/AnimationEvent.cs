using UnityEngine;
using System.Collections;

public class AnimationEvent : MonoBehaviour {

	public void Hide(){
		this.gameObject.SetActive(false);
	}

	public void DestoryMe(){
		Destroy(this.gameObject);
	}

	public void DestoryParent(){
		Destroy(this.transform.parent.gameObject);
	}

	public void ShakeParent(){
		iTween.ShakePosition(this.transform.parent.gameObject, new Vector3(.03f, .03f, .03f), 0.3f);
	}
}
