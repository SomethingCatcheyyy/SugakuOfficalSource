using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gemu;
public class CloneEvent : RandomEvent
{
	
	public override void Initialize(EnvironmentController controller, System.Random rng)
	{
		base.Initialize(controller, rng);
		Debug.Log ("Clonez");
	}

	NPC npcClone;

	string namer(Character charr)
	{
		if (CarmellaBaldiExtension.BasePlugin.plugin.assetMan.ContainsKey (string.Concat ("Clone_", charr))) {
			return charr.ToString ();
		}
		return "Pencir";
	}

	public override void Begin()
	{
		base.Begin ();
		Singleton<CoreGameManager>.Instance.GetHud (0).BaldiTv.baldi_Enable (false);
		NPC npcToCloen = ec.npcsToSpawn [Random.Range (0, ec.npcsToSpawn.Count)];
		npcClone = npcToCloen; //its random everytime!
		ec.SpawnNPC(npcClone, IntVector2.GetGridPosition(npcToCloen.transform.position));
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle (CarmellaBaldiExtension.BasePlugin.plugin.assetMan.Get<SoundObject>("Clone_" + namer(npcClone.Character)));
		DebugMenu.LogEvent(string.Concat("Cloned ", npcToCloen.name, " try finding them"));
	}
	public override void End()
	{
		base.End();
		DebugMenu.LogEvent ("bNOPE! they're here to stay!");
		//npcClone.Despawn ();
		Singleton<CoreGameManager>.Instance.GetHud (0).BaldiTv.baldi_Enable (true);
	}
}

