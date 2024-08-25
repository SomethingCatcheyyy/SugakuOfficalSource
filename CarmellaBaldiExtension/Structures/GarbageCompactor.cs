using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GarbageCompactor : EnvironmentObject, IClickable<int>
{
	public List<ItemObject> storage = new List<ItemObject>();

	public AudioManager audMan;
	public SoundObject useAud;
	public SoundObject grabAud;

	AudioSource propagate;


	void Start()
	{
		gameObject.AddComponent<PropagatedAudioManager> ();
		audMan = GetComponent<PropagatedAudioManager> ();


		GameObject audd = new GameObject ("Propagator");
		audd.transform.parent = base.transform;
		audd.transform.position = base.transform.position;
		audd.AddComponent<AudioSource> ();
		propagate = audd.GetComponent<AudioSource> ();
		propagate.playOnAwake = false;
		propagate.loop = false;
		audMan.audioDevice = propagate;
	}
	// Use this for initialization

	public void Clicked(int playerNumber)
	{
		PlayerManager pm = Singleton<CoreGameManager>.Instance.GetPlayer (playerNumber);
		ItemObject item = pm.itm.items [pm.itm.selectedItem];
		if (item.itemType != Items.None) {
			storage.Add (item);
			pm.itm.RemoveItem (pm.itm.selectedItem);
			audMan.PlaySingle (useAud);
		} else {
			if (storage.Count > 0) {
				pm.itm.SetItem (storage [storage.Count - 1], pm.itm.selectedItem);
				storage.Remove (storage [storage.Count - 1]);
				audMan.PlaySingle (grabAud);
			}
		}
	}

	// Token: 0x0600018A RID: 394 RVA: 0x00002492 File Offset: 0x00000692
	public void ClickableSighted(int player)
	{
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

