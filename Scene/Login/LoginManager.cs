using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour {
	
	public Animator initiallyOpen;
	public InputField account;

	private Animator m_Open;
	
	const string k_OpenTransitionName = "Open";

	public void OnEnable()
	{
		if (initiallyOpen == null)
			return;
		OpenPanel(initiallyOpen);
	}

	public void OpenPanel (Animator anim)
	{
		if (m_Open == anim)
			return;
		anim.gameObject.SetActive(true);
		CloseCurrent();
		
		m_Open = anim;
		m_Open.SetBool(k_OpenTransitionName, true);
	}

	public void CloseCurrent()
	{
		if (m_Open == null)
			return;
		
		m_Open.SetBool(k_OpenTransitionName, false);
		m_Open = null;
	}

	public void Login()
	{
		string text = account.text;
		if(text == ""){
			#if UNITY_EDITOR
			Helper.QuickLogin(SceneType.town);
			#else
			GameManager.Instance.ErrorHint(Lang.ACCOUNT_EMPTY);
			#endif
			return;
		}
		GameServer.Instance.Login(text, delegate(string obj) {
			switch (obj) {
			case "-1":
			case "-2":
				GameManager.Instance.ErrorHint(Lang.ACCOUNT_NOT_EXIST);
				break;
			default:
				GameServer.session_id = obj;
				GameServer.Instance.Request(ServerAction.getUserData, null, delegate() {
					GameManager.Instance.StartHeartBeat();
					GameManager.Instance.ChangeScene(SceneType.town);
				});
				break;
			}
		});
	}
}
