using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using CarmellaBaldiExtension;

namespace Sugaku.NPCS
{
public class Rhythm : CustomNPC, IEntityTrigger
{
	private AudioManager audioManager;


	public SoundObject[] audWander;
	public SoundObject[] audEnd;
	public SoundObject[] audMad;
	public SoundObject[] audMadWander;


	public SoundObject audTooEarly;
	public SoundObject audLate;
	public SoundObject audLeft;

	public SoundObject audNotice;
	public SoundObject audDoor;
	public SoundObject audInstructions;

	public SoundObject musGame;
	public SoundObject musEnd;
	public SoundObject musBad;

	public SoundObject audCrash;


	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}


	private MovementModifier moveMod = new MovementModifier(Vector3.zero, 0f);

	private MovementModifier smashMod  = new MovementModifier (Vector3.zero, 0.25f);


	public Vector3 spawnPoint;

	public GameObject gameHotspot;
	void GenerateHotSpot()
	{
		GameObject hotspot = new GameObject ("Hotspot");
		hotspot.transform.parent = gameObject.transform;
		hotspot.transform.position = gameObject.transform.position;
		SphereCollider lol = hotspot.AddComponent<SphereCollider> ();
		lol.radius = 4f;
		lol.isTrigger = true;
		Rigidbody rb = hotspot.AddComponent<Rigidbody> ();
		rb.constraints = RigidbodyConstraints.FreezeAll;
		rb.useGravity = false;
		RhythmHotspot hs = hotspot.AddComponent<RhythmHotspot> ();
		hs.rhythm = this;
		gameHotspot = hotspot;
	}


	public void BashPlayer(PlayerManager player)
	{
		this.navigator.Entity.AddForce (new Force (base.transform.position - player.transform.position, 10, -20));
		player.Am.moveMods.Add (smashMod);
		StartCoroutine (Removemod (player));
	}

	IEnumerator Removemod(PlayerManager player)
	{
		DebugMenu.LogEvent ("YOUE GOTT MASHED");
		yield return new WaitForSeconds (15f);
		player.Am.moveMods.Remove (smashMod);
		yield break;
	}

	public override void Initialize()
	{
		base.Initialize();
		loopAnim = false;
		lol.npcTimeScale = 0f;
		lol.environmentTimeScale = 1f;
		audioManager = GetComponent<PropagatedAudioManager> ();
			audioManager.SetSubcolor (new Color (0.5f, 0f, 1f));
		navigator.SetRoomAvoidance (false);
		UpdateSprite ("idle");
		GenerateHotSpot ();
		gameHotspot.SetActive (false);
		spawnPoint = base.transform.position;
		this.behaviorStateMachine.ChangeState(new Rhythm_Wandering(this, 5f));
	}


	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
	}

	public void CalloutChance(bool mad = false)
	{
		if (UnityEngine.Random.value <= 0.05f) {
			if (!mad) {
				this.audioManager.PlayRandomAudio(this.audWander);
				return;
			}
			this.audioManager.PlayRandomAudio(this.audMadWander);
		}
	}

	public void StartGame(PlayerManager player)
	{
		StartCoroutine (TurnPlayer (player, 1f));
		transform.LookAt (player.transform);
		audioManager.FlushQueue (true);
		ResetPitch ();
		pressedInput = false;
		this.navigator.Entity.AddForce (new Force (base.transform.position - player.transform.position, 10, -20));
		StartCoroutine (GameSequence ());
	}

	public bool beatCheck = false;

	public bool pressedInput = false;

	public TimeScaleModifier lol = new TimeScaleModifier();

	void TooLate()
	{
		ec.RemoveTimeScale (lol);
		UpdateSprite ("uhoh");
		audioManager.FlushQueue (true);
		audioManager.PlaySingle (musBad);
		audioManager.PlaySingle (audLate);
		Invoke ("MadMode", 3f);
	}

	public void LeftGame(PlayerMovement plm)
	{
		plm.am.moveMods.Remove(this.moveMod);
		StopAllCoroutines ();
		ec.RemoveTimeScale (lol);
		UpdateSprite ("uhoh");
		audioManager.FlushQueue (true);
		audioManager.PlaySingle (musBad);
		audioManager.PlaySingle (audLeft);
		Invoke ("MadMode", 3f);
	}

	void TooEarly()
	{
		StopAllCoroutines ();
		ec.RemoveTimeScale (lol);
		UpdateSprite ("uhoh");
		audioManager.FlushQueue (true);
		audioManager.PlaySingle (musBad);
		audioManager.PlaySingle (audTooEarly);
		Invoke ("MadMode", 3f);
	}

	public void PressBeat()
	{
		PlayAnimation("gameC", 12f, 2, delegate() {
			
		});
		pressedInput = true;
		if (!beatCheck) {
			TooEarly ();
		}
	}
	void penis()
	{
		Debug.Log ("delegate methods are a fucking joke");
	}
	void MadMode()
	{
		audioManager.QueueRandomAudio (audMad);
		UpdateSprite ("mad");
		this.behaviorStateMachine.ChangeState(new Rhythm_Mad(this, Singleton<CoreGameManager>.Instance.GetPlayer(0)));

	}

	IEnumerator GameSequence()
	{
		ec.AddTimeScale (lol);
		PlayAnimation("gameC", 12f,  2, delegate(){
			
		});
		yield return new WaitForSeconds (1f);
		UpdateSprite ("gameO");
		audioManager.QueueAudio (audInstructions);
		yield return new WaitForSeconds (audInstructions.soundClip.length);
		UpdateSprite ("gameC1");
		audioManager.PlaySingle (musGame);
		yield return new WaitForSeconds (4.8f);
		PlayAnimation("beat", 12f, 2, delegate() {
			
		});
		gameHotspot.SetActive (true);
		yield return new WaitForSeconds (0.4f);
		beatCheck = true;
		yield return new WaitForSeconds (0.3f);
		beatCheck = false;
		PlayAnimation("gameC", 12f, 2, delegate() {

		});
		gameHotspot.SetActive (false);
		yield return new WaitForSeconds (2.3f);
		if (!CanProceed ()) {
			TooLate ();
			yield break;
		}
		yield return new WaitForSeconds (2.4f);
		this.behaviorStateMachine.ChangeState(new Rhythm_Wandering(this, 30f));
		ec.RemoveTimeScale (lol);
		Singleton<CoreGameManager>.Instance.AddPoints (50, 0, true);
		yield break;
	}

	bool CanProceed()
	{
		if (!pressedInput) {
			return false;
		}
		return true;
	}

	public bool PlayerLeft(PlayerManager player)
	{
		return Vector3.Distance(player.transform.position, base.transform.position) > 20f;
	}


	private float currentPitchValue = -1.5f;

	private float pitchIncreaseMaxModifier = 4f;

	private float pitchIncreaseSpeedModifier = 10f;

	public void StartRev()
	{
		audioManager.QueueAudio (audNotice);
		this.audioManager.SetLoop (true);
	}

	public void ResetPitch()
	{
		this.audioManager.pitchModifier = 1;
		this.currentPitchValue = -1.5f;
	}

	public void IncreasePitch ()
	{
		this.currentPitchValue += Time.deltaTime;
		this.audioManager.pitchModifier = Mathf.Log ((this.currentPitchValue + this.pitchIncreaseSpeedModifier) / this.pitchIncreaseSpeedModifier * this.pitchIncreaseMaxModifier);
	}


	private IEnumerator TurnPlayer(PlayerManager player, float speed)
	{
		float time = 0.5f;
		Vector3 vector = default(Vector3);
		player.plm.am.moveMods.Add(this.moveMod);
		while (time > 0f)
		{
			vector = Vector3.RotateTowards(player.transform.forward, (base.transform.position - player.transform.position).normalized, Time.deltaTime * 2f * 3.14159274f * speed, 0f);
			Debug.DrawRay(player.transform.position, vector, Color.yellow);
			player.transform.rotation = Quaternion.LookRotation(vector, Vector3.up);
			time -= Time.deltaTime;
			yield return null;
		}
		player.plm.am.moveMods.Remove(this.moveMod);
		yield break;
	}
}

public class RhythmHotspot : MonoBehaviour, IClickable<int>
{
	public Rhythm rhythm;
	public void Clicked(int playerNumber)
	{
		PlayerManager pm = Singleton<CoreGameManager>.Instance.GetPlayer (playerNumber);
	
		rhythm.PressBeat ();

		gameObject.SetActive (false);
	}

	// Token: 0x0600018A RID: 394 RVA: 0x00002492 File Offset: 0x00000692
	public void ClickableSighted(int player)
	{
	}

	// Token: 0x0600018B RID: 395 RVA: 0x00002492 File Offset: 0x00000692
	public void ClickableUnsighted(int player)
	{
	}

	// Token: 0x0600018C RID: 396 RVA: 0x000037FE File Offset: 0x000019FE
	public bool ClickableHidden()
	{
		return false;
	}

	// Token: 0x0600018D RID: 397 RVA: 0x00003801 File Offset: 0x00001A01
	public bool ClickableRequiresNormalHeight()
	{
		return true;
	}
}

public class Rhythm_StateBase : NpcState
{
	protected Rhythm rhythm;

	public Rhythm_StateBase(Rhythm rhythm) : base(rhythm)
	{
		this.rhythm = rhythm;
	}
}

public class Rhythm_Wandering : Rhythm_StateBase
{
	float calloutTime = 3f;

	float gameCooldown = 30f;

	public Rhythm_Wandering(Rhythm rhythm, float gameCooldown = 30f) : base(rhythm)
	{
		this.gameCooldown = gameCooldown;
	}

	public override void Enter()
	{
		base.Enter();
		npc.Navigator.SetSpeed (16f);
		npc.Navigator.maxSpeed = 16f;
		base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 74));
		rhythm.UpdateSprite ("idle");
	}

	public bool Chasing;

	public override void Update()
	{
		base.Update();
		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		this.gameCooldown -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.rhythm.CalloutChance();
			this.calloutTime = 3f;
		}
		if (Chasing) {
			rhythm.IncreasePitch ();
		}
	}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.CompareTag ("Player") && gameCooldown <= 0f) {

			if (!other.GetComponent<PlayerManager> ().tagged) {
				npc.behaviorStateMachine.ChangeState (new Rhythm_Game (rhythm, other.GetComponent<PlayerManager> ()));
			}
		}
	}

	public override void DoorHit(StandardDoor door)
	{
		base.DoorHit (door);
		rhythm.AudioManager.PlaySingle (rhythm.audDoor);
	}
	//NoLateTeacher_Wander test;
	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);
		if (gameCooldown <= 0f && !player.tagged) {
			npc.Navigator.SetSpeed (24f);
			rhythm.UpdateSprite ("notice");
			if (!Chasing) {
				rhythm.StartRev ();
			}
			Chasing = true;
			base.ChangeNavigationState (new NavigationState_TargetPlayer (this.npc, 74, player.transform.position));
		}
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight (player);
		if (Chasing) {
			currentNavigationState.UpdatePosition (player.transform.position);
		}
	}

	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
		if (Chasing) {
			rhythm.AudioManager.FlushQueue (true);
			npc.Navigator.SetSpeed (16f);
			npc.Navigator.maxSpeed = 16f;
			rhythm.UpdateSprite ("idle");
			rhythm.ResetPitch ();
			Chasing = false;
			base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 74));
		}
	}

}

public class Rhythm_Game : Rhythm_StateBase
{
	protected PlayerManager player;
	public Rhythm_Game(Rhythm rhythm, PlayerManager player) : base(rhythm)
	{
		this.player = player;
	}

	public override void Enter()
	{
		base.Enter();
		rhythm.StartGame (this.player);
		base.ChangeNavigationState (new NavigationState_DoNothing (this.npc, 74));
	}

	bool Gaming = true;

	public override void Update()
	{
		base.Update();
		if (rhythm.PlayerLeft (player) && Gaming) {
			rhythm.LeftGame (player.plm);
			Gaming = false;
		}
	}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
	}

}

public class Rhythm_Mad : Rhythm_StateBase
{
	protected PlayerManager player;
	public Rhythm_Mad(Rhythm rhythm, PlayerManager player) : base(rhythm)
	{
		this.player = player;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState (new NavigationState_TargetPosition (this.npc, 74, rhythm.spawnPoint));
	}

	public override void Update()
	{
		base.Update();
	}
	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
		npc.behaviorStateMachine.ChangeState (new Rhythm_Hunting (rhythm, player));

	}
}

public class Rhythm_Hunting : Rhythm_StateBase
{

	MovementModifier moveMod = new MovementModifier (Vector3.zero, 0f);

	float calloutTime = 3f;
	float hitTimer = 1f;

	protected PlayerManager player;
	public Rhythm_Hunting(Rhythm rhythm, PlayerManager player) : base(rhythm)
	{
		this.player = player;
	}

	public override void Exit ()
	{
		base.Exit ();
		this.npc.Navigator.passableObstacles.Remove (PassableObstacle.Window);
	}

	public override void Hear (Vector3 position, int value)
	{
		base.Hear (position, value);

		if (!this.npc.looker.PlayerInSight ()) {
			base.ChangeNavigationState (new NavigationState_TargetPosition(this.npc, 74, position));

		}
	}

	IEnumerator breakWindow(Window window)
	{
		window.Break (false);
		rhythm.AudioManager.PlaySingle (rhythm.audCrash);
		rhythm.GetComponent<ActivityModifier> ().moveMods.Add (moveMod);
		rhythm.PlayAnimation("hit", 12f, 6, delegate() {
			rhythm.UpdateSprite("chasing");
		});
		rhythm.GetComponent<ActivityModifier> ().moveMods.Remove (moveMod);
		yield break;
	}
	 
	public override void OnStateTriggerEnter (Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (this.npc.Navigator.passableObstacles.Contains (PassableObstacle.Window) && other.CompareTag ("Window")) {
			rhythm.StartCoroutine (breakWindow (other.GetComponent<Window> ()));
		}
		if (other.CompareTag ("Player")) {
			
			rhythm.BashPlayer (player);
			rhythm.AudioManager.PlaySingle (rhythm.audCrash);
			rhythm.PlayAnimation("hit", 12f, 6, delegate() {
				
			});
			rhythm.GetComponent<ActivityModifier> ().moveMods.Add (moveMod);
			hit = true;
		}
	}
	public override void PlayerSighted (PlayerManager player)
	{
		base.PlayerSighted (player);
		if (this.player == player) {
			base.ChangeNavigationState (new NavigationState_TargetPlayer (this.npc, 74, player.transform.position));
		}
	}

	public override void PlayerInSight (PlayerManager player)
	{
		base.PlayerInSight (player);
		if (this.player == player) {
			currentNavigationState.UpdatePosition (player.transform.position);
		}
	}

	public bool hit = false;

	public override void Enter()
	{
		base.Enter();
		rhythm.UpdateSprite ("chasing");
		this.npc.Navigator.passableObstacles.Add (PassableObstacle.Window);
		base.ChangeNavigationState (new NavigationState_TargetPlayer (this.npc, 74, player.transform.position));
	}

	public override void Update()
	{
		base.Update();
		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.rhythm.CalloutChance(true);
			this.calloutTime = 3f;
		}
		if (hit) {
			hitTimer -= Time.deltaTime * this.npc.TimeScale;

			if (this.hitTimer <= 0f)
			{
				rhythm.GetComponent<ActivityModifier> ().moveMods.Remove (moveMod);
				npc.behaviorStateMachine.ChangeState (new Rhythm_Wandering (rhythm, 30f));
			}
		}
		if (Random.Range (0, 1000) == 100) {
			rhythm.AudioManager.PlaySingle (rhythm.audCrash);
		}
	}
	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
		if (!hit) {
			rhythm.AudioManager.PlaySingle (rhythm.audCrash);
				base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 74));
		}
	}


	public override void DoorHit(StandardDoor door)
	{
		base.DoorHit (door);
		rhythm.AudioManager.PlaySingle (rhythm.audDoor);
	}
}
}