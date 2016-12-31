using UnityEngine;
using System.Collections;

public class Savage : BaseUnit {

	public override void SetProperty(){
		preAtkTime = 6f / 30;
		preSkill1Time = 35f / 30;
		preSkill2Time = 35f / 30;
		radius = 0.5f;
		attackEffect = BattleManager.Instance.GetEffect("12378");
	}

}