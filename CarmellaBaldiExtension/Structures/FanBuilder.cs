using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanBuilder : ObjectBuilder
{
	// Token: 0x06000572 RID: 1394 RVA: 0x00029B20 File Offset: 0x00027D20
	public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
	{
		List<Cell> list = new List<Cell>();
		List<TileShape> list2 = new List<TileShape>();
		list2.Add(TileShape.Open);
		list = room.GetTilesOfShape(list2, false);
		for (int i = 0; i < list.Count; i++)
		{
			if (ec.TrapCheck(list[i]))
			{
				list.RemoveAt(i);
				i--;
			}
		}
		if (list.Count == 0)
		{
			list2.Clear();
			list2.Add(TileShape.Single);
			list = room.GetTilesOfShape(list2, false);
		}
		Cell cell = null;
		while (list.Count > 0 && cell == null)
		{
			int index = cRng.Next(0, list.Count);
			if (list[index].HasAnyHardCoverage || ec.TrapCheck(list[index]))
			{
				list.RemoveAt(index);
			}
			else
			{
				cell = list[index];
			}
		}
		if (cell != null)
		{
			this.fan = UnityEngine.Object.Instantiate<Fan>(this.fanPre, cell.ObjectBase);
			this.fan.Ec = ec;
			bool spinClockwise = true;
			if (cRng.Next(0, 2) == 1)
			{
				spinClockwise = false;
			}
			this.fan.Setup((Direction)cRng.Next(0, 4), cell, spinClockwise);
			cell.HardCoverEntirely();
			if (GameButton.BuildInArea(ec, cell.position, cell.position, this.buttonRange, this.fan.gameObject, this.buttonPre, cRng) == null)
			{
				Debug.LogWarning("No suitable location for a FAN button was found. Destroying the roto hall");
				UnityEngine.Object.Destroy(this.fan);
			}
		}
		Debug.Log ("Its builtd?");
	}

	// Token: 0x0400054A RID: 1354
	[SerializeField]
	public Fan fanPre;

	// Token: 0x0400054B RID: 1355
	private Fan fan;

	// Token: 0x0400054E RID: 1358
	[SerializeField]
	public GameButton buttonPre;

	// Token: 0x0400054F RID: 1359
	[SerializeField]
	private int buttonRange = 3;
}

