using UnityEngine;
using System.Collections;

namespace Sugaku.NPCS
{
public class Placeface : CustomNPC, IEntityTrigger
{

	private AudioManager audioManager;

	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

	public Vector3 spawnPoint;

	public SoundObject elephantHit;
	public SoundObject activateSound;


	public override void Initialize()
	{
		base.Initialize();

		audioManager = GetComponent<PropagatedAudioManager> ();
		spawnPoint = base.transform.position;
			audioManager.SetSubcolor (new Color ().from255 (157f, 77f, 77f));
		navigator.SetRoomAvoidance (false);
		navigator.SetSpeed (16f);

		room = getRoom ();

		this.behaviorStateMachine.ChangeState(new Placeface_Wander(this, 30f));

	}

	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
	}

	RoomController room;

	public RoomController getRoom()
	{
		foreach (RoomController room in ec.rooms) {
			if (room.color == Color.blue) {
				base.transform.position = ec.RealRoomMid (room);
				spawnPoint = ec.RealRoomMid (room);
				DebugMenu.LogEvent ("Found room, Defaulting position");
				return room;
			}
		}
		if (ec.offices.Count > 0) {
			DebugMenu.LogEvent ("Found office, Failsave protocol iniated");
			return ec.offices [0];
		}
		IntVector2 intVector = IntVector2.GetGridPosition (spawnPoint);
		DebugMenu.LogEvent ("No room was found, Defaulting to spawn tile");
		return ec.cells [intVector.x, intVector.z].room;
	}

	public void LockRoom()
	{
		foreach (Door door in room.doors) {
			door.Shut ();
			door.LockTimed (9f);
		}
	}
}

public class Placeface_StateBase : NpcState
{
	protected Placeface placeface;

	public Placeface_StateBase(Placeface placeface) : base(placeface)
	{
		this.placeface = placeface;
	}
}

public class Placeface_Wander : Placeface_StateBase
{

	public float crazyCooldown = 30f;

	public Placeface_Wander(Placeface placeface, float cooldown) : base(placeface)
	{
		crazyCooldown = cooldown;
	}

	public override void Enter()
	{
		base.Enter ();
		npc.Navigator.SetSpeed (16f);
		npc.Navigator.maxSpeed = 16f;
		placeface.UpdateSprite ("idle");
		base.ChangeNavigationState (new NavigationState_WanderRandom (npc, 0));
	}

	public override void Update()
	{
		base.Update();
		this.crazyCooldown -= Time.deltaTime * this.npc.TimeScale;
		if (this.crazyCooldown <= 0f)
		{
			npc.behaviorStateMachine.ChangeState (new Placeface_Crazy (placeface, Random.Range (5f, 15f)));
		}
	}
}

public class Placeface_Crazy : Placeface_StateBase
{

	public float calmCooldown = 30f;

	public Placeface_Crazy(Placeface placeface, float cooldown) : base(placeface)
	{
		calmCooldown = cooldown;
	}

	public override void Enter()
	{
		base.Enter ();
		npc.Navigator.SetSpeed (36f);
		placeface.UpdateSprite ("crazy");
		placeface.AudioManager.PlaySingle (placeface.activateSound);
		base.ChangeNavigationState (new NavigationState_WanderRandom (npc, 0));
	}

	public override void Update()
	{
		base.Update();
		this.calmCooldown -= Time.deltaTime * this.npc.TimeScale;
		if (this.calmCooldown <= 0f)
		{
			npc.behaviorStateMachine.ChangeState (new Placeface_Wander (placeface, Random.Range (15f, 45f)));
		}
	}


	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.CompareTag("Player") || other.CompareTag("NPC")) {
			other.transform.position = placeface.spawnPoint;
			placeface.LockRoom ();
			placeface.AudioManager.PlaySingle (placeface.elephantHit);
		}
	}
}
}