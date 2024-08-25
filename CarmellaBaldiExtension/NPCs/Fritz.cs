using UnityEngine;
using System.Collections;


namespace Sugaku.NPCS
{
public class Fritz : CustomNPC, IEntityTrigger
{

	private AudioManager audioManager;

	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}


	public SoundObject sweepAud;
	public SoundObject sweepEndAud;

	public SoundObject[] sweepingAud;
	public SoundObject[] idleAud;


	public override void Initialize()
	{
		base.Initialize();

		audioManager = GetComponent<PropagatedAudioManager> ();

		navigator.SetRoomAvoidance (false);
		navigator.SetSpeed (16f);

		this.behaviorStateMachine.ChangeState(new Fritz_Wander(this, 30f));

	}

	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
	}

	public void Bump(Entity entity)
	{
		entity.AddForce(new Force((Navigator.Velocity).normalized, 20f, -10f));
	}


	public void CalloutChance(bool sweeping)
	{
		if (UnityEngine.Random.value <= 0.05f) {
			if (!sweeping) {
				this.audioManager.PlayRandomAudio (this.idleAud);
				return;
			}
			this.audioManager.PlayRandomAudio (this.sweepingAud);
		}
	}

}

public class Fritz_StateBase : NpcState
{
	protected Fritz fritz;

	public Fritz_StateBase(Fritz fritz) : base(fritz)
	{
		this.fritz = fritz;
	}
}

public class Fritz_Wander : Fritz_StateBase
{

	public float sweepCooldown = 30f;
	float calloutTime = 3f;

	public Fritz_Wander(Fritz fritz, float cooldown) : base(fritz)
	{
		sweepCooldown = cooldown;
	}

	public override void Enter()
	{
		base.Enter ();
		npc.Navigator.SetSpeed (16f);
		npc.Navigator.maxSpeed = 16f;
		fritz.UpdateSprite ("idle");
		base.ChangeNavigationState (new NavigationState_WanderRandom (npc, 0));
	}

	public override void Update()
	{
		base.Update();
		this.sweepCooldown -= Time.deltaTime * this.npc.TimeScale;
		if (this.sweepCooldown <= 0f)
		{
			npc.behaviorStateMachine.ChangeState (new Fritz_Sweeping (fritz, Random.Range (15f, 45f)));
		}

		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.fritz.CalloutChance(false);
			this.calloutTime = 3f;
		}
	}
}

public class Fritz_Sweeping : Fritz_StateBase
{

	public float cooldown = 30f;

	float calloutTime = 3f;

	public Fritz_Sweeping(Fritz fritz, float cooldown) : base(fritz)
	{
		this.cooldown = cooldown;
	}

	public override void Enter()
	{
		base.Enter ();
		fritz.AudioManager.PlaySingle (fritz.sweepAud);
		fritz.UpdateSprite ("sweep");
			float val = Random.Range (16f, 24f);

			npc.Navigator.SetSpeed (val);
			npc.Navigator.maxSpeed = val;
	}

	public override void Update()
	{
		base.Update();
		this.cooldown -= Time.deltaTime * this.npc.TimeScale;
		if (this.cooldown <= 0f)
		{
			fritz.AudioManager.PlaySingle (fritz.sweepEndAud);
			npc.behaviorStateMachine.ChangeState (new Fritz_Wander (fritz, Random.Range (15f, 45f)));
		}

		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.fritz.CalloutChance(true);
			this.calloutTime = 3f;
		}
	}


	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.CompareTag("Player") || other.CompareTag("NPC")) {
			fritz.Bump (other.GetComponent<Entity> ());
		}
	}
}
}