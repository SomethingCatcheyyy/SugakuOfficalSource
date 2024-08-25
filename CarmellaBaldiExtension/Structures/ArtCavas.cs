using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sugaku.NPCS;

public class ArtCavas : EnvironmentObject, IClickable<int>
{
	public List<Coloury> colourys = new List<Coloury>();

	float step = 0.0f;

	public SpriteRenderer renderer;
	public Sprite[] allSprites;

	void Start()
	{
		renderer.sprite = allSprites [0];
		//renderer.gameObject.layer = 9;
	}

	public void Reset()
	{
		step = 0f;
		renderer.sprite = allSprites [0];
	}

	public void Clicked(int playerNumber)
	{
		if (colourys.Count > 0) {
			foreach (Coloury coloury in colourys) {
				if (coloury.ArtGame) {
					PlayerManager pm = Singleton<CoreGameManager>.Instance.GetPlayer (playerNumber);
					step += 0.5f;
					renderer.sprite = allSprites [Mathf.FloorToInt (step)];
					if (step >= 5f) {
						coloury.CompleteGame ();
					}
				}
			}
		}
	}

	// Token: 0x0600018A RID: 394 RVA: 0x00002492 File Offset: 0x00000692
	public void ClickableSighted(int player)
	{
		DebugMenu.LogEvent ("It highigt");
	}

	// Token: 0x0600018B RID: 395 RVA: 0x00002492 File Offset: 0x00000692
	public void ClickableUnsighted(int player)
	{
	}

	// Token: 0x0600018C RID: 396 RVA: 0x000037FE File Offset: 0x000019FE
	public bool ClickableHidden()
	{
		return false;
	}

	// Token: 0x0600018D RID: 397 RVA: 0x00003801 File Offset: 0x00001A01
	public bool ClickableRequiresNormalHeight()
	{
		return true;
	}
}

