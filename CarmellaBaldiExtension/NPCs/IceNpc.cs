using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using Gemu;
using HarmonyLib;

namespace Sugaku.NPCS
{
public class IceNpc : NPC, IEntityTrigger
{

	private AudioManager audioManager;

	public Fog fog = ObjectFunctions.createFog (Color.cyan, 5f, 200f, 10, 10f);

	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

	private AudioManager skateAudio;

	public float wanderSpeed = 40f;
	public float turnSpeed = 12.5f;
	public float angleRange = 15f;

	public Sprite[] spritess;

	public MovementModifier playerMod = new MovementModifier(Vector3.zero, 0.4f);

	public SoundObject skateNoise;

	public SoundObject frostNoise;


	public SoundObject[] happySounds;
	public SoundObject[] frozenSounds;

	public override void Initialize()
	{
		base.Initialize();
		audioManager = GetComponent<PropagatedAudioManager> ();
		navigator.SetRoomAvoidance (true);
		navigator.accel = 5f;
		this.behaviorStateMachine.ChangeState(new Ice_MainState(this));
		SpriteRotatorCustom sr = spriteRenderer [0].gameObject.AddComponent<SpriteRotatorCustom> ();
		sr.sprites = spritess;
		sr.spriteRenderer = spriteRenderer [0];
		sr.offset = 2;

		GameObject pamGO = new GameObject ("Skate Audio");
		pamGO.transform.parent = base.transform;
		pamGO.transform.position = base.transform.position;

		PropagatedAudioManager pam = pamGO.AddComponent<PropagatedAudioManager> ();
		pam.positional = audioManager.positional;
		skateAudio = pam;
		StartCoroutine (audioDelay ());


			audioManager.SetSubcolor (Color.cyan);
	}

	IEnumerator audioDelay()
	{
		skateAudio.audioDevice = audioManager.audioDevice;
		skateAudio.maintainLoop = true;
		skateAudio.loop = true;
		yield return null;
		skateAudio.QueueAudio (skateNoise);
		yield break;
	}

	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
		skateAudio.volumeModifier = CalculateSkateVolume ();
	}

	public float CalculateSkateVolume()
	{
		float baseVal = (Navigator.speed / wanderSpeed);
		return Mathf.Max (baseVal, 1f);
	}

	public void CalloutChance()
	{
		if (UnityEngine.Random.value <= 0.05f) {
			this.audioManager.PlayRandomAudio(this.happySounds);
		}
	}

	public void PlayerFreeze()
	{
		this.ec.AddFog(this.fog);
	}

	public void EntityFreeze(Collider other)
	{
		if (other.isTrigger) {
			if(other.CompareTag ("Player"))
			{
				Entity entity = other.GetComponent<Entity> ();
				bool boots = (bool)AccessTools.Field (typeof(Entity), "ignoreAddend").GetValue (entity);
				if (!boots) {
					ActivityModifier am = other.GetComponent<ActivityModifier> ();
					am.moveMods.Add (this.playerMod);
					Slip (entity);
					if (other.CompareTag ("Player")) {
						PlayerFreeze ();
					}
					audioManager.PlaySingle (frostNoise);
					audioManager.PlayRandomAudio (frozenSounds);
					StartCoroutine (RemoveMod (am, other));
				}
			}
			if (other.CompareTag ("NPC")) {

				ActivityModifier am = other.GetComponent<ActivityModifier> ();
				am.moveMods.Add(this.playerMod);
				Entity entity = other.GetComponent<Entity> ();
				if (entity != null) {
					Slip (entity);
				}
				audioManager.PlaySingle (frostNoise);
				audioManager.PlayRandomAudio (frozenSounds);
				StartCoroutine (RemoveMod (am, other));
			}
		}
	}

	public void Slip(Entity entity)
	{
		entity.AddForce(new Force(-(Navigator.Velocity).normalized, 10f, -5f));
	}


	IEnumerator RemoveMod(ActivityModifier am, Collider other)
	{
		yield return new WaitForSeconds (15f);
		am.moveMods.Remove (playerMod);
		if (other.CompareTag ("Player")) {
			this.ec.RemoveFog(this.fog);
		}
		yield break;
	}

}

public class Ice_StateBase : NpcState
{
	protected IceNpc icesled;

	public Ice_StateBase(IceNpc icesled) : base (icesled)
	{
		this.icesled = icesled;
	}
}

public class Ice_MainState : Ice_StateBase
{

	private NavigationState targetState;

	private Vector3 _rotation;


	private Vector3 nextTarget;


	private IntVector2 currentStandardTargetPos;

	private float _angleDiff;

	private float calloutTime = 3f;


	public Ice_MainState(IceNpc icesled) : base (icesled)
	{
	}

	public override void Enter()
	{
		base.Enter();
		this.targetState = new NavigationState_TargetPosition(this.npc, 74, this.npc.transform.position);
	}

	public override void Update()
	{
		if (this.nextTarget != Vector3.zero)
		{
			this.TargetPosition(this.nextTarget);
		}
		if (!this.npc.Navigator.HasDestination && this.npc.Navigator.Speed <= this.icesled.wanderSpeed + 1f)
		{
			base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 74));
		}
		if (this.npc.Navigator.NextPoint != this.npc.transform.position)
		{
			this._angleDiff = Mathf.DeltaAngle(this.npc.transform.eulerAngles.y, Mathf.Atan2(this.npc.Navigator.NextPoint.x - this.npc.transform.position.x, this.npc.Navigator.NextPoint.z - this.npc.transform.position.z) * 57.29578f);
		}
		else
		{
			this._angleDiff = 0f;
		}
		if (Mathf.Abs(this._angleDiff) > this.icesled.angleRange)
		{
			this.npc.Navigator.maxSpeed = 0.1f;
			this.npc.Navigator.SetSpeed(0.1f);
		}
		else
		{
			this.npc.Navigator.maxSpeed = this.icesled.wanderSpeed;
		}
		if (Mathf.Abs(this._angleDiff) <= this.icesled.angleRange)
		{
			this._rotation = this.npc.transform.eulerAngles;
			this._rotation.y = this._rotation.y + Mathf.Min(this.icesled.turnSpeed * Time.deltaTime * this.npc.TimeScale, Mathf.Abs(this._angleDiff)) * Mathf.Sign(this._angleDiff);
			this.npc.transform.eulerAngles = this._rotation;
		}
		base.Update();
		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.icesled.CalloutChance();
			this.calloutTime = 3f;
		}
	}
	public void TargetPosition(Vector3 target)
	{
		if (this.npc.Navigator.Speed > this.icesled.wanderSpeed + 1f)
		{
			this.nextTarget = target;
			return;
		}
		base.ChangeNavigationState(this.targetState);
		this.targetState.UpdatePosition(target);
		this.npc.Navigator.SkipCurrentDestinationPoint();
		this.nextTarget = Vector3.zero;
	}
	public override void DestinationEmpty()
	{
		if (!this.npc.Navigator.Wandering)
		{
			this.npc.Navigator.maxSpeed = 0.1f;
			this.npc.Navigator.SetSpeed(0.1f);
		}
		base.DestinationEmpty();
		this.currentStandardTargetPos.x = 0;
		this.currentStandardTargetPos.z = 0;
		if (this.npc.Navigator.Speed <= this.icesled.wanderSpeed + 1f)
		{
			base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 74));
		}
	}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		icesled.EntityFreeze (other);
	}
}


}