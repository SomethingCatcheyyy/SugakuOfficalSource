using UnityEngine;
using System.Collections;
using HarmonyLib;
using TMPro;
using CarmellaBaldiExtension;
using Gemu;

[HarmonyPatch(typeof(MathMachine))]
public class MathMachineBonusPatch
{

	public static MathMachineBonusPatch selfPatch;

	[HarmonyPatch("Completed")]
	[HarmonyPrefix]
	public static void Prefix(MathMachine __instance)
	{
		//selfPatch = this;
		if (__instance.InBonusMode) {
			doJunk (__instance);
			BaseGameAddons.instance.MathMachineBonuses++;
		}
	}

	private static void doJunk(MathMachine machine)
	{
		TMP_Text val1Text = (TMP_Text)AccessTools.Field (typeof(MathMachine), "val1Text").GetValue (machine);
		TMP_Text val2Text = (TMP_Text)AccessTools.Field (typeof(MathMachine), "val2Text").GetValue (machine);
		TMP_Text signText = (TMP_Text)AccessTools.Field (typeof(MathMachine), "signText").GetValue (machine);
		TMP_Text answerText = (TMP_Text)AccessTools.Field (typeof(MathMachine), "answerText").GetValue (machine);

		MathMachine[] machines = UnityEngine.Object.FindObjectsOfType<MathMachine> ();

		val1Text.text = (BaseGameAddons.instance.MathMachineBonuses + 1).ToString();
		val2Text.text = (machines.Length - 1).ToString();
		signText.text = "/";
		answerText.text = "!";
		if (BaseGameAddons.instance.MathMachineBonuses >= machines.Length - 2)
		{
			Notebook nb = (Notebook)AccessTools.Field (typeof(MathMachine), "notebook").GetValue (machine);
			machine.room.ec.CreateItem (machine.room, WeightedSelection<ItemObject>.RandomSelection (items), nb.transform.position);
		}
	}

	static WeightedItemObject[] items = new WeightedItemObject[]
	{
		new WeightedItemObject{
			selection = ObjectFunctions.FindResourceOfName<ItemObject>("PortalPoster"),
			weight = 100
		},
		BasePlugin.plugin.assetMan.Get<WeightedItemObject>("Remote Control"),
		new WeightedItemObject{
			selection = ObjectFunctions.FindResourceOfName<ItemObject>("Teleporter"),
			weight = 80
		},
		new WeightedItemObject{
			selection = ObjectFunctions.FindResourceOfName<ItemObject>("GrapplingHook"),
			weight = 70
		},
		new WeightedItemObject{
			selection = ObjectFunctions.FindResourceOfName<ItemObject>("Apple"),
			weight = 10
		}
	};
}

