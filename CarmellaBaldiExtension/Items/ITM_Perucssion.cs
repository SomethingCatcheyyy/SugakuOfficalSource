using UnityEngine;
using System.Collections;

namespace Sugaku.CustomItems
{
	
public class ITM_Perucssion : Item
{

	public SoundObject bang;

	// Token: 0x0600041D RID: 1053 RVA: 0x00025978 File Offset: 0x00023B78
	public override bool Use(PlayerManager pm)
	{
			Debug.Log ("AAAA");
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(this.bang);
			Collider[] array = new Collider[16];
			int num = Physics.OverlapSphereNonAlloc(pm.transform.position, 15f, array, 131072, QueryTriggerInteraction.Collide);
		for (int i = 0; i < num; i++) {
			if (array [i].isTrigger && array [i].CompareTag ("NPC")) {
				Entity component = array [i].GetComponent<Entity> ();
				if (component != null) {
					pm.RuleBreak ("Bullying", 1f);
					pm.ec.MakeNoise (base.transform.position, 40);
					component.Squish (30f);
					UnityEngine.Object.Destroy (base.gameObject);
					return true;
				}
			}

		}
		foreach (CoinDoor coinDoor in FindObjectsOfType<CoinDoor>()) {
			if (Vector3.Distance (coinDoor.transform.position, pm.transform.position) <= 15f) {
				coinDoor.InsertItem (pm, pm.ec);
				pm.ec.MakeNoise (base.transform.position, 40);
			}
		}
		foreach (Window WINDOW in FindObjectsOfType<Window>()) {
			if (Vector3.Distance (WINDOW.transform.position, pm.transform.position) <= 15f) {
				WINDOW.Break (true);
			}
		}
		foreach (LaserDoor laser in FindObjectsOfType<LaserDoor>()) {
			if (Vector3.Distance (laser.transform.position, pm.transform.position) <= 15f) {
				laser.SwitchState ();
				pm.ec.MakeNoise (base.transform.position, 40);
			}
		}
		foreach (SodaMachine sodaMachine in FindObjectsOfType<SodaMachine>()) {
			if (Vector3.Distance (sodaMachine.transform.position, pm.transform.position) <= 15f) {
				sodaMachine.InsertItem (pm, pm.ec);
				pm.ec.MakeNoise (base.transform.position, 40);
			}
		}
		foreach (StandardDoor door in FindObjectsOfType<StandardDoor>()) {
			if (Vector3.Distance (door.transform.position, pm.transform.position) <= 15f) {
				door.Unlock ();
				door.OpenTimed (10f, true);
				pm.ec.MakeNoise (base.transform.position, 40);
			}
		}
		foreach (MathMachine machine in FindObjectsOfType<MathMachine>()) {
			if (Vector3.Distance (machine.transform.position, pm.transform.position) <= 15f) {
				if (machine.Corrupted) {
					machine.Corrupt (false);
				}
				pm.ec.MakeNoise (base.transform.position, 40);
			}
		}

		UnityEngine.Object.Destroy(base.gameObject);
		return false;
	}

}

}