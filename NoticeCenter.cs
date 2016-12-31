using UnityEngine;
using System.Collections;

public class NoticeCenter : MonoBehaviour {

	public delegate void NoticeHandler ();

	//刷新体力
	public static event NoticeHandler OnRefreshStamina;
	public static void CallRefreshStamina ()  
	{  
		if (OnRefreshStamina != null)  
			OnRefreshStamina ();  
	}  
}
