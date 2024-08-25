using UnityEngine;
using System.Collections;
using Gemu;
using System.Collections.Generic;

namespace Sugaku.NPCS
{
public class Skeleton : NPC, IEntityTrigger
{

	bool turning;
	private MovementModifier moveMod = new MovementModifier(Vector3.zero, 0f);


	private MovementModifier moveMod2 = new MovementModifier(Vector3.zero, 0.1f);

	private float turnSpeed = 1f;

	public Sprite[] allSprites;

	public SoundObject warpAudio;
	public SoundObject movementNoise;
	public SoundObject[] noises;

	public SoundObject noticeSound;

	AudioManager audMan;


	public override void Initialize()
	{
		base.Initialize();

		SpriteRotatorCustom sr = spriteRenderer [0].gameObject.AddComponent<SpriteRotatorCustom> ();
		sr.sprites = allSprites;
		sr.spriteRenderer = spriteRenderer [0];
		sr.offset = 2;

		audMan = GetComponent<PropagatedAudioManager> ();
			audMan.SetSubcolor (new Color (0.25f, 0.5f, 0.6f));
		navigator.SetRoomAvoidance (false);
		this.behaviorStateMachine.ChangeState(new Skeleton_Wandering(this, 0f));


	}


	public bool FacingNextPoint()
	{
		if (this.turning)
		{
			return true;
		}
		if ((double)Vector3.Angle(base.transform.forward, this.navigator.NextPoint - base.transform.position) <= 22.5)
		{
			Vector3 lhs = this.navigator.NextPoint - base.transform.position;
			if (lhs != Vector3.zero)
			{
				base.transform.rotation = Quaternion.LookRotation(lhs.normalized, Vector3.up);
			}
			return true;
		}
		return false;
	}

	public void PauseAndTurn()
	{
		if (!this.turning)
		{
			this.turning = true;
			this.navigator.ClearRemainingDistanceThisFrame();
			base.StartCoroutine(this.PauseAndTurner());
		}
	}

	public void StartNoticeAction(PlayerManager player)
	{
		FOVManager.instance.SetFov (90f);
		player.Am.moveMods.Add (moveMod2);
		this.navigator.Am.moveMods.Add (this.moveMod);
		audMan.FlushQueue (true);
		audMan.PlaySingle (noticeSound);
	}

	public void OnSwitch(PlayerManager player)
	{
		FOVManager.instance.SetFov (60f);
		player.Am.moveMods.Remove (moveMod2);
		this.navigator.Am.moveMods.Remove (this.moveMod);
	}

	public IEnumerator PauseAndTurner()
	{
		Vector3 target = this.navigator.NextPoint;
		this.navigator.Am.moveMods.Add (this.moveMod);
		float time = 1f;
		while (time > 0f) {
			time -= Time.deltaTime * base.TimeScale;
			yield return null;
		}
		time = 1f;
		Vector3 vector = default(Vector3);
		while (time > 0f || (double)Vector3.Angle (base.transform.forward, target - base.transform.position) > 22.5) {
			vector = Vector3.RotateTowards (base.transform.forward, (target - base.transform.position).normalized, Time.deltaTime * 2f * 3.14159274f * this.turnSpeed, 0f);
			if (vector != Vector3.zero) {
				base.transform.rotation = Quaternion.LookRotation (vector, Vector3.up);
			}
			time -= Time.deltaTime;
			yield return null;
		}
		base.transform.rotation = Quaternion.LookRotation ((target - base.transform.position).normalized, Vector3.up);
		this.turning = false;
		this.navigator.Am.moveMods.Remove (this.moveMod);
		yield break;
	}

	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
	}
	public bool FacingPlayer(PlayerManager player)
	{
		return (double)Vector3.Angle(base.transform.forward, player.transform.position - base.transform.position) <= 22.5;
	}

	public void CalloutChance()
	{
		if (UnityEngine.Random.value <= 0.05f) {
			this.audMan.PlayRandomAudio(this.noises);
		}
	}

}

public class Skeleton_StateBase : NpcState
{
	protected Skeleton skeleton;

	public Skeleton_StateBase(Skeleton skeleton) : base(skeleton)
	{
		this.skeleton = skeleton;
	}
}

public class Skeleton_Wandering : Skeleton_StateBase
{
	protected float cooldown;
	protected float chargeTime = 5f;
	private float calloutTime = 3f;

	bool startedNotice = false;

	public Skeleton_Wandering(Skeleton skeleton, float cooldown) : base(skeleton)
	{
		this.cooldown = cooldown;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 0));
	}
	public override void MadeNavigationDecision()
	{
		base.MadeNavigationDecision();
		this.skeleton.PauseAndTurn();
	}

	// Token: 0x0600016B RID: 363 RVA: 0x0001BAA0 File Offset: 0x00019CA0
	public override void Update()
	{
		if (this.cooldown > 0f)
		{
			this.cooldown -= Time.deltaTime * this.npc.TimeScale;
		}
		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.skeleton.CalloutChance();
			this.calloutTime = 3f;
		}
		if (!this.skeleton.FacingNextPoint())
		{
			base.Update();
		}
	}

	List<string> RedlistedNames = new List<string>()
	{
		"Skeleton",
		"ChalkFace",
		"FloatingItem",
		"Klutz",
		"Baldi"
	};

	bool ContainsRedlisted(string name)
	{
		return RedlistedNames.Contains (name);
	}

	// DrReflex_Hunting
	// Token: 0x0600014F RID: 335 RVA: 0x00008423 File Offset: 0x00006623
	public override void PlayerLost(PlayerManager player)
	{
		base.PlayerLost(player);
		startedNotice = false;
		skeleton.OnSwitch (player);
	}


	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight(player);


		if (skeleton.FacingPlayer (player)) {
			if (npc.looker.IsVisible) {
				if (!startedNotice) {
					skeleton.StartNoticeAction (player);
					startedNotice = true;
				}
			} else {
				if (startedNotice) {
					skeleton.OnSwitch (player);
					chargeTime = 3f;
					startedNotice = false;
				}
			}
		} else {
			if (startedNotice) {
				skeleton.OnSwitch (player);
				chargeTime = 3f;
				startedNotice = false;
			}
		}
		if (startedNotice) {
			
			skeleton.transform.LookAt (player.transform);
			chargeTime -= Time.deltaTime * this.npc.TimeScale;
			
			if (chargeTime <= 0f) {

				NPC[] npcsAll = UnityEngine.Object.FindObjectsOfType<NPC> ();
				List<NPC> npcsToSwap = new List<NPC> ();
				foreach (NPC npc in npcsAll) {
					if (!ContainsRedlisted (npc.Character.ToString())) {
						npcsToSwap.Add (npc);
					}
				}
				System.Random rng = new System.Random ();
				NPC npcToSwapWith = npcsToSwap [rng.Next (npcsToSwap.Count)];

				int lol = Random.Range (0, 2);
				if (lol == 1) {
					ObjectFunctions.SwapPositions (player.transform, npcToSwapWith.transform);
					DebugMenu.LogEvent ("Warped Player with npc");
				} else {
					ObjectFunctions.SwapPositions (skeleton.transform, npcToSwapWith.transform);
					DebugMenu.LogEvent ("Warped Self with " + npcToSwapWith);
				}
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle (skeleton.warpAudio);
				chargeTime = 3f;
			}
		}
	}
}
}