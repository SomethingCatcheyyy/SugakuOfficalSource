using UnityEngine;
using System.Collections;
using Gemu;
using MTM101BaldAPI.Reflection;

namespace Sugaku.NPCS
{
	
public class Carmella : CustomNPC, IEntityTrigger
{
	public bool offerMode;

	public SoundObject[] idleAud;
	public SoundObject offerAud;
	public SoundObject[] leaveAud;
	public SoundObject[] thankAud;
	public SoundObject[] explodeAud;

	public SoundObject xyloNoise;
	public SoundObject boom;

	private AudioManager audioManager;

	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		GenerateHotSpot ();
		audioManager = GetComponent<PropagatedAudioManager> ();
			audioManager.ReflectionSetVariable("subtitleColor", new Color (0.66f, 1f, 0.32f));
		navigator.SetRoomAvoidance (false);
		navigator.SetSpeed (16f);
		ActivateHotspot (false);
		this.behaviorStateMachine.ChangeState(new Carmella_DefaultState(this));
	}
	public GameObject giftHotspot;
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
		CarmellaHotspot hs = hotspot.AddComponent<CarmellaHotspot> ();
		hs.carmella = this;
		giftHotspot = hotspot;
	}


	public void ActivateHotspot(bool show)
	{
		giftHotspot.SetActive (show);
	}

	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
	}

	public void CalloutChance()
	{
		if (UnityEngine.Random.value <= 0.05f) {
			this.audioManager.PlayRandomAudio(this.idleAud);
		}
	}


	public bool PlayerLeft(PlayerManager player)
	{
		return Vector3.Distance(player.transform.position, base.transform.position) > 15f && offerMode;
	}

	public ItemObject giftItem;

	public void OnClicked(PlayerManager pm)
	{
		int num = UnityEngine.Random.Range (0, 3);
		if (num == 1) {
			//DefectiveCode
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle (boom);
				pm.GetComponent<Entity> ().AddForce (new Force (-pm.transform.forward, 40f, -40f));
			ec.MakeNoise (base.transform.position, 74);
			behaviorStateMachine.ChangeState(new Carmella_Exploded(this, 15f));
		} else {
			if (pm.itm.InventoryFull ()) {
					Pickup pickupPre = (Pickup)ec.ReflectionGetVariable ("pickupPre");

					Pickup doodoo = UnityEngine.Object.Instantiate<Pickup>(pickupPre, ec.transform);

					doodoo.item = giftItem;
					doodoo.transform.position = base.transform.position;
			} else {
				pm.itm.AddItem (giftItem);
			}
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle (ObjectFunctions.FindResourceOfName<SoundObject> ("YTPPickup_0"));

			behaviorStateMachine.ChangeState(new Carmella_Thank(this, 15f));
		}
	}
}

public class CarmellaHotspot : MonoBehaviour, IClickable<int>
{
	public Carmella carmella;
	public void Clicked(int playerNumber)
	{
		PlayerManager pm = Singleton<CoreGameManager>.Instance.GetPlayer (playerNumber);
		if (carmella.offerMode) {
			carmella.OnClicked (pm);
		}
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

public class Carmella_StateBase: NpcState
{
	protected Carmella carmella;

	public Carmella_StateBase(Carmella carmella) : base(carmella)
	{
		this.carmella = carmella;
	}
}

public class Carmella_DefaultState : Carmella_StateBase
{
	private float calloutTime = 3f;

	public Carmella_DefaultState (Carmella carmella) : base (carmella)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 0));
		carmella.UpdateSprite ("idle");
	}

	public override void Update()
	{
		base.Update();
		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.carmella.CalloutChance();
			this.calloutTime = 3f;
		}
	}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.CompareTag ("Player")) {
			PlayerManager pm = other.GetComponent<PlayerManager> ();
			npc.behaviorStateMachine.ChangeState(new Carmella_Offer(carmella, pm));
		}
	}
}

public class Carmella_Offer : Carmella_StateBase
{
	protected PlayerManager player;

	public Carmella_Offer (Carmella carmella, PlayerManager player) : base (carmella)
	{
		this.player = player;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState(new NavigationState_DoNothing(this.npc, 0));
		carmella.UpdateSprite ("offer");
		carmella.offerMode = true;
		carmella.AudioManager.QueueAudio (carmella.offerAud);
		carmella.ActivateHotspot (true);
	}

	public override void Update()
	{
		base.Update();
		if (carmella.PlayerLeft (player)) {
			npc.behaviorStateMachine.ChangeState(new Carmella_Displeased(carmella, 15f));
		}
	}
}

public class Carmella_Displeased : Carmella_StateBase
{
	protected float coolDown;
	public Carmella_Displeased (Carmella carmella, float coolDown) : base (carmella)
	{
		this.coolDown = coolDown;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 0));
		carmella.UpdateSprite ("disappointedo");
		carmella.offerMode = false;
		carmella.AudioManager.FlushQueue (true);
		carmella.AudioManager.QueueRandomAudio (carmella.leaveAud);
		carmella.ActivateHotspot (false);
	}

	void UpdateSprite()
	{
		if (carmella.AudioManager.AnyAudioIsPlaying) 
		{
			carmella.UpdateSprite ("disappointedo");
			return;
		}
		carmella.UpdateSprite ("disappointedC");
	}

	public override void Update()
	{
		base.Update();
		UpdateSprite ();
		if (coolDown > 0f) {
			coolDown -= 1f * Time.deltaTime;
			return;
		}
		npc.behaviorStateMachine.ChangeState(new Carmella_DefaultState(carmella));
	}
}

public class Carmella_Thank : Carmella_StateBase
{
	protected float coolDown;
	public Carmella_Thank (Carmella carmella, float coolDown) : base (carmella)
	{
		this.coolDown = coolDown;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 0));

		carmella.UpdateSprite ("thank");
		carmella.offerMode = false;
		carmella.AudioManager.FlushQueue (true);
		carmella.AudioManager.QueueRandomAudio (carmella.thankAud);
		carmella.ActivateHotspot (false);
	}

	public override void Update()
	{
		base.Update();
		if (coolDown > 0f) {
			coolDown -= 1f * Time.deltaTime;
			return;
		}
		npc.behaviorStateMachine.ChangeState(new Carmella_DefaultState(carmella));
	}
}

public class Carmella_Exploded : Carmella_StateBase
{
	protected float coolDown;
	public Carmella_Exploded (Carmella carmella, float coolDown) : base (carmella)
	{
		this.coolDown = coolDown;
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 0));

		carmella.UpdateSprite ("explodeO");
		carmella.offerMode = false;
		carmella.AudioManager.FlushQueue (true);
		carmella.AudioManager.QueueRandomAudio (carmella.explodeAud);
		carmella.ActivateHotspot (false);
	}

	void UpdateSprite()
	{
		if (carmella.AudioManager.AnyAudioIsPlaying) 
		{
			carmella.UpdateSprite ("explodeO");
			return;
		}
		carmella.UpdateSprite ("explodeC");
	}

	public override void Update()
	{
		base.Update();
		UpdateSprite ();
		if (coolDown > 0f) {
			coolDown -= 1f * Time.deltaTime;
			return;
		}
		npc.behaviorStateMachine.ChangeState(new Carmella_DefaultState(carmella));
	}
}
}