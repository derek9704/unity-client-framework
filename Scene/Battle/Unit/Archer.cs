using UnityEngine;
using System.Collections;

public class Archer : BaseUnit {

	public override void SetProperty(){
		preAtkTime = 7f / 30;
		preSkill1Time = 25f / 30;
		preSkill2Time = 10f / 30;
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

	private BaseUnit skillAim;

	public override void DoSkill1(){
		if(skill1 != null) {
			skillAim = troop.hpLostMax();
			GameObject arrow = Instantiate(Resources.Load("Unit/6W")) as GameObject;
			arrow.transform.position = transform.position + new Vector3(0, .5f, 0);
			iTween.MoveTo (arrow, iTween.Hash("position", skillAim.transform.position + new Vector3(0, .5f, 0), "easeType", "linear", "speed", 20, "oncomplete", "FinishSkillFlying", "oncompleteparams", arrow, "oncompletetarget", this.gameObject));
		}
	}

	private void FinishSkillFlying(GameObject arrow){
		Destroy(arrow);
		float dmg = CalcDamage() + skill1.arg1;
		skillAim.Damage(dmg, this);
	}

	public override void StartFight(){
		if(skill2 != null) {
			foreach (var unit in troop.team) {
				Dot dot = new Dot();
				dot.type = DotType.atkUp;
				dot.num = skill2.arg1;
				dot.unit = this;
				unit.AddDot(dot);
				if(unit == this){
					dot.eff = Instantiate(BattleManager.Instance.GetEffect("12363")) as GameObject;
					dot.eff.transform.SetParent(this.transform, false);
				}
			}
		}
		base.StartFight();
	}

	public override void DeadEvent(){
		if(skill2 != null) {
			foreach (var unit in troop.team) {
				unit.RemoveDot(DotType.atkUp);
			}
		}
	}

}