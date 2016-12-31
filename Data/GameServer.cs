using UnityEngine;
using System.Collections;
using System;

public enum ServerAction
{
	none,
	getUserData, heartbeat, setUserBattleTeam, 
	transferMainHero, upgradeHero, upgradeHeroSkill, 
	enterMine, leaveMine, enterMineBattle, refreshMine,
	getArenaInfo, getArenaRank, getArenaLog, setArenaTeam, startArenaBattle, finishArenaBattle
}

public enum ServerCommand
{
	errorMsg, correctMsg, confirmMsg, noticeMsg, pageJump
}

public class GameServer : MonoBehaviour {

	public static GameServer Instance;

	public static string session_id;

	public static JsonClass data = new JsonClass();
	
	void Awake()
	{
		Instance = this;
	}

	public void Request(ServerAction action, JsonNode args = null, Action func = null){
		GameManager.Instance.LockScreen();
		JsonClass jdata = new JsonClass();
		jdata["action"] = action.ToString();
		if(args != null) jdata["args"] = args;
		string str = jdata.ToJson();
		print(str);
		StartCoroutine(CallServer(str, func));
	}

	IEnumerator CallServer(string str, Action func){
		WWWForm form = new WWWForm();
		form.AddField("d", str);
		form.AddField("sid", session_id);
		using(WWW www = new WWW(Consts.REQUEST_ADDR, form)){
			yield return www;
			if (www.error != null){
				GameManager.Instance.ErrorHint(www.error);
			}else{
				//处理www.text
				print(www.text);
				JsonNode jdata = Json.Parse(www.text);
				if(jdata["responseData"]){
					bool result = true;
					jdata = jdata["responseData"];
					int count = jdata.Count;
					for (int i = 0; i < count; i++) {
						JsonNode itemResponseData = jdata[i];
						if(itemResponseData["command"]){
							string cmd = itemResponseData["command"];
							if(cmd == "errorMsg"){
								result = false;
							}
							GameManager.Instance.CallCommand((ServerCommand)Enum.Parse(typeof(ServerCommand), cmd), itemResponseData["args"]);
						}else{
							if(itemResponseData["data"]){
								string path = itemResponseData["data"];
								string[] arrKey = path.Split("/"[0]);
								JsonNode tmp = data;
								int j;
								for (j = 0; j < arrKey.Length - 1; j++) {
									tmp = tmp[arrKey[j]];
								}
								tmp[arrKey[j]] = itemResponseData["args"];
							}else{
								result = false;
							}
						}
					}
					//成功的请求
					if (result && func != null)
					{
						func();
					}
				}else{
					print (jdata);
				}
			}
			GameManager.Instance.UnlockScreen();
		}
	}

	public void Login(string userport, Action<string> func){
		GameManager.Instance.LockScreen();
		StartCoroutine(LoginServer(userport, func));
	}

	IEnumerator LoginServer(string userport, Action<string> func){
		WWWForm form = new WWWForm();
		form.AddField("userport", userport);
		using(WWW www = new WWW(Consts.LOGIN_ADDR, form)){
			yield return www;
			if (www.error != null){
				GameManager.Instance.ErrorHint(www.error);
			}else{
				if(func != null) func(www.text);
			}
			GameManager.Instance.UnlockScreen();
		}
	}
}
