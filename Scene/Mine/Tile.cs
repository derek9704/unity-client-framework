using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TileType{
	iron = 100,
	wall = 200,
	redGem = 301,
	yellowGem = 302,
	blueGem = 303,
	greenGem = 304,
	effCore = 400,
	chest = 500,
	torch = 600,
	entryArea = 1002,
	entry = 1001,
	aim = 2000,
	enemy = 900,
	camp = 700
}

public class Tile : MonoBehaviour {

	public int id;
	public int row;
	public int col;
	public TileType type;
	public int hp;
	public int current_hp;
	public bool locked;
	public bool visible;
	public bool destroyed;
	public bool canClick;

	private GameObject lockAnim;

//	无敌墙体	1
//	泥土墙体	10
//	脆弱墙体	10
//	普通墙体	10
//	坚固墙体	10
//	红宝石墙	20
//	黄宝石墙	21
//	蓝宝石墙	22
//	绿宝石墙	23
//	冲击核心	30
//	魔幻核心	31
//	生命之心	32
//	迷之宝箱	40
//	火盆	50
//	入口点	101
//	入口空地	100
//	目标点	200
//	敌方单位	90

	public void Init(int id, int row, int col, int type, int hp, int current_hp, bool locked, bool visible, bool destroyed, bool canClick){
		this.id = id;
		this.row = row;
		this.col = col;
		this.type = (TileType)type;
		this.hp = hp;
		this.current_hp = current_hp;
		this.destroyed = destroyed;
		this.canClick = canClick;
		SetVisible(visible, true);
		SetLock(locked);
		SetRatio();
	}

	public JsonClass ToJsonData(){
		JsonClass o = new JsonClass();
		o["id"] = id;
		o["row"] = row;
		o["col"] = col;
		o["type"] = (int)type;
		o["hp"] = hp;
		o["current_hp"] = current_hp;
		o["visible"] = visible;
		o["locked"] = locked;
		o["destroyed"] = destroyed;
		o["canClick"] = canClick;
		return o;
	}
	
	public void SetVisible(bool visible, bool init = false){
		if(!init){
			if(destroyed) return;
			if(this.visible == visible) return;
		}
		this.visible = visible;
		this.gameObject.SetActive(visible);
	}

	public void SetCanClick(bool canClick){
		if(this.canClick) return;
		this.canClick = canClick;
		if(canClick && (type == TileType.aim || type == TileType.enemy)){
			MineManager.Instance.LockAroundTile(this);
		}
	}

	public void defeatEnemy(){
		if(type != TileType.aim && type != TileType.enemy) return;
		SetVisible(false);
		destroyed = true;
		MineManager.Instance.RevealAroundTile(this);
		MineManager.Instance.UnlockAroundTile(this);
	}

	public void SetLock(bool locked){
		if(destroyed || type == TileType.entry || type == TileType.entryArea || type == TileType.aim || type == TileType.enemy || type == TileType.iron) return;
		if(this.locked == locked) return;
		if(locked){
			lockAnim = Instantiate(MineManager.Instance.GetEffect("Lock")) as GameObject;
			lockAnim.transform.SetParent(this.gameObject.transform, false);
		}else{
			Destroy(lockAnim);
		}
		this.locked = locked;
	}

	public void SetRatio(){
		if(hp == 0) return;
		float ratio = (float)current_hp / (float)hp;
		if(ratio <= 1f/3f) {
			Vector3 pos = transform.position;
			pos.y = -0.3f;
			this.transform.position = pos;
		}
		else if(ratio <= 2f/3f) {
			Vector3 pos = transform.position;
			pos.y = -0.15f;
			this.transform.position = pos;
		}
	}

	public void Click(){
		if(type == TileType.iron) return; //无敌墙体
		if(locked || !canClick) return;
		//hit enemy
		if(type == TileType.aim || type == TileType.enemy){
			//show enemy list
			MineManager.enemyPos = new Vector2(row, col);
			MineManager.Instance.ShowEnemyList(Helper.GetScreenPoint(transform.position));
			return;
		}
		//hit tile
		if(!MineManager.Instance.UseStamina(GameServer.data["mine"]["stamina"]["hit"])) return;
		//hammer
		MineManager.Instance.showHammer(transform.position);
		//hp
		current_hp -= 60;
		if(current_hp < 0) current_hp = 0;
		GameObject eff;
		if(current_hp == 0) {
			switch (type) {
			case TileType.redGem:
				GenReward(GemType.redGem);
				eff = Instantiate(MineManager.Instance.GetEffect("12353")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1.3f, 0);
				break;
			case TileType.yellowGem:
				GenReward(GemType.yellowGem);
				eff = Instantiate(MineManager.Instance.GetEffect("12354")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1.3f, 0);
				break;
			case TileType.blueGem:
				GenReward(GemType.blueGem);
				eff = Instantiate(MineManager.Instance.GetEffect("12355")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1.3f, 0);
				break;
			case TileType.greenGem:
				GenReward(GemType.greenGem);
				eff = Instantiate(MineManager.Instance.GetEffect("12356")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1.3f, 0);
				break;
			case TileType.wall:
				eff = Instantiate(MineManager.Instance.GetEffect("12346")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1f, 0);
				iTween.ShakePosition(Camera.main.gameObject, new Vector3(.1f, .1f, .1f), 0.5f);
				break;
			}
			SetVisible(false);
			destroyed = true;
			MineManager.Instance.RevealAroundTile(this);
		}
		else{
			iTween.ShakePosition(this.gameObject, new Vector3(.03f, .03f, .03f), 0.3f);
			switch (type) {
			case TileType.redGem:
				eff = Instantiate(MineManager.Instance.GetEffect("12353_1")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1.3f, 0);
				break;
			case TileType.yellowGem:
				eff = Instantiate(MineManager.Instance.GetEffect("12354_1")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1.3f, 0);
				break;
			case TileType.blueGem:
				eff = Instantiate(MineManager.Instance.GetEffect("12355_1")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1.3f, 0);
				break;
			case TileType.greenGem:
				eff = Instantiate(MineManager.Instance.GetEffect("12356_1")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1.3f, 0);
				break;
			case TileType.wall:
				eff = Instantiate(MineManager.Instance.GetEffect("12346_1")) as GameObject;
				eff.transform.position = this.transform.position + new Vector3(0, 1f, 0);
				break;
			}
			SetRatio();
		}
	}

	public void GenReward(GemType gemType){
		int maxNum = Random.Range(5, 8);
		for (int i = 0; i < maxNum; i++) {
			GameObject loot = Instantiate(MineManager.Instance.GetLoot(gemType.ToString())) as GameObject;
			loot.transform.SetParent(MineManager.Instance.lootPanel, false);
			Vector3 pos = Helper.GetScreenPoint(transform.position);
			pos.y += 50;
			loot.GetComponent<RectTransform>().anchoredPosition = pos;
		}
	}
}