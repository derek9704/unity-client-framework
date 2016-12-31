using UnityEngine;
using System.Collections;

public class Rabbit : BaseUnit {

	public override void SetProperty(){
		preAtkTime = 8f / 30;
		preSkill1Time = 18f / 30;
		preSkill2Time = 18f / 30;
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
		if(skill1 != null){
			GameObject arrow = Instantiate(Resources.Load("Unit/6W")) as GameObject;
			arrow.transform.position = transform.position + new Vector3(0, .5f, 0);
			iTween.MoveTo (arrow, iTween.Hash("position", target.transform.position + new Vector3(0, .5f, 0), "easeType", "linear", "speed", 20, "oncomplete", "FinishSkillFlying", "oncompleteparams", arrow, "oncompletetarget", this.gameObject));
		}
	}
	
	private void FinishSkillFlying(GameObject arrow){
		Destroy(arrow);
		foreach (var unit in troop.opponent.team) {
			float dmg = CalcDamage() + skill1.arg1;
			unit.Damage(dmg, this);
			int percentage = (skill1.level - unit.level) * 20 + 100;
			if(Helper.Rand100Hit(percentage)){
				Dot dot = new Dot();
				dot.type = DotType.speedDown;
				dot.num = skill1.arg3 / 100;
				dot.duration = skill1.arg4;
				dot.unit = this;
				unit.AddDot(dot);
			}
		}
	}
}