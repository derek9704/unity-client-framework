using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Snowman : BaseUnit {

	public override void SetProperty(){
		preAtkTime = 10f / 30;
		preSkill1Time = 22f / 30;
		preSkill2Time = 22f / 30;
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
		if(skill1 != null) StartCoroutine(Blizzard());
	}
	
	IEnumerator Blizzard ()
	{
		int count = (int)skill1.arg3;
		for (int i = 0; i < count; i++) {
			BaseUnit unit = troop.GetRandEnemy();
			if(!unit) return false;
			GameObject eff = Instantiate(BattleManager.Instance.GetEffect("12361")) as GameObject;
			eff.transform.position = unit.transform.position + Vector3.up * 10;
			iTween.MoveTo(eff, iTween.Hash("position", unit.transform.position, "easeType", "linear", "time", 0.45f));
			yield return new WaitForSeconds(0.6f);
			float dmg = CalcDamage() + skill1.arg1;
			unit.Damage(dmg, this);
			int percentage = (skill1.level - unit.level) * 20 + 100;
			if(Helper.Rand100Hit(percentage)){
				Dot dot = new Dot();
				dot.type = DotType.freeze;
				dot.duration = skill1.arg4;
				dot.unit = this;
				unit.AddDot(dot);
			}
		}
	}

	public override void DeadEvent(){
		if(skill2 != null){
			foreach (var unit in troop.opponent.team) {
				float dmg = CalcDamage() + skill2.arg1;
				unit.Damage(dmg, this);
			}
		}
	}
}