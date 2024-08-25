using UnityEngine;
using System.Collections;
using MTM101BaldAPI.Reflection;
using Gemu;
using System.Collections.Generic;

public class PartsRoomFunction : RoomFunction
{
	//this is kinda ripped from b. carnell, then again uhhhhh almost all generation is like that :P
	public override void Build(LevelBuilder builder, System.Random rng)
	{
		base.Build (builder, rng);

		List<Cell> cells = room.GetTilesOfShape (tileShapes, true);

		int num = 0;
		Cell cell = null;
		while (cell == null && cells.Count > 0 && num < this.maxAttempts)
		{
			int index = rng.Next(0, cells.Count);
			num++;
			if (!cells[index].HasFreeWall)
			{
				cells.Remove(cells[index]);
			}
			else
			{
				cell = cells[index];
			}
		}
		LightBasedButton functionComp = base.gameObject.AddComponent<LightBasedButton> ();
		functionComp.ec = room.ec;
		if (cell != null && GameButton.BuildInArea(room.ec, cell.position, cell.position, 1, functionComp.gameObject, this.buttonPre, rng) == null)
		{
			Debug.LogWarning("WTF FAILURE???");
		}

	}

	public override void Initialize(RoomController room)
	{
		base.Initialize (room);
		foreach (Cell vector in room.cells) {
			Color prevColor = vector.lightColor;
			Color newColor = new Color (
				prevColor.r * 0.5f,
				prevColor.g * 0.5f,
				prevColor.b * (196f / 255f)
			                 );
			vector.lightColor = newColor;
		}
	}

	public GameButton buttonPre;

	private List<TileShape> tileShapes = new List<TileShape>
	{
		TileShape.Corner,
		TileShape.Single
	};

	private int maxAttempts = 10;
}

