using UnityEngine;
using System.Collections;
using HarmonyLib;

public class BaseGameAddons : MonoBehaviour
{
	public int MathMachineBonuses = 0;

	public static BaseGameAddons instance;

	void Start()
	{
		instance = this;
	}
}

[HarmonyPatch(typeof(BaseGameManager))]
[HarmonyPatch("Start")]
public class BaseGamePatch
{

	public static void Prefix(BaseGameManager __instance)
	{
		__instance.gameObject.AddComponent<BaseGameAddons> ();
	}

}
