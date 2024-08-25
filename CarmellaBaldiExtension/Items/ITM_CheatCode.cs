using UnityEngine;
using System.Collections;
using HarmonyLib;

namespace Sugaku.CustomItems
{
public class ITM_CheatCode : Item
{
	// Token: 0x0600041D RID: 1053 RVA: 0x00025978 File Offset: 0x00023B78
	public override bool Use(PlayerManager pm)
	{
		Debug.Log ("AAAA");
		foreach(MathMachine component in FindObjectsOfType<MathMachine>())
		{
			if (Vector3.Distance(pm.transform.position, component.transform.position) <= 15f && !component.IsCompleted)
			{
				Debug.Log ("AAAA2");
				OnUse (component, pm);
				UnityEngine.Object.Destroy(base.gameObject);
				return true;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
		return false;
	}

	void OnUse(MathMachine machine, PlayerManager dumb)
	{
		int pointadd = (int)AccessTools.Field (typeof(MathMachine), "normalPoints").GetValue (machine);
		float dime = (float)AccessTools.Field (typeof(MathMachine), "baldiPause").GetValue (machine);
		MeshRenderer mrrr = (MeshRenderer)AccessTools.Field (typeof(MathMachine), "meshRenderer").GetValue (machine);
		Material cv = (Material)AccessTools.Field (typeof(MathMachine), "correctMat").GetValue (machine);

		Singleton<CoreGameManager>.Instance.AddPoints(pointadd, 0, true);
		machine.Completed(0, true, machine);

		if (mrrr != null)
		{
			mrrr.sharedMaterial = cv;
		}
	}

}

}