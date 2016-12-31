using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TeamPanel : MonoBehaviour {

	public GameObject characterRow;
	public RectTransform teamContentOn;
	public RectTransform teamContentOff;
	public Text teamMainHeroName;
	public Text teamMainHeroLevel;
	public Text teamMainHeroFc;
	public Image teamMainHeroHead;
	public Text teamTotalFc;
	
	private List<string> battleTeam;

	public void Init(){
		JsonNode temp = GameServer.data["user"]["battleTeam"];
		battleTeam = new List<string>();
		for (int i = 0; i < temp.Count; i++){
			battleTeam.Add(temp[i]);
		}
		RefreshTeamPanel();
	}

	public void SetUserBattleTeam(){
		JsonClass obj = new JsonClass();
		obj["tids"] = Json.Parse(battleTeam);
		GameServer.Instance.Request(ServerAction.setUserBattleTeam, obj, delegate() {
			TownManager.Instance.ClosePanel();
		});
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
			Text[] texts = unit.GetComponentsInChildren<Text>();
			texts[0].text = heroes[key]["level"];
			texts[1].text = "战斗力 " + heroes[key]["totalFc"];
			texts[2].text = heroes[key]["name"];
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
