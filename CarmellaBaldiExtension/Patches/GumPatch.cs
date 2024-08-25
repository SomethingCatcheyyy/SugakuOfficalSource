using UnityEngine;
using System.Collections;
using HarmonyLib;
[HarmonyPatch(typeof(Beans))]
public class GumPatch
{

	[HarmonyPatch("GumHit")]
	[HarmonyPrefix]
	public static void GuiltyAf(Beans __instance)
	{
		AccessTools.Method (typeof(NPC), "SetGuilt", null, null).Invoke (__instance, new object[]
			{
				2f,
				"Bullying"
			});
	}
}

