using UnityEngine;
using System.Collections;

public class Stone : BaseUnit {
	
	public override void SetProperty(){
		preAtkTime = 6f / 30;
		preSkill1Time = 6f / 30;
		preSkill2Time = 6f / 30;
		radius = 0.8f;
		attackEffect = BattleManager.Instance.GetEffect("12378");
	}
	
}
