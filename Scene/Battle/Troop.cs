using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Troop : MonoBehaviour {

	public enum Side
	{
		my, enemy
	}

	public Side side;

	[HideInInspector]
	public bool auto;
	[HideInInspector]
	public Troop opponent;
	[HideInInspector]
	public List<BaseUnit> team;

	private int skillBtnIndex = 2;

	public void Init(){
		team = new List<BaseUnit>();
		int posIndex = 1;
		Dictionary<JsonNode, int> battleTeam = new Dictionary<JsonNode, int>();
		if(side == Side.my){
			auto = false;
			opponent = BattleManager.Instance.enemyTroop;
			string mainHero = GameServer.data["user"]["mainHero"];
			JsonNode recruitTeam = GameServer.data["user"]["battleTeam"];
			JsonNode heroes = GameServer.data["heroes"];
			battleTeam.Add(heroes[mainHero], heroes[mainHero]["weight"]);
			for (int i = 0; i < recruitTeam.Count; i++){
				string tid = recruitTeam[i];
				battleTeam.Add(heroes[tid], heroes[tid]["weight"]);
			}
		}else{
			auto = true;
			opponent = BattleManager.Instance.myTroop;
			JsonNode enemyTeam;
			if(GameManager.Instance.battleType == BattleType.mine){
				enemyTeam = GameServer.data["mine"]["enemy"];
			}else{
				enemyTeam = GameServer.data["arena"]["battleData"];
			}
			for (int i = 0; i < enemyTeam.Count; i++){
				battleTeam.Add(enemyTeam[i], enemyTeam[i]["weight"]);
			}
		}
		foreach (var item in battleTeam.OrderByDescending(s=>s.Value))
		{
			GenUnit(item.Key, posIndex);
			posIndex++;
		}

		if(!BattleManager.Instance.specialMap){
			iTween.MoveAdd(this.gameObject, iTween.Hash("x", side == Side.my ? -15 : 15, "easeType", "linear", "time", 3f));
			foreach(BaseUnit unit in team){
				unit.DoRun();
			}
		}else{
			foreach(BaseUnit unit in team){
				unit.DoRun();
				unit.navMeshAgent.destination = Vector3.zero;
			}
		}
	}

	private void GenUnit(JsonNode data, int posIndex){
		Vector3 initPos = Vector3.zero;
		GameObject skillBtn = null;
		if(side == Side.my){
			if(data["type"] == 1) skillBtn = BattleManager.Instance.skill1;
			else{
				skillBtn = typeof(BattleManager).GetField("skill" + skillBtnIndex.ToString()).GetValue(BattleManager.Instance) as GameObject;
				skillBtnIndex++;
			}
			switch (posIndex) {
			case 1:
				initPos = new Vector3(22f, 0, 0.4f);
				break;
			case 2:
				initPos = new Vector3(25f, 0, -1.5f);
				break;
			case 3:
				initPos = new Vector3(28f, 0, 3f);
				break;
			case 4:
				initPos = new Vector3(31f, 0, -5f);
				break;
			}
		}else{	
			switch (posIndex) {
			case 1:
				initPos = new Vector3(-22f, 0, -1.5f);
				break;
			case 2:
				initPos = new Vector3(-25f, 0, 0.4f);
				break;
			case 3:
				initPos = new Vector3(-28f, 0, -5f);
				break;
			case 4:
				initPos = new Vector3(-31f, 0, 3f);
				break;
			}
		} 
		BaseUnit one = (Instantiate(Resources.Load("Unit/" + data["tid"]), initPos, Quaternion.identity) as GameObject).GetComponent<BaseUnit>();
		one.Init(data, this, skillBtn);
		one.transform.SetParent(this.transform, false);
		team.Add(one);
	}

	public void StartFight(){
		foreach(BaseUnit unit in team){
			unit.StartFight();
		}
	}

	public void Victory(){
		foreach(BaseUnit unit in team){
			StartCoroutine(unit.DoVictory());
		}
	}

	public int GetDeadNum(){
		int deadNum = 0;
		foreach(BaseUnit unit in team){
			if(unit.current_hp == 0) deadNum++;
		}
		return deadNum;
	}

	public void InformDead(){
		if(GetDeadNum() < team.Count) return;
		opponent.Victory();
		BattleManager.Instance.GameOver(side == Side.enemy);
	}

	public BaseUnit hpLostMax()
	{
		BaseUnit aimOne = null;
		foreach(BaseUnit target in opponent.team){
			if(target.current_hp == 0) continue;
			if(!aimOne) {
				aimOne = target;
				continue;
			}
			if(target.current_hp < aimOne.current_hp) {
				aimOne = target;
			}
		}
		return aimOne;
	}

	public BaseUnit hpLostProportionMax()
	{
		BaseUnit aimOne = null;
		foreach(BaseUnit target in opponent.team){
			if(target.current_hp == 0) continue;
			if(!aimOne) {
				aimOne = target;
				continue;
			}
			if(target.current_hp / target.hp < aimOne.current_hp / aimOne.hp) {
				aimOne = target;
			}
		}
		return aimOne;
	}

	public BaseUnit GetRandEnemy()
	{
		List<BaseUnit> aliveTeam = new List<BaseUnit>();
		foreach(BaseUnit target in opponent.team){
			if(target.current_hp > 0) {
				aliveTeam.Add(target);
			}
		}
		if(aliveTeam.Count < 1) return null;
		int randNum = Random.Range(0, aliveTeam.Count);
		return(aliveTeam[randNum]);
	}
}
