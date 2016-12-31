using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour {

	public enum Status{
		ready, start, end
	}

	public static BattleManager Instance;

	private static bool myTeamAuto = false;

	public Canvas canvas;
	public Text timeCount;
	public Text levelName;
	public GameObject autoOn;
	public GameObject autoOff;
	public GameObject pauseBtn;
	public GameObject pausePanel;
	public GameObject skill1;
	public GameObject skill2;
	public GameObject skill3;
	public GameObject skill4;
	public GameObject skillName;
	public GameObject victoryPanel;
	public GameObject losePanel;
	public GameObject uiSet;
	public GameObject castSkillParticle;
	public Troop myTroop;
	public Troop enemyTroop;
	public Status status;

	public bool specialMap = false;

	public List<GameObject> hud;
	public List<GameObject> effects;

	private float ambientLight = -0.3f;
	private float battleTime;
	private bool isWin = false;

	void Awake()
	{
		Instance = this;
		if(!specialMap){
			string fieldName = Random.value > 0.5f ? "12346" : "12347";
			Instantiate(Resources.Load("Battle/Map/" + fieldName));
		}
		canvas.planeDistance = 0;
		myTroop.Init();
		enemyTroop.Init();
	}

	void Start()
	{
		status = Status.ready;
		if(GameManager.Instance.battleType == BattleType.mine){
			levelName.text = GameServer.data["mine"]["name"];
			if(myTeamAuto){
				AutoOn();
			}
		}
		else{
			levelName.text = "竞技场";
			autoOn.SetActive(true);
			autoOff.SetActive(false);
			myTroop.auto = true;
		}
		battleTime = 60;
		RenderSettings.ambientLight = new Color(0, 0, 0, 1);
		Invoke("StartFight", specialMap ? 3f : 3f);
	}

	void FixedUpdate(){
		if(status == Status.ready && ambientLight < 0.64f){
			ambientLight += Time.deltaTime / 3;
			if(ambientLight > 0.64f) ambientLight = 0.64f;
			if(ambientLight > 0) RenderSettings.ambientLight = new Color(ambientLight, ambientLight, ambientLight, 1);
		}
		else if(status == Status.start){
			battleTime -= Time.deltaTime;
			if(battleTime <= 0){
				battleTime = 0;
				GameOver(false);
			}
			timeCount.text = "Time：" + Mathf.Ceil(battleTime).ToString();
		}
	}

	private static List<AnimationClip> clips = new List<AnimationClip>();

	public bool InformAnimationClip(AnimationClip clip)
	{
		if(clips.Contains(clip)) {
			return false;
		}
		clips.Add(clip);
		return true;
	}

	public GameObject GetEffect(string id)
	{
		foreach (var item in effects) {
			if(item.name == id) return item;
		}
		return null;
	}

	public GameObject GetHUD(string id)
	{
		foreach (var item in hud) {
			if(item.name == id) return item;
		}
		return null;
	}

	public void GameOver(bool win){
		isWin = win;
		status = Status.end;
		Invoke("ShowResult", 3f);
	}

	private void ShowStar1(){
		Transform star = victoryPanel.transform.FindChild("Star1");
		star.gameObject.SetActive(true);
	}

	private void ShowStar2(){
		Transform star = victoryPanel.transform.FindChild("Star2");
		star.gameObject.SetActive(true);
	}

	private void ShowStar3(){
		Transform star = victoryPanel.transform.FindChild("Star3");
		star.gameObject.SetActive(true);
	}

	private void ShowResult(){
		float maxDamage = 0;
		foreach(BaseUnit unit in myTroop.team){
			if(unit.exportDamage > maxDamage) maxDamage = unit.exportDamage;
		}
		foreach(BaseUnit unit in enemyTroop.team){
			if(unit.exportDamage > maxDamage) maxDamage = unit.exportDamage;
		}
		GameObject resultPanel;
		string myRow, hisRow;
		if(isWin) {
			resultPanel = victoryPanel;
			myRow = "VictoryMyRow";
			hisRow = "VictoryHisRow";
			int deadNum = myTroop.GetDeadNum();
			Invoke("ShowStar1", 0.4f);
			switch (deadNum) {
			case 0:
				Invoke("ShowStar2", 0.8f);
				Invoke("ShowStar3", 1.2f);
				break;
			case 1:
				Invoke("ShowStar2", 0.8f);
				break;
			}
		}
		else {
			resultPanel = losePanel;
			myRow = "LoseMyRow";
			hisRow = "LoseHisRow";
		}
		resultPanel.SetActive(true);
		Transform myInfo = resultPanel.transform.FindChild("MyInfo");
		Helper.DestroyChildren(myInfo);
		foreach(BaseUnit unit in myTroop.team){
			GameObject row = Instantiate(BattleManager.Instance.GetHUD(myRow)) as GameObject;
			row.transform.SetParent(myInfo, false);
			if(unit.type == 1){
				row.transform.SetAsFirstSibling();
			}
			row.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.GetHeadImage(unit.tid);
			row.transform.GetChild(1).GetComponent<Text>().text = ((int)unit.exportDamage).ToString();
			row.transform.GetChild(3).GetComponent<Image>().fillAmount = unit.exportDamage / maxDamage;
		}
		Transform hisInfo = resultPanel.transform.FindChild("HisInfo");
		Helper.DestroyChildren(hisInfo);
		foreach(BaseUnit unit in enemyTroop.team){
			GameObject row = Instantiate(BattleManager.Instance.GetHUD(hisRow)) as GameObject;
			row.transform.SetParent(hisInfo, false);
			if(unit.type == 1){
				row.transform.SetAsFirstSibling();
			}
			row.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.GetHeadImage(unit.tid);
			row.transform.GetChild(1).GetComponent<Text>().text = ((int)unit.exportDamage).ToString();
			row.transform.GetChild(3).GetComponent<Image>().fillAmount = unit.exportDamage / maxDamage;
		}
	}

	private void StartFight(){
		canvas.planeDistance = 5;
		status = Status.start;
		myTroop.StartFight();
		enemyTroop.StartFight();
	}

	public void Leave(){
		Time.timeScale = 1;
		if(GameManager.Instance.battleType == BattleType.mine){
			MineManager.battleFlag = isWin ? 1 : 2;
			GameManager.Instance.ChangeScene(SceneType.mine);
		}else{
			JsonClass obj = new JsonClass();
			obj["win"] = isWin ? 1 : 0;
			obj["rivalId"] = GameServer.data["arena"]["battleData"][0]["owner"];
			GameServer.Instance.Request(ServerAction.finishArenaBattle, obj, delegate() {
				TownManager.battleFlag = isWin ? 1 : 2;
				GameManager.Instance.ChangeScene(SceneType.town);
			});
		}
	}

	public void PauseGame()
	{
		if(status != Status.start) return;
		pausePanel.SetActive(true);
		Time.timeScale = 0;
	}
	
	public void ResumeGame()
	{
		pausePanel.SetActive(false);
		Time.timeScale = 1;
		if(castSkillParticle.activeSelf){
			castSkillParticle.GetComponent<CastSkillParticle>().Resume();
		}
	}

	public void LightOff(BaseUnit unit){
		foreach (var one in myTroop.team) {
			one.SetBodyDark();
		}
		foreach (var one in enemyTroop.team) {
			one.SetBodyDark();
		}
		unit.SetBodyWhite();
		Time.timeScale = 0;
		RenderSettings.ambientLight = new Color32(0, 0, 0, 255);
		skillName.GetComponent<Text>().text = unit.skill1.name;
		skillName.SetActive(true);
		//技能特效
		castSkillParticle.SetActive(false);
		castSkillParticle.SetActive(true);
		castSkillParticle.transform.position = unit.transform.position + new Vector3(0, 2, 0);
	}
	
	public void LightOn(){
		Time.timeScale = 1;
		foreach (var one in myTroop.team) {
			one.SetBodyWhite();
		}
		foreach (var one in enemyTroop.team) {
			one.SetBodyWhite();
		}
		RenderSettings.ambientLight = new Color32(163, 163, 163, 255);
		skillName.SetActive(false);
	}

	public void AutoOn()
	{
		if(GameManager.Instance.battleType == BattleType.arena) return; //竞技场必须自动
		autoOn.SetActive(true);
		autoOff.SetActive(false);
		myTeamAuto = myTroop.auto = true;
	}
	
	public void AutoOff()
	{
		if(GameManager.Instance.battleType == BattleType.arena) return; //竞技场必须自动
		autoOff.SetActive(true);
		autoOn.SetActive(false);
		myTeamAuto = myTroop.auto = false;
	}

}
