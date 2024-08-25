using UnityEngine;
using System.Collections;


namespace Sugaku.CustomItems
{
public class ITM_Sled : Item
{
	public MovementModifier moveMod = new MovementModifier (Vector3.forward, 1f);

	public float time = 15f;

	public override bool Use(PlayerManager pm)
	{
		this.pm = pm;
		pm.Am.moveMods.Add (moveMod);
		return true;
	}

	void Update()
	{
		time -= Time.deltaTime * pm.ec.EnvironmentTimeScale;
		pm.Am.moveMods.Remove (moveMod);
		if (time >= 0f) {
			moveMod.movementAddend = pm.transform.forward * 30f;
			pm.Am.moveMods.Add (moveMod);
			return;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}

}