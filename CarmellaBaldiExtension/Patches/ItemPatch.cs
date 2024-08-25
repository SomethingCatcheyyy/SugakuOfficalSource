using UnityEngine;
using HarmonyLib;
using System.Collections;
[HarmonyPatch(typeof(ItemManager))]
public class ItemPatch
{
	public static SoundObject audSwitch;
	[HarmonyPatch("UpdateSelect")]
	[HarmonyPrefix]
	public static void Prefix()
	{
		if (Singleton<CoreGameManager>.Instance.sceneObject.levelTitle != "C3") {
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle (audSwitch);
		}
	}
}

