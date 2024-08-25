using UnityEngine;
using System.Collections;


namespace Sugaku.CustomItems
{
public class ITM_Remote : Item
{

	public SoundObject audUse;
	public SoundObject audClick;

	// Token: 0x0600041D RID: 1053 RVA: 0x00025978 File Offset: 0x00023B78
	public override bool Use(PlayerManager pm)
	{
		RaycastHit hit;
		if (Physics.Raycast (pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera (pm.playerNumber).transform.forward, out hit, 100f)) {
			foreach (IButtonReceiver buttonReceiver in hit.transform.GetComponents<IButtonReceiver>()) {
				if (buttonReceiver != null) {
					buttonReceiver.ButtonPressed (true);
					Object.Destroy (base.gameObject);
					if (this.audUse != null) {
						DebugMenu.LogEvent (" here");
						Singleton<CoreGameManager>.Instance.audMan.PlaySingle (this.audUse);
					}
					return true;
				}
			}
		}
		foreach (LockdownDoor lockdownDoor in FindObjectsOfType<LockdownDoor>()) {
			if (lockdownDoor.IsOpen && Vector3.Distance(pm.transform.position, lockdownDoor.transform.position) <= 25f) {
				lockdownDoor.Shut ();
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle (this.audUse);
				Object.Destroy (base.gameObject);
				return true;
			}
		}
		foreach (LaserDoor laserDoor in FindObjectsOfType<LaserDoor>()) {
			if (Vector3.Distance(pm.transform.position, laserDoor.transform.position) <= 15f) {
				laserDoor.SwitchState ();
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle (this.audUse);
				Object.Destroy (base.gameObject);
				return true;
			}
		}
		DebugMenu.LogEvent ("Not here");
		Object.Destroy (base.gameObject);
		Singleton<CoreGameManager>.Instance.audMan.PlaySingle (this.audClick);
		return false;
	}
}

}