using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gemu;

namespace Sugaku.NPCS
{
public class Zark : CustomNPC, IEntityTrigger
{

	private AudioManager audioManager;


	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

	public SoundObject[] audIdle;
	public SoundObject[] audRide;
	public SoundObject[] audThank;


	public override void Initialize()
	{
		base.Initialize();
		audioManager = GetComponent<PropagatedAudioManager> ();
			audioManager.SetSubcolor (new Color ().from255 (63f, 72f, 204f));
		GenerateHotSpot ();
		UpdateSprite ("idle");
		navigator.SetRoomAvoidance (false);
		this.behaviorStateMachine.ChangeState(new Zark_Wander(this));
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
		ZarkHotspot hs = hotspot.AddComponent<ZarkHotspot> ();
		hs.zark = this;
		giftHotspot = hotspot;
	}

	float t = 0f;
	float spriteSpeed = 45f;

	public PlayerManager player;

	float ridecoolDown = 1f;

		int entitieCount = 0;

	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
		t -= spriteSpeed * Time.deltaTime * ec.EnvironmentTimeScale;
		spriteRenderer [0].SetSpriteRotation (t);
		this.moveMod.movementAddend = this.navigator.Velocity.normalized * this.navigator.speed * this.navigator.Am.Multiplier;
		if (playerRiding) {
			ridecoolDown -= Time.deltaTime * ec.EnvironmentTimeScale;
			if (ridecoolDown <= 0f) {
				if (Singleton<InputManager>.Instance.GetDigitalInput ("Interact", true)) {
					ActivityModifier externalActivity = player.GetComponent<Entity> ().ExternalActivity;

					if (externalActivity.moveMods.Contains (moveMod)) {
						audioManager.PlayRandomAudio (audThank);
						externalActivity.moveMods.Remove (this.moveMod);
							entities.Remove (player.GetComponent<Entity> ());
							OnEntityChange (false);
					}
					playerRiding = false;
						ridecoolDown = 1f;
						OnEntityChange (false);
				}
			}
		}

		foreach (Entity entity in entities) {
			if (Vector3.Distance (entity.transform.position, base.transform.position) >= 6f) {
				ActivityModifier externalActivity = entity.ExternalActivity;
				if (externalActivity.moveMods.Contains (moveMod)) {
					audioManager.PlayRandomAudio (audThank);
					externalActivity.moveMods.Remove (this.moveMod);
					entities.Remove (entity);
						OnEntityChange (false);
				}
			}
		}
	}

		void OnEntityChange(bool addend)
		{
			if (addend) {
				entitieCount += 2;
			}
			entitieCount--;
			if (entitieCount == 0) {

				navigator.SetSpeed (22f);
				navigator.maxSpeed = 22f;
				return;
			}

			navigator.SetSpeed (32f);
			navigator.maxSpeed = 32f;
		}

	public bool playerRiding = false;

	public void PlayerRide(PlayerManager pm)
	{
		player = pm;
		pm.Teleport(base.transform.position);
		Entity component = pm.GetComponent<Entity>();
		ActivityModifier externalActivity = component.ExternalActivity;
		if (!externalActivity.moveMods.Contains(this.moveMod))
		{
			audioManager.PlayRandomAudio (audRide);
			externalActivity.moveMods.Add(this.moveMod);
			entities.Add (component);
				playerRiding = true;
				OnEntityChange (true);
		}
	}

	MovementModifier moveMod = new MovementModifier(Vector3.zero, 0f);

	List<Entity> entities = new List<Entity>();

	protected override void VirtualOnTriggerEnter(Collider other)
	{
		if (other.isTrigger && other.CompareTag("NPC"))
		{
			if (Random.value <= 0.2f) {
				Entity component = other.GetComponent<Entity> ();
					NPC npc = other.GetComponent<NPC> ();
					if (component != null && component.Velocity.magnitude > 0f && npc.GetType() != this.GetType()) {
					ActivityModifier externalActivity = component.ExternalActivity;
					if (!externalActivity.moveMods.Contains (this.moveMod)) {
						audioManager.PlayRandomAudio (audRide);
						externalActivity.moveMods.Add (this.moveMod);
						entities.Add (component);
							StartCoroutine (rideTimer (component));
							OnEntityChange (true);
					}
				}
			}
		}
	}

	IEnumerator rideTimer(Entity entity)
	{
		float num = Random.Range (15f, 45f);
		yield return new WaitForSeconds (num);
		ActivityModifier externalActivity = entity.ExternalActivity;
		if (externalActivity.moveMods.Contains (moveMod)) {
			audioManager.PlayRandomAudio (audThank);
			externalActivity.moveMods.Remove (this.moveMod);
				entities.Remove (entity);
				OnEntityChange (false);
		}
		yield break;
	}


	public void CalloutChance()
	{
		if (UnityEngine.Random.value <= 0.2f) {
			this.audioManager.PlayRandomAudio(this.audIdle);
		}
	}
}

public class ZarkHotspot : MonoBehaviour, IClickable<int>
{
	public Zark zark;
	public void Clicked(int playerNumber)
	{
		PlayerManager pm = Singleton<CoreGameManager>.Instance.GetPlayer (playerNumber);
		if (!zark.playerRiding) {
			zark.PlayerRide (pm);
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
		return zark.playerRiding;
	}

	// Token: 0x0600018D RID: 397 RVA: 0x00003801 File Offset: 0x00001A01
	public bool ClickableRequiresNormalHeight()
	{
		return true;
	}
}

public class Zark_StateBase : NpcState
{
	protected Zark zark;

	public Zark_StateBase(Zark zark) : base(zark)
	{
		this.zark = zark;
	}
}

public class Zark_Wander : Zark_StateBase
{

	float calloutTime = 3f;

	public Zark_Wander(Zark zark) : base(zark)
	{
	}

	public override void Enter()
	{
		base.Enter ();
		npc.Navigator.SetSpeed (22f);
		npc.Navigator.maxSpeed = 22f;
		base.ChangeNavigationState (new NavigationState_WanderRandom (npc, 0));
	}

	public override void Update()
	{
		base.Update();
		calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (calloutTime <= 0f) {
			zark.CalloutChance ();
			calloutTime = 3f;
		}
	}
}
}