using UnityEngine;
using System.Collections;

public class Cat : BaseUnit {

	public override void SetProperty(){
		preAtkTime = 8f / 30;
		preSkill1Time = 7f / 30;
		preSkill2Time = 7f / 30;
		radius = 0.8f;
		attackEffect = BattleManager.Instance.GetEffect("12378");
	}

	public override void DoSkill1(){
		if(skill1 != null) StartCoroutine(ClawHit());
	}

	IEnumerator ClawHit ()
	{
		float dmg = CalcDamage() + skill1.arg1;
		dmg /= skill1.arg3;
		for (int i = 0; i < (int)skill1.arg3; i++) {
			yield return new WaitForSeconds(0.2f);
			target.Damage(dmg, this);
		}
	}

	public override void DamageEvent(){
		if(skill2 != null) {
			float decrease = (hp - current_hp) / hp * 100 - 50;
			if(decrease > 0){
				Dot dot = new Dot();
				dot.type = DotType.atkUp;
				dot.num = skill2.arg1 * decrease / skill2.arg3;
				dot.unit = this;
				AddDot(dot);
			}
		}
	}
}