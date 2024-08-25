using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Sugaku.NPCS
{
public class Crabby : CustomNPC, IEntityTrigger
{
	List<Cell> spawnPoses = new List<Cell>();

	List<RoomCategory> roomCategorys = new List<RoomCategory>()
	{
		RoomCategory.Class,
		RoomCategory.Faculty,
		RoomCategory.Office,
		RoomCategory.Special,
		RoomCategory.Test
	};

	int roomID = 0;
	float roomCountdown = 32f;
	public bool pauseCountdown = false;

	private AudioManager audioManager;


	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

	public SoundObject audActivate;
	public SoundObject audTeleport;

	public override void Initialize()
	{
		base.Initialize();
		audioManager = GetComponent<PropagatedAudioManager> ();
		this.behaviorStateMachine.ChangeState(new Crabby_Idle(this));
		this.behaviorStateMachine.ChangeNavigationState(new NavigationState_Disabled(this));
		List<RoomController> list = new List<RoomController>();
		foreach (RoomController roomController in this.ec.rooms)
		{
			if (roomCategorys.Contains(roomController.category))
			{
				list.Add(roomController);
			}
		}
		while (list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			this.SetSpawn(list[index]);
			list.RemoveAt(index);
		}

		base.transform.position = spawnPoses [0].FloorWorldPosition;

		spriteBase = transform.Find ("SpriteBase").gameObject;
		spriteRenderer = new SpriteRenderer[]
		{
			spriteBase.transform.GetChild(0).GetComponent<SpriteRenderer>()
		};
		//this shits a fucking joke

		if (spriteRenderer.Length > 0) {
			UpdateSprite ("idle");
		}
			audioManager.SetSubcolor (new Color(0.9f, 0.1f, 0.1f));

		ec.map.AddArrow (base.transform, Color.red);
	}

	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
		if (!pauseCountdown) {
			roomCountdown -= Time.deltaTime * TimeScale;
			if (roomCountdown <= 0f) {
				QuickSwitch ();
			}
		}
	}

	public void SetSpawn(RoomController room)
	{
		List<Cell> cells = room.AllTilesNoGarbage (true, false);
		if (cells.Count > 0) {
			int num = Random.Range (0, cells.Count);
			spawnPoses.Add (cells [num]);
		}
	}

	public void QuickSwitch()
	{

		roomID = (roomID + 1) % spawnPoses.Count;
		base.transform.position = spawnPoses [roomID].FloorWorldPosition;
		roomCountdown = 32f;
	}
}

public class Crabby_StateBase : NpcState
{
	protected Crabby crabby;

	public Crabby_StateBase(Crabby crabby) : base(crabby)
	{
		this.crabby = crabby;
	}
}

public class Crabby_Idle : Crabby_StateBase
{

	float chargeTime = 2f;

	public Crabby_Idle(Crabby crabby) : base(crabby)
	{
	}

	public override void Enter()
	{
		base.Enter ();
		crabby.UpdateSprite ("idle");
		base.ChangeNavigationState (new NavigationState_Disabled (npc));
	}

	public override void Update()
	{
		base.Update();
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight(player);
		crabby.pauseCountdown = true;
		chargeTime -= Time.deltaTime * npc.TimeScale;
		if (chargeTime <= 0f) {
			npc.behaviorStateMachine.ChangeState(new Crabby_Charging(crabby));
		}
	}


	public override void PlayerLost(PlayerManager player)
	{
		base.PlayerLost(player);
		crabby.pauseCountdown = false;
	}
}


public class Crabby_Charging : Crabby_StateBase
{

	float timer = 3f;

	public Crabby_Charging(Crabby crabby) : base(crabby)
	{
	}

	public override void Enter()
	{
		base.Enter ();
		crabby.UpdateSprite ("activate");
		crabby.AudioManager.PlaySingle (crabby.audActivate);
		DebugMenu.LogEvent ("CHARGING");
	}

	List<string> RedlistedNames = new List<string>()
	{
		"Chalkles",
		"FloatingItem",
		"Klutz",
		"Bully",
		"Baldi"
	};

	bool ContainsRedlisted(string name)
	{
		return RedlistedNames.Contains (name);
	}

	public override void Update()
	{
		base.Update();

		timer -= Time.deltaTime * npc.TimeScale;
		if (timer <= 0f) {
			NPC[] npcsAll = UnityEngine.Object.FindObjectsOfType<NPC> ();
			List<NPC> npcsToSwap = new List<NPC> ();
			foreach (NPC npcz in npcsAll) {
				if (!ContainsRedlisted (npcz.Character.ToString())) {
					npcsToSwap.Add (npcz);
				}
			}
			System.Random rng = new System.Random ();
			NPC npcToSwapWith = npcsToSwap [rng.Next (npcsToSwap.Count)];

			Vector3 prevPosition = crabby.transform.position;
			npcToSwapWith.transform.position = prevPosition;
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle (crabby.audTeleport);
			crabby.QuickSwitch ();

			DebugMenu.LogEvent ("Warped Self with " + npcToSwapWith);
			npc.behaviorStateMachine.ChangeState(new Crabby_Idle(crabby));
		}
	}

}
}