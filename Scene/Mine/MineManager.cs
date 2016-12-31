using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class MineManager : MonoBehaviour {

	public static MineManager Instance;

	public Text coinNum;
	public Text goldNum;
	public Text staminaNum;
	public Image staminaBar;
	public Text userLevel;
	public Text userName;
	public GameObject hammer;
	public GameObject mist;
	public GameObject ground;
	public Transform tilePoint;
	public RectTransform lootPanel;
	public List<GameObject> blocks;
	public List<GameObject> effects;
	public List<GameObject> loots;

	public GameObject enemyList;
	public GameObject characterHead;

	private Texture2D mask;
	private Tile [][] tileMap;
	private static int usedStaminaCount = 0;

	private Vector3 mouseDownPos;
	private Vector3 lastMousePos;

	public static Vector2 enemyPos;
	public static int battleFlag = 0; //0未战斗1胜利2失败

	private static float orthographicSize = 0;
	private static Vector3 cameraPos = Vector3.zero;

	void Awake()
	{
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
		coinNum.text = GameServer.data["user"]["coin"];
		goldNum.text = GameServer.data["user"]["gold"];
		userLevel.text = GameServer.data["user"]["level"];
		userName.text = GameServer.data["user"]["name"];
		//set enemy
		JsonNode heroes = GameServer.data["mine"]["enemy"];
		RectTransform characterContent = enemyList.transform.GetChild(0).GetComponent<RectTransform>();
		for (int i = 0; i < heroes.Count; i++) {
			GameObject unit = Instantiate(characterHead) as GameObject;
			unit.GetComponentInChildren<Image>().sprite = GameManager.Instance.GetHeadImage(heroes[i]["tid"]);
			unit.GetComponentInChildren<Text>().text = heroes[i]["level"];
			unit.transform.SetParent(characterContent, false);
		}
		//set cloud
		JsonNode map = GameServer.data["mine"]["map"];
		int mapWidth = Mathf.CeilToInt((float)map.Count / 4);
		int mapHeight = Mathf.CeilToInt((float)map[0].Count / 4);
		mist.transform.localScale = new Vector3(mapWidth * 4, mapHeight * 4, 0);
		mist.transform.position = new Vector3(mapWidth * 2, 0.5f, mapHeight * 2);
		mist.GetComponent<Renderer>().material.mainTextureScale = new Vector2(mapWidth, mapHeight);
		//set around cloud
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				if(i == 0 && j == 0) continue;
				GameObject aroundMist = Instantiate(mist);
				aroundMist.transform.position = new Vector3(mapWidth * 4 * (0.5f + i), 0.5f, mapHeight * 4 * (0.5f + j));
				aroundMist.GetComponent<Renderer>().material.SetTexture("_Mask", null);
			}
		}
		//set ground
		ground.transform.localScale = new Vector3(mapWidth * 4 - 1, mapHeight * 4 - 1, 0);
		ground.transform.position = new Vector3(mapWidth * 2 - .5f, 0, mapHeight * 2 - .5f);
		ground.GetComponent<Renderer>().material.mainTextureScale = new Vector2(mapWidth * 4 - 1, mapHeight * 4 - 1);
		//generate mask
		mask = new Texture2D(mapWidth * 4, mapHeight * 4, TextureFormat.ARGB32, false);
		mist.GetComponent<Renderer>().material.SetTexture("_Mask", mask);
		for (int i = 0; i < mapWidth * 4; i++) {
			for (int j = 0; j < mapHeight * 4; j++) {
				Color32 cc = new Color(1, 1, 1, 1);
				mask.SetPixel(i, j, cc);
			}
		}
		//generate tile
		int nRow = map.Count;
		int nCol = map[0].Count;
		tileMap = new Tile[nRow][];
		List<Tile> entry = new List<Tile>();
		for (int i = 0; i < nRow; ++i) {
			tileMap[i] = new Tile[nCol];
			for (int j = 0; j < nCol; ++j) {
				JsonNode data = map[i][j];
				GameObject tile = Instantiate(GetBlock(data["id"])) as GameObject;
				Tile script = tile.AddComponent<Tile>();
				tile.transform.position = new Vector3(i + .5f, 0, j + .5f);
				script.Init(data["id"], i, j, data["type"], data["hp"], data["current_hp"], data["locked"], data["visible"], data["destroyed"], data["canClick"]);
				tile.transform.SetParent(tilePoint, false);
				tileMap[i][j] = script;
				if(data["type"] == (int)TileType.entry || data["type"] == (int)TileType.entryArea || data["destroyed"] || data["visible"]) {
					//搞下云
					SetMask(i, j);
					entry.Add(script);
					//定位摄像机位置
					if(data["type"] == (int)TileType.entry){
						if(GameServer.data["mine"]["times"] == 0 || cameraPos == Vector3.zero)
							cameraPos = Camera.main.transform.position = new Vector3(i + 16, 25, j + 13);
						else
							Camera.main.transform.position = cameraPos;
					}
				}
			}
		}
		if(GameServer.data["mine"]["times"] == 0){
			foreach (var item in entry) {
				RevealAroundTile(item);
			}
		}else{
			mask.Apply();
			if(orthographicSize != 0) Camera.main.orthographicSize = orthographicSize;
		}
		if(battleFlag > 0){
			if(battleFlag == 1){
				tileMap[(int)enemyPos.x][(int)enemyPos.y].defeatEnemy();
			}else{
				UseStamina(GameServer.data["mine"]["stamina"]["lose"] - GameServer.data["mine"]["stamina"]["win"]);
			}
			battleFlag = 0;
		}else{
			usedStaminaCount = 0;
		}
		RefreshStamina();
	}

	public GameObject GetBlock(string id)
	{
		foreach (var item in blocks) {
			if(item.name == id) return item;
		}
		return null;
	}

	public GameObject GetLoot(string id)
	{
		foreach (var item in loots) {
			if(item.name == id) return item;
		}
		return null;
	}

	public GameObject GetEffect(string id)
	{
		foreach (var item in effects) {
			if(item.name == id) return item;
		}
		return null;
	}

	private void RefreshStamina(){
		int nowNum = GameServer.data["user"]["stamina"] - usedStaminaCount;
		staminaNum.text = nowNum.ToString() + " / " + GameServer.data["user"]["staminaMax"];
		staminaBar.fillAmount = nowNum / (float)GameServer.data["user"]["staminaMax"];
	}

	public void ShowEnemyList(Vector2 pos){
		pos.y += 90;
		enemyList.GetComponent<RectTransform>().anchoredPosition = pos;
		enemyList.SetActive(true);
	}

	public void HideEnemyList(){
		enemyList.SetActive(false);
	}
	
	public void RevealAroundTile(Tile tile){
		int leftCol = Mathf.Max(tile.col - 1, 0);
		int rightCol = Mathf.Min(tile.col + 1, tileMap[0].Length - 1);
		int upRow = Mathf.Max(tile.row - 1, 0);
		int downRow = Mathf.Min(tile.row + 1, tileMap.Length - 1);
		for (int i = upRow; i <= downRow; i++) {
			for (int j = leftCol; j <= rightCol; j++) {
				tileMap[i][j].SetVisible(true);
				SetMask(i, j);
				int abs = Mathf.Abs(j - tile.col) + Mathf.Abs(i - tile.row);
				tileMap[i][j].SetCanClick(abs == 1);
			}
		}
		mask.Apply();
	}

	public void LockAroundTile(Tile tile){
		int leftCol = Mathf.Max(tile.col - 1, 0);
		int rightCol = Mathf.Min(tile.col + 1, tileMap[0].Length - 1);
		int upRow = Mathf.Max(tile.row - 1, 0);
		int downRow = Mathf.Min(tile.row + 1, tileMap.Length - 1);
		for (int i = upRow; i <= downRow; i++) {
			for (int j = leftCol; j <= rightCol; j++) {
				if(i == tile.row && j == tile.col) continue;
				tileMap[i][j].SetLock(true);
			}
		}
	}
	
	public void UnlockAroundTile(Tile tile){
		int leftCol = Mathf.Max(tile.col - 1, 0);
		int rightCol = Mathf.Min(tile.col + 1, tileMap[0].Length - 1);
		int upRow = Mathf.Max(tile.row - 1, 0);
		int downRow = Mathf.Min(tile.row + 1, tileMap.Length - 1);
		for (int i = upRow; i <= downRow; i++) {
			for (int j = leftCol; j <= rightCol; j++) {
				if(i == tile.row && j == tile.col) continue;
				//判断一下周围是否还有怪物
				if(!DoseAroundTileExistMonster(tileMap[i][j]))
					tileMap[i][j].SetLock(false);
			}
		}
	}

	private bool DoseAroundTileExistMonster(Tile tile){
		int leftCol = Mathf.Max(tile.col - 1, 0);
		int rightCol = Mathf.Min(tile.col + 1, tileMap[0].Length - 1);
		int upRow = Mathf.Max(tile.row - 1, 0);
		int downRow = Mathf.Min(tile.row + 1, tileMap.Length - 1);
		for (int i = upRow; i <= downRow; i++) {
			for (int j = leftCol; j <= rightCol; j++) {
				if(i == tile.row && j == tile.col) continue;
				if(tileMap[i][j].visible && tileMap[i][j].canClick && (tileMap[i][j].type == TileType.aim || tileMap[i][j].type == TileType.enemy)){
					return true;
				}
			}
		}
		return false;
	}

	private void SetMask(int i, int j){
		mask.SetPixel(i, j, new Color(1, 1, 1, 0));
	}

	public void showHammer(Vector3 pos){
		pos.y += 1f;
		hammer.transform.position = pos;
		hammer.SetActive(true);
	}

	public void Leave(){
		JsonClass obj = new JsonClass();
		obj["stamina"] = usedStaminaCount;
		obj["coin"] = 0;
		obj["gold"] = 0;
		obj["map"] = SaveMap();
		GameManager.Instance.ChangeScene(SceneType.town, ServerAction.leaveMine, obj);
	}

	private JsonArray SaveMap(){
		JsonArray map = new JsonArray();
		for (int i = 0; i < tileMap.Length; i++) {
			JsonArray tmp = new JsonArray();
			map.Add(tmp);
			for (int j = 0; j < tileMap[0].Length; j++) {
				tmp.Add(tileMap[i][j].ToJsonData());
			}
		}
		GameServer.data["mine"]["map"] = map;
		GameServer.data["mine"]["times"] = GameServer.data["mine"]["times"] + 1;
		return map;
	}

	public void StartBattle(){
		if(!UseStamina(GameServer.data["mine"]["stamina"]["win"])) return;
		SaveMap();
		GameManager.Instance.battleType = BattleType.mine;
		GameManager.Instance.ChangeScene(SceneType.battle);
	}

	public bool UseStamina(int num = 0){
		int initNum = GameServer.data["user"]["stamina"];
		int nowNum = initNum - num - usedStaminaCount;
		if(nowNum < 0) {
			GameManager.Instance.ErrorHint(Lang.NOT_ENOUGH_STAMINA);
			return false;
		}
		usedStaminaCount += num;
		RefreshStamina();
		return true;
	}

	void Update () {
		if(EventSystem.current.IsPointerOverGameObject()) return;
		if(InputControl.TouchCount() == 1){
			Vector3 nowPos = InputControl.GetPosition();
			if(InputControl.MouseDown()){
				lastMousePos = mouseDownPos = nowPos;
			}
			else if(InputControl.MouseUp()){
				HideEnemyList();
				if(Vector3.Distance(mouseDownPos, nowPos) < 5){
					Ray ray = Camera.main.ScreenPointToRay(mouseDownPos);
					RaycastHit hit;
					if ( Physics.Raycast(ray, out hit, 100) ){
						ExecClick(hit);
					}
				}
			}
			else if(InputControl.MouseMove()){
				if(Vector3.Distance(mouseDownPos, nowPos) < 10) return;
				Vector3 diff = nowPos - lastMousePos;
				lastMousePos = nowPos;
				Camera.main.transform.Translate(new Vector3(diff.x * -0.01f, diff.y * -0.01f, diff.y * 0.84f * -0.01f));
				Vector3 pos = Camera.main.transform.position;
				//todo pos边界
				cameraPos = Camera.main.transform.position = pos;
			}
		}else if(InputControl.TouchCount() == 2){
			// 记录两个手指的位置
			Vector2 finger1 = new Vector2();
			Vector2 finger2 = new Vector2();
			
			// 记录两个手指的移动
			Vector2 mov1 = new Vector2();
			Vector2 mov2 = new Vector2();
			
			for (int i=0; i<2; i++ )
			{
				Touch touch = Input.touches[i];
				
				if (touch.phase == TouchPhase.Ended )
					break;
				
				if ( touch.phase == TouchPhase.Moved )
				{
					float mov = 0;
					if (i == 0)
					{
						finger1 = touch.position;
						mov1 = touch.deltaPosition;
					}
					else
					{
						finger2 = touch.position;
						mov2 = touch.deltaPosition;
						
						if (finger1.x > finger2.x)
						{
							mov = mov1.x;
						}
						else
						{
							mov = mov2.x;
						}
						
						if (finger1.y > finger2.y)
						{
							mov+= mov1.y;
						}
						else
						{
							mov+= mov2.y;
						}
						Camera.main.orthographicSize += -mov * Time.deltaTime;
						if(Camera.main.orthographicSize > 4) Camera.main.orthographicSize = 4;
						if(Camera.main.orthographicSize < 2) Camera.main.orthographicSize = 2;
						orthographicSize = Camera.main.orthographicSize;
					}
				}
			}
		}
	}

	private void ExecClick(RaycastHit hit){
		hit.transform.GetComponent<Tile>().Click();
	}

}
