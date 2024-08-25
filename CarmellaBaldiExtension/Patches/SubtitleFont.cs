using UnityEngine;
using System.Collections;
using HarmonyLib;
using CarmellaBaldiExtension;

[HarmonyPatch(typeof(SubtitleController))]
[HarmonyPatch("Initialize")]
public class SubtitleFont
{

	public static void Prefix(SubtitleController __instance)
	{
		__instance.text.font = Resources.Load<TMPro.TMP_FontAsset> ("COMIC_12_Pro");
	}
}

