using UnityEngine;
using System.Collections;
using HarmonyLib;

[HarmonyPatch(typeof(GameCamera))]
public class FOVPatch
{
	[HarmonyPatch("Awake")]
	[HarmonyPrefix]
	public static void Prefix(GameCamera __instance)
	{
		__instance.gameObject.AddComponent<FOVManager> ();
	}

	[HarmonyPatch("LateUpdate")]
	[HarmonyPostfix]
	public static void Postfix(GameCamera __instance)
	{
		if (__instance.gameObject.GetComponent<FOVManager> () != null) {
			__instance.gameObject.GetComponent<FOVManager> ().UpdateCams (new Camera[]{ 
				__instance.camCom,
				__instance.billboardCam
			});
		}
	}
}

