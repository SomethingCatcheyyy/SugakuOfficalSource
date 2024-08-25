using UnityEngine;
using System.Collections;
using HarmonyLib;


namespace Sugaku.NPCS
{
public class FloatingItem : NPC
{
	public ItemEvent eventt;
	public WeightedItemObject[] itemTable;

	public override void Initialize()
	{
		base.Initialize();

		eventt = FindObjectOfType<ItemEvent> ();

		System.Random rng = new System.Random ();

		createPickup (rng);

		spriteBase.SetActive (false);

		navigator.SetRoomAvoidance (true);
		this.behaviorStateMachine.ChangeState(new FloatingItem_Wander(this));
		eventt.itemz.Add (this);
	}
	Pickup pickup;

	// Use this for initialization
	void createPickup(System.Random crng)
	{
		Pickup pickupPre = (Pickup)AccessTools.Field (typeof(EnvironmentController), "pickupPre").GetValue (ec);
		pickup = UnityEngine.Object.Instantiate<Pickup>(pickupPre, ec.transform);
		ItemObject debugTest = WeightedSelection<ItemObject>.ControlledRandomSelection (itemTable, crng);
		DebugMenu.LogEvent (debugTest.name + " " + debugTest.nameKey);
		pickup.item = debugTest;//WeightedSelection<ItemObject>.ControlledRandomSelection (itemTable, crng);
	}

	protected override void VirtualUpdate()
	{
		base.VirtualUpdate();
		pickup.transform.position = base.transform.position;
	}

	public void DespawnPickup()
	{
		Destroy (pickup.gameObject);
	}
}

public class FloatingItem_StateBase : NpcState
{
	protected FloatingItem floatingItem;

	public FloatingItem_StateBase(FloatingItem floatingItem) : base(floatingItem)
	{
		this.floatingItem = floatingItem;
	}
}

public class FloatingItem_Wander : FloatingItem_StateBase
{
	public FloatingItem_Wander(FloatingItem floatingItem) : base(floatingItem)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.ChangeNavigationState(new NavigationState_WanderRandom(this.npc, 0));
	}

}
}