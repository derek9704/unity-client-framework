using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BloodSlider : MonoBehaviour {

	private BaseUnit unit;
	private Image image;
	private float timeCount;
	private RectTransform rectTransform;

	public void Init(BaseUnit unit){
		this.unit = unit;
		image = transform.GetChild(0).GetComponent<Image>();
		rectTransform = GetComponent<RectTransform>();
		this.transform.SetParent(BattleManager.Instance.uiSet.transform, false);
		this.gameObject.SetActive(false);
	}
	
	void FixedUpdate (){
		timeCount -= Time.deltaTime;
		if(timeCount <= 0){
			this.gameObject.SetActive(false);
		}
		RefreshBar();
	}

	public void Show(){
		RefreshBar();
		this.gameObject.SetActive(true);
		timeCount = 2;
	}

	private void RefreshBar(){
		image.fillAmount = unit.current_hp / unit.hp;
		rectTransform.anchoredPosition = Helper.GetScreenPoint(unit.transform.position) + new Vector2(0, 100);
	}

}
