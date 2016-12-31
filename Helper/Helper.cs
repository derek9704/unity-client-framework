using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Helper : MonoBehaviour {

	public static void QuickLogin(SceneType name){
		GameServer.Instance.Login("1", delegate(string obj) {
			switch (obj) {
			case "-1":
			case "-2":
				GameManager.Instance.ErrorHint(Lang.ACCOUNT_NOT_EXIST);
				break;
			default:
				GameServer.session_id = obj;
				GameServer.Instance.Request(ServerAction.getUserData, null, delegate() {
					GameManager.Instance.StartHeartBeat();
					GameManager.Instance.ChangeScene(name);
				});
				break;
			}
		});	
	}

	public static Vector2 GetScreenPoint(Vector3 pos){
		Vector3 pt = Camera.main.WorldToScreenPoint(pos);
		float x = pt.x * Consts.SCREEN_WIDTH / Screen.width;
		float y = pt.y * Consts.SCREEN_HEIGHT / Screen.height;
		return new Vector2(x, y);
	}

	public static void Log(object msg){
		#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )
		GameManager.Instance.Log(msg.ToString());
		#else
		print (msg);
		#endif
	}

	public static void DestroyChildren(Transform obj){
		int childCount = obj.childCount;
		for (int i = 0; i < childCount; i++) {
			Destroy(obj.GetChild(i).gameObject);
		}
	}

	public static List<T> ShuffleList<T>(List<T> list){
		System.Random random = new System.Random();
		List<T> newList = new List<T>();
		foreach (T item in list)
		{
			newList.Insert(random.Next(newList.Count), item);
		}
		return newList;
	}

	public static bool Rand100Hit(int num){
		return num >= Random.Range(0, 100);
	}

	public static void AmplifyParticle(GameObject obj, float coe){
		obj.transform.localScale = new Vector3(coe, coe, coe);
		ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < particles.Length; i++) {
			particles[i].startSize = particles[i].startSize * coe;
		}
	}
}
