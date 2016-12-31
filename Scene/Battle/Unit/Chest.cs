using UnityEngine;
using System.Collections;

public class Chest : BaseUnit {
	
	public override void SetProperty(){
		preAtkTime = 12f / 30;
		preSkill1Time = 6f / 30;
		preSkill2Time = 6f / 30;
		radius = 1;
		attackEffect = BattleManager.Instance.GetEffect("12378");
	}
	
}
