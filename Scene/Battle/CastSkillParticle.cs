using UnityEngine;
using System.Collections;

public class CastSkillParticle : MonoBehaviour {

	private float lastTime;
	private ParticleSystem[] particles;
	private float timeCount = 0;
	
	// Use this for initialization
	void Start ()
	{
		particles = GetComponentsInChildren<ParticleSystem>();
	}

	void OnEnable ()
	{
		timeCount = 0;
		lastTime = Time.realtimeSinceStartup;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(BattleManager.Instance.pausePanel.activeSelf) return;

		float deltaTime = Time.realtimeSinceStartup - lastTime;
		for (int i = 0; i < particles.Length; i++) {
			particles[i].Simulate(deltaTime, false, false);
		}
		timeCount += deltaTime;
		if(timeCount > 1){
			BattleManager.Instance.LightOn();
			this.gameObject.SetActive(false);
		}
		lastTime = Time.realtimeSinceStartup;
	}

	public void Resume(){
		lastTime = Time.realtimeSinceStartup;
	}
	
}