using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InjuryNum : AnimationEvent {
	
	public void Init(float value, BaseUnit unit){
		GetComponentInChildren<Text>().text = (Mathf.FloorToInt(value)).ToString();
		this.transform.parent.SetParent(BattleManager.Instance.uiSet.transform, false);
		this.transform.parent.GetComponent<RectTransform>().anchoredPosition = Helper.GetScreenPoint(unit.transform.position) + new Vector2(0, -450);
	}
}
