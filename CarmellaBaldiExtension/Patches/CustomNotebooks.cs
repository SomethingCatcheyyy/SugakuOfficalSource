using UnityEngine;
using System.Collections;
using HarmonyLib;
using CarmellaBaldiExtension;
[HarmonyPatch(typeof(Notebook))]
public class CustomNotebooks
{

	[HarmonyPatch("Start")]
	[HarmonyPostfix]
	public static void Postfix(Notebook __instance)
	{
		SpriteRenderer leSPrite = (SpriteRenderer)AccessTools.Field (typeof(Notebook), "sprite").GetValue (__instance);
		leSPrite.sprite = BasePlugin.plugin.assetMan.Get<Sprite>("BlankBook");
		leSPrite.color = Color.HSVToRGB (Random.Range (0f, 1f), Random.Range (0.9f, 1f), Random.Range (0.9f, 1f));//new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
	}
}

