using UnityEngine;
using System.Collections;
using HarmonyLib;
using MTM101BaldAPI;

[HarmonyPatch(typeof(DrReflex_Chasing))]

public class ApplePatch
{
	[HarmonyPatch("OnStateTriggerStay")]
	[HarmonyPrefix]
	public static void Prefix(DrReflex_Chasing __instance)
	{
		//this.animator.SetTrigger ("StartChasing");
		PlayerManager pm = Singleton<CoreGameManager>.Instance.GetPlayer (0);
		DrReflex refex = (DrReflex)AccessTools.Field (typeof(DrReflex_StateBase), "drReflex").GetValue (__instance);

		if (Vector3.Distance (refex.transform.position, pm.transform.position) <= 10f) {
			if (pm.itm.Has (EnumExtensions.GetFromExtendedName<Items>("Orange"))) {
				pm.itm.Remove (EnumExtensions.GetFromExtendedName<Items>("Orange"));
				refex.behaviorStateMachine.ChangeState (new DrReflex_Flee (refex, pm));
				return;
			}
		}
	}
}

public class DrReflex_Flee : DrReflex_StateBase
{

	protected PlayerManager player;
	public DrReflex_Flee (DrReflex drReflex, PlayerManager player) : base (drReflex)
	{
		this.player = player;
	}

	//
	// Methods

	public override void Enter ()
	{
		base.Enter ();
		drReflex.Animator.SetTrigger ("StartChasing");
		drReflex.AudioManager.FlushQueue (true);
		this.drReflex.HeadToOffice ();
	}

	public override void DestinationEmpty ()
	{
		base.DestinationEmpty ();
		this.npc.behaviorStateMachine.ChangeState (new DrReflex_Wandering (this.drReflex, 0f));
	}

}
