using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class CharacterPanel : MonoBehaviour {
	
	public Button characterBasicBtn;
	public Button characterSkillBtn;
	public Button characterDetailBtn;
	public Button characterTransferBtn;
	
	public GameObject panelCharacterBasic;
	public GameObject panelCharacterSkill;
	public GameObject panelCharacterDetail;
	public GameObject panelCharacterTransfer;
	
	public RectTransform characterContent;
	public GameObject characterHead;
	public RectTransform careerContent;
	public GameObject careerRow;

	public GameObject dragZone;
	public Text characterName;
	public Text characterTotalFc;
	public Text characterLevel;
	public Text characterCoin;
	public Text characterDescribe;
	public Text characterDescribe2;
	public Text characterAbility;
	public GameObject characterSkill1Panel;
	public GameObject characterSkill2Panel;
	
	public Transform characterPoint;

	[HideInInspector]
	public GameObject characterView;

	private GameObject selectedCharacter;

	private GameObject openedCharacterPanel;
	private Button clickedCharacterBtn;

	void Start () {
		//设定一下DRAG ZONE
		EventTriggerListener.Get(dragZone).onDrag = onDragZone;
		EventTriggerListener.Get(dragZone).onClick = onClickZone;
	}

	public void Init(){
		OpenCharacterBasic();
		refreshCharacterPanel();
	}

	public void OpenCharacterBasic(){
		setCharacterPanel(characterBasicBtn, panelCharacterBasic);
	}

	public void OpenCharacterSkill(){
		setCharacterPanel(characterSkillBtn, panelCharacterSkill);
	}

	public void OpenCharacterDetail(){
		setCharacterPanel(characterDetailBtn, panelCharacterDetail);
	}

	public void OpenCharacterTransfer(){
		setCharacterPanel(characterTransferBtn, panelCharacterTransfer);
	}

	public void UpgradeHero(){
		JsonClass obj = new JsonClass();
		obj["tid"] = selectedCharacter.name;
		GameServer.Instance.Request(ServerAction.upgradeHero, obj, delegate() {
			string level = selectedCharacter.GetComponentInChildren<Text>().text;
			selectedCharacter.GetComponentInChildren<Text>().text = (int.Parse(level) + 1).ToString();
			SelectCharacter(selectedCharacter);
			TownManager.Instance.coinNum.text = GameServer.data["user"]["coin"];
		});
	}

	public void UpgradeSkill(int type){
		JsonClass obj = new JsonClass();
		obj["tid"] = selectedCharacter.name;
		obj["index"] = type;
		GameServer.Instance.Request(ServerAction.upgradeHeroSkill, obj, delegate() {
			SelectCharacter(selectedCharacter);
			TownManager.Instance.coinNum.text = GameServer.data["user"]["coin"];
		});
	}

	public void TransferMainHero(int tid){
		JsonClass obj = new JsonClass();
		obj["tid"] = tid;
		GameServer.Instance.Request(ServerAction.transferMainHero, obj, delegate() {
			refreshCharacterPanel();
		});
	}

	private void onDragZone(GameObject sender, PointerEventData eventData){
		characterView.transform.Rotate(0, eventData.delta.x * -1, 0);
	}

	private void onClickZone(GameObject sender, PointerEventData eventData){
		characterView.GetComponent<BaseUnit>().PlayRandom();
	}

	private void refreshCharacterPanel(){
		Helper.DestroyChildren(characterContent);
		Helper.DestroyChildren(careerContent);
		JsonNode heroes = GameServer.data["heroes"];
		string mainHero = GameServer.data["user"]["mainHero"];
		SetCareerBar(heroes[mainHero], true);
		int count = 1 - 3;
		foreach (string key in heroes.Keys) {
			if(heroes[key]["type"] != "1" || key == mainHero) continue;
			SetCareerBar(heroes[key], false);
			count++;
		}
		if(count > 0){
			careerContent.GetComponent<RectTransform>().sizeDelta = new Vector2(275, 133 + 41 * count);
		}

		SetColumnBar(heroes[mainHero]);
		count = 1 - 6;
		foreach (string key in heroes.Keys) {
			if(heroes[key]["type"] == "1") continue;
			SetColumnBar(heroes[key]);
			count++;
		}
		//计算一下content的高度
		if(count > 0){
			characterContent.GetComponent<RectTransform>().sizeDelta = new Vector2(79, 383 + 63 * count);
		}
	}

	private void SetCareerBar(JsonNode one, bool isMain){
		GameObject unit = Instantiate(careerRow) as GameObject;
		unit.transform.SetParent(careerContent, false);
		if(isMain) unit.transform.GetChild(0).GetComponent<Text>().text = "当前职业";
		unit.transform.GetChild(1).GetComponent<Text>().text = one["name"];
		unit.GetComponent<Button>().onClick.AddListener(delegate(){
			TransferMainHero(one["tid"]);
		});
	}

	private void SetColumnBar(JsonNode one){
		GameObject unit = Instantiate(characterHead) as GameObject;
		unit.name = one["tid"];
		unit.transform.SetParent(characterContent, false);
		unit.GetComponentInChildren<Image>().sprite = GameManager.Instance.GetHeadImage(one["tid"]);
		unit.GetComponentInChildren<Text>().text = one["level"];
		unit.GetComponent<Button>().onClick.AddListener(delegate(){
			SelectCharacter(unit);
		});
		if(one["type"] == 1) SelectCharacter(unit);
	}

	private void SelectCharacter(GameObject sender){
		ColorBlock colorBlock = ColorBlock.defaultColorBlock;
		if(selectedCharacter) selectedCharacter.GetComponent<Button>().colors = colorBlock;
		if(characterView) Destroy(characterView);
		colorBlock.normalColor = colorBlock.highlightedColor = Color.yellow;
		selectedCharacter = sender;
		selectedCharacter.GetComponent<Button>().colors = colorBlock;
		string tid = sender.name;
		//get hero object
		JsonNode hero = GameServer.data["heroes"][tid];
		characterName.text = hero["name"];
		characterTotalFc.text = hero["totalFc"];
		characterLevel.text = "Lv" + hero["level"];
		characterCoin.text = hero["upgradeCoin"];
		characterDescribe.text = characterDescribe2.text = hero["describe"];
		string ability = "";
		ability += "物攻：" + hero["atk"] + "\n";
		ability += "魔攻：" + hero["matk"] + "\n";
		ability += "生命：" + hero["hp"];
		characterAbility.text = ability;
		characterSkill1Panel.transform.FindChild("Name").GetComponent<Text>().text = "主动技能 - " + hero["skill1"]["name"];
		characterSkill1Panel.transform.FindChild("Level").GetComponent<Text>().text = "Lv" + hero["skill1"]["level"];
		characterSkill1Panel.transform.FindChild("Coin").GetComponent<Text>().text = "Lv" + hero["skill1"]["upgradeCoin"];
		characterSkill1Panel.transform.FindChild("Describe").GetComponent<Text>().text = hero["skill1"]["describe"];
		characterSkill2Panel.transform.FindChild("Name").GetComponent<Text>().text = "被动技能 - " + hero["skill2"]["name"];
		characterSkill2Panel.transform.FindChild("Level").GetComponent<Text>().text = "Lv" + hero["skill2"]["level"];
		characterSkill2Panel.transform.FindChild("Coin").GetComponent<Text>().text = "Lv" + hero["skill2"]["upgradeCoin"];
		characterSkill2Panel.transform.FindChild("Describe").GetComponent<Text>().text = hero["skill2"]["describe"];
		characterView = Instantiate(Resources.Load("Unit/" + tid)) as GameObject;
		characterView.transform.SetParent(characterPoint, false);
		//非主角英雄不显示转换按钮
		bool flag = hero["type"] == "1";
		characterTransferBtn.gameObject.SetActive(flag);
	}

	private void setCharacterPanel(Button btn, GameObject panel){
		if(openedCharacterPanel) openedCharacterPanel.SetActive(false);
		ColorBlock colorBlock = ColorBlock.defaultColorBlock;
		if(clickedCharacterBtn) clickedCharacterBtn.colors = colorBlock;
		colorBlock.normalColor = colorBlock.highlightedColor = Color.yellow;
		btn.colors = colorBlock;
		clickedCharacterBtn = btn;
		panel.SetActive(true);
		openedCharacterPanel = panel;
	}

}
