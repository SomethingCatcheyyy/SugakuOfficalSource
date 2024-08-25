using UnityEngine;
using System.Collections;

public class DoorEvent : RandomEvent
{

	public override void Initialize(EnvironmentController controller, System.Random rng)
	{
		base.Initialize(controller, rng);
		Debug.Log ("Door Lockage");
	}


	public override void Begin()
	{
		base.Begin ();
		foreach (Door door in ec.mainHall.doors) {
			door.LockTimed (EventTime);
		}
		Singleton<CoreGameManager>.Instance.GetHud (0).BaldiTv.baldi_Enable (false);
	}
	public override void End()
	{
		base.End();
		foreach (Door door in ec.mainHall.doors) {
			door.Unlock ();
		}
		Singleton<CoreGameManager>.Instance.GetHud (0).BaldiTv.baldi_Enable (true);
	}
}

