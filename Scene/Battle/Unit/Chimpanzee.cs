using UnityEngine;
using System.Collections;

public class Chimpanzee : BaseUnit {

	public override void SetProperty(){
		preAtkTime = 10f / 30;
		preSkill1Time = 10f / 30;
		preSkill2Time = 12f / 30;
		radius = 0.8f;
		attackEffect = BattleManager.Instance.GetEffect("12378");
	}

	public override void DoSkill1(){
		if(skill1 != null) {
			foreach (var unit in troop.opponent.team) {
				float dmg = CalcDamage() + skill1.arg1;
				unit.Damage(dmg, this);
				unit.BeKnockBacked(); //击飞
			}
		}
	}

	public override void StartFight(){
		if(skill2 != null) {
			Dot dot = new Dot();
			dot.type = DotType.reduceDmg;
			dot.num = skill2.arg1;
			dot.unit = this;
			AddDot(dot);
		}
		base.StartFight();
	}
}
