using UnityEngine;
using System.Collections;

public class InputControl {

	public static bool MouseDown(){
		#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )
		return Input.touches[0].phase == TouchPhase.Began;
		#else
		return Input.GetMouseButtonDown(0);
		#endif
	}

	public static bool MouseUp(){
		#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )
		return Input.touches[0].phase == TouchPhase.Ended;
		#else
		return Input.GetMouseButtonUp(0);
		#endif
	}

	public static bool MouseMove(){
		#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )
		return Input.touches[0].phase == TouchPhase.Moved;
		#else
		return Input.GetMouseButton(0);
		#endif
	}

	public static Vector3 GetPosition(){
		#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )
		return Input.touches[0].position;
		#else
		return Input.mousePosition;
		#endif
	}

	public static int TouchCount(){
		#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )
		return Input.touchCount;
		#else
		return 1;
		#endif
	}

}
