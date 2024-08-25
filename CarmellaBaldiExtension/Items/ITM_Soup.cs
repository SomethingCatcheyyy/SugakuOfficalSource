using System.Collections;
using UnityEngine;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;

using Gemu;

namespace CarmellaBaldiExtension
{
	public class ITM_Soup : Item
	{

		/*float stamDropMod = 0f;
		float stamMod = 0f;
		float stamRiseMod = 0f;
		float speedMod = 0f; scrapped*/

		int[] statusSlots = new int[]{ -1, -1, -1 };

		MovementModifier moveMod = new MovementModifier(Vector3.zero, 1f);
		float baseHeight;
		Fog fog;

		float baseFOV = 60f;

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;

			moveMod.movementAddend = new Vector3 (Random.Range (-4f, 4f), 0f, Random.Range (-4f, 4f));
			fog = ObjectFunctions.createFog (new Color (Random.Range (0f, 2f), Random.Range (0f, 2f), Random.Range (0f, 2f)), Random.Range (10f, 1000f), Random.Range (100f, 300f), 99, Random.Range (10f, 50f));

			baseHeight = pm.GetComponent<Entity> ().BaseHeight;

			for (int i = 0; i < 3; i++) {
				AddEffect (i);
			}

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle (ObjectFunctions.FindResourceOfName<SoundObject> ("WaterSlurp"));

			return true;
		}

		public float time = 15f;

		bool hasEffect(int value)
		{
			for (int i = 0; i < 3; i++) {
				if (statusSlots [i] == value) {
					return true;
				}
			}
			return false;
		}

		void AddEffect(int slot)
		{
			int value = Random.Range(0, 5);
			while (hasEffect (value)) {
				value = Random.Range(0, 5);
			}
			switch (value) {
			case 0:
				pm.ec.AddFog (fog);
				break;
			case 1:
				pm.GetComponent<Entity> ().AddForce (new Force (new Vector3 (Random.Range (-10f, 10f), 0f, Random.Range (-10f, 10f)), Random.Range (10f, 100f), Random.Range (-100f, -10f)));
				break;
			case 2:
				pm.Am.moveMods.Add (moveMod);
				break;
			case 3:
				pm.GetComponent<Entity> ().SetHeight (Random.Range (baseHeight / 2f, baseHeight * 2f));
				break;
			case 4:
				FOVManager.instance.SetFov (Random.Range (50f, 90f));
				break;
			}
			statusSlots [slot] = value;
		}

		void RemoveEffect(int slot)
		{
			switch (statusSlots[slot]) {
			case 0:
				pm.ec.RemoveFog (fog);
				break;
			case 2:
				pm.Am.moveMods.Remove (moveMod);
				break;
			case 3:
				pm.GetComponent<Entity> ().SetHeight (baseHeight);
				break;
			case 4:
				FOVManager.instance.SetFov (60f);
				break;
			}
			statusSlots [slot] = -1;
		}

		void Update()
		{
			time -= Time.deltaTime * pm.ec.EnvironmentTimeScale;
			if (time <= 0f) {
				
				time = 99f;
				for (int i = 0; i < 3; i++) {
					RemoveEffect (i);
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

}

