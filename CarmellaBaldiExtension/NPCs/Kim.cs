using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CarmellaBaldiExtension;
using Rewired;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.UI;
using MTM101BaldAPI;


namespace Sugaku.NPCS
{
public class Kim  : CustomNPC, IEntityTrigger
{
	private AudioManager audioManager;

	public GameObject instructions;

	public AudioManager AudioManager
	{
		get
		{
			return this.audioManager;
		}
	}

	public SoundObject[] audWander;
	public SoundObject audNotice;
	public SoundObject audGame;
	public SoundObject audCountdown;
	public SoundObject[] audWin;
	public SoundObject[] audLose;

	public SoundObject audTie;

	public SoundObject[] rpsInputs;



	public override void Initialize()
	{
		base.Initialize();
		AddInstructions ();
		audioManager = GetComponent<PropagatedAudioManager> ();
		navigator.SetRoomAvoidance (false);
		this.behaviorStateMachine.ChangeState(new Kim_Wandering(this));
		instructions.SetActive (false);
			audioManager.SetSubcolor (new Color().from255(163f, 21f, 238f));

	}

	Canvas lol;

	void AddInstructions()
	{
		lol = GUITools.CreateCanvas ("Kim UI", 1280f, 720f);

		Image rockButton = UIHelpers.CreateImage (BasePlugin.plugin.assetMan.Get<Sprite>("Button_Rock"), lol.transform, new Vector3(
			-256f,
			250f,
			0f
		), false, 1f);

		rockButton.gameObject.AddComponent<RPSButton> ().Setup (rockButton, 
			BasePlugin.plugin.assetMan.Get<Sprite> ("Button_Rock"),
			BasePlugin.plugin.assetMan.Get<Sprite> ("Button_RockPress"),
			0);

		//rockButtonReal.highlightedSprite = BasePlugin.plugin.assetMan.Get<Sprite> ("Button_RockPress");
		//rockButtonReal.unhighlightedSprite = BasePlugin.plugin.assetMan.Get<Sprite> ("Button_Rock");

		Image paperButton = UIHelpers.CreateImage (BasePlugin.plugin.assetMan.Get<Sprite>("Button_Paper"), lol.transform, new Vector3(
			0f,
			250f,
			0f
		), false, 1f);

		paperButton.gameObject.AddComponent<RPSButton> ().Setup (paperButton, 
			BasePlugin.plugin.assetMan.Get<Sprite> ("Button_Paper"),
			BasePlugin.plugin.assetMan.Get<Sprite> ("Button_PaperPress"),
			1);
			//BeginSequence (Random.Range (0, 3), 1, Singleton<CoreGameManager>.Instance.GetPlayer(0));


		Image scissorsButton = UIHelpers.CreateImage (BasePlugin.plugin.assetMan.Get<Sprite>("Button_Scissors"), lol.transform, new Vector3(
			256f,
			250f,
			0f
		), false, 1f);

		scissorsButton.gameObject.AddComponent<RPSButton> ().Setup (scissorsButton, 
			BasePlugin.plugin.assetMan.Get<Sprite> ("Button_Scissors"),
			BasePlugin.plugin.assetMan.Get<Sprite> ("Button_ScissorsPress"),
			2);

		//StandardMenuButton sB = paperButton.gameObject.ConvertToButton <StandardMenuButton>();
		//sB.highlightedSprite = BasePlugin.plugin.assetMan.Get<Sprite> ("Button_ScissorsPress");
		//sB.unhighlightedSprite = BasePlugin.plugin.assetMan.Get<Sprite> ("Button_Scissors");

		//paperBut.highlightedSprite = BasePlugin.plugin.assetMan.Get<Sprite> ("Button_PaperPress");
		//paperBut.unhighlightedSprite = BasePlugin.plugin.assetMan.Get<Sprite> ("Button_Paper");
		//paperBut.swapOnHigh = true;

		instructions = lol.gameObject;
	}

	string GetChoiceSprite(int val)
	{
		switch (val) {
		case 0:
			return "chR";
		case 1:
			return "chP";
		case 2:
			return "chS";
		default:
			return "WAHT THE HELL DID YOU DO?!?!?";
		}
	}

	public void BeginSequence(int entC, int plaC, PlayerManager player)
	{
		instructions.SetActive (false);
		StartCoroutine(RPSSequence(entC, plaC, player));
	}

	IEnumerator RPSSequence(int entC, int plaC, PlayerManager player)
	{
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle (rpsInputs [plaC]);
		audioManager.FlushQueue (true);
		audioManager.PlaySingle (audCountdown);
		UpdateSprite ("decision");
		yield return new WaitForSeconds (1f);
		UpdateSprite (GetChoiceSprite (entC));
		//Singleton<CoreGameManager>.Instance.audMan.PlaySingle (BasePlugin.plugin.assetMan.Get<SoundObject>("genericWhip"));
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle (rpsInputs [entC]);
		yield return new WaitForSeconds (1f);
		OnInput (entC, plaC, player);
		yield break;
	}

	void OnInput(int entityChoice, int choice, PlayerManager pm)
	{
		switch (entityChoice) {
		case 0:
			switch (choice) {
			case 0:
				GameTie (pm);
				break;
			case 1:
				EndGame (true, pm);
				break;
			case 2:
				EndGame (false, pm);
				break;
			}
			break;
		case 1:
			switch (choice) {
			case 0:
				EndGame (false, pm);
				break;
			case 1:
				GameTie (pm);
				break;
			case 2:
				EndGame (true, pm);
				break;
			}
			break;
		case 2:
			switch (choice) {
			case 0:
				EndGame (true, pm);
				break;
			case 1:
				EndGame (false, pm);
				break;
			case 2:
				GameTie (pm);
				break;
			}
			break;
		}
	}

	public void LeaveFailsave(PlayerManager pm)
	{

		pm.Am.moveMods.Remove (moveMod);
		instructions.SetActive (false);
		behaviorStateMachine.ChangeState (new Kim_Lose (this, 15f));
	}

	// Token: 0x06000098 RID: 152 RVA: 0x0000233B File Offset: 0x0000053B
	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
	}

	public void CalloutChance()
	{
		if (UnityEngine.Random.value <= 0.1f) {
			this.audioManager.PlayRandomAudio(this.audWander);
		}
	}


	public bool PlayerLeft(PlayerManager player)
	{
		return Vector3.Distance(player.transform.position, base.transform.position) > 20f;
	}

	public void EndGame(bool win, PlayerManager pm)
	{
		pm.Am.moveMods.Remove (moveMod);
		if (win) {
			Singleton<CoreGameManager>.Instance.AddPoints (25, pm.playerNumber, true);
			audioManager.PlayRandomAudio(this.audLose);
			behaviorStateMachine.ChangeState (new Kim_Lose (this, 25f));
			return;
		}
		int num = Mathf.Min (Singleton<CoreGameManager>.Instance.GetPoints (pm.playerNumber), 25);
		Singleton<CoreGameManager>.Instance.AddPoints (-num, pm.playerNumber, true);
		audioManager.PlayRandomAudio(this.audWin);
		behaviorStateMachine.ChangeState (new Kim_Win (this, 25f));
	}

	public void GameTie(PlayerManager pm)
	{
		UpdateSprite ("cringed");
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle (audTie);
			pm.Am.moveMods.Remove (moveMod);

		behaviorStateMachine.ChangeState (new Kim_Tie (this, 1f, pm));
	}

	MovementModifier moveMod = new MovementModifier (Vector3.zero, 0f);
	public void BeginGame(PlayerManager pm)
	{
		pm.Am.moveMods.Add (moveMod);
		instructions.SetActive (true);
		behaviorStateMachine.ChangeState (new Kim_Game (this, pm));
	}


}

public class Kim_StateBase : NpcState
{
	protected Kim kim;

	public Kim_StateBase(Kim kim) : base(kim)
	{
		this.kim = kim;
	}
}

public class Kim_Wandering : Kim_StateBase
{

	bool calledOut = false;
	private float calloutTime = 3f;
	float gameTimer = 5f;

	public Kim_Wandering(Kim kim, float timer = 5f) : base(kim)
	{
		gameTimer = timer;
	}

	public override void Enter()
	{
		base.Enter();
		npc.Navigator.SetSpeed (16f);
		npc.Navigator.maxSpeed = 16f;
		kim.UpdateSprite ("idle");
		base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 0));
	}

	public override void Update()
	{
		base.Update();
		this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.kim.CalloutChance();
			this.calloutTime = 3f;
		}
		gameTimer -= Time.deltaTime * this.npc.TimeScale;
	}

	public override void PlayerSighted(PlayerManager player)
	{
		base.PlayerSighted (player);
		if (!player.tagged && gameTimer <= 0f) {
			if (!calledOut) {
				npc.Navigator.SetSpeed (22f);
				kim.UpdateSprite ("notice");
				kim.AudioManager.PlaySingle (kim.audNotice);
				base.ChangeNavigationState (new NavigationState_TargetPlayer (this.npc, 0, player.transform.position));
				calledOut = true;
			}

		}
	}

	public override void PlayerInSight(PlayerManager player)
	{
		base.PlayerInSight (player);
		if (!player.tagged && calledOut && gameTimer <= 0f) {
			currentNavigationState.UpdatePosition (player.transform.position);
		}
	}

	public override void DestinationEmpty()
	{
		base.DestinationEmpty ();
		if (calledOut) {
			npc.Navigator.SetSpeed (16f);
			npc.Navigator.maxSpeed = 16f;
			kim.UpdateSprite ("idle");
			base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 0));
			calledOut = false;	
		}
	}

	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
		if (other.CompareTag ("Player")) {
			PlayerManager pm = other.GetComponent<PlayerManager> ();
			if (!pm.tagged && gameTimer <= 0f) {
				kim.BeginGame (pm);
			}
		}
	}

}

public class Kim_Game : Kim_StateBase
{
	protected PlayerManager pm;
	protected bool explain;
	public Kim_Game(Kim kim, PlayerManager player, bool explain = true) : base(kim)
	{
		pm = player;
		this.explain = explain;
	}

	public override void Enter()
	{
		base.Enter();
		kim.UpdateSprite ("game");
		if (explain) {
			kim.AudioManager.PlaySingle (kim.audGame);
		}
		base.ChangeNavigationState (new NavigationState_DoNothing (this.npc, 0));
	}

	public bool inputting = true;

	public override void Update()
	{
		base.Update();
		/*this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.carmella.CalloutChance();
			this.calloutTime = 3f;
		}*/

		if (kim.PlayerLeft (pm)) {
			kim.LeaveFailsave (pm);
		}

		if (inputting) {
			if (Singleton<InputManager>.Instance.GetDigitalInput ("Interact", true)) {
				inputting = false;
				kim.BeginSequence (Random.Range (0, 3), RPSButton.currentSelection, pm);
			}

			if (Input.GetKeyDown(KeyCode.RightArrow)) {
				RPSButton.currentSelection = (RPSButton.currentSelection + 1) % 3;
			}

			if (Input.GetKeyDown(KeyCode.LeftArrow)) {
				RPSButton.currentSelection = (RPSButton.currentSelection - 1);
				if (RPSButton.currentSelection < 0) {
					RPSButton.currentSelection = 2;
				}
			}
		}
	}


	public override void OnStateTriggerEnter(Collider other)
	{
		base.OnStateTriggerEnter (other);
	}
}


public class Kim_Win : Kim_StateBase
{
	protected float coolDown;
	public Kim_Win(Kim kim, float coolDown) : base(kim)
	{
		this.coolDown = coolDown;
	}

	public override void Enter()
	{
		base.Enter();
		kim.UpdateSprite ("won");
		base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 0));
		Debug.Log ("YOU LOST or tied?");
	}

	public override void Update()
	{
		base.Update();
		/*this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.carmella.CalloutChance();
			this.calloutTime = 3f;
		}*/

		if (coolDown > 0f) {
				coolDown -= 1f * Time.deltaTime * npc.TimeScale;
			return;
		}
		npc.behaviorStateMachine.ChangeState(new Kim_Wandering(kim, 10f));
	}


}
public class Kim_Lose : Kim_StateBase
{
	protected float coolDown;
	public Kim_Lose(Kim kim, float coolDown) : base(kim)
	{
		this.coolDown = coolDown;
	}

	public override void Enter()
	{
		base.Enter();
		npc.Navigator.SetSpeed (16f);
		npc.Navigator.maxSpeed = 16f;
		kim.UpdateSprite ("lost");
		base.ChangeNavigationState (new NavigationState_WanderRandom (this.npc, 0));
		Debug.Log ("YOU WON");
	}

	public override void Update()
	{
		base.Update();
		/*this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.carmella.CalloutChance();
			this.calloutTime = 3f;
		}*/
		if (coolDown > 0f) {
				coolDown -= 1f * Time.deltaTime * npc.TimeScale;
			return;
		}
		npc.behaviorStateMachine.ChangeState(new Kim_Wandering(kim, 10f));
	}


}

public class Kim_Tie : Kim_StateBase
{
	protected float coolDown;
	protected PlayerManager player;
	public Kim_Tie(Kim kim, float coolDown, PlayerManager player) : base(kim)
	{
		this.player = player;
		this.coolDown = coolDown;
	}

	public override void Enter()
	{
		base.Enter();
	}

	public override void Update()
	{
		base.Update();
		/*this.calloutTime -= Time.deltaTime * this.npc.TimeScale;
		if (this.calloutTime <= 0f)
		{
			this.carmella.CalloutChance();
			this.calloutTime = 3f;
		}*/
		if (coolDown > 0f) {
				coolDown -= 1f * Time.deltaTime * npc.TimeScale;
			return;
		}
			npc.behaviorStateMachine.ChangeState(new Kim_Lose(kim, 25f));
	}


	}
}

public class RPSButton : MonoBehaviour
{
	public Image source;
	public Sprite baseSprite;
	public Sprite selectSprite;

	public int selectionID;

	public static int currentSelection;

	public void Setup(Image source, Sprite baseSprite, Sprite selectSprite, int ID)
	{
		this.source = source;
		this.baseSprite = baseSprite;
		this.selectSprite = selectSprite;
		this.selectionID = ID;
	}

	public void Update()
	{
		if (RPSButton.currentSelection == selectionID) {
			source.sprite = selectSprite;
			return;
		}
		source.sprite = baseSprite;
	}
}