using UnityEngine;
using System.Collections.Generic;
using System;

public class LaserDoorBuilder : ObjectBuilder
{
	// Token: 0x06000556 RID: 1366 RVA: 0x00028F34 File Offset: 0x00027134
	public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
	{
		int num = 0;
		List<Cell> tilesOfShape = room.GetTilesOfShape(this.tileShapes, true);
		Cell cell = null;
		while (cell == null && tilesOfShape.Count > 0 && num < this.maxAttempts)
		{
			int index = cRng.Next(0, tilesOfShape.Count);
			num++;
			if (ec.TrapCheck(tilesOfShape[index]) || tilesOfShape[index].open || tilesOfShape[index].HasAnyHardCoverage)
			{
				tilesOfShape.Remove(tilesOfShape[index]);
			}
			else
			{
				cell = tilesOfShape[index];
			}
		}
		if (cell != null)
		{
			LaserDoor laserDoor = UnityEngine.Object.Instantiate<LaserDoor>(this.doorPre, room.transform);
			Direction direction = cell.AllOpenNavDirections[cRng.Next(0, cell.AllOpenNavDirections.Count)];
			cell.HardCoverEntirely();
			laserDoor.transform.position = cell.FloorWorldPosition;
			laserDoor.transform.rotation = direction.ToRotation();
			laserDoor.Setup (direction, cell, ec);
		}
		else
		{
			Debug.LogWarning("Laser door builder was unable to find a valid position for the door!");
		}
	}

	public LaserDoor doorPre;

	// Token: 0x04000529 RID: 1321
	// Token: 0x0400052A RID: 1322
	[SerializeField]
	private List<TileShape> tileShapes = new List<TileShape>
	{
		TileShape.Straight
	};

	// Token: 0x0400052B RID: 1323
	[SerializeField]
	private int maxAttempts = 10;

}
