using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Sugaku.NPCS
{

public class Billbert : NPC, IEntityTrigger
{

	List<Cell> points = new List<Cell>();

	List<TileShape> possibleShapes = new List<TileShape>
	{
		TileShape.Corner,
		TileShape.End,
		TileShape.Single
	};

	public Sprite walkSprite;

	public List<Sprite> runSprites;

	int curFrame = 0;

	public AudioManager audMan;

	public SoundObject run;

	public bool runningEnabled = false;

	public override void Initialize()
	{
		base.Initialize();

		audMan = GetComponent<PropagatedAudioManager> ();
			audMan.SetSubcolor (Color.red);
		navigator.SetRoomAvoidance (true);
		this.behaviorStateMachine.ChangeState(new Billbert_Idle(this));
	}



	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
		if (runningEnabled) {
			int i = curFrame;
			curFrame = (i + 1) % runSprites.Count;
			spriteRenderer [0].sprite = runSprites [curFrame];
			return;
		}
		spriteRenderer [0].sprite = walkSprite;
	}


	public void Bump(Entity entity)
	{
		entity.AddForce(new Force((Navigator.Velocity).normalized, 40f, -30f));
	}
}
	
public class Billbert_StateBase: NpcState
{
	protected Billbert billbert;

	public Billbert_StateBase(Billbert billbert) : base(billbert)
	{
		this.billbert = billbert;
	}
}

public class Billbert_Idle : Billbert_StateBase
{


	public Billbert_Idle (Billbert billbert) : base (billbert)
	{
	}

	public override void Enter()
	{
		base.Enter();
		billbert.runningEnabled = false;
		DebugMenu.LogEvent ("?????");
		IntVector2 intVector = IntVector2.GetGridPosition (billbert.transform.position);
		base.ChangeNavigationState(new NavigationState_DoNothing(this.npc, 64));
	}

	public override void Update()
	{
		base.Update();
	}

	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);
		if (!billbert.runningEnabled) {
			billbert.behaviorStateMachine.ChangeState(new Billbert_Run(billbert, player));
			//Directions.DirFromVector3(player.transform.position - base.transform.position, 45f).ToRotation();
		}
	}
}
public class Billbert_Run : Billbert_StateBase
{
	PlayerManager player;

	List<List<Cell>> halls = new List<List<Cell>>();

	public Cell cell;

	Direction dir = Direction.North;

	public Billbert_Run (Billbert billbert, PlayerManager player) : base (billbert)
	{
		this.player = player;
	}

	public override void Enter()
	{
		base.Enter();
		dir = Directions.DirFromVector3 (player.transform.position - billbert.transform.position, 45f);
		billbert.transform.rotation = dir.ToRotation();
		//FindPath ();

		//Vector3 pos = GetDestionaion (dir);

		IntVector2 holdOn;
		billbert.ec.FurthestTileInDir (IntVector2.GetGridPosition (billbert.transform.position), dir, out holdOn);
		Cell ciell = billbert.ec.cells [holdOn.x, holdOn.z];

		billbert.runningEnabled = true;
		DebugMenu.LogEvent ("Run Tiem!");
		billbert.Navigator.SetSpeed (30f);

		base.ChangeNavigationState(new NavigationState_TargetPosition(this.npc, 64, ciell.FloorWorldPosition));
		billbert.audMan.SetLoop (true);
		billbert.audMan.QueueAudio (billbert.run);
	}

	public override void Update()
	{
		base.Update();
	}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.CompareTag ("NPC") || other.CompareTag ("Player")) {
			billbert.Bump (other.GetComponent<Entity> ());
		}
	}


	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
		Debug.Log ("Nope");
		billbert.audMan.FlushQueue (true);
		billbert.behaviorStateMachine.ChangeState(new Billbert_Idle(billbert));
	}

}
}