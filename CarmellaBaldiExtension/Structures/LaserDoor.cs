using UnityEngine;
using System.Collections;
using CarmellaBaldiExtension;
using Gemu;

public class LaserDoor : EnvironmentObject
{
	float timeTillSwitch = 30f;

	private Cell tile;


	public SpriteRenderer rend;

	BoxCollider border;

	public Sprite closedSprite;
	public Sprite openSprite;

	bool beamOn = false;

	Direction dir;


	public bool throwawayInstance = true;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!throwawayInstance) {
			if (timeTillSwitch > 0f) {
				timeTillSwitch -= 1f * Time.deltaTime * ec.EnvironmentTimeScale;
				return;
			}
			SwitchState ();
		}
	}

	public void Setup(Direction dir, Cell newTile, EnvironmentController ec)
	{
		
		GameObject srobject = new GameObject ("SpriteBase");
		srobject.transform.parent = base.transform;
		srobject.transform.position = base.transform.position + (Vector3.up * 5f);
		srobject.layer = 9;
		srobject.transform.Rotate (dir.ToRotation().eulerAngles);
		srobject.AddComponent<SpriteRenderer> ();
		rend = srobject.GetComponent<SpriteRenderer> ();
		this.tile = newTile;

		rend.sprite = openSprite;

		rend.material = ObjectFunctions.FindResourceOfName<Material> ("SpriteWithFog_Forward_NoBillboard");


		gameObject.AddComponent<BoxCollider> ();
		border = gameObject.GetComponent<BoxCollider> ();
		border.size = new Vector3 (10f, 10f, 1f);
		border.center = new Vector3 (0f, 5f, 0f);

		this.dir = dir;
		this.ec = ec;

		throwawayInstance = false;

		SwitchState ();
	}

	public void SwitchState()
	{
		beamOn = !beamOn;
		border.enabled = beamOn;
		tile.Block (dir, beamOn);
		tile.Block (dir.GetOpposite(), beamOn);
		if (beamOn) {
			rend.sprite = closedSprite;
		}
		else {
			rend.sprite = openSprite;
		}
		timeTillSwitch = 30f;
	}
}

