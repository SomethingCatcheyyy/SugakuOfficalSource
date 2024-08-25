using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MTM101BaldAPI.Reflection;
using Gemu;

namespace Sugaku.NPCS
{
public class WildcardWilly : CustomNPC
{

	public MovementModifier standBy = new MovementModifier (Vector3.zero, 0f);

	public MovementModifier freezer = new MovementModifier (Vector3.zero, 0.1f);

		public ItemObject soupItem;

	private AudioManager audioManager;

	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

		public SoundObject[] guiltYells;
		public SoundObject ringAudio;
		public SoundObject drummerMode;
		public SoundObject bangAudio;
		public SoundObject soupAccept;
		public SoundObject switchAud;

		public SoundObject[] wanderSoup;
		public SoundObject[] wanderWhistle;

		public SoundObject hammerAnnouncement;




		public AudioManager secondaryAudio;

	public override void Initialize()
	{
		base.Initialize();


		audioManager = GetComponent<PropagatedAudioManager> ();

		navigator.SetRoomAvoidance (false);
		navigator.SetSpeed (20f);

			audioManager.SetSubcolor (Color.blue);



			GameObject pamGO = new GameObject ("Skate Audio");
			pamGO.transform.parent = base.transform;
			pamGO.transform.position = base.transform.position;

			PropagatedAudioManager pam = pamGO.AddComponent<PropagatedAudioManager> ();
			pam.positional = audioManager.positional;
			secondaryAudio = pam;

			soupItem = CarmellaBaldiExtension.BasePlugin.plugin.assetMan.Get<ItemObject> ("WillysSoup");

		this.behaviorStateMachine.ChangeState(getNextState());
			navigationStateMachine.ChangeState (new NavigationState_WanderRandom (this, 99));
	}

		public void PlayAudioQuick(SoundObject soundObject, bool secondary)
		{
			if (secondary) {
				secondaryAudio.PlaySingle (soundObject);
				return;
			}
			audioManager.PlaySingle (soundObject);
		}

		public bool offerFinished = false;


	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
	}

	public void SwitchState()
	{
		this.behaviorStateMachine.ChangeState(getNextState());
	}

		public void CalloutChance(SoundObject[] idleAud)
		{
			if (UnityEngine.Random.value <= 0.05f) {
				this.audioManager.PlayRandomAudio(idleAud);
			}
		}

	public IEnumerator freezeTimer(NPC npc)
	{
		yield return new WaitForSeconds (10f);
		npc.Unfreeze (freezer);
		yield break;
	}

	RoomController getSpecialRoom(List<RoomController> rooms)
	{
		foreach (RoomController room in rooms) {
			if (room.category == RoomCategory.Special) {
				return room;
			}
		}
		return ec.rooms[0];
	}

	public string getSpecialRoomCategory (List<RoomFunction> functions)
	{
		foreach (RoomFunction function in functions) {
			if (function.GetType () == typeof(SilenceRoomFunction)) {
				return "Library";
			}
			if (function.GetType () == typeof(StaminaBoostRoomFunction)) {
				return "Playground";
			}
		}
		return "Cafeteria";
	}


	public Willy_StateBase nextState;

	public Willy_StateBase getNextState()
	{
			int num = Random.Range (0, 6);
			switch (num) {
			case 1:
				return new WSS_Drummer (this);
			case 2:
				return new WSS_Percussion (this);
			case 3:
				return new WSS_Soup (this);
			case 4:
				return new WSS_Whistler (this);
			}

			return new WSS_Alerter (this);
	}

		float timeInSight = 0f;

		int strikes = 0;

		public void ObservePlayer (PlayerManager player)
		{
			if (player.Disobeying && !player.Tagged) {
				this.timeInSight  += Time.deltaTime * base.TimeScale;
				if (this.timeInSight >= player.GuiltySensitivity) {
					Warning (player.ruleBreak, player);
				}
			}
		}

		public void ResetTime()
		{
			this.timeInSight = 0f;
		}

		void RevertWhistle()
		{
			behaviorStateMachine.ChangeState (new WSS_Whistler (this));
			GetComponent<Entity> ().ExternalActivity.moveMods.Remove (standBy);
		}

		public void Warning(string brokenRule, PlayerManager player)
		{
			strikes++;
			GetComponent<Entity> ().ExternalActivity.moveMods.Add (standBy);

			if (strikes < 3) {
				behaviorStateMachine.ChangeState (new WSS_WhistleWarning (this));
				SoundObject lol = CarmellaBaldiExtension.BasePlugin.plugin.assetMan.Get<SoundObject> (string.Format ("Willy Warning {0}", strikes));
				audioManager.PlaySingle (lol);
				Invoke ("RevertWhistle", lol.soundClip.length);
				return;
			}
			if (strikes > 2) {
				Scold (brokenRule, player);
				return;
			}
		}

		public void Scold (string brokenRule, PlayerManager player)
		{
			behaviorStateMachine.ChangeState (new WSS_WhistleScreamer (this));
			this.audioManager.FlushQueue (true);
			ec.GetNPC ("Principal").GetComponent<Principal> ().WhistleReact (player.transform.position);
			if (brokenRule != null) {
				player.RuleBreak (brokenRule, 10f);
				SoundObject lol = CarmellaBaldiExtension.BasePlugin.plugin.assetMan.Get<SoundObject> (string.Format ("Willy Yell {0}", brokenRule));
				audioManager.PlaySingle (lol);
			}
			Invoke ("EndWhistle", 5f);
			strikes = 0;
		}

		void EndWhistle()
		{
			GetComponent<Entity> ().ExternalActivity.moveMods.Remove (standBy);
			ResetAnimation ();
			SwitchState ();
		}

		public void ResetAnimation()
		{
			loopAnim = false;
			UpdateSprite ("idle");
		}

		public void OnRing()
		{
			GetComponent<Entity> ().ExternalActivity.moveMods.Add (standBy);
			base.Invoke ("FastUnfreeze", 5f);
			base.Invoke ("SwitchState", 5f);
		}

		void FastUnfreeze()
		{
			GetComponent<Entity> ().ExternalActivity.moveMods.Remove (standBy);
		}

}

public class Willy_StateBase : NpcState
{
	protected WildcardWilly willy;

	public Willy_StateBase(WildcardWilly willy) : base(willy)
	{
		this.willy = willy;
	}

		public override void Enter ()
		{
			base.Enter ();

		}

		public override void Exit ()
		{
			willy.ResetAnimation ();
			base.Exit ();
		}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.CompareTag ("Player")) {
				PlayerEnter (other.GetComponent<PlayerManager> ());
		}
		if (other.GetComponent<Entity> () != null) {
			EntityEnter (other.GetComponent<Entity> ());
		}
	}

		public virtual void PlayerEnter(PlayerManager player)
		{
			
		}
		public virtual void EntityEnter(Entity entity)
		{

		}

	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight (player);
	}

	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
	}
}

//Helpful Side, Good as hell bro!
/*public class Willy_Helpful : Willy_StateBase
{

	public WillySubstate createHelpfulState()
	{
		return new WSS_TimeFreeze (willy);
			//guide mode being merged into librarian mechanic
	}

	public Willy_Helpful(WildcardWilly willy, float cooldown) : base(willy, cooldown)
	{
	}

	public override void Enter()
	{
		substate = createHelpfulState ();
		base.Enter ();
		base.ChangeNavigationState (new NavigationState_WanderRandom (npc, 0));
	}

	public override void Update()
	{
		base.Update();
	}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
	}

	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight (player);
	}

	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
	}
}

//Classes for his "annoying" mode, connects either to his special-room related mechanic, or his helpful mode
public class Willy_Annoying : Willy_StateBase
{

	public WillySubstate createAnnoyingState()
	{
		int num = Random.Range (0, 4);
		switch(num)
		{
		case 1:
			return new WSS_Percussion (willy);
		case 2:
			return new WSS_Drummer (willy);
		case 3:
			return new WSS_Whistler (willy);
		}
		return new WSS_Alerter (willy);
	}

	public Willy_Annoying(WildcardWilly willy, float cooldown) : base(willy, cooldown)
	{
	}

	public override void Enter()
	{
		substate = createAnnoyingState ();
		base.Enter ();
		base.ChangeNavigationState (new NavigationState_WanderRandom (npc, 0));
	}

	public override void Update()
	{
		base.Update();
	}
	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
	}

	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight (player);
	}

	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
	}
}

//Special Room Mechanics, Only use the one according to the special room generated
public class Willy_Chef : Willy_StateBase
{

	public Willy_Chef(WildcardWilly willy, float cooldown) : base(willy, cooldown)
	{
	}

	public override void Enter()
	{
		substate = new WSS_Soup (willy);
		base.Enter ();
		base.ChangeNavigationState (new NavigationState_WanderRandom (npc, 0));
	}

	public override void Update()
	{
		base.Update();
	}
	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
	}

	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight (player);
	}

	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
	}
}
public class Willy_Library : Willy_StateBase
{

	public Willy_Library(WildcardWilly willy, float cooldown) : base(willy, cooldown)
	{
	}

	public override void Enter()
	{
		base.Enter ();
		base.ChangeNavigationState (new NavigationState_WanderRandom (npc, 0));
	}

	public override void Update()
	{
		base.Update();
	}
	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
	}

	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight (player);
	}

	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
	}
}

//Substates
public class WillySubstate
{
	public WildcardWilly willy;

	public WillySubstate(WildcardWilly willy)
	{
		this.willy = willy;
	}

	public virtual void Enter()
	{
		willy.loopAnim = false;
	}
	public virtual void Update()
	{

	}
	public virtual void DestinationEmpty()
	{

	}
	public virtual void PlayerEnter()
	{

	}
	public virtual void PlayerSee(PlayerManager player)
	{

	}
	public virtual void PlayerSight(PlayerManager player)
	{

	}
	public virtual void EntityEnter(Entity entity)
	{

	}
}

public class WSS_Guide : WillySubstate
{
	public WSS_Guide(WildcardWilly willy) : base(willy)
	{
	}
	public override void Enter()
	{
		base.Enter();
		willy.UpdateSprite ("idle");
		DebugMenu.LogEvent ("Willy is in Guide mode");
	}

		bool clicky = false;

		int phase = 0;

		float timerLeft = 5f;

	public override void Update()
	{
		base.Update();
			if (willy.offerFinished && !clicky) {
				willy.GetComponent<ActivityModifier> ().moveMods.Remove (willy.standBy);
				willy.Whotspot.gameObject.SetActive (false);
				StartSequence ();
				willy.offerFinished = false;
				clicky = true;
			}

			if (phase == 2) {
				timerLeft -= willy.TimeScale * Time.deltaTime;
				if (timerLeft <= 0f) {
					willy.SwitchState ();
				}
			}
	}


		RoomController getClassRoom()
		{
			List<RoomController> roms = willy.ec.rooms;
			List<RoomController> classRoms = new List<RoomController> ();
			foreach (RoomController room in roms) {
				if (room.category == RoomCategory.Class) {
					classRoms.Add (room);

				}
			}
			return classRoms [Random.Range (0, classRoms.Count)];
		}

		void StartSequence()
		{
			RoomController targetRoom = getClassRoom ();

			List<IntVector2> vectors =  targetRoom.entitySafeCells;
			IntVector2 vector = vectors [Random.Range (0, vectors.Count)];



			willy.navigationStateMachine.ChangeState (new NavigationState_TargetPosition (willy, 74, willy.ec.cells [vector.x, vector.z].FloorWorldPosition));
			phase = 1;
		}

	public override void DestinationEmpty()
	{
		base.DestinationEmpty();
			if (phase == 1) {
				Singleton<CoreGameManager>.Instance.GetPlayer (0).transform.position = willy.transform.position;
				willy.navigationStateMachine.ChangeState (new NavigationState_WanderRandom (willy, 74));
				phase = 2;

			}
	}
	public override void PlayerEnter()
	{
		base.PlayerEnter();
	}
	public override void PlayerSee(PlayerManager player)
	{
		base.PlayerSee(player);
	}
	public override void PlayerSight(PlayerManager player)
	{
		base.PlayerSight(player);
	}
	public override void EntityEnter(Entity entity)
	{
		base.EntityEnter(entity);
			if (entity.GetComponent<PlayerManager> () != null && phase == 0) {
				willy.GetComponent<ActivityModifier> ().moveMods.Add (willy.standBy);
				willy.Whotspot.gameObject.SetActive (true);
			}
		}
}

	public class WSS_TimeFreeze : Willy_StateBase
{
	public WSS_TimeFreeze(WildcardWilly willy) : base(willy)
	{
	}
	public override void Enter()
	{
		base.Enter();
			willy.secondaryAudio.PlaySingle (willy.switchAud);
		willy.UpdateSprite ("timeIdle");
		DebugMenu.LogEvent ("Willy is in Time-Freeze mode");
	}
	public override void Update()
	{
		base.Update();
	}

	List<string> chars = new List<string>()
	{
		"Baldi",
		"DrRefelx",
		"Coloury",
		"Principal",
		"Rhythm",
		"Pomp"
	};
		public override void PlayerInSight(PlayerManager player)
	{
			base.PlayerInSight(player);
		foreach (NPC npc in player.ec.Npcs) {
			if (chars.Contains (npc.Character.ToString ())) {
				if (Vector3.Distance (npc.transform.position, player.transform.position) <= 30f) {
						
						willy.GetComponent<ActivityModifier>().moveMods.Add(willy.standBy);
						npc.Freeze (willy.freezer, Color.cyan);
						willy.StartCoroutine (willy.freezeTimer (npc));
							npc.behaviorStateMachine.ChangeState (new WSS_PostFreeze (willy));
					
				}
			}
		}
	}


}*/

	public class WSS_Soup : Willy_StateBase
{

	public WSS_Soup(WildcardWilly willy) : base(willy)
	{
	}
	public override void Enter()
	{
			base.Enter();
			willy.secondaryAudio.PlaySingle (willy.switchAud);
		willy.UpdateSprite ("soup");
		DebugMenu.LogEvent ("Willy is in Soup mode");
	}
		public override void PlayerEnter(PlayerManager player)
	{
			base.PlayerEnter(player);


			willy.PlayAudioQuick (willy.soupAccept, false);
			player.itm.AddItem (willy.soupItem);
			npc.behaviorStateMachine.ChangeState (new WSS_SoupFinish (willy));
	}

		float calloutChance = 3f;

		public override void Update()
		{
			base.Update();
			calloutChance -= npc.TimeScale * Time.deltaTime;
			if (calloutChance <= 0f) {
				willy.CalloutChance (willy.wanderSoup);
				calloutChance = 3f;
			}
		}
}

	/*public class WSS_PostFreeze : Willy_StateBase
	{

		float timer = 1f;
		public WSS_PostFreeze(WildcardWilly willy) : base(willy)
		{
		}

		public override void Enter ()
		{
			base.Enter ();
			DebugMenu.LogEvent ("NPC has been shot");
			willy.UpdateSprite ("timeShoot");
		}

		public override void Update()
		{
			base.Update();
			timer -= willy.TimeScale * Time.deltaTime;
			if (timer <= 0f) {
				DebugMenu.LogEvent ("Timer is 0, switching state");

				willy.GetComponent<ActivityModifier> ().moveMods.Remove (willy.standBy);
				willy.SwitchState ();
			}

		}
	}*/

	public class WSS_SoupFinish : Willy_StateBase
	{

		float timer = 5f;
		public WSS_SoupFinish(WildcardWilly willy) : base(willy)
		{
		}

		public override void Enter ()
		{
			base.Enter ();
			willy.UpdateSprite ("soupAccept");
		}

		public override void Update()
		{
			base.Update();
				timer -= willy.TimeScale * Time.deltaTime;
				if (timer <= 0f) {
					willy.SwitchState ();
				timer = 5f;
				}

		}
	}


	public class WSS_Percussion : Willy_StateBase
{

		float timer = 30f;
	public WSS_Percussion(WildcardWilly willy) : base(willy)
	{

	}
	public override void Enter()
	{
		base.Enter();
			timer = Random.Range (30f, 45f);
			willy.secondaryAudio.PlaySingle (willy.switchAud);
			willy.PlayAudioQuick (willy.hammerAnnouncement, false);
		willy.UpdateSprite ("hammer");
		DebugMenu.LogEvent ("Willy is in Percussion mode");
	}
	public override void Update()
	{
		base.Update();
			timer -= Time.deltaTime * willy.TimeScale;
			if (timer <= 0f) {
				willy.SwitchState ();
				timer = 30f;
			}
	}
	public override void DestinationEmpty()
	{
		base.DestinationEmpty();
	}
		public override void PlayerEnter(PlayerManager player)
	{
			base.PlayerEnter(player);
	}
	public override void EntityEnter(Entity entity)
	{
		base.EntityEnter(entity);
		if (!entity.Squished) {
			entity.Squish (15f);
				willy.PlayAnimation ("bang", 12f, 3, delegate() {
				willy.UpdateSprite ("hammer");
			});
				willy.PlayAudioQuick (willy.bangAudio, true);

		}
	}
}

	public class WSS_Drummer : Willy_StateBase
{
		float timerLeft = 30f;
	public WSS_Drummer(WildcardWilly willy) : base(willy)
	{
	}
	public override void Enter()
	{
			base.Enter();

			willy.secondaryAudio.PlaySingle (willy.switchAud);
			willy.PlayAudioQuick (willy.drummerMode, false);
		willy.loopAnim = true;
		willy.PlayAnimation ("drum", 12, 4, delegate() {
			
		});
		DebugMenu.LogEvent ("Willy is in Drummer mode");
	}
	public override void Update()
	{
		base.Update();
			timerLeft -= Time.deltaTime * willy.TimeScale;
			if (timerLeft <= 0f) {
				willy.SwitchState ();
			}
	}
}

	public class WSS_Whistler : Willy_StateBase
{
	public WSS_Whistler(WildcardWilly willy) : base(willy)
	{
	}
	public override void Enter()
	{
		base.Enter();

			willy.secondaryAudio.PlaySingle (willy.switchAud);
			willy.UpdateSprite ("whistleIdle");
		DebugMenu.LogEvent ("Willy is in Whistle mode");
	}

		float calloutChance = 3f;

	public override void Update()
	{
		base.Update();
			calloutChance -= npc.TimeScale * Time.deltaTime;
			if (calloutChance <= 0f) {
				willy.CalloutChance (willy.wanderWhistle);
				calloutChance = 3f;
			}
	}
	public override void DestinationEmpty()
	{
		base.DestinationEmpty();
	}
	public override void EntityEnter(Entity entity)
	{
		base.EntityEnter(entity);
	}

		public override void PlayerInSight (PlayerManager player)
		{
			base.PlayerInSight (player);
			willy.ObservePlayer (player);
		}
}

	public class WSS_WhistleWarning : Willy_StateBase
	{

		float timer = 5f;
		public WSS_WhistleWarning(WildcardWilly willy) : base(willy)
		{
		}

		public override void Enter ()
		{
			base.Enter ();
			willy.UpdateSprite ("whistleWarning");
		}

		public override void Update()
		{
			base.Update();

		}
	}

	public class WSS_WhistleScreamer : Willy_StateBase
	{

		public WSS_WhistleScreamer(WildcardWilly willy) : base(willy)
		{
		}

		public override void Enter ()
		{
			base.Enter ();
			willy.loopAnim = true;
			willy.PlayAnimation ("whistleScream", 12f, 2, delegate() {
				
			});
		}

		public override void Update()
		{
			base.Update();

		}
	}

	public class WSS_Alerter : Willy_StateBase
{
	public WSS_Alerter(WildcardWilly willy) : base(willy)
	{
	}
	public override void Enter()
	{
		base.Enter();

			willy.secondaryAudio.PlaySingle (willy.switchAud);

		willy.UpdateSprite ("idle");
		DebugMenu.LogEvent ("Willy is in Alert mode");
	}
	public override void Update()
	{
		base.Update();
	}
	public override void DestinationEmpty()
	{
		base.DestinationEmpty();
	}
		public override void PlayerEnter(PlayerManager player)
	{
			base.PlayerEnter(player);
		willy.loopAnim = true;
		willy.PlayAnimation ("ring", 12f, 2, delegate() {
			
		});

			willy.PlayAudioQuick (willy.ringAudio, true);
		willy.ec.MakeNoise (willy.transform.position, 75);
			willy.OnRing ();
	}

}
}