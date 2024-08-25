using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HarmonyLib;
using CarmellaBaldiExtension;
using MTM101BaldAPI;


namespace Sugaku.NPCS
{
	
public class Coloury : CustomNPC, IEntityTrigger
{

	public bool ArtGame;

	private AudioManager audioManager;

	public Vector3 spawnPoint;

	public float timeLeft;

	public SoundObject[] numAudio;
	public SoundObject[] idleAudio;
	public SoundObject timesUp;
	public SoundObject noticeAud;
	public SoundObject winAud;


	public SoundObject madAud;
	public SoundObject screamAud;

	public ArtCavas failsaveCanvas;

	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

	ArtCavas canvas;

	public override void Initialize()
	{
		base.Initialize();

		audioManager = GetComponent<PropagatedAudioManager> ();

		canvas = FindObjectOfType<ArtCavas> ();

		canvas.colourys.Add(this);
		room = getRoom ();
		canvas.transform.position = room.cells [room.cells.Count - 1].FloorWorldPosition + (Vector3.up *4f);
		AddOverlay ();
		paintCanvas.SetActive (false);
		navigator.SetRoomAvoidance (false);
		navigator.SetSpeed (16f);
		this.behaviorStateMachine.ChangeState(new Coloury_Wandering(this));

	}

	public GameObject paintCanvas;

	void AddOverlay()
	{
		DebugMenu.LogEvent ("Begin Image Creation....");
		Canvas lol = GUITools.CreateCanvas ("Coloury Overlay", 640f, 360f);
		DebugMenu.LogEvent ("Canvas Loaded....");
		//DebugMenu.LogEvent ("GameObject Loaded....");
		DebugMenu.LogEvent ("Setting up image....");
		RawImage gameInstrucs = GUITools.CreateImage ("Paint Splat", "Coloury_Overlay", new Vector2 (0f, 0f), new Vector2 (640f, 360f), 5, lol.transform);
		paintCanvas = gameInstrucs.gameObject;
		DebugMenu.LogEvent ("Done!");
	}

	int curTimeVal = 4;
	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
		if (ArtGame) {
			if (curTimeVal >= 0) {
			
				if (timeLeft > 0f) {
					timeLeft -= Time.deltaTime * TimeScale * ec.EnvironmentTimeScale;
					return;
				}
				curTimeVal--;
				audioManager.PlaySingle (numAudio [curTimeVal]);
				timeLeft = 1f;
				return;
			}
			ArtGame = false;
			audioManager.PlaySingle (timesUp);
			base.Invoke ("MadSequence", 1f);
		}
	}


	public void MadSequence()
	{
		behaviorStateMachine.ChangeState (new Coloury_Mad (this));
		if (ArtGame) {
			ArtGame = false;
		}
		StartCoroutine (madMode ());
	}

	IEnumerator madMode()
	{
		yield return new WaitForSeconds (6f);
		ec.MakeNoise (transform.position, 99);
		audioManager.PlaySingle (screamAud);
	}

	public bool PlayerLeft(PlayerManager player)
	{
		return Vector3.Distance(player.transform.position, base.transform.position) > 35f;
	}



	public void Splatter()
	{
		paintCanvas.SetActive (true);
		behaviorStateMachine.ChangeState (new Coloury_Wandering (this, 45f));
		Invoke ("quickDeact", 10f);
	}

	void quickDeact()
	{
		paintCanvas.SetActive (false);
	}

	public void BeginGame(PlayerManager player)
	{
		canvas.Reset ();
		timeLeft = 1f;
		curTimeVal = 4;
		base.transform.position = spawnPoint;
		player.Teleport (spawnPoint + transform.right * 10f);
		/*if (Vector3.Distance (canvas.transform.position, player.transform.position) > 30f) {
			canvas.transform.position = (player.transform.forward * 10f);
		}*/
		audioManager.PlaySingle (numAudio [4]);
		this.behaviorStateMachine.ChangeState(new Coloury_Game(this, player));
		ArtGame = true;
	}

	Pickup pickup;
	public WeightedItemObject[] itemTable;
	public SoundObject xylo;

	public void CalloutChance()
	{
		if (UnityEngine.Random.value <= 0.05f) {
			this.audioManager.PlayRandomAudio(this.idleAudio);
		}
	}

	public void CompleteGame()
	{
		ArtGame = false;
		System.Random rng = new System.Random ();
		Pickup pickupPre = (Pickup)AccessTools.Field (typeof(EnvironmentController), "pickupPre").GetValue (ec);
		pickup = UnityEngine.Object.Instantiate<Pickup>(pickupPre, ec.transform);
		pickup.item = WeightedSelection<ItemObject>.ControlledRandomSelection (itemTable, rng);
		canvas.transform.LookAt (players [0].transform);
		pickup.transform.position = canvas.transform.position + (canvas.transform.forward * 5f) + Vector3.up;
		audioManager.PlaySingle (winAud);
		this.behaviorStateMachine.ChangeState(new Coloury_Win(this, 30f));
	}


	RoomController room;
	Color correctCol = new Color (1f, 0.4f, 0.0f);
	public RoomController getRoom()
	{
		foreach (RoomController room in ec.rooms) {
			if (room.category == EnumExtensions.GetFromExtendedName<RoomCategory>("Art")) {
				base.transform.position = ec.RealRoomMid (room);
				spawnPoint = ec.RealRoomMid (room);
				DebugMenu.LogEvent ("Found room, Defaulting position");
				return room;
			}
		}
		IntVector2 intVector = IntVector2.GetGridPosition (spawnPoint);
		DebugMenu.LogEvent ("No room was found, Defaulting to spawn tile");
		return ec.cells [intVector.x, intVector.z].room;
	}

}

public class Coloury_StateBase : NpcState
{
	protected Coloury coloury;

	public Coloury_StateBase(Coloury coloury) : base(coloury)
	{
		this.coloury = coloury;
	}
}

public class Coloury_Wandering : Coloury_StateBase
{

	bool calledOut = false;

	private float calloutTime = 3f;

	float artCooldown = 15f;

	public Coloury_Wandering(Coloury coloury, float cooldown = 15f) : base(coloury)
	{
		artCooldown = cooldown;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 0));

		coloury.UpdateSprite ("idle");
	}

	public override void Update()
	{
		base.Update();
		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.coloury.CalloutChance();
			this.calloutTime = 3f;
		}
		this.artCooldown -= Time.deltaTime * this.npc.TimeScale;

	}

	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);

		if (!player.tagged && this.artCooldown <= 0f) {

			if (!calledOut) {
				npc.Navigator.SetSpeed (20f);
				coloury.UpdateSprite ("notice");
				coloury.AudioManager.PlaySingle (coloury.noticeAud);
				base.ChangeNavigationState (new NavigationState_TargetPlayer (this.npc, 0, player.transform.position));
				calledOut = true;
			}
		}
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight (player);
		if (!player.tagged && this.artCooldown <= 0f && calledOut) {
			currentNavigationState.UpdatePosition (player.transform.position);
		}
	}

	public override void PlayerLost(PlayerManager player)
	{
		base.PlayerLost (player);
		npc.Navigator.SetSpeed (16f);
		npc.Navigator.maxSpeed = 16f;
		coloury.UpdateSprite ("idle");
		base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 0));
		calledOut = false;	
	}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.GetComponent<PlayerManager> () != null && this.artCooldown <= 0f) {
			if (!other.GetComponent<PlayerManager> ().tagged) {
				coloury.BeginGame (other.GetComponent<PlayerManager> ());
			}
		}
	}
}

public class Coloury_Game : Coloury_StateBase
{
	protected PlayerManager player;
	public Coloury_Game(Coloury coloury, PlayerManager player) : base(coloury)
	{
		this.player = player;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState (new NavigationState_DoNothing (this.npc, 0));
	}

	public override void Update()
	{
		base.Update();
		if (coloury.PlayerLeft (player)) {
			coloury.MadSequence ();
		}
	}

}

public class Coloury_Mad : Coloury_StateBase
{
	float countdown = 6f;
	public Coloury_Mad(Coloury coloury) : base(coloury)
	{
		
	}

	public override void Enter()
	{
		base.Enter();
		coloury.UpdateSprite ("mad");
		coloury.AudioManager.PlaySingle (coloury.madAud);
	}

	bool hunting = false;

	public override void Update()
	{
		base.Update();
		countdown -= 1f * Time.deltaTime * npc.ec.EnvironmentTimeScale;
		if (countdown <= 0f && !hunting) {
			base.ChangeNavigationState (new NavigationState_TargetPlayer (this.npc, 0, Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position));
			hunting = true;
		}
		if (hunting) {
			currentNavigationState.UpdatePosition (Singleton<CoreGameManager>.Instance.GetPlayer (0).transform.position);
		}
	}


	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.CompareTag("Player") && hunting) {
			coloury.Splatter ();
		}
	}
	/*
		yield return new WaitForSeconds (5f);
		spriteRenderer [0].sprite = happySprite;
		behaviorStateMachine.ChangeState (new Coloury_Wandering (this));*/
}

public class Coloury_Win : Coloury_StateBase
{
	float cooldown = 15f;
	public Coloury_Win(Coloury coloury, float cooldown) : base(coloury)
	{
		this.cooldown = cooldown;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 0));

		coloury.UpdateSprite ("idle");
	}

	public override void Update()
	{
		base.Update();
		if (cooldown > 0f) {
			cooldown -= 1f * Time.deltaTime;
			return;
		}
		npc.behaviorStateMachine.ChangeState (new Coloury_Wandering (coloury));
	}
}
}