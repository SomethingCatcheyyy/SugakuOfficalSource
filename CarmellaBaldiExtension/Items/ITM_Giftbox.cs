using UnityEngine;
using System.Collections;
using CarmellaBaldiExtension;


namespace Sugaku.CustomItems
{
public class ITM_Giftbox : Item
{

	public WeightedItemObject[] itemLootTable;
	public SoundObject xyloNoise;


	public override bool Use(PlayerManager pm)
	{
		CoolReward (pm);
		UnityEngine.Object.Destroy(base.gameObject);
		return true;
	}

	void CoolReward(PlayerManager pm)
	{
		EnvironmentController ec = pm.ec;
		itemLootTable = BasePlugin.plugin.giftloottable;
		int num = UnityEngine.Random.Range (0, 2);
		switch (num) {
		case 0:
			ItemObject blab;
			blab = WeightedSelection<ItemObject>.RandomSelection (itemLootTable);
			if (pm.itm.InventoryFull ()) {
				pm.itm.SetItem (blab, pm.itm.selectedItem);
			} else {
				pm.itm.AddItem (blab);
			}
			break;
		case 1:
			Singleton<CoreGameManager>.Instance.AddPoints (UnityEngine.Random.Range (10, 100), pm.playerNumber, true);
			break;
		}
	}

}

}