using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArenaPanel : MonoBehaviour {

	public GameObject characterRow;
	public GameObject logRow;
	public GameObject myTeamRow;
	public GameObject rivalTeamRow;
	public GameObject rankRow;
	public GameObject rivalRow;

	public RectTransform teamContentOn;
	public RectTransform teamContentOff;
	public Text teamMainHeroName;
	public Text teamMainHeroLevel;
	public Text teamMainHeroFc;
	public Image teamMainHeroHead;
	public Text teamTotalFc;

	public RectTransform listContent;

	public Text myRank;
	public Text myFC;
	public RectTransform myTeam;
	public RectTransform rivalTeam;
	public GameObject subPanel;
	public GameObject setTeamPanel;
	public GameObject listViewPanel;
	public GameObject playerTeamPanel;
	
	private List<string> battleTeam;

	public void OpenSetTeamPanel(){
		subPanel.SetActive(true);
		setTeamPanel.SetActive(true);
		listViewPanel.SetActive(false);
		playerTeamPanel.SetActive(false);
		RefreshTeamPanel();
	}

	public void OpenPlayerTeamPanel(JsonNode data){
		subPanel.SetActive(true);
		setTeamPanel.SetActive(false);
		listViewPanel.SetActive(false);
		playerTeamPanel.SetActive(true);
		Helper.DestroyChildren(playerTeamPanel.transform);
		for (int i = 0; i < data.Count; i++) {
			GameObject unit = Instantiate(rivalTeamRow) as GameObject;
			unit.transform.SetParent(playerTeamPanel.transform, false);
			unit.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.GetHeadImage(data[i]["tid"]);
			unit.transform.GetChild(1).GetComponent<Text>().text = data[i]["level"];
		}
	}

	public void OpenRankPanel(){
		GameServer.Instance.Request(ServerAction.getArenaRank, null, delegate() {
			subPanel.SetActive(true);
			setTeamPanel.SetActive(false);
			playerTeamPanel.SetActive(false);
			listViewPanel.SetActive(true);
			Helper.DestroyChildren(listContent);
			JsonNode ranks = GameServer.data["arena"]["rankList"];
			for (int i = 0; i < ranks.Count; i++) {
				GameObject unit = Instantiate(rankRow) as GameObject;
				unit.transform.SetParent(listContent, false);
				unit.transform.GetChild(1).GetComponent<Text>().text = ranks[i]["name"];
				unit.transform.GetChild(2).GetComponent<Text>().text = ranks[i]["level"] + "级";
				unit.transform.GetChild(3).GetComponent<Text>().text = ranks[i]["rank"];
			}
			listContent.GetComponent<RectTransform>().sizeDelta = new Vector2(557, 70 * ranks.Count);
		});
	}

	public void OpenLogPanel(){
		GameServer.Instance.Request(ServerAction.getArenaLog, null, delegate() {
			subPanel.SetActive(true);
			setTeamPanel.SetActive(false);
			playerTeamPanel.SetActive(false);
			listViewPanel.SetActive(true);
			Helper.DestroyChildren(listContent);
			JsonNode logs = GameServer.data["arena"]["log"];
			for (int i = 0; i < logs.Count; i++) {
				GameObject unit = Instantiate(logRow) as GameObject;
				unit.transform.SetParent(listContent, false);
				unit.transform.GetChild(1).GetComponent<Text>().text = logs[i]["rivalName"];
				unit.transform.GetChild(2).GetComponent<Text>().text = logs[i]["rivalLevel"];
				if(logs[i]["win"] == 0){
					unit.transform.GetChild(3).gameObject.SetActive(true);
					unit.transform.GetChild(3).GetComponent<Text>().text = "败↓" + logs[i]["rankChange"];
				}else{
					unit.transform.GetChild(4).gameObject.SetActive(true);
					unit.transform.GetChild(4).GetComponent<Text>().text = "胜↑" + logs[i]["rankChange"];
				}
//				unit.transform.GetChild(5).GetComponent<Text>().text = logs[i]["time"];
			}
			listContent.GetComponent<RectTransform>().sizeDelta = new Vector2(557, 70 * logs.Count);
		});
	}

	public void RefreshRivals(){
		GameServer.Instance.Request(ServerAction.getArenaInfo, null, delegate() {
			RefreshArenaPanel();
		});
	}

	public void CloseSubPanel(){
		subPanel.SetActive(false);
	}

	public void Init(){
		myRank.text = GameServer.data["arena"]["myRank"];
		CloseSubPanel();
		RefreshArenaPanel();
	}
	
	public void SetUserArenaTeam(){
		JsonClass obj = new JsonClass();
		obj["tids"] = Json.Parse(battleTeam);
		GameServer.Instance.Request(ServerAction.setArenaTeam, obj, delegate() {
			CloseSubPanel();
			RefreshArenaPanel();
		});
	}

	private void RefreshArenaPanel(){
		battleTeam = new List<string>();
		JsonNode temp = GameServer.data["arena"]["defenceTeam"];
		battleTeam.Add(GameServer.data["user"]["mainHero"]);
		for (int i = 0; i < temp.Count; i++){
			battleTeam.Add(temp[i]);
		}
		Helper.DestroyChildren(myTeam);
		Helper.DestroyChildren(rivalTeam);
		JsonNode heroes = GameServer.data["heroes"];
		float totalFc = 0;
		for (int i = 0; i < battleTeam.Count; i++){
			string key = battleTeam[i];
			GameObject unit = Instantiate(myTeamRow) as GameObject;
			unit.transform.SetParent(myTeam, false);
			unit.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.GetHeadImage(key);
			unit.transform.GetChild(1).GetComponent<Text>().text = heroes[key]["level"];
			totalFc += heroes[key]["totalFc"];
		}
		battleTeam.Remove(GameServer.data["user"]["mainHero"]);
		myFC.text = "战斗力 " + totalFc.ToString();
		JsonNode rivals = GameServer.data["arena"]["rivals"];
		for (int i = 0; i < rivals.Count; i++){
			GameObject unit = Instantiate(rivalRow) as GameObject;
			unit.transform.SetParent(rivalTeam, false);
			unit.transform.GetChild(1).GetComponent<Text>().text = rivals[i]["name"];
			unit.transform.GetChild(2).GetComponent<Text>().text = rivals[i]["level"];
			JsonNode defenceTeam = rivals[i]["defenceTeam"];
			unit.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate(){
				OpenPlayerTeamPanel(defenceTeam);
			});
			float fc = 0;
			for (int j = 0; j < defenceTeam.Count; j++) {
				fc += defenceTeam[j]["fc"];
			}
			unit.transform.GetChild(3).GetComponent<Text>().text = "战斗力 " + fc.ToString();
			int rivalRank = rivals[i]["rank"];
			unit.transform.GetChild(4).GetComponent<Text>().text = "排名 " + rivalRank.ToString();
			unit.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(delegate(){
				JsonClass obj = new JsonClass();
				obj["rank"] = rivalRank;
				GameServer.Instance.Request(ServerAction.startArenaBattle, obj, delegate() {
					GameManager.Instance.battleType = BattleType.arena;
					GameManager.Instance.ChangeScene(SceneType.battle);
				});
			});
		}
	}
	
	private void RefreshTeamPanel(){
		Helper.DestroyChildren(teamContentOn);
		Helper.DestroyChildren(teamContentOff);
		JsonNode heroes = GameServer.data["heroes"];
		string mainHero = GameServer.data["user"]["mainHero"];
		teamMainHeroFc.text = "战斗力 " + heroes[mainHero]["totalFc"];
		teamMainHeroHead.sprite = GameManager.Instance.GetHeadImage(mainHero);
		teamMainHeroName.text = heroes[mainHero]["name"];
		teamMainHeroLevel.text = heroes[mainHero]["level"];
		float totalFc = heroes[mainHero]["totalFc"];
		for (int i = 0; i < battleTeam.Count; i++){
			string key = battleTeam[i];
			if(key == mainHero) continue;
			GameObject unit = Instantiate(characterRow) as GameObject;
			unit.name = key;
			unit.transform.SetParent(teamContentOn, false);
			unit.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.GetHeadImage(key);
			Text[] texts = unit.GetComponentsInChildren<Text>();
			texts[0].text = heroes[key]["level"];
			texts[1].text = "战斗力 " + heroes[key]["totalFc"];
			texts[2].text = heroes[key]["name"];
			unit.GetComponent<Button>().onClick.AddListener(delegate(){
				SetTeamOff(unit);
			});
			totalFc += heroes[key]["totalFc"];
		}
		teamTotalFc.text = "总战斗力 " + totalFc.ToString();
		int count = 0;
		foreach (string key in heroes.Keys) {
			if(heroes[key]["type"] == "1") continue;
			if(battleTeam.Contains(key)) continue;
			GameObject unit = Instantiate(characterRow) as GameObject;
			unit.name = key;
			unit.transform.SetParent(teamContentOff, false);
			unit.transform.GetChild(0).GetComponent<Image>().sprite = GameManager.Instance.GetHeadImage(key);
			unit.transform.GetChild(1).GetComponent<Text>().text = heroes[key]["level"];
			unit.transform.GetChild(2).GetComponent<Text>().text = "战斗力 " + heroes[key]["totalFc"];
			unit.transform.GetChild(3).GetComponent<Text>().text = heroes[key]["name"];
			unit.GetComponent<Button>().onClick.AddListener(delegate(){
				SetTeamOn(unit);
			});
			count++;
		}
		//计算一下content的高度
		count -= 6;
		if(count > 0){
			teamContentOff.GetComponent<RectTransform>().sizeDelta = new Vector2(252, 383 + 63 * count);
		}
	}
	
	private void SetTeamOn(GameObject unit){
		if(battleTeam.Count >= 3) return;
		battleTeam.Add(unit.name);
		Dictionary<string, int> dic = new Dictionary<string, int>();
		JsonNode heroes = GameServer.data["heroes"];
		//排序一次
		for (int i = 0; i < battleTeam.Count; i++){
			string key = battleTeam[i];
			dic.Add(key, heroes[key]["weight"]);
		}
		battleTeam.Clear();
		foreach (var item in dic.OrderByDescending(s=>s.Value))
		{
			battleTeam.Add(item.Key);
		}
		
		RefreshTeamPanel();
	}
	
	private void SetTeamOff(GameObject unit){
		battleTeam.Remove(unit.name);
		RefreshTeamPanel();
	}

}
