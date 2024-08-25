using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Sugaku.NPCS;

public class ItemEvent : RandomEvent
{
	public FloatingItem itemPre;

	public List<FloatingItem> itemz = new List<FloatingItem>();

	List<TileShape> possibleShapes = new List<TileShape>
	{
		TileShape.Corner,
		TileShape.End,
		TileShape.Single
	};


	List<Cell> points = new List<Cell>();

	public override void Initialize(EnvironmentController controller, System.Random rng)
	{
		base.Initialize(controller, rng);
		Debug.Log ("YES YES THIS ONE");
	}

	// Use this for initialization
	void SpawnItem()
	{
		ec.SpawnNPC (itemPre, points [this.crng.Next (points.Count)].position);
	}

	public override void Begin()
	{
		base.Begin ();

		foreach (Cell cell in ec.cells) {
			if (possibleShapes.Contains (cell.shape) && cell.room.category == RoomCategory.Hall) {
				points.Add (cell);
			}
		}

		for (int i = 0; i < 50; i++) {
			SpawnItem ();
		}
		Singleton<CoreGameManager>.Instance.GetHud (0).BaldiTv.baldi_Enable (false);

	}
	public override void End()
	{
		base.End();
		foreach (FloatingItem item in itemz)
		{
			item.DespawnPickup ();
			item.Despawn ();
		}
		Singleton<CoreGameManager>.Instance.GetHud (0).BaldiTv.baldi_Enable (true);
	}
}


