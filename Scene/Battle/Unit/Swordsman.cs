using UnityEngine;
using System.Collections;

public class Swordsman : BaseUnit {
	
	public override void SetProperty(){
		preAtkTime = 6f / 30;
		preSkill1Time = 11f / 30;
		preSkill2Time = 12f / 30;
		radius = 0.5f;
		attackEffect = BattleManager.Instance.GetEffect("12377");
	}

	public override void DoNormalAttack(){
		GameObject eff = Instantiate(BattleManager.Instance.GetEffect("12353")) as GameObject;
		eff.transform.position = this.transform.position;
		eff.transform.rotation = this.transform.rotation;
		base.DoNormalAttack();
	}

	public override void DoSkill1(){
		if(skill1 != null){
			foreach (var unit in troop.opponent.team) {
				float dmg = CalcDamage() + skill1.arg1;
				unit.Damage(dmg, this);
			}
		}
	}

	public override void StartFight(){
		if(skill2 != null){
			foreach (var unit in troop.team) {
				Dot dot = new Dot();
				dot.type = DotType.shield;
				dot.num = skill2.arg1;
				dot.unit = this;
				unit.AddDot(dot);
			}
		}
		base.StartFight();
	}
}