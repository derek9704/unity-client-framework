using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum DotType{
	//冻结 昏迷 燃烧 缠绕 减伤 加伤 流血 护盾 ATK提升 MATK提升 迟缓 提速 愈合
	freeze, dazzle, burning, twine, addDmg, reduceDmg, bleeding, shield, atkUp, matkUp, speedDown, speedUp, heal, knockback
}

public abstract class BaseUnit : MonoBehaviour {

	public enum Mode{
		show, battle
	}

	public class Dot
	{
		public DotType type;
		public float num;
		public float duration = 1000;
		public float timeCount = 0;
		public BaseUnit unit;
		public GameObject eff;
	}
	
	public class Skill {
		public string name;
		public int effect;
		public int level;
		public float arg1;
		public float arg2;
		public float arg3;
		public float arg4;
	}

	protected Mode mode = Mode.show;

	public Skill skill1;
	public Skill skill2;
	public string tid;
	public float size;
	public int type;
	public float atk;
	public float matk;
	public float critical;
	public float critical_coe;
	public float move_speed;
	public float atk_speed;
	public float atk_range;
	public Troop troop;
	public float radius;
	public float hp;
	public float current_hp;
	public int level;
	public float exportDamage = 0;
	public float gauge = 0;
	public HeroAction action;

	protected Animation thisAnimation;
	protected Renderer[] renderers;
	public NavMeshAgent navMeshAgent;
	public NavMeshObstacle navMeshObstacle;
	protected BloodSlider bloodSlider;
	protected GameObject skillBtn;
	protected GameObject allowCastSkill;

	protected BaseUnit target;
	protected float injuryColorTime = 0;
	protected float atkInterval = 0;
	protected float lastDistance;

	protected float preAtkTime = 0.5f;
	protected float preSkill1Time = 0.5f;
	protected float preSkill2Time = 0.5f;
	protected Object attackEffect;

	protected List<Dot> dots = new List<Dot>();

	void Awake(){
		renderers = GetComponentsInChildren<Renderer>();
		thisAnimation = GetComponent<Animation>();
	}

	public void Init(JsonNode data, Troop troop, GameObject skillBtn){
		SetProperty();
		mode = Mode.battle;
		this.troop = troop;
		this.skillBtn = skillBtn;
		tid = data["tid"];
		size = data["size"];
		atk = data["atk"];
		matk = data["matk"];
		level = data["level"];
		current_hp = hp = data["hp"];
		critical = data["critical"];
		critical_coe = data["critical_multiple"];
		atk_range = data["atk_range"];
		atk_speed = data["atk_speed"];
		move_speed = data["move_speed"];
		skill1 = GetSkill(data["skill1"]);
		skill2 = GetSkill(data["skill2"]);
		type = data["type"];

		GameObject stepDust = Instantiate(BattleManager.Instance.GetEffect("12352")) as GameObject;
		stepDust.transform.SetParent(this.transform, false);

		string bloodSliderName = troop.side == Troop.Side.my ? "MyBlood" : "EnemyBlood";
		bloodSlider = (Instantiate(BattleManager.Instance.GetHUD(bloodSliderName)) as GameObject).GetComponent<BloodSlider>();
		bloodSlider.Init(this);

		string haloName = troop.side == Troop.Side.my ? "12380" : "12381";
		GameObject halo = Instantiate(BattleManager.Instance.GetEffect(haloName)) as GameObject;
		Helper.AmplifyParticle(halo, radius * 2);
		halo.transform.SetParent(this.transform, false);
		
		transform.localScale = new Vector3(size, size, size);

		navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
		navMeshObstacle.carving = true;
		navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
		navMeshObstacle.enabled = false;
		navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
		navMeshAgent.speed = move_speed;
		navMeshAgent.angularSpeed = 300;
		navMeshAgent.acceleration = 100;
		radius = radius * size;
		navMeshAgent.radius = navMeshObstacle.radius = radius * 1.5f;
		//rotation
		if(troop.side == Troop.Side.my) transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
		else transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));

		if(skillBtn){
			skillBtn.SetActive(true);
			EventTriggerListener.Get(skillBtn).onClick = CastSkill;
			skillBtn.transform.GetChild(1).GetComponent<Image>().fillAmount = current_hp / hp;
			allowCastSkill = skillBtn.transform.GetChild(3).gameObject;
		}
		AddGauge(500); //初始500怒气
		//add event
		AddEvent(HeroAction.attack, "DoNormalAttack", preAtkTime);
		AddEvent(HeroAction.skill_cast, "DoSkill1", preSkill1Time);
		AddEvent(HeroAction.skill_attack, "DoSkill2", preSkill2Time);
	}

	private void AddEvent(HeroAction action, string functionName, float time){	
		if(BattleManager.Instance.InformAnimationClip(thisAnimation.GetClip(action.ToString()))){
			UnityEngine.AnimationEvent evt = new UnityEngine.AnimationEvent();
			evt.functionName = functionName;
			evt.time = time;
			thisAnimation.GetClip(action.ToString()).AddEvent(evt);
		}
	}

	void FixedUpdate ()
	{
		if(Time.timeScale == 0) return;
		if(mode == Mode.show){
			if (thisAnimation.isPlaying) return;
			DoAction (HeroAction.idle);
		}else{
			if(injuryColorTime > 0) {
				injuryColorTime -= Time.deltaTime;
				if(injuryColorTime <= 0) SetBodyIllumin(Color.black);
			}
			if(BattleManager.Instance.status == BattleManager.Status.start && current_hp > 0){
				bool standstill = false;
				foreach (var item in dots) {
					if (item.type == DotType.dazzle || item.type == DotType.freeze || item.type == DotType.knockback) {
						standstill = true;
					}
				}
				//处理一下图标
				if(skillBtn){
					if(allowCastSkill.activeSelf){
						if(target.current_hp <= 0 || gauge < 1000 || standstill) allowCastSkill.SetActive(false);
					}else{
						if(target.current_hp > 0 && !navMeshAgent.enabled && gauge == 1000 && !standstill) allowCastSkill.SetActive(true);
					}
				}
				//普攻间隔倒计时
				atkInterval -= Time.deltaTime;
				if(!standstill){
					float dist = Vector3.Distance(transform.position, target.transform.position) - radius - target.radius;
					if((target.current_hp <= 0 || dist > atk_range) && action == HeroAction.idle){
						bool twine = false;
						foreach (var item in dots) {
							if (item.type == DotType.twine) {
								twine = true;
							}
						}
						if(action == HeroAction.idle && navMeshObstacle.enabled && !twine) {
							navMeshObstacle.enabled = false;
							//这里是为了让obstacle挖的洞先消失
							Invoke("SearchTarget", 0.01f);
						}
					}
					if(navMeshAgent.enabled){
						if(dist <= atk_range){
							navMeshAgent.enabled = false;
							navMeshObstacle.enabled = true;
							DoAction(HeroAction.idle);
							transform.LookAt (target.transform);
						}else if(dist >= lastDistance){
							navMeshAgent.destination = target.transform.position;
						}
						lastDistance = dist;
					}else{
						if(navMeshObstacle.enabled && dist <= atk_range){
							if(action == HeroAction.idle && atkInterval <= 0){
								atkInterval = 1 / atk_speed;
								DoAction(HeroAction.attack);
							}
							//自动释放技能
							if(troop.auto) CastSkill(null, null);
						}
					}
				}
				//处理DOT
				for (int i = 0; i < dots.Count; i++) {
					Dot item = dots[i];
					int lastSec = Mathf.FloorToInt(item.duration);
					item.duration -= Time.deltaTime;
					if(Mathf.FloorToInt(item.duration) != lastSec){ //伤害及治疗类DOT
						switch (item.type) {
						case DotType.burning:
						case DotType.bleeding:
							Damage(item.num, item.unit, true);
							break;
						case DotType.heal:
							Heal(item.num);
							break;
						}
					}
					if(item.duration <= 0){
						switch (item.type) {
						case DotType.freeze:
							thisAnimation[action.ToString()].speed = 1;
							if(navMeshAgent.enabled) {
								ResumeMoving();
							}
							else DoAction(HeroAction.idle);
							break;
						case DotType.dazzle:
						case DotType.knockback:
						case DotType.twine:
							ResumeMoving();
							break;
						case DotType.atkUp:
							atk -= item.num;
							break;
						case DotType.matkUp:
							matk -= item.num;
							break;
						case DotType.speedDown:
							atk_speed /= (1 - item.num);
							move_speed /= (1 - item.num);
							navMeshAgent.speed = move_speed;
							break;
						case DotType.speedUp:
							atk_speed /= (1 + item.num);
							move_speed /= (1 + item.num);
							navMeshAgent.speed = move_speed;
							break;
						}
						if(item.eff) Destroy(item.eff);
						dots.Remove(item);
					}
				}
			}
			switch (action) {
			case HeroAction.attack:
			case HeroAction.skill_cast:
			case HeroAction.skill_attack:
			case HeroAction.hurt:
			case HeroAction.win:
				if (thisAnimation.isPlaying) return;
				DoAction (HeroAction.idle);
				break;
			}
		}
	}

	public void Heal(float value){
		if(current_hp == 0) return;
		current_hp += value;
		if(current_hp > hp) current_hp = hp;
		GameObject healNum = Instantiate(BattleManager.Instance.GetHUD("HealNum")) as GameObject;
		healNum.transform.GetChild(0).GetComponent<InjuryNum>().Init(value, this);
	}

	public void RemoveDot(DotType type){
		if(current_hp == 0) return;
		foreach (var item in dots) {
			if(item.type == type){
				item.duration = 0;
				return;
			}
		}
	}

	private void StopMoving(){
		if(navMeshAgent.enabled) navMeshAgent.Stop();
	}

	private void ResumeMoving(){
		foreach (var item in dots) {
			if (item.type == DotType.dazzle || item.type == DotType.freeze || item.type == DotType.knockback || item.type == DotType.twine) {
				return;
			}
		}
		if(navMeshAgent.enabled) {
			DoAction(HeroAction.run);
			navMeshAgent.Resume();
		}
	}

	public void AddDot(Dot one){
		if(current_hp == 0) return;
		one.duration -= 0.01f; //伤害及治疗类DOT 1秒后产生作用
		foreach (var item in dots) {
			if(item.type == one.type){
				item.num = one.num;
				item.duration = one.duration;
				return;
			}
		}
		switch (one.type) {
		case DotType.freeze:
			thisAnimation[action.ToString()].speed = 0;
			StopMoving();
			one.eff = Instantiate(BattleManager.Instance.GetEffect("12373_1")) as GameObject;
			one.eff.transform.SetParent(this.transform, false);
			break;
		case DotType.dazzle:
			DoAction(HeroAction.idle);
			StopMoving();
			one.eff = Instantiate(BattleManager.Instance.GetEffect("12357")) as GameObject;
			one.eff.transform.SetParent(this.transform, false);
			break;
		case DotType.knockback:
			DoAction(HeroAction.hurt);
			StopMoving();
			break;
		case DotType.twine:
			StopMoving();
			break;
		case DotType.burning:
			one.eff = Instantiate(BattleManager.Instance.GetEffect("12371")) as GameObject;
			one.eff.transform.SetParent(this.transform, false);
			break;
		case DotType.addDmg:
			break;
		case DotType.reduceDmg:
			break;
		case DotType.bleeding:
			break;
		case DotType.shield:
			one.eff = Instantiate(BattleManager.Instance.GetEffect("12362")) as GameObject;
			one.eff.transform.SetParent(this.transform, false);
			break;
		case DotType.atkUp:
			atk += one.num;
			break;
		case DotType.matkUp:
			matk += one.num;
			break;
		case DotType.speedDown:
			atk_speed *= (1 - one.num);
			move_speed *= (1 - one.num);
			one.eff = Instantiate(BattleManager.Instance.GetEffect("12374")) as GameObject;
			one.eff.transform.SetParent(this.transform, false);
			navMeshAgent.speed = move_speed;
			break;
		case DotType.speedUp:
			atk_speed *= (1 + one.num);
			move_speed *= (1 + one.num);
			navMeshAgent.speed = move_speed;
			break;
		case DotType.heal:
			break;
		}
		if(one.eff != null){
			Helper.AmplifyParticle(one.eff, radius * 2);
		}
		dots.Add(one);
	}

	private void SetBodyLight(Color color){
		for (int i = 0; i < renderers.Length; i++) {
			renderers[i].material.SetColor("_LightCol", color);
		}
	}
	
	public void BeKnockBacked(){
		if(current_hp == 0) return;
		Dot dot = new Dot();
		dot.type = DotType.knockback;
		dot.duration = 1;
		AddDot(dot);
		Vector3[] paths = new Vector3[3];
		paths[0] = this.transform.position;
		float coe = troop.side == Troop.Side.my ? 1 : -1;
		paths[1] = paths[0] + new Vector3(5f * coe, 5f, 0);
		paths[2] = paths[0] + new Vector3(10 * coe, 0, 0);
		iTween.MoveTo(this.gameObject, iTween.Hash("path", paths, "easeType", "easeInQuad", "time", dot.duration));
	}

	public void SetBodyIllumin(Color color){
		for (int i = 0; i < renderers.Length; i++) {
			renderers[i].material.SetColor("_IlluminCol", color);
		}
	}

	protected float CalcDamage(){
		float damage = atk + matk * 1.25f;
		foreach (var item in dots) {
			if (item.type == DotType.addDmg) {
				damage += item.num;
			}
		}
		return damage;
	}

	public virtual void StartFight(){
		SearchTarget();
	}

	public virtual void DoNormalAttack(){
		DoNormalAttack(target);
	}

	public virtual void DoNormalAttack(BaseUnit aim){
		Instantiate(attackEffect, aim.transform.position + new Vector3(0, .5f, 0), Quaternion.identity);
		float dmg = CalcDamage();
		AddGauge(50);
		aim.Damage(dmg, this);
	}

	public virtual void DoSkill1(){

	}

	public virtual void DoSkill2(){
		
	}

	public virtual void DeadEvent(){
		
	}

	public virtual void DamageEvent(){
		
	}

	public void SetBodyDark (){
		SetBodyLight(new Color(0.2f, 0.2f, 0.2f));
	}
	
	public void SetBodyWhite(){
		SetBodyLight(Color.white);
	}

	public abstract void SetProperty();

	public void AddGauge(float num){
		if(current_hp == 0) return;
		gauge += num;
		gauge = Mathf.Clamp(gauge, 0, 1000);
		if(skillBtn) {
			skillBtn.transform.GetChild(2).GetComponent<Image>().fillAmount = gauge / 1000;
			if(gauge == 1000) {
				skillBtn.transform.GetChild(4).GetComponent<Image>().sprite = GameManager.Instance.GetHeadImage(tid + "L");
			}
			else {
				skillBtn.transform.GetChild(4).GetComponent<Image>().sprite = GameManager.Instance.GetHeadImage(tid);
				allowCastSkill.SetActive(false);
			}
		}
	}
	
	protected void CastSkill(GameObject sender, PointerEventData eventData)
	{
		if(skill1 == null) return;
		if(BattleManager.Instance.status != BattleManager.Status.start || current_hp == 0 || gauge < 1000 || navMeshAgent.enabled) return;
		if(skillBtn && !allowCastSkill.activeSelf) return;
		AddGauge(-1000);
		DoAction(HeroAction.skill_cast);
		BattleManager.Instance.LightOff(this);
	}

	protected Skill GetSkill(JsonNode data){
		if(!data) return null;
		Skill skill = new Skill();
		skill.name = data["name"];
		skill.effect = data["effect"];
		skill.level = data["level"];
		skill.arg1 = data["arg1"];
		skill.arg2 = data["arg2"];
		if(data["arg3"]) skill.arg3 = data["arg3"];
		if(data["arg4"]) skill.arg4 = data["arg4"];
		return skill;
	}

	protected void SearchTarget(){
		if(current_hp == 0 || BattleManager.Instance.status != BattleManager.Status.start) return;
		//这里还是要再算一遍，因为是延时调用
		foreach (var item in dots) {
			if (item.type == DotType.dazzle || item.type == DotType.freeze || item.type == DotType.knockback) {
				return;
			}
		}
		bool first = true;
		foreach(BaseUnit one in troop.opponent.team){
			if(one.current_hp > 0) {
				if(first){
					target = one;
					first = false;
				}else{
					float distance1 = Vector3.Distance(transform.position, target.transform.position) - radius - target.radius;
					float distance2 = Vector3.Distance(transform.position, one.transform.position) - radius - one.radius;
					if(distance1 > distance2){
						target = one;
					}
				}
			}
		}
		DoAction (HeroAction.run);
		navMeshAgent.enabled = true;
		navMeshAgent.destination = target.transform.position;
		lastDistance = 100;
	}

	public BaseUnit GetNearestFellow(){
		BaseUnit aim = null;
		bool first = true;
		foreach(BaseUnit one in troop.team){
			if(one.current_hp > 0 && one != this) {
				if(first){
					aim = one;
					first = false;
				}else{
					float distance1 = Vector3.Distance(transform.position, aim.transform.position) - radius - aim.radius;
					float distance2 = Vector3.Distance(transform.position, one.transform.position) - radius - one.radius;
					if(distance1 > distance2){
						aim = one;
					}
				}
			}
		}
		return aim;
	}

	public void Damage(float value, BaseUnit attacker = null, bool isDot = false){
		if(current_hp == 0) return;
		//暴击
		bool isCrit = false;
		if(!isDot && Random.Range(0, 1f) <= attacker.critical){
			value *= attacker.critical_coe;
			isCrit = true;
		}
		//减伤DOT
		foreach (var item in dots) {
			if(item.type == DotType.reduceDmg){
				value = Mathf.Max(1, value - item.num);
			}
		}
		//shield
		foreach (var item in dots) {
			if(item.type == DotType.shield){
				if(item.num > value){
					item.num -= value;
					return;
				}else{
					value -= item.num;
					item.duration = 0;
				}
			}
		}
		if(!isDot){
			if((action == HeroAction.idle || action == HeroAction.hurt)){
				DoAction(HeroAction.hurt);
			}
			SetBodyIllumin(new Color(0.5f, 0.5f, 0.5f));
			injuryColorTime = 0.2f;
		}
		if(isCrit){
			GameObject critNum = Instantiate(BattleManager.Instance.GetHUD("CriticalNum")) as GameObject;
			critNum.transform.GetChild(0).GetComponent<InjuryNum>().Init(value, this);
		}else{
			GameObject normalNum = Instantiate(BattleManager.Instance.GetHUD("NormalNum")) as GameObject;
			normalNum.transform.GetChild(0).GetComponent<InjuryNum>().Init(value, this);
		}

		attacker.exportDamage += value;

		AddGauge(1000 * value / hp / 2);

		bloodSlider.Show();

		current_hp -= value;
		if(current_hp <= 0){
			StopMoving();
			current_hp = 0;
			DeadEvent();
			foreach (var item in dots) {
				if(item.eff) Destroy(item.eff);
			}
			DoAction(HeroAction.die);
			iTween.MoveAdd(this.gameObject, iTween.Hash("y", -3f, "speed", 1, "oncomplete", "Hide", "delay", 2));
			attacker.AddGauge(300);
			troop.InformDead();
			if(skillBtn){
				AddGauge(-1000);
				skillBtn.transform.GetChild(4).GetComponent<Image>().color = new Color(.4f, .4f, .4f);
			}
		}else{
			DamageEvent();
		}
		if(skillBtn) {
			skillBtn.transform.GetChild(1).GetComponent<Image>().fillAmount = current_hp / hp;
		}
	}

	protected void Hide(){
		this.gameObject.SetActive(false);
	}
	
	public void PlayRandom ()
	{
		int length = System.Enum.GetNames(typeof(HeroAction)).GetLength(0);
		int pick = Random.Range(0, length);
		DoAction((HeroAction)pick);
	}

	public IEnumerator DoVictory(){
		if(current_hp == 0) yield return null;
		StopMoving();
		yield return new WaitForSeconds(1);
		DoAction(HeroAction.win);
	}

	public void DoRun(){
		DoAction(HeroAction.run);
	}
	
	private void DoAction (HeroAction changeAction)
	{
		if(thisAnimation[action.ToString()].speed == 0) {
			if(changeAction == HeroAction.die) thisAnimation[action.ToString()].speed = 1;
			else return;
		}
		if(action == changeAction && thisAnimation[changeAction.ToString()].wrapMode != WrapMode.Loop){
			thisAnimation[changeAction.ToString()].time = 0;
		}else{
			action = changeAction;
		}
		thisAnimation.Play (changeAction.ToString());
	}
}
