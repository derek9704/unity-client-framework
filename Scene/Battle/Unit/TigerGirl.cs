using UnityEngine;
using System.Collections;

public class TigerGirl : BaseUnit {
	
	public override void SetProperty(){
		preAtkTime = 14f / 30;
		preSkill1Time = 16f / 30;
		preSkill2Time = 16f / 30;
		radius = 0.5f;
		attackEffect = BattleManager.Instance.GetEffect("12378");
	}
	
	private int normalAttackCount = 0;
	private BaseUnit aim;

	public override void DoNormalAttack(){
		aim = target;
		normalAttackCount++;
		GameObject arrow = Instantiate(Resources.Load("Unit/4W")) as GameObject;
		arrow.transform.position = transform.position + new Vector3(0, .5f, 0);
		iTween.MoveTo (arrow, iTween.Hash("position", target.transform.position + new Vector3(0, .5f, 0), "easeType", "linear", "speed", 20, "oncomplete", "FinishFlying", "oncompleteparams", arrow, "oncompletetarget", this.gameObject));
	}
	
	private void FinishFlying(GameObject arrow){
		if(skill2 != null && normalAttackCount == (int)skill2.arg3){
			normalAttackCount = 0;
			Instantiate(attackEffect, aim.transform.position + new Vector3(0, .5f, 0), Quaternion.identity);
			float dmg = CalcDamage() + skill2.arg1;
			AddGauge(100);
			aim.Damage(dmg, this);
			aim.AddGauge(-1 * skill2.arg4);
		}else{
			base.DoNormalAttack(aim);
		}
		Destroy(arrow);
	}

	private BaseUnit skillAim;
	private int bounceCount;
	
	public override void DoSkill1(){
		if(skill1 != null){
			bounceCount = (int)skill1.arg3;
			skillAim = target;
			GameObject arrow = Instantiate(Resources.Load("Unit/4W")) as GameObject;
			arrow.transform.position = transform.position + new Vector3(0, .5f, 0);
			iTween.MoveTo (arrow, iTween.Hash("position", skillAim.transform.position + new Vector3(0, .5f, 0), "easeType", "linear", "speed", 20, "oncomplete", "FinishSkillFlying", "oncompleteparams", arrow, "oncompletetarget", this.gameObject));
		}
	}
	
	private void FinishSkillFlying(GameObject arrow){
		float dmg = CalcDamage() + skill1.arg1;
		skillAim.Damage(dmg, this);
		bounceCount--;
		if(bounceCount < 0){
			Destroy(arrow);
		}else{
			skillAim = skillAim.GetNearestFellow();
			if(!skillAim){
				Destroy(arrow);
			}else{
				iTween.MoveTo (arrow, iTween.Hash("position", skillAim.transform.position + new Vector3(0, .5f, 0), "easeType", "linear", "speed", 20, "oncomplete", "FinishSkillFlying", "oncompleteparams", arrow, "oncompletetarget", this.gameObject));
			}
		}
	}

}