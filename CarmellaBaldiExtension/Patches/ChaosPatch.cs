using UnityEngine;
using System.Collections;
using HarmonyLib;


[HarmonyPatch(typeof(BaseGameManager))]
[HarmonyPatch("CollectNotebook")]
public class ChaosPatch : MonoBehaviour
{
	/*
			this.ec.RandomizeEvents(this.ec.EventsCount, 30f, 30f, 180f, new System.Random());
			this.ec.StartEventTimers();*/
	// Token: 0x060000AC RID: 172 RVA: 0x00006821 File Offset: 0x00004A21

	public static SoundObject xylo;
	public static void Prefix(BaseGameManager __instance)
		{
		if (Singleton<CoreGameManager>.Instance.sceneObject.levelTitle == "D4")
			{
			__instance.Ec.SpawnNPCs ();
			__instance.Ec.RandomizeEvents(__instance.Ec.EventsCount, 30f, 30f, 180f, new System.Random());
			__instance.Ec.StartEventTimers ();
			}
		}

}

