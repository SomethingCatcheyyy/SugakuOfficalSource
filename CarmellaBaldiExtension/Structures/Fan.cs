using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CarmellaBaldiExtension;
using HarmonyLib;
using Gemu;

public class Fan : EnvironmentObject, IButtonReceiver
{
	public float turnCooldown = 30f;
	public bool turnedOn = true;

	// Token: 0x0400053A RID: 1338
	private Cell tile;

	// Token: 0x0400053B RID: 1339
	private Vector3 _rotation;

	// Token: 0x0400053D RID: 1341
	[SerializeField]
	private AudioManager audMan;

	public SoundObject fanAudi;

	List<Collider> movesToRemove = new List<Collider> ();

	// Token: 0x0400053F RID: 1343
	[SerializeField]
	private Direction currentDir;

	// Token: 0x04000541 RID: 1345
	[SerializeField]
	private float speed = 18f;

	// Token: 0x04000542 RID: 1346
	private bool moving;

	public SpriteRenderer rend;
	public Sprite[] spritemap;

	public bool throwawayInstance = true;


	// Token: 0x040000F8 RID: 248
	private List<List<Cell>> halls = new List<List<Cell>>();

	// Token: 0x040000F9 RID: 249
	private List<Cell> currentHall = new List<Cell>();


	// Token: 0x040000FB RID: 2
	public Transform windGraphicsParent;

	public MeshRenderer[] windGraphics = new MeshRenderer[0];

	BoxCollider wind;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!throwawayInstance) {
			if (turnedOn) {
				TurningCooldown ();
			}
			if (moving) {
				moveMOd.movementAddend = transform.forward * 5f;
			}
		}
	}

	AudioSource propagate;

	public void Setup(Direction dir, Cell newTile, bool spinClockwise)
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

		StartCoroutine (audioDelay ());

		GameObject srobject = new GameObject ("SpriteBase");
		srobject.transform.parent = base.transform;
		srobject.transform.position = base.transform.position;
		srobject.layer = 9;
		srobject.AddComponent<SpriteRenderer> ();
		rend = srobject.GetComponent<SpriteRenderer> ();
		rend.material = ObjectFunctions.FindResourceOfName<Material> ("SpriteStandard_Billboard");
		rend.gameObject.AddComponent<SpriteRotatorCustom> ();
		SpriteRotatorCustom sr = rend.gameObject.GetComponent<SpriteRotatorCustom> ();
		sr.sprites = spritemap;
		sr.spriteRenderer = rend;
		sr.offset = 2;
		throwawayInstance = false;
		this.currentDir = dir;
		this.tile = newTile;
		directions = tile.AllOpenNavDirections;
		base.transform.rotation = this.currentDir.ToRotation();
		gameObject.AddComponent<BoxCollider> ();
		wind = gameObject.GetComponent<BoxCollider> ();
		wind.isTrigger = true;
		wind.size = new Vector3 (5f, 4f, 50f);
		wind.center = new Vector3 (0f, 4f, 25f);

		GameObject visualTemp = GameObject.CreatePrimitive (PrimitiveType.Cube);
		visualTemp.AddComponent<MeshRenderer> ();
		visualTemp.transform.position = base.transform.position;
		visualTemp.transform.localScale = new Vector3 (50f, 4f, 50f);
		visualTemp.transform.position += new Vector3 (0f, 4f, 40f);
		visualTemp.SetActive (false);

		gameObject.AddComponent<Rigidbody> ();
		Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
		rb.constraints = RigidbodyConstraints.FreezeAll;
		rb.useGravity = false;

		moveMOd = new MovementModifier(transform.forward * 5f, 1f);
	}


	IEnumerator audioDelay()
	{
		audMan.audioDevice = propagate;
		audMan.maintainLoop = true;
		audMan.loop = true;
		yield return null;
		audMan.QueueAudio (fanAudi);
		yield return null;
		audMan.PlayQueue ();
		yield break;
	}

	public MovementModifier moveMOd;

	void OnTriggerEnter(Collider other)
	{
		if (wind.enabled) {
			if (other.CompareTag ("NPC") || other.CompareTag ("Player")) {
				if(!other.GetComponent<ActivityModifier> ().moveMods.Contains(moveMOd))
				{
					other.GetComponent<ActivityModifier> ().moveMods.Add (moveMOd);
					movesToRemove.Add (other);
				}
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag ("NPC") || other.CompareTag("Player")) {
			other.GetComponent<ActivityModifier> ().moveMods.Remove (moveMOd);
			movesToRemove.Remove (other);
		}
	}
		

	void TurningCooldown()
	{
		if (turnCooldown > 0f) {
			turnCooldown -= 1f * Time.deltaTime * ec.EnvironmentTimeScale;
			return;
		}
		Rotate ();
		turnCooldown = 30f;
	}

	int currentDirection = 0;
	List<Direction> directions;

	private Direction GetNextDirection(int val)
	{
		int num = currentDirection + val;
		currentDirection = num;
		currentDirection %= directions.Count;
		return directions [currentDirection];
	}

	private void Rotate()
	{
		if (!this.moving)
		{
			Direction nextDirection = this.GetNextDirection(1);
			base.StartCoroutine(this.Rotator(1, nextDirection));
		}
	}



	private IEnumerator Rotator(int spinVal, Direction targetDir)
	{
		//StopBlowing ();
		wind.enabled = false;
		foreach (Collider removeMod in movesToRemove) {
			removeMod.GetComponent<ActivityModifier> ().moveMods.Remove (moveMOd);
		}
		movesToRemove.Clear ();
		audMan.volumeModifier = 0f;
		this.moving = true;
		bool keepMoving = true;
		float previousAngle = base.transform.eulerAngles.y;
		while (keepMoving)
		{
			this._rotation = base.transform.eulerAngles;
			this._rotation.y = this._rotation.y + this.speed * (float)spinVal * this.ec.EnvironmentTimeScale * Time.deltaTime;
			switch (targetDir)
			{
			case Direction.North:
				if ((spinVal == 1 && this._rotation.y >= 360f) || (spinVal == -1 && this._rotation.y <= 0f))
				{
					this._rotation.y = 0f;
					keepMoving = false;
				}
				break;
			case Direction.East:
				if ((spinVal == 1 && this._rotation.y >= 90f && (this._rotation.y <= previousAngle || previousAngle < 90f)) || (spinVal == -1 && this._rotation.y <= 90f && (this._rotation.y >= previousAngle || previousAngle > 90f)))
				{
					this._rotation.y = 90f;
					keepMoving = false;
				}
				break;
			case Direction.South:
				if ((spinVal == 1 && this._rotation.y >= 180f && (this._rotation.y <= previousAngle || previousAngle < 180f)) || (spinVal == -1 && this._rotation.y <= 180f && (this._rotation.y >= previousAngle || previousAngle > 180f)))
				{
					this._rotation.y = 180f;
					keepMoving = false;
				}
				break;
			case Direction.West:
				if ((spinVal == 1 && this._rotation.y >= 270f && (this._rotation.y <= previousAngle || previousAngle < 270f)) || (spinVal == -1 && this._rotation.y <= 270f && (this._rotation.y >= previousAngle || previousAngle > 270f)))
				{
					this._rotation.y = 270f;
					keepMoving = false;
				}
				break;
			}
			if (this._rotation.y >= 360f)
			{
				this._rotation.y = this._rotation.y - 360f;
			}
			else if (this._rotation.y < 0f)
			{
				this._rotation.y = this._rotation.y + 360f;
			}
			base.transform.eulerAngles = this._rotation;
			yield return null;
		}
		this.currentDir = targetDir;
		this.moving = false;
		wind.enabled = turnedOn;
		//Blow ();
		audMan.volumeModifier = 1f;
		yield break;
	}

	public void ButtonPressed(bool val)
	{
		turnedOn = !turnedOn;
		OnButtonPress (turnedOn);
	}

	void OnButtonPress(bool val)
	{
		switch (val) {
		case false:
			foreach (Collider removeMod in movesToRemove) {
				removeMod.GetComponent<ActivityModifier> ().moveMods.Remove (moveMOd);
			}
			movesToRemove.Clear ();
			wind.center += Vector3.up * 100f;
			break;
		case true:
			wind.center -= Vector3.up * 100f;
			break;
		}
	}
}

