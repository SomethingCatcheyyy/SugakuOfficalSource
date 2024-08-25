using UnityEngine;
using System.Collections;
using CarmellaBaldiExtension;
using Gemu;

namespace Sugaku.CustomItems
{

public class ITM_Butter : Item, IEntityTrigger
{
	public Entity entity;
	public EnvironmentController ec;
	bool flying = true;

	private ActivityModifier actMod;

	public AudioManager audMan;
	public SoundObject audSplat;

	public Sprite butterSprite;

	public void SetupVisuals()
	{
		SpriteRenderer sprite = ObjectFunctions.CreateSpriteRender ("SpriteBase", true, base.transform);
		sprite.sprite = butterSprite;
		sprite.gameObject.layer = 9;
		sprite.sortingLayerID = 12;
	}

	AudioSource propagate;

	// Token: 0x060000B7 RID: 183 RVA: 0x000054F7 File Offset: 0x000036F7
	public void EntityTriggerStay(Collider other)
	{
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x000054F9 File Offset: 0x000036F9
	public void EntityTriggerExit(Collider other)
	{
	}


	public override bool Use(PlayerManager pm)
	{
		this.ec = pm.ec;
		base.transform.position = pm.transform.position;
		base.transform.rotation = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).camCom.transform.rotation;
		this.entity.Initialize(this.ec, base.transform.position);
		this.entity.SetActive(true);
		this.entity.Enable (true);
		this.entity.OnEntityMoveInitialCollision += this.OnEntityMoveCollision;

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
		audMan.maintainLoop = true;
		audMan.loop = true;

		return true;
	}



	private void OnEntityMoveCollision(RaycastHit hit)
	{
		if (this.flying && hit.transform.gameObject.layer != 2)
		{
			this.flying = false;
			this.entity.SetFrozen(true);
			this.actMod = null;
			base.transform.rotation = Quaternion.LookRotation(hit.normal * -1f, Vector3.up);
			this.audMan.FlushQueue(true);
			this.audMan.PlaySingle(this.audSplat);
		}
	}

	private void Update()
	{
		if (this.flying)
		{
			if (entity != null && ec != null) {
				this.entity.UpdateInternalMovement (base.transform.forward * 25f * this.ec.EnvironmentTimeScale);
			}	
			return;
		}
		if (this.actMod != null)
		{
			this.entity.UpdateInternalMovement(Vector3.zero);
			base.transform.position = this.actMod.transform.position;
		}
	}

	private MovementModifier moveMod = new MovementModifier(Vector3.zero, 0.4f);

	public void EntityTriggerEnter(Collider other)
	{
		DebugMenu.LogEvent ("Detected");
		if (this.flying)
		{
			DebugMenu.LogEvent ("FOUND TRIG " + other.name);
			if (other.isTrigger && (other.CompareTag("NPC")))
			{
				this.actMod = other.GetComponent<ActivityModifier>();
				base.StartCoroutine(this.Timer(15f));
				this.audMan.FlushQueue(true);
				this.actMod.moveMods.Add(this.moveMod);
				this.audMan.PlaySingle(this.audSplat);
				//pm.RuleBreak ("Bullying", 1f);
				this.flying = false;
				return;
			}
		}
	}

	private IEnumerator Timer(float time)
	{
		while (time > 0f)
		{
			time -= Time.deltaTime * this.ec.EnvironmentTimeScale;
			yield return null;
		}
		this.actMod.moveMods.Remove(this.moveMod);
		Destroy (base.gameObject);
		yield break;
	}

}

}