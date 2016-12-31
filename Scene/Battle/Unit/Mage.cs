using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mage : BaseUnit {

	public override void SetProperty(){
		preAtkTime = 6f / 30;
		preSkill1Time = 22f / 30;
		preSkill2Time = 12f / 30;
		radius = 0.5f;
		attackEffect = BattleManager.Instance.GetEffect("12378");
	}

	private BaseUnit aim;

	public override void DoNormalAttack(){
		aim = target;
		GameObject arrow = Instantiate(Resources.Load("Unit/6W")) as GameObject;
		arrow.transform.position = transform.position + new Vector3(0, .5f, 0);
		iTween.MoveTo (arrow, iTween.Hash("position", target.transform.position + new Vector3(0, .5f, 0), "easeType", "linear", "speed", 20, "oncomplete", "FinishFlying", "oncompleteparams", arrow, "oncompletetarget", this.gameObject));
	}
	
	private void FinishFlying(GameObject arrow){
		Destroy(arrow);
		base.DoNormalAttack(aim);
	}
	
	public override void DoSkill1(){
		if(skill1 != null) {
			GameObject eff = Instantiate(BattleManager.Instance.GetEffect("12355")) as GameObject;
			eff.transform.SetParent(this.transform, false);
			List<BaseUnit> list = Helper.ShuffleList(troop.opponent.team);
			int count = (int)skill1.arg3;
			if(Helper.Rand100Hit((int)skill1.arg4)) count++;
			foreach (var unit in list) {
				float dmg = CalcDamage() + skill1.arg1;
				unit.Damage(dmg, this);
				eff = Instantiate(BattleManager.Instance.GetEffect("12355_1")) as GameObject;
				eff.transform.SetParent(unit.transform, false);
				count--;
				if(count == 0) return;
			}
		}
	}

	public override void StartFight(){
		if(skill2 != null){
			foreach (var unit in troop.team) {
				Dot dot = new Dot();
				dot.type = DotType.matkUp;
				dot.num = skill2.arg1;
				dot.unit = this;
				unit.AddDot(dot);
				if(unit == this){
					dot.eff = Instantiate(BattleManager.Instance.GetEffect("12364")) as GameObject;
					dot.eff.transform.SetParent(this.transform, false);
				}
			}
		}
		base.StartFight();
	}
	
	public override void DeadEvent(){
		if(skill2 != null){
			foreach (var unit in troop.team) {
				unit.RemoveDot(DotType.matkUp);
			}
		}
	}
}