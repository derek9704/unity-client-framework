using UnityEngine;
using System.Collections;

public class Dragon : BaseUnit {

	public override void SetProperty(){
		preAtkTime = 11f / 30;
		preSkill1Time = 15f / 30;
		preSkill2Time = 15f / 30;
		radius = 0.8f;
		attackEffect = BattleManager.Instance.GetEffect("12378");
	}

	private int normalAttackCount = 0;
	private BaseUnit aim;

	public override void DoNormalAttack(){
		aim = target;
		normalAttackCount++;
		GameObject arrow = Instantiate(Resources.Load("Unit/6W")) as GameObject;
		arrow.transform.position = transform.position + new Vector3(0, .5f, 0);
		iTween.MoveTo (arrow, iTween.Hash("position", target.transform.position + new Vector3(0, .5f, 0), "easeType", "linear", "speed", 20, "oncomplete", "FinishFlying", "oncompleteparams", arrow, "oncompletetarget", this.gameObject));
	}
	
	private void FinishFlying(GameObject arrow){
		if(skill2 != null && normalAttackCount == (int)skill2.arg3){
			normalAttackCount = 0;
			float dmg = CalcDamage() + skill2.arg1;
			dmg /= skill2.arg4;
			Dot dot = new Dot();
			dot.type = DotType.burning;
			dot.num = dmg;
			dot.duration = skill2.arg4;
			dot.unit = this;
			aim.AddDot(dot);
		}else{
			base.DoNormalAttack(aim);
		}
		Destroy(arrow);
	}

	public override void DoSkill1(){
		if(skill1 != null) StartCoroutine(DeepBreathe());
	}
	
	IEnumerator DeepBreathe ()
	{
		float dmg = CalcDamage() + skill1.arg1;
		dmg /= skill1.arg3;
		for (int i = 0; i < (int)skill1.arg3; i++) {
			yield return new WaitForSeconds(0.2f);
			target.Damage(dmg, this);
		}
	}

}