using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour {

	public Slider slider;
	private AsyncOperation op;
	private bool callBackDone = false;

	IEnumerator Start()
	{
		yield return new WaitForSeconds(0.01f);
		StartCoroutine(Load());
		GameManager.Instance.ExcuteCallback(delegate() {
			if(op != null && op.progress >= 0.9f){
				op.allowSceneActivation = true;
			}else{
				callBackDone = true;
			}
		});
	}

	private IEnumerator Load() {
		op = Application.LoadLevelAsync(GameManager.Instance.sceneName.ToString());
		op.allowSceneActivation = false;
		while(op.progress < 0.9f) {
			slider.value = op.progress;
			yield return new WaitForEndOfFrame();
		}
		slider.value = 1;
		if(callBackDone){
			op.allowSceneActivation = true;
		}
	}
}