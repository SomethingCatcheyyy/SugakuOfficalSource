using UnityEngine;
using System.Collections;

namespace Sugaku.CustomItems
{
public class ITM_WhistleImproved : Item
{

	public SoundObject audWhistle;

	// Token: 0x0600041D RID: 1053 RVA: 0x00025978 File Offset: 0x00023B78
	public override bool Use(PlayerManager pm)
	{
		foreach (NPC npc in pm.ec.Npcs)
		{
			if (npc.Character == Character.Principal)
			{
				npc.GetComponent<Principal>().WhistleReact(pm.transform.position);
			}
		}
		pm.ec.MakeNoise (base.transform.position, 74);
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle(this.audWhistle);
		UnityEngine.Object.Destroy(base.gameObject);
		return true;
	}
}

}