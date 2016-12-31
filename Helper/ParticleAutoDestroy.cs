using UnityEngine;
using System.Collections;

public class ParticleAutoDestroy : MonoBehaviour
{
	
	void OnEnable()
	{
		StartCoroutine(CheckIfAlive());
	}
	
	IEnumerator CheckIfAlive ()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.5f);
			ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>();
			bool flag = true;
			for (int i = 0; i < pss.Length; i++) {
				if(pss[i].IsAlive()) flag = false;
			}
			if(flag) Destroy(this.gameObject);
		}
	}
}