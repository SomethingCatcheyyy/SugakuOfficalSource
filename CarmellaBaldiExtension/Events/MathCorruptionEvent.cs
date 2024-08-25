using UnityEngine;
using System.Collections;

public class MathCorruptionEvent : RandomEvent
{

	public override void Initialize(EnvironmentController controller, System.Random rng)
	{
		base.Initialize(controller, rng);
		Debug.Log ("Math will break");
	}


	public override void Begin()
	{
		base.Begin ();
		foreach (MathMachine mathMachine in FindObjectsOfType<MathMachine>()) {
			if (!mathMachine.IsCompleted) {
				mathMachine.Corrupt (true);
			}
		}
		Singleton<CoreGameManager>.Instance.GetHud (0).BaldiTv.baldi_Enable (false);
	}
	public override void End()
	{
		base.End();
		foreach (MathMachine mathMachine in FindObjectsOfType<MathMachine>()) {
			if (!mathMachine.IsCompleted) {
				mathMachine.Corrupt (false);
			}
		}
		Singleton<CoreGameManager>.Instance.GetHud (0).BaldiTv.baldi_Enable (true);
	}
}

