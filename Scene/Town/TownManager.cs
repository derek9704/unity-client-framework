using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TownManager : MonoBehaviour {

	public static TownManager Instance;
	
	public Text coinNum;
	public Text goldNum;
	public Text staminaNum;
	public Text userLevel;
	public Text userName;

	public GameObject btns1;
	public GameObject btns2;
	public GameObject btns3;

	public Transform point1;
	public Transform point2;
	public Transform point3;

	public Image playerHead;
	public Image staminaBar;

	public GameObject panel;
	public GameObject panelTeam;
	public GameObject panelCharacter;
	public GameObject panelArena;

	private TeamPanel teamPanel;
	private CharacterPanel characterPanel;
	private ArenaPanel arenaPanel;

	private GameObject openedBtns;
	private Vector3 lastCameraPos;
	
	private Vector3 mouseDownPos;
	private Vector3 lastMousePos;

	private GameObject chooseOne;

	public static int battleFlag = 0; //0未战斗1胜利2失败

	void Awake () {
		Instance = this;
	}

	void OnEnable()
	{
		NoticeCenter.OnRefreshStamina += RefreshStamina;
	}

	void OnDisable()
	{
		NoticeCenter.OnRefreshStamina -= RefreshStamina;
	}

	void Start () {
		teamPanel = GetComponent<TeamPanel>();
		characterPanel = GetComponent<CharacterPanel>();
		arenaPanel = GetComponent<ArenaPanel>();

		coinNum.text = GameServer.data["user"]["coin"];
		goldNum.text = (string)GameServer.data["user"]["gold"];
		userLevel.text = (string)GameServer.data["user"]["level"];
		userName.text = GameServer.data["user"]["name"];
		RefreshStamina();

		if(battleFlag > 0){
			battleFlag = 0;
			ClickUnit("Unit2");
			OpenArenaPanel();
		}
	}

	private void RefreshStamina(){
		staminaNum.text = GameServer.data["user"]["stamina"] + " / " + GameServer.data["user"]["staminaMax"];
		staminaBar.fillAmount = (float)GameServer.data["user"]["stamina"] / (float)GameServer.data["user"]["staminaMax"];
	}

	void Update () {
		if(Camera.main.orthographicSize != 5f) return;
		if(InputControl.TouchCount() == 1){
			Vector3 nowPos = InputControl.GetPosition();
			if(InputControl.MouseDown()){
				lastMousePos = mouseDownPos = nowPos;
				Ray ray = Camera.main.ScreenPointToRay(mouseDownPos);
				RaycastHit hit;
				if ( Physics.Raycast(ray, out hit, 50) ){
					chooseOne = hit.transform.gameObject;
					chooseOne.GetComponent<BaseUnit>().SetBodyIllumin(new Color(0.3f, 0.3f, 0.3f));
				}
			}
			else if(InputControl.MouseUp()){
				if(chooseOne){
					if(Vector3.Distance(mouseDownPos, nowPos) < 5){
						ClickUnit(chooseOne.name);
					}
					chooseOne.GetComponent<BaseUnit>().SetBodyIllumin(Color.black);
					chooseOne = null;
				}
			}
			else if(InputControl.MouseMove()){
				Vector3 diff = nowPos - lastMousePos;
				lastMousePos = nowPos;
				if(Vector3.Distance(mouseDownPos, nowPos) < 10) return;
				Vector3 pos = Camera.main.transform.position + new Vector3(diff.x * -0.03f, 0, 0);
				if(pos.x < -6.88f) pos.x = -6.88f;
				if(pos.x > 7.09f) pos.x = 7.09f;
				Camera.main.transform.position = pos;
			}
		}
	}

	public void EnterMine(){
		GameManager.Instance.ChangeScene(SceneType.mine, ServerAction.enterMine);
	}

	public void RefreshMine(){
		GameServer.Instance.Request(ServerAction.refreshMine);
	}

	public void CloseBtns(){
		openedBtns.SetActive(false);
		Camera.main.transform.position = lastCameraPos;
		Camera.main.orthographicSize = 5f;
	}

	public void OpenTeamPanel(){
		panel.SetActive(true);
		panelTeam.SetActive(true);
		teamPanel.Init();
	}

	public void OpenCharacterPanel(){
		panel.SetActive(true);
		panelCharacter.SetActive(true);
		characterPanel.Init();
	}

	public void OpenArenaPanel(){
		GameServer.Instance.Request(ServerAction.getArenaInfo, null, delegate() {
			panel.SetActive(true);
			panelArena.SetActive(true);
			arenaPanel.Init();
		});
	}

	public void ClosePanel(){
		panel.SetActive(false);
		panelCharacter.SetActive(false);
		panelTeam.SetActive(false);
		panelArena.SetActive(false);
		Destroy(characterPanel.characterView);
	}

	private void ClickUnit(string name){
		if(openedBtns) openedBtns.SetActive(false);
		lastCameraPos = Camera.main.transform.position;
		Camera.main.orthographicSize = 1.87f;
		switch (name) {
		case "Unit1":
			btns1.SetActive(true);
			openedBtns = btns1;
			Camera.main.transform.position = point1.position;
			break;
		case "Unit2":
			btns2.SetActive(true);
			openedBtns = btns2;
			Camera.main.transform.position = point2.position;
			break;
		case "Unit3":
			btns3.SetActive(true);
			openedBtns = btns3;
			Camera.main.transform.position = point3.position;
			break;
		}
	}

}