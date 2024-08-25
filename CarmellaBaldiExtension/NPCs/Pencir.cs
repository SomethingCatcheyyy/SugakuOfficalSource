using UnityEngine;
using System.Collections;


namespace Sugaku.NPCS
{
public class Pencir : NPC, IEntityTrigger
{


	private AudioManager audioManager;


	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

	private AudioManager moveAudio;

	public float wanderSpeed = 30f;
	public float turnSpeed = 12.5f;
	public float angleRange = 15f;

	public Sprite[] allSprites;

	public SoundObject moveAud;
	public SoundObject audStab;

	public float stam = 0f;

	public bool canStab = true;

	MovementModifier standstill = new MovementModifier(Vector3.zero, 0f);

	public override void Initialize()
	{
		base.Initialize();
		stam = Singleton<CoreGameManager>.Instance.GetPlayer(0).GetComponent<PlayerMovement> ().staminaRise;
		audioManager = GetComponent<PropagatedAudioManager> ();

		SpriteRotatorCustom sr = spriteRenderer [0].gameObject.AddComponent<SpriteRotatorCustom> ();
		sr.sprites = allSprites;
		sr.spriteRenderer = spriteRenderer [0];
		sr.offset = 2;

		navigator.SetRoomAvoidance (true);
		this.behaviorStateMachine.ChangeState(new Pencir_MainState(this));

		GameObject pamGO = new GameObject ("Skate Audio");
		pamGO.transform.parent = base.transform;
		pamGO.transform.position = base.transform.position;

		PropagatedAudioManager pam = pamGO.AddComponent<PropagatedAudioManager> ();
		pam.positional = audioManager.positional;
		moveAudio = pam;
		StartCoroutine (audioDelay ());

		base.ignoreBelts = true;
	}

	IEnumerator audioDelay()
	{
		moveAudio.audioDevice = audioManager.audioDevice;
		moveAudio.maintainLoop = true;
		moveAudio.loop = true;
		yield return null;
		moveAudio.QueueAudio (moveAud);
		yield break;
	}

	MovementModifier moveMod = new MovementModifier(Vector3.zero, 0.6f);

	protected override void VirtualOnTriggerEnter(Collider other)
	{
		if (navigator.Velocity.magnitude >= 0.5f && canStab) {
			if (other.CompareTag ("Player")) {
				PlayerManager pm = other.GetComponent<PlayerManager> ();
				SetGuilt (1f, "Bullying");
				StartCoroutine (STAB (pm));
			}
			if (other.CompareTag ("NPC")) {
				NPC otherN = other.GetComponent<NPC> ();
				SetGuilt (1f, "Bullying");
				StartCoroutine (stabMNPC (otherN));
			}
		}


	}

	public override void SentToDetention()
	{
		base.SentToDetention ();
		canStab = false;
		GetComponent<NPC>().Freeze (standstill, Color.gray);
		base.Invoke ("OkYah", 15f);
	}

	void OkYah()
	{
		canStab = true;
		GetComponent<NPC>().Unfreeze (standstill);
	}

	IEnumerator STAB(PlayerManager pm)
	{
		audioManager.PlaySingle (audStab);
		pm.Am.moveMods.Add (moveMod);
		pm.GetComponent<PlayerMovement> ().staminaRise = 0f;
		pm.GetComponent<PlayerMovement> ().stamina = 0f;
		yield return new WaitForSeconds (20f);
		pm.Am.moveMods.Remove (moveMod);
		pm.GetComponent<PlayerMovement> ().staminaRise = stam;
	}
	IEnumerator stabMNPC(NPC npc)
	{
		audioManager.PlaySingle (audStab);
		npc.Navigator.Am.moveMods.Add (moveMod);
		yield return new WaitForSeconds (20f);
		npc.Navigator.Am.moveMods.Remove (moveMod);
	}
}

public class Pencir_StateBase : NpcState
{
	protected Pencir pencir;

	public Pencir_StateBase(Pencir pencir) : base (pencir)
	{
		this.pencir = pencir;
	}
}

public class Pencir_MainState : Pencir_StateBase
{


	private Vector3 nextTarget;

 



	private float _angleDiff;


	public Pencir_MainState(Pencir pencir) : base (pencir)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState( new NavigationState_WanderRandom(this.npc, 74));
	}

	public override void Update()
	{
		base.Update();
		/*if (!this.npc.Navigator.HasDestination && this.npc.Navigator.Speed <= this.pencir.wanderSpeed + 1f)
		{
			base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 74));
		}*/
		if(npc.Navigator.HasDestination)
		{
			this._angleDiff = Mathf.DeltaAngle(this.npc.transform.eulerAngles.y, Mathf.Atan2(this.npc.Navigator.NextPoint.x - this.npc.transform.position.x, this.npc.Navigator.NextPoint.z - this.npc.transform.position.z) * 57.29578f);

			if (Mathf.Abs(this._angleDiff) > this.pencir.angleRange)
			{
				this.npc.Navigator.maxSpeed = 0f;
				this.npc.Navigator.SetSpeed(0f);
			}
			else
			{
				this.npc.Navigator.maxSpeed = this.pencir.wanderSpeed;
			}
		}
		/*if (Mathf.Abs(this._angleDiff) <= this.pencir.angleRange)
		{
			//this._rotation = this.npc.transform.eulerAngles;
			////this._rotation.y = this._rotation.y + Mathf.Min(this.pencir.turnSpeed * Time.deltaTime * this.npc.TimeScale, Mathf.Abs(this._angleDiff)) * Mathf.Sign(this._angleDiff);
			//this.npc.transform.eulerAngles = this._rotation;
		}*/
		npc.transform.Rotate(new Vector3(0f, pencir.turnSpeed * Mathf.Sign(_angleDiff) * Time.deltaTime * npc.ec.EnvironmentTimeScale, 0f));

	}

	public void TargetPosition(Vector3 target)
	{
		if (this.npc.Navigator.Speed > this.pencir.wanderSpeed + 1f)
		{
			this.nextTarget = target;
			return;
		}
		//base.ChangeNavigationState(this.targetState);
		//this.targetState.UpdatePosition(target);
		this.npc.Navigator.SkipCurrentDestinationPoint();
		this.nextTarget = Vector3.zero;
	}
	public override void DestinationEmpty()
	{
		base.DestinationEmpty();
	}


}
}