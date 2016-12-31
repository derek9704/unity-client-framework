using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public GameObject correctHintObject;
	public GameObject errorHintObject;
	public GameObject lockPanelObject;
	public Image swapImage;
	public Text log;
	public List<Sprite> headImageCollection;

	[HideInInspector]
	public BattleType battleType;
	[HideInInspector]
	public SceneType sceneName;

	public static GameManager Instance;

	void Awake()
	{
		Instance = this;
		iTween.Defaults.easeType = iTween.EaseType.linear;
		StartCoroutine(LoadConfig());
		DontDestroyOnLoad(this.gameObject);
	}

	//配置文件中的字段要求和config表中相同
	private IEnumerator LoadConfig(){
		//Load Config
		using(WWW www = new WWW(Consts.STREAMINGASSETS_ADDR + "Config.csv")){
			yield return www;
			string text = www.text;
			string[] arr = text.Split("\r"[0]);
			for(int i =0;i < arr.Length; i++)
			{
				arr[i] = arr[i].Replace("\n", "");
				string[] arr2 = arr[i].Split (" "[0]);
				var pro = typeof(Consts).GetField(arr2[0]);
				pro.SetValue(null,arr2[1]);
			}
		}
	}

	public void CorrectHint(string msg)
	{
		GameObject obj =  Instantiate(correctHintObject) as GameObject;
		obj.GetComponent<Text>().text = msg;
		obj.transform.SetParent(this.transform, false);
		iTween.MoveAdd(obj, iTween.Hash("y", 120, "speed", 100, "oncomplete", "DestroyObj", "oncompleteparams", obj, "oncompletetarget", this.gameObject));
	}

	public void ErrorHint(string msg)
	{
		GameObject obj =  Instantiate(errorHintObject) as GameObject;
		obj.GetComponent<Text>().text = msg;
		obj.transform.SetParent(this.transform, false);
		iTween.MoveAdd(obj, iTween.Hash("y", 120, "speed", 100, "oncomplete", "DestroyObj", "oncompleteparams", obj, "oncompletetarget", this.gameObject));
	}

	public void Log(string msg)
	{
		log.text = log.text + msg + "\n";
	}

	private void DestroyObj(GameObject obj){
		Destroy(obj);
	}

	public void LockScreen()
	{
		lockPanelObject.SetActive(true);
	}

	public void UnlockScreen()
	{
		lockPanelObject.SetActive(false);
	}

	public IEnumerator LightOff(){
		float alpha = 0;
		while (true) {
			swapImage.color = new Color(0, 0, 0, alpha);
			alpha += 0.1f;
			yield return new WaitForSeconds(0.05f);
			if(alpha >= 1) {
				return false;
			}
		}
	}

	public IEnumerator LightOn(){
		float alpha = 1;
		while (true) {
			swapImage.color = new Color(0, 0, 0, alpha);
			alpha -= 0.1f;
			yield return new WaitForSeconds(0.05f);
			if(alpha <= 0) {
				return false;
			}
		}
	}
	
	private ServerAction action;
	private JsonNode args;

	public void ExcuteCallback(Action callback)
	{
		if(action != ServerAction.none){
			GameServer.Instance.Request(action, args, callback);
			action = ServerAction.none;
			args = null;
		}else{
			callback();
		}
	}

	public void ChangeScene(SceneType sceneName, ServerAction action = ServerAction.none, JsonNode args = null)
	{
		if(sceneName == SceneType.battle){
			if(UnityEngine.Random.value > 0.4f) sceneName = SceneType.battle2;
		}
		this.sceneName = sceneName;
		this.action = action;
		this.args = args;
		Application.LoadLevel (SceneType.loading.ToString());
	}

	public void CallCommand(ServerCommand command, JsonNode args)
	{
		switch (command) {
		case ServerCommand.confirmMsg:
			break;
		case ServerCommand.correctMsg:
			CorrectHint(args);
			break;
		case ServerCommand.errorMsg:
			ErrorHint(args);
			break;
		case ServerCommand.noticeMsg:
			break;
		case ServerCommand.pageJump:
			break;
		}
	}

	public Sprite GetHeadImage(string tid)
	{
		foreach (var item in headImageCollection) {
			if(item.name == tid) return item;
		}
		return null;
	}

	public void StartHeartBeat()
	{
		if(!Consts.OPEN_HEARTBEAT) return;
		GameServer.Instance.Request(ServerAction.heartbeat, null, delegate() {
			NoticeCenter.CallRefreshStamina();
		});
		Invoke("StartHeartBeat", 10);
	}
}
