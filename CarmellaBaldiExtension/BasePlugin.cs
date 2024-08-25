using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

using Sugaku.NPCS;
using Sugaku.CustomItems;

using Gemu;
using Gemu.RoomTools;

namespace CarmellaBaldiExtension
{
	[BepInPlugin("dee4.games.baldiplus.carmellamod", "Sugaku Mod Pack", "1.2.0")]
	[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
	public class BasePlugin : BaseUnityPlugin
	{
		public AssetManager assetMan = new AssetManager();

		public static BasePlugin plugin;

		public ModContents modContents;

		public Dictionary<string, WeightedNPC> newNPCS = new Dictionary<string, WeightedNPC>();
		public Dictionary<string, WeightedItemObject> newItems = new Dictionary<string, WeightedItemObject>();
		public Dictionary<string, WeightedRoomAsset> newRooms = new Dictionary<string, WeightedRoomAsset>();
		public Dictionary<string, WeightedObjectBuilder> newStructures = new Dictionary<string, WeightedObjectBuilder>();


		public Dictionary<string, WeightedRandomEvent> newEvents = new Dictionary<string, WeightedRandomEvent>();
		List<WeightedRoomAsset> hal = new List<WeightedRoomAsset> ();

		public List<WeightedPosterObject> newPosters = new List<WeightedPosterObject>();


		string path = "";
		string path2 = "lol";
		string path3 = "";

		SoundMetadata skeletonMeta = new SoundMetadata (SoundType.Voice, new Color (0.25f, 0.5f, 0.6f));

		//internal static ConfigEntry<bool> config_enableChars;

		void Awake()
		{
			Harmony harmony = new Harmony ("dee4.games.baldiplus.carmellamod");
			harmony.PatchAllConditionals ();
			//AssetLoader. //AssetLoader.LoadLanguageFolder(Path.Combine(AssetLoader.GetModPath(this), "Language/English/"));
			modContents = new ModContents();

			path = Path.Combine(AssetLoader.GetModPath(this), "Textures");
			path2 = Path.Combine(AssetLoader.GetModPath(this), "Audio");
			path3 = Path.Combine(AssetLoader.GetModPath(this), "RoomData");

			modContents.imagePath = path;
			modContents.audioPath = path2;

			//this.InitConfigValues();
			WillyWhistleModeSpecial();
			CloneEventAudioSpecial ();

			LoadingEvents.RegisterOnLoadingScreenStart (base.Info, OnLoad());

			GeneratorManagement.Register (this, GenerationModType.Addend, new Action<string, int, CustomLevelObject> (BenerationFuck));

			plugin = this;

			Debug.Log ("You're now playing the Sugaku Mod Pack v1.2, by dee4! Enjoy!");

			ModdedSaveGame.AddSaveHandler(base.Info);
		}

		void WillyWhistleModeSpecial()
		{
			assetMan.Add<SoundObject> ("Willy Warning 1", ObjectFunctions.CreateSound ("NPCs/wc_warning1", modContents.audioPath, genericMeta, "WIL_AlertFirstWarn", "ogg"));
			assetMan.Add<SoundObject> ("Willy Warning 2", ObjectFunctions.CreateSound ("NPCs/wc_warning2", modContents.audioPath, genericMeta, "WIL_AlertSecondWarn", "ogg"));

			assetMan.Add<SoundObject> ("Willy Yell Running", ObjectFunctions.CreateSound ("NPCs/wc_screamRunning", modContents.audioPath, genericMeta, "WIL_AlertRun", "ogg"));
			assetMan.Add<SoundObject> ("Willy Yell Faculty", ObjectFunctions.CreateSound ("NPCs/wc_screamFaculty", modContents.audioPath, genericMeta, "WIL_AlertFaculty", "ogg"));
			assetMan.Add<SoundObject> ("Willy Yell Escaping", ObjectFunctions.CreateSound ("NPCs/wc_screamEscape", modContents.audioPath, genericMeta, "WIL_AlertEscape", "ogg"));
			assetMan.Add<SoundObject> ("Willy Yell Drinking", ObjectFunctions.CreateSound ("NPCs/wc_screamDrinking", modContents.audioPath, genericMeta, "WIL_AlertDrink", "ogg"));
			assetMan.Add<SoundObject> ("Willy Yell Bullying", ObjectFunctions.CreateSound ("NPCs/wc_screamBullying", modContents.audioPath, genericMeta, "WIL_AlertBully", "ogg"));
			assetMan.Add<SoundObject> ("Willy Yell Lockers", ObjectFunctions.CreateSound ("NPCs/wc_screamBullying", modContents.audioPath, genericMeta, "WIL_AlertBully", "ogg"));

		}

		void CloneEventAudioSpecial()
		{
			
			assetMan.Add<SoundObject> ("Clone_Baldi", ObjectFunctions.CreateSound ("Events/cln_baldi", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Principal", ObjectFunctions.CreateSound ("Events/cln_principal", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Bully", ObjectFunctions.CreateSound ("Events/cln_bully", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Beans", ObjectFunctions.CreateSound ("Events/cln_beans", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Chalkles", ObjectFunctions.CreateSound ("Events/cln_chalk", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Cumulo", ObjectFunctions.CreateSound ("Events/cln_copter", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Crafters", ObjectFunctions.CreateSound ("Events/cln_crafters", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Sweep", ObjectFunctions.CreateSound ("Events/cln_sweep", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Prize", ObjectFunctions.CreateSound ("Events/cln_prize", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Carmella", ObjectFunctions.CreateSound ("Events/cln_carmella", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_IceSled", ObjectFunctions.CreateSound ("Events/cln_ice", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Billbert", ObjectFunctions.CreateSound ("Events/cln_billbert", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Kim", ObjectFunctions.CreateSound ("Events/cln_kim", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Fritz", ObjectFunctions.CreateSound ("Events/cln_fritz", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Zarkstur", ObjectFunctions.CreateSound ("Events/cln_zark", modContents.audioPath, genericMeta, "no", "ogg", false));
			assetMan.Add<SoundObject> ("Clone_Pencir", ObjectFunctions.CreateSound ("Events/cln_pencir", modContents.audioPath, genericMeta, "no", "ogg", false));
		}

		public static Transform getCustomDec(string name)
		{
			return plugin.assetMan.Get<Transform> (name);
		}

		string[] pstnames = new string[]
		{
			"yeah",
			"dark",
			"fun",
			"hydrated",
			"ruler",
			"blueprints",
			"jari",
			"no",
			"help",
			"cute",
			"julie",
			"pipe",
			"kitodin",
			"uncanny",
			"bdon-sm146_compressed",
			"jade",
			"heyyou",
			"cube4",
			"angryfaic",
			"stanlin",
			"voices",
			"rps",
			"trick",
			"superb",
			"photo",
			"B",
			"coolDude",
			"joe",
			"putz",
			"simpson",
			"imposter",
			"fiddles",
			"president",
			"demsweets",
			"spooky",
			"life",
			"teeth",
			"irenFanart",
			"cheatingdevice"
		};

		int[] pstweights = new int[]
		{
			100,
			100,
			90,
			110,
			90,
			95,
			70,
			90,
			90,
			80,
			76,
			74,
			100,
			32,
			69,
			110,
			110,
			110,
			110,
			110,
			32,
			90,
			86,
			69,
			83,
			40,
			62,
			51,
			32,
			63,
			73,
			66,
			81,
			68,
			14,
			72,
			66,
			64,
			73
		};

		List<WeightedTexture2D> additionalFloors = new List<WeightedTexture2D> ();
		List<WeightedTexture2D> additionalWALLS = new List<WeightedTexture2D> ();

		List<WeightedPosterObject> createCustomPosters(string[] paramaterz, string path)
		{
			List<WeightedPosterObject> result = new List<WeightedPosterObject> ();
			for(int i = 0; i < paramaterz.Length; i++)
			{
				PosterObject n = ObjectCreators.CreatePosterObject (new Texture2D[] {
					AssetLoader.TextureFromFile (string.Concat(path, "/Posters/pst_", paramaterz[i], ".png"))
				});
				WeightedPosterObject nn = new WeightedPosterObject{ 
					selection = n,
					weight = pstweights[i] - 10
				}; 
				assetMan.Add<PosterObject> (String.Concat ("pst_", paramaterz [i]), n);
				result.Add (nn);
			}
			return result;
		}

		

		List<WeightedTransform> vendingReplacements()
		{
			List<WeightedTransform> wt = new List<WeightedTransform> ();
			wt.Add (new WeightedTransform {
				selection = ObjectFunctions.FindResourceOfName<Transform> ("ZestyMachine"),
				weight = 100
			});
			wt.Add (new WeightedTransform {
				selection = ObjectFunctions.FindResourceOfName<Transform> ("SodaMachine"),
				weight = 100
			});
			wt.Add (new WeightedTransform {
				selection = ObjectFunctions.FindResourceOfName<Transform> ("CrazyVendingMachineBSODA"),
				weight = 10
			});
			wt.Add (new WeightedTransform {
				selection = ObjectFunctions.FindResourceOfName<Transform> ("CrazyVendingMachineZesty"),
				weight = 10
			});
			return wt;
		}

		List<BasicObjectData> getVendingData()
		{
			List<BasicObjectData> guby = new List<BasicObjectData> ();
			for (int i = 1; i < 5; i++) {
				BasicObjectData vending = new BasicObjectData ();
				vending.position = new Vector3 (5f * (float)(i + 1), 0f, 5f);
				vending.rotation = new Quaternion (0f, 1f, 0f, 0f);
				vending.prefab = ObjectFunctions.FindResourceOfName<Transform> ("ZestyMachine");
				vending.replaceable = true;
				guby.Add (vending);
			}
			for (int j = 0; j < 5; j++) {  //overlap fix
				BasicObjectData vending = new BasicObjectData ();
				vending.position = new Vector3 (5f * (float)(j + 1), 0f, 15f);
				vending.rotation = new Quaternion (0f, 0f, 0f, 0f);
				vending.prefab = ObjectFunctions.FindResourceOfName<Transform> ("ZestyMachine");
				vending.replaceable = true;
				guby.Add (vending);
			}
			return guby;
		}

		List<BasicObjectSwapData> getSwapsV(List<BasicObjectData> objectz)
		{
			List<BasicObjectSwapData> gnarp = new List<BasicObjectSwapData> ();
			foreach (BasicObjectData bod in objectz) {
				BasicObjectSwapData sd = new BasicObjectSwapData ();
				sd.prefabToSwap = bod.prefab;
				sd.chance = 0.74f;
				sd.potentialReplacements = vendingReplacements ().ToArray ();
				gnarp.Add (sd);
			}
			return gnarp;
		}

		List<BasicObjectSwapData> getSwapsL(List<BasicObjectData> objectz)
		{
			List<BasicObjectSwapData> gnarp = new List<BasicObjectSwapData> ();
			foreach (BasicObjectData bod in objectz) {
				BasicObjectSwapData sd = new BasicObjectSwapData ();
				sd.prefabToSwap = bod.prefab;
				sd.chance = 0.5f;
				sd.potentialReplacements = new WeightedTransform[]
				{
					new WeightedTransform
					{
						selection = ObjectFunctions.FindResourceOfName<Transform>("BlueLocker"),
						weight = 50
					},
					new WeightedTransform
					{
						selection = ObjectFunctions.FindResourceOfName<Transform>("Locker"),
						weight = 50
					}
				};
				gnarp.Add (sd);
			}
			return gnarp;
		}

		

		List<RoomAsset> CustomOfficePrefix()
		{

			string path3 = Path.Combine(AssetLoader.GetModPath(this), "RoomData");

			RoomAsset officeRef = ObjectFunctions.FindResourceOfName<RoomAsset> ("Room_Office_0");
			List<RoomAsset> output = new List<RoomAsset> ();
			RoomAsset newOffice = ScriptableObject.CreateInstance<RoomAsset> ();
			newOffice.hasActivity = false;
			newOffice.activity = new ActivityData ();
			newOffice.ceilTex = officeRef.ceilTex;
			newOffice.wallTex = officeRef.wallTex;
			newOffice.florTex = officeRef.florTex;
			newOffice.keepTextures = false;
			newOffice.doorMats = officeRef.doorMats;
			newOffice.potentialDoorPositions = new List<IntVector2>{ 
				new IntVector2(2, 0),
				new IntVector2(2, 4)
			};
			newOffice.cells = RoomParser.parseRoomTiles (File.ReadAllText (path3 + "/Office_PlusShaped.json"));
			newOffice.standardLightCells.Add(new IntVector2(2, 2));
			newOffice.entitySafeCells.Add(new IntVector2(2, 2));
			newOffice.eventSafeCells.Add(new IntVector2(2, 2));
			newOffice.eventSafeCells.Add(new IntVector2(2, 2));
			newOffice.lightPre = MTM101BaldiDevAPI.roomAssetMeta.Get("Room_ReflexOffice_0").value.lightPre;
			newOffice.color = new Color (1f, 1f, 0f);
			newOffice.category = RoomCategory.Office;
			newOffice.basicObjects = RoomParser.parseRoomData (File.ReadAllText (path3 + "/Office_DECOR.json"), assetMan);
			//newOffice.windowChance = 1f; prevent crashes
			newOffice.posterChance = 0.25f;
			newOffice.roomFunctionContainer = officeRef.roomFunctionContainer;
			newOffice.roomFunction = officeRef.roomFunction;
			assetMan.Add<RoomAsset> ("Room_Office_Custom", newOffice);

			RoomAsset newOffice2 = ScriptableObject.CreateInstance<RoomAsset> ();
			newOffice2.hasActivity = false;
			newOffice2.activity = new ActivityData ();
			newOffice2.ceilTex = officeRef.ceilTex;
			newOffice2.wallTex = officeRef.wallTex;
			newOffice2.florTex = officeRef.florTex;
			newOffice2.keepTextures = false;
			newOffice2.doorMats = officeRef.doorMats;
			newOffice2.potentialDoorPositions = new List<IntVector2>{ 
				new IntVector2(0, 1),
				new IntVector2(1, 6),
				new IntVector2(4, 6),
				new IntVector2(5, 4)
			};
			newOffice2.cells = RoomParser.parseRoomTiles (File.ReadAllText (path3 + "/Office_Demo.json"));
			newOffice2.standardLightCells.Add(new IntVector2(1, 3));
			newOffice2.standardLightCells.Add(new IntVector2(5, 4));
			newOffice2.entitySafeCells = new List<IntVector2> () {
				new IntVector2 (1, 3),
				new IntVector2 (4, 5),
				new IntVector2 (2, 3)
			};
			newOffice2.eventSafeCells = new List<IntVector2> () {
				new IntVector2 (1, 3),
				new IntVector2 (4, 5),
				new IntVector2 (2, 3)
			};
			newOffice2.lightPre = MTM101BaldiDevAPI.roomAssetMeta.Get("Room_ReflexOffice_0").value.lightPre;
			newOffice2.color = new Color (1f, 1f, 0f);
			newOffice2.category = RoomCategory.Office;
			newOffice2.basicObjects = RoomParser.parseRoomData (File.ReadAllText (path3 + "/Office_DECOR2.json"), assetMan);
			newOffice2.posterChance = 0.25f;
			newOffice2.roomFunctionContainer = officeRef.roomFunctionContainer;
			newOffice2.roomFunction = officeRef.roomFunction;
			assetMan.Add<RoomAsset> ("Room_Office_Custom2", newOffice2);

			output.Add (newOffice);
			output.Add (newOffice2);
			return output;
		}

		List<string> newHalls = new List<string> ()
		{
			"Plus",
			"Wide",
			"h",
			"BIG",
			"Grid",
			"BigEnd"
		};

		List<RoomAsset> CustomHalls()
		{

			string path3 = Path.Combine(AssetLoader.GetModPath(this), "RoomData");

			List<RoomAsset> output = new List<RoomAsset> ();
			for (int i = 0; i < newHalls.Count; i++) {
				RoomAsset hall1 = ScriptableObject.CreateInstance<RoomAsset> ();
				hall1.hasActivity = false;
				hall1.activity = new ActivityData ();
				hall1.cells = RoomParser.parseRoomTiles (File.ReadAllText (string.Concat(path3, "/HallFormation_", newHalls[i], ".json")));
				assetMan.Add<RoomAsset> ("Room_HallFormation_" + newHalls[i], hall1);
				output.Add (hall1);
			}

			return output;
		}

		SoundMetadata genericMeta = new SoundMetadata (SoundType.Effect, Color.white);



		IEnumerator OnLoad()
		{
			yield return 2;
			yield return "Doing Patch Work...";

			Sprite npcPlaceholder = ObjectFunctions.CreateSprite ("NPCs/PlaceholderNPC", path, 0.5f, 32f);


			SoundObject swt = ObjectFunctions.CreateSound ("itm_switch", path2, genericMeta, "dude", "wav");
			swt.subtitle = false;
			ItemPatch.audSwitch = swt;

			ChaosPatch.xylo = ObjectFunctions.FindResourceOfName<SoundObject> ("Xylophone");

			Sprite blankBook = ObjectFunctions.CreateSprite ("BlankBook", path, 0.5f, 95f);

			assetMan.Add ("BlankBook", blankBook);

			Sprite ridg = ObjectFunctions.CreateSprite ("Notebook_Ridges", path, 0.5f, 95f);

			assetMan.Add ("BookRidges", ridg);


			yield return "Loading Posters...";
			newPosters = createCustomPosters (pstnames, path);

			yield return "Loading Items...";
			SetupItems (path, path2);



			yield return "Loading Stuff...";
			GameObject canvasArt = new GameObject ("Art Canvas");
			SphereCollider lol = canvasArt.AddComponent<SphereCollider> ();
			lol.radius = 4f;
			lol.isTrigger = true;


			SpriteRenderer spr = canvasArt.AddComponent<SpriteRenderer> ();
			spr.material = ObjectFunctions.FindResourceOfName<Material> ("SpriteStandard_Billboard");

			Rigidbody rb = canvasArt.AddComponent<Rigidbody> ();
			rb.constraints = RigidbodyConstraints.FreezeAll;
			rb.useGravity = false;

			ArtCavas ac = canvasArt.AddComponent<ArtCavas> ();
			ac.allSprites = ObjectFunctions.CreateSpriteArray ("Decorative/Canvas_", 28, 0.4f, 6, path);

			ac.renderer = spr;
			canvasArt.ConvertToPrefab (true);
			assetMan.Add<Transform> ("ArtCanvas", canvasArt.transform);



			yield return "Loading Textures...";
			assetMan.Add ("Floor_Metal", GenerateFloorTex ("Metallic", path));
			assetMan.Add ("Floor_Checkered", GenerateFloorTex ("Checkered", path));
			assetMan.Add ("Floor_Saloon", GenerateFloorTex ("Saloon", path));
			assetMan.Add ("Floor_Stone", GenerateFloorTex ("Stone", path));
			assetMan.Add ("Floor_Wood", GenerateFloorTex ("Wood", path));

			assetMan.Add ("Wall_BlueSaloon", GenerateWallTex ("BlueSaloon", path));
			assetMan.Add ("Wall_Sandpaper", GenerateWallTex ("Sandpaper", path));
			assetMan.Add ("Wall_Stucco", GenerateWallTex ("Stucco", path));
			assetMan.Add ("Wall_Bricks", GenerateWallTex ("Bricks", path));


			assetMan.Add<Sprite> ("Button_Rock", ObjectFunctions.CreateSprite ("Interface/RPS/rockIdle", modContents.imagePath, 0.5f, 32f));
			assetMan.Add<Sprite> ("Button_Paper", ObjectFunctions.CreateSprite ("Interface/RPS/paperIdle", modContents.imagePath, 0.5f, 32f));
			assetMan.Add<Sprite> ("Button_Scissors", ObjectFunctions.CreateSprite ("Interface/RPS/scissorsIdle", modContents.imagePath, 0.5f, 32f));

			assetMan.Add<Sprite> ("Button_RockPress", ObjectFunctions.CreateSprite ("Interface/RPS/rockPress", modContents.imagePath, 0.5f, 32f));
			assetMan.Add<Sprite> ("Button_PaperPress", ObjectFunctions.CreateSprite ("Interface/RPS/paperPress", modContents.imagePath, 0.5f, 32f));
			assetMan.Add<Sprite> ("Button_ScissorsPress", ObjectFunctions.CreateSprite ("Interface/RPS/scissorsPress", modContents.imagePath, 0.5f, 32f));

			//Debug.Log ("Textures Loaded");

			additionalFloors.Add (assetMan.Get<WeightedTexture2D>("Floor_Metal"));
			additionalFloors.Add (assetMan.Get<WeightedTexture2D>("Floor_Checkered"));
			additionalFloors.Add (assetMan.Get<WeightedTexture2D>("Floor_Saloon"));
			additionalFloors.Add (assetMan.Get<WeightedTexture2D>("Floor_Stone"));
			additionalFloors.Add (assetMan.Get<WeightedTexture2D>("Floor_Wood"));

			additionalWALLS.Add (assetMan.Get<WeightedTexture2D>("Wall_BlueSaloon"));
			additionalWALLS.Add (assetMan.Get<WeightedTexture2D>("Wall_Sandpaper"));
			additionalWALLS.Add (assetMan.Get<WeightedTexture2D>("Wall_Stucco"));
			additionalWALLS.Add (assetMan.Get<WeightedTexture2D>("Wall_Bricks"));

			//Debug.Log ("Textures Added into Data");

			yield return "Loading Rooms...";
			SetupRooms (path, path2, path3);

			yield return "Loading Structures...";
			Obstacle lol54 = EnumExtensions.ExtendEnum<Obstacle> ("Fan");
			Obstacle lol55 = EnumExtensions.ExtendEnum<Obstacle> ("LaserDoor");

			List<WeightedObjectBuilder> additionalObjects = new List<WeightedObjectBuilder> ();

			GameObject fanStruct = new GameObject ("FanStructure");
			fanStruct.AddComponent<Fan> ();
			Fan fan = fanStruct.GetComponent<Fan> ();



			SoundObject fanBlow = ObjectFunctions.CreateSound ("FanLoop", path2, genericMeta, "FAN_Run");
			fan.fanAudi = fanBlow;

			List<Sprite> fansprites = new List<Sprite> ();

			Texture2D spriteAtlas2 = AssetLoader.TextureFromFile (path + "/FanSprites.png");

			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < 4; j++) {
					Rect eRECTion = new Rect (((float)j * 256f), ((float)i * 256f), 256f, 256f);
					Sprite newSprite = Sprite.Create(spriteAtlas2, 
						eRECTion, 
						new Vector2(0.5f, -0.05f), 26 );
					fansprites.Add (newSprite);
				}
			}

			fan.spritemap = fansprites.ToArray ();

			fanStruct.ConvertToPrefab (false);

			GameObject fanBuild = new GameObject ("FanBuild");
			fanBuild.AddComponent<FanBuilder> ();
			FanBuilder fanBuildScript = fanBuild.GetComponent<FanBuilder> ();

			fanBuildScript.fanPre = fanStruct.GetComponent<Fan> ();
			fanBuildScript.buttonPre = ObjectFunctions.FindResourceOfName<GameButton> ("GameButton");
			fanBuildScript.obstacle = lol54;

			fanBuild.ConvertToPrefab (false);
			
			WeightedObjectBuilder fanBuilder = new WeightedObjectBuilder {
				selection = fanBuildScript,
				weight = 80
			};
			

			GameObject laserDoor = new GameObject ("Laser Door");
			laserDoor.AddComponent<LaserDoor> ();
			LaserDoor laserScript = laserDoor.GetComponent<LaserDoor> ();

			Sprite laserSpriteInactive = ObjectFunctions.CreateSprite ("LaserDoor_O", path, 0.5f, 25.6f);
			Sprite laserSpriteActive = ObjectFunctions.CreateSprite ("LaserDoor_C", path, 0.5f, 25.6f);

			laserScript.closedSprite = laserSpriteActive;
			laserScript.openSprite = laserSpriteInactive;

			laserDoor.ConvertToPrefab (true);

			GameObject laserBuild = new GameObject ("Laser Builder");
			laserBuild.AddComponent<LaserDoorBuilder> ();
			LaserDoorBuilder laserBuildScript = laserBuild.GetComponent<LaserDoorBuilder> ();
			laserBuildScript.doorPre = laserDoor.GetComponent<LaserDoor>();
			laserBuildScript.obstacle = lol55;
			laserBuild.ConvertToPrefab (true);

			WeightedObjectBuilder laserBuilder = new WeightedObjectBuilder {
				selection = laserBuildScript,
				weight = 100
			};

			additionalObjects.Add (fanBuilder);
			additionalObjects.Add (laserBuilder);

			newStructures.Add ("Fan", fanBuilder);
			newStructures.Add ("LaserDoor", laserBuilder);

			SoundObject lolololol = ObjectFunctions.CreateSound("genericWhip", path2, genericMeta, "", "wav");
			lolololol.subtitle = false;
			assetMan.Add<SoundObject> ("genericWhip", lolololol);

			yield return "Loading NPCS...";

			skeletonMeta = new SoundMetadata (SoundType.Voice, new Color (0.25f, 0.5f, 0.6f));

			SetupNPCS (path, path2);
			yield return "Loading Custom Room Shapes...";


			//List<RoomAsset> additionalFacultys = GenerateFacultyRoom ();
			List<RoomAsset> addonOffice = CustomOfficePrefix ();
			List<RoomAsset> addonHall = CustomHalls ();

			/*List<WeightedRoomAsset> weightedFacultys = new List<WeightedRoomAsset> ()
			{
				new WeightedRoomAsset
				{
					selection = additionalFacultys[0],
					weight = 50
				}
			};*/


			List<WeightedRoomAsset> offic = new List<WeightedRoomAsset> ()
			{
				new WeightedRoomAsset
				{
					selection = addonOffice[0],
					weight = 100
				},
				new WeightedRoomAsset
				{
					selection = addonOffice[1],
					weight = 100
				}
			};

			assetMan.Add<RoomAsset> ("Office_Custom", addonOffice [0]);
			assetMan.Add<RoomAsset> ("Office_Custom2", addonOffice [1]);

			hal = new List<WeightedRoomAsset> ()
			{
				new WeightedRoomAsset
				{
					selection = addonHall[0],
					weight = 74
				},
				new WeightedRoomAsset
				{
					selection = addonHall[1],
					weight = 124
				},
				new WeightedRoomAsset
				{
					selection = addonHall[2],
					weight = 99
				},
				new WeightedRoomAsset
				{
					selection = addonHall[3],
					weight = 69
				},
				new WeightedRoomAsset
				{
					selection = addonHall[4],
					weight = 74
				},
				new WeightedRoomAsset
				{
					selection = addonHall[5],
					weight = 74
				}
			};
			RoomAsset specialREF = ObjectFunctions.FindResourceOfName<RoomAsset> ("Room_Cafeteria1");

			yield return "Loading Special Room...";

			SoundObject placeholderAudioFile = ObjectFunctions.CreateSound ("placeholderScream", modContents.audioPath, genericMeta, "BAL_EventPlaceholder");

			yield return "Loading Events(1/4)...";
			FloatingItem itemPre = new NPCBuilder<FloatingItem> (base.Info)
				.AddLooker ()
				.AddMetaFlag (NPCFlags.Standard)
				.IgnoreBelts ()
				.IgnorePlayerOnSpawn ()
				.SetAirborne ()
				.SetEnum ("FloatingItem")
				.SetName ("FloatingItem")
				.Build ();//ObjectCreators.CreateNPC<FloatingItem> ("FloatingItem", Character.Beans, modContents.npcPoster["Carmella"], true, false, false, 10, 250);

			itemPre.itemTable = ItemFunctions.parseItemTable (File.ReadAllText (
				Path.Combine (AssetLoader.GetModPath (this), "LootTabels") + "/itemEvent.json"), assetMan).ToArray();



			ItemEvent eventnew = new RandomEventBuilder<ItemEvent> (base.Info).SetSound(
				ObjectFunctions.CreateSound("NPCs/skel_event_items", modContents.audioPath, skeletonMeta, "SKEL_EventItems")
			).SetEnum ("Items").SetMinMaxTime (20f, 40f).SetName ("Item Sea").SetMeta(RandomEventFlags.None, "ItemEvent").Build ();
			eventnew.itemPre = itemPre;

			CloneEvent cloneEVT = new RandomEventBuilder<CloneEvent> (base.Info).SetSound (
				ObjectFunctions.CreateSound("NPCs/skel_event_clones", modContents.audioPath, skeletonMeta, "SKEL_EventClones")
			).SetEnum ("Clone").SetMinMaxTime (10f, 15f).SetName ("Clone Machine").SetMeta(RandomEventFlags.CharacterSpecific, "CloneEvent").Build ();
		
			MathCorruptionEvent mathEVT = new RandomEventBuilder<MathCorruptionEvent> (base.Info).SetSound(
				ObjectFunctions.CreateSound("NPCs/skel_event_math", modContents.audioPath, skeletonMeta, "SKEL_EventCorruption")
			).SetEnum ("Corruption").SetMinMaxTime (10f, 25f).SetName ("Math Machine Corruption").SetMeta(RandomEventFlags.RoomSpecific, "MathEvent").Build ();

			DoorEvent doorEVT = new RandomEventBuilder<DoorEvent> (base.Info).SetSound(
				ObjectFunctions.CreateSound("NPCs/skel_event_doors", modContents.audioPath, skeletonMeta, "SKEL_EventDoor")
			).SetEnum ("Door").SetMinMaxTime (20f, 35f).SetName ("Door Lockdown").SetMeta(RandomEventFlags.None, "DoorEvent").Build ();

			WeightedRandomEvent ie = new WeightedRandomEvent
			{
				selection = eventnew,
				weight = 50
			};

			WeightedRandomEvent ce = new WeightedRandomEvent
			{
				selection = cloneEVT,
				weight = 70
			};

			WeightedRandomEvent me = new WeightedRandomEvent
			{
				selection = mathEVT,
				weight = 80
			};

			WeightedRandomEvent de = new WeightedRandomEvent
			{
				selection = doorEVT,
				weight = 85
			};

			modContents.newEvents.Add ("ItemEvent", ie);
			modContents.newEvents.Add ("CloneEvent", ce);
			modContents.newEvents.Add ("MathEvent", me);
			modContents.newEvents.Add ("DoorEvent", de);

			yield return "Finalizing...";

			ObjectBuilderMetaStorage.Instance.Add (new ObjectBuilderMeta (base.Info, fanBuildScript));
			ObjectBuilderMetaStorage.Instance.Add (new ObjectBuilderMeta (base.Info, laserBuildScript));

			LevelAsset baseEndlessData = ObjectFunctions.FindResourceOfName<LevelAsset> ("EC_-1431528365_EndlessMedium_Converted_31109695");

			List<PosterData> endlessPosterDat = RoomParser.parsePosterData (File.ReadAllText (path3 + "/EndlessMediumPosterData.json"), assetMan);

			foreach(PosterData posterData in endlessPosterDat)
			{
				baseEndlessData.posters.Add (posterData); 
			}
			baseEndlessData.rooms [0].florTex = assetMan.Get<WeightedTexture2D> ("Floor_Checkered").selection;
			baseEndlessData.MarkAsNeverUnload ();
			yield break;
		}

		public void SetupNPCS(string path, string path2)
		{
			string metadataPath = Path.Combine(AssetLoader.GetModPath(this), "NPCData");
			RoomCategory[] roomz = new RoomCategory[] {
				RoomCategory.Hall,
				RoomCategory.Special,
				RoomCategory.Class
			};

			SoundMetadata carmellaMeta = new SoundMetadata (SoundType.Voice, new Color (0.66f, 1f, 0.32f));
			SoundMetadata iceMeta = new SoundMetadata (SoundType.Voice, new Color (0.25f, 0.75f, 1f));
			SoundMetadata iceMeta2 = new SoundMetadata (SoundType.Effect, new Color (0.25f, 0.75f, 1f));
			SoundMetadata billMeta = new SoundMetadata(SoundType.Effect, new Color (0.929f, 0.1f, 0.14f));
			SoundMetadata skeletonSFXMeta = new SoundMetadata (SoundType.Effect, new Color (0.25f, 0.5f, 0.6f));
			SoundMetadata rhythmMeta = new SoundMetadata (SoundType.Voice, new Color (0.5f, 0f, 1f));
			Color kColor = new Color (138f / 255f, 57f / 255f, 204f / 255f);
			SoundMetadata kMeta = new SoundMetadata (SoundType.Voice, kColor);
			SoundMetadata clrMeta = new SoundMetadata (SoundType.Voice, new Color (0.96f, 0.49f, 0f));
			SoundMetadata placeMeta = new SoundMetadata (SoundType.Voice, new Color (0.73f, 0.54f, 0.4f));
			SoundMetadata fritData = new SoundMetadata (SoundType.Voice, new Color (0.47f, 0.7f, 0.21f));

			SoundMetadata klutzSub = new SoundMetadata (SoundType.Effect, new Color (0.95f, 0.1f, 0.1f));


			Debug.Log ("Loading Carmella");
			Carmella carmella = (Carmella)ObjectFunctions.CreateCustomNPC<Carmella> (
				                    string.Concat (metadataPath, "/carmella.json"),
				                    base.Info,
				                    modContents,
				                    NPCFlags.Standard,
				                    roomz);

			string lootPath = Path.Combine(AssetLoader.GetModPath(this), "LootTabels");

			giftloottable = ItemFunctions.parseItemTable(File.ReadAllText(
				string.Concat(lootPath, "/carmella.json")
			), assetMan).ToArray();


			carmella.giftItem = modContents.newItems ["MysteryGift"].selection;


			carmella.idleAud = ObjectFunctions.CreateSoundArray ("NPCs/cml_idle_", path2, 3, carmellaMeta, "CML_Rand");
			carmella.leaveAud = ObjectFunctions.CreateSoundArray ("NPCs/cml_leave_", path2, 3, carmellaMeta, "CML_Dissapointed");
			carmella.thankAud = ObjectFunctions.CreateSoundArray ("NPCs/cml_enjoy_", path2, 3, carmellaMeta, "CML_Thank");
			carmella.offerAud = ObjectFunctions.CreateSound("NPCs/cml_offer", path2, carmellaMeta, "CML_Offer");
			carmella.explodeAud = ObjectFunctions.CreateSoundArray ("NPCs/cml_explode_", path2, 3, carmellaMeta, "CML_Oops");

			carmella.xyloNoise = ObjectFunctions.FindResourceOfName<SoundObject> ("Xylophone");
			carmella.boom = ObjectFunctions.CreateSound("NPCs/Explosion", path2, genericMeta, "BOOM", "wav", false);



			Debug.Log ("Loading Ice Sled");

			IceNpc icesled = (IceNpc)ObjectFunctions.CreateBasicNPC<IceNpc> (
				string.Concat (metadataPath, "/iceSled.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				new RoomCategory[]{RoomCategory.Hall});

			icesled.spritess = ObjectFunctions.GenerateAtlas ("/NPCs/IceOnSled", path, 2, 4, 0.45f, 28f);


			icesled.skateNoise = ObjectFunctions.CreateSound ("NPCs/ic_skate", path2, iceMeta2, "ICE_Skate");
			icesled.frostNoise = ObjectFunctions.CreateSound ("NPCs/ic_freeze", path2, iceMeta2, "ICE_Freeze");
			icesled.happySounds = ObjectFunctions.CreateSoundArray ("NPCs/ic_happy", path2, 3, iceMeta, "ICE_Rand");
			icesled.frozenSounds = ObjectFunctions.CreateSoundArray ("NPCs/ic_frozen", path2, 5, iceMeta, "ICE_Frost");

			Debug.Log ("Loading Billbert");

			Billbert billberty = (Billbert)ObjectFunctions.CreateBasicNPC<Billbert> (
				string.Concat (metadataPath, "/billbert.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				new RoomCategory[]{RoomCategory.Hall});

			Sprite[] globalBillSprites = ObjectFunctions.GenerateAtlas ("/NPCs/BillbertAnims_Dithering", path, 2, 4, 0.45f, 28f);
	

			List<Sprite> runAnim = new List<Sprite> {
				globalBillSprites[5],
				globalBillSprites[6],
				globalBillSprites[7],
				globalBillSprites[0],
				globalBillSprites[1],
				globalBillSprites[0],
				globalBillSprites[7],
				globalBillSprites[6]
			};

			billberty.runSprites = runAnim;
			billberty.walkSprite = globalBillSprites[4];


			billberty.run = ObjectFunctions.CreateSound ("NPCs/bill_run", path2, billMeta, "BIL_Run");

			Debug.Log ("Loading Skeleton");

			Skeleton skelly = (Skeleton)ObjectFunctions.CreateBasicNPC<Skeleton> (
				string.Concat (metadataPath, "/skeleton.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				new RoomCategory[]{EnumExtensions.GetFromExtendedName<RoomCategory>("Parts"), RoomCategory.Faculty});

			Sprite[] skelSprites = ObjectFunctions.GenerateAtlas ("/NPCs/Endo_Lol", path, 2, 4, 0.4f, 32f);

			skelly.allSprites = skelSprites;




			skelly.warpAudio = ObjectFunctions.CreateSound ("NPCs/skel_teleport", path2, skeletonSFXMeta, "SKEL_Warp");
			skelly.noticeSound = ObjectFunctions.CreateSound ("NPCs/skel_notice", path2, skeletonSFXMeta, "SKEL_Spot");


			skelly.noises = ObjectFunctions.CreateSoundArray ("NPCs/skel_noise", path2, 5, skeletonMeta, "SKEL_Talk");
	


			Debug.Log ("Loading Rhythm");

			Rhythm rhythmdude = (Rhythm)ObjectFunctions.CreateCustomNPC<Rhythm> (
				string.Concat (metadataPath, "/rhythm.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				new RoomCategory[]{EnumExtensions.GetFromExtendedName<RoomCategory>("Music")},
				modContents.newRooms ["MusicRoom"]);

			//Rhythm Sound data
			rhythmdude.audWander = ObjectFunctions.CreateSoundArray  ("NPCs/rtm_wander", path2, 3, rhythmMeta, "RTM_Wander", "ogg",  true);
			rhythmdude.audDoor = ObjectFunctions.CreateSound ("NPCs/rtm_doorHit", path2, genericMeta, "WHAT THE FUCK?!", "ogg", false);
			rhythmdude.audNotice = ObjectFunctions.CreateSound ("NPCs/rtm_rev", path2, rhythmMeta, "RTM_Rev", "ogg");
			rhythmdude.audInstructions = ObjectFunctions.CreateSound ("NPCs/rtm_instructions", path2, rhythmMeta, "RTM_Instructions", "wav");
			rhythmdude.audCrash = ObjectFunctions.CreateSound ("NPCs/rtm_CYMBALS", path2, rhythmMeta, "RTM_Bat", "ogg");
			rhythmdude.audLeft = ObjectFunctions.CreateSound ("NPCs/rtm_walkaway", path2, rhythmMeta, "RTM_WalkAway", "ogg");
			rhythmdude.audLate = ObjectFunctions.CreateSound ("NPCs/rtm_missedbeat", path2, rhythmMeta, "RTM_MissedBeat", "ogg");
			rhythmdude.audTooEarly = ObjectFunctions.CreateSound ("NPCs/rtm_tooearly", path2, rhythmMeta, "RTM_TooEarly", "ogg");
			rhythmdude.audMad = ObjectFunctions.CreateSoundArray ("NPCs/rtm_happensNow", path2, 2, rhythmMeta, "RTM_HappensNow", "ogg", true);
			rhythmdude.audMadWander = ObjectFunctions.CreateSoundArray ("NPCs/rtm_Mad", path2, 3, rhythmMeta, "RTM_Angry", "ogg", true);

			rhythmdude.musBad =  ObjectFunctions.CreateSound ("NPCs/mus_RhythmBad", path2, genericMeta, "hi sabri", "ogg", false);
			rhythmdude.musGame = ObjectFunctions.CreateSound ("NPCs/mus_Rhythm", path2, genericMeta, "dude wat", "ogg", false);

			//Kim junk goes here
			Texture2D kInstructions = AssetLoader.TextureFromFile (path + "/UI_RPS.png");
			assetMan.Add ("Kim_Instructions", kInstructions);

			Debug.Log ("Loading Kim");

			Kim kimChar = (Kim)ObjectFunctions.CreateCustomNPC<Kim> (
				              string.Concat (metadataPath, "/kim.json"),
				              base.Info,
				              modContents,
				              NPCFlags.Standard,
				              roomz);
			

			//Kim audio goes here
			kimChar.audNotice = ObjectFunctions.CreateSound ("NPCs/kim_heyyou", path2, kMeta, "KIM_Notice", "ogg");
			kimChar.audGame = ObjectFunctions.CreateSound  ("NPCs/kim_game", path2, kMeta, "KIM_Game", "ogg");
			kimChar.audCountdown = ObjectFunctions.CreateSound  ("NPCs/kim_rps", path2, kMeta, "KIM_Countdown", "ogg");
			kimChar.audWander = ObjectFunctions.CreateSoundArray ("NPCs/kim_wander", path2, 3, kMeta, "KIM_Random", "ogg", true);
			kimChar.audWin = ObjectFunctions.CreateSoundArray ("NPCs/kim_win", path2, 3, kMeta, "KIM_Win", "ogg", true);
			kimChar.audLose = ObjectFunctions.CreateSoundArray ("NPCs/kim_lost", path2, 3, kMeta, "KIM_Lose", "ogg", true);
			kimChar.audTie = ObjectFunctions.CreateSound  ("tie", path2, genericMeta, "GAME_TIE");
			kimChar.rpsInputs = ObjectFunctions.CreateSoundArray ("RPS_Input", path2, 3, genericMeta, "THATS NOT... WHAT?", "ogg", false);

			//Coloury(this ones a brain scratcher)
			Debug.Log ("Loading Coloury");

			Coloury clrChr = (Coloury)ObjectFunctions.CreateCustomNPC<Coloury> (
				string.Concat (metadataPath, "/coloury.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				new RoomCategory[]{EnumExtensions.GetFromExtendedName<RoomCategory>("Art")},
				modContents.newRooms ["ArtClass"]);
			
			clrChr.itemTable = ItemFunctions.parseItemTable (File.ReadAllText (
				string.Concat (lootPath, "/coloury.json")), assetMan).ToArray();
			Texture2D cOverlay = AssetLoader.TextureFromFile (path + "/UI_PaintSplat.png");
			assetMan.Add ("Coloury_Overlay", cOverlay);
			clrChr.failsaveCanvas = assetMan.Get<Transform> ("ArtCanvas").GetComponent<ArtCavas>();

			//Coloury Audio

			clrChr.idleAudio = ObjectFunctions.CreateSoundArray ("NPCs/clr_idle", path2, 3, clrMeta, "CLR_Idle");
			clrChr.numAudio = ObjectFunctions.CreateSoundArray ("NPCs/clr_num", path2, 5, clrMeta, "CLR_Num");
			clrChr.screamAud = ObjectFunctions.CreateSound ("NPCs/clr_scream", path2, clrMeta, "CLR_Scream");
			clrChr.madAud = ObjectFunctions.CreateSound ("NPCs/clr_mad", path2, clrMeta, "CLR_Mad");
			clrChr.noticeAud = ObjectFunctions.CreateSound ("NPCs/clr_notice", path2, clrMeta, "CLR_Notice");
			clrChr.timesUp = ObjectFunctions.CreateSound ("NPCs/clr_timesup", path2, clrMeta, "CLR_TimesUp");
			clrChr.winAud = ObjectFunctions.CreateSound ("NPCs/clr_win", path2, clrMeta, "CLR_Win");

			clrChr.xylo = ObjectFunctions.FindResourceOfName<SoundObject> ("Xylophone");

			Debug.Log ("Loading Placeface");
			Placeface placerfacer = (Placeface)ObjectFunctions.CreateCustomNPC<Placeface> (
				string.Concat (metadataPath, "/placeface.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				new RoomCategory[]{RoomCategory.Test},
				modContents.newRooms ["RGBRoom"]);
			//kill me


			placerfacer.elephantHit = ObjectFunctions.CreateSound ("NPCs/pla_elephantHit", path2, placeMeta, "Vfx_Test_ElHit");
			placerfacer.activateSound = ObjectFunctions.CreateSound ("NPCs/pla_parrot", path2, placeMeta, "PLA_PARROT");



			Debug.Log ("Loading Fritz");

			Fritz fritzDude = (Fritz)ObjectFunctions.CreateCustomNPC<Fritz> (
				string.Concat (metadataPath, "/fritz.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				roomz);
			
			fritzDude.idleAud = ObjectFunctions.CreateSoundArray ("NPCs/ftz_wander", path2, 3, fritData, "FTZ_Random", "ogg", true);
			fritzDude.sweepAud = ObjectFunctions.CreateSound ("NPCs/ftz_sweepIntro", path2, fritData, "FTZ_SweepIntro", "ogg");
			fritzDude.sweepingAud = ObjectFunctions.CreateSoundArray ("NPCs/ftz_sweep", path2, 3, fritData, "FTZ_Sweeping", "ogg", true);
			fritzDude.sweepEndAud = ObjectFunctions.CreateSound  ("NPCs/ftz_sweepEnd", path2, fritData, "FTZ_SweepEnd", "ogg");

					

			Debug.Log ("Loading Zarkstur");

			Zark zarkstuu = (Zark)ObjectFunctions.CreateCustomNPC<Zark> (
				string.Concat (metadataPath, "/zarkstur.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				roomz);

			zarkstuu.audIdle = ObjectFunctions.CreateSoundArray ("NPCs/zrk_idle", path2, 3, new SoundMetadata (
				SoundType.Voice,
				new Color (1f, (96f / 255f), (22f / 255f))), "ZRK_Idle", "ogg", true);

			zarkstuu.audRide = ObjectFunctions.CreateSoundArray ("NPCs/zrk_ride", path2, 3, new SoundMetadata (
				SoundType.Voice,
				new Color ((203f / 255f), (72f / 255f), (210f / 255f))), "ZRK_Ride", "ogg", true);

			zarkstuu.audThank = ObjectFunctions.CreateSoundArray ("NPCs/zrk_thank", path2, 3, new SoundMetadata (
				SoundType.Voice,
				new Color ((20f / 255f), (100f / 255f), (252f / 255f))), "ZRK_Thank", "ogg", true);



			Debug.Log ("Loading Pencir");

			Pencir penisc = (Pencir)ObjectFunctions.CreateBasicNPC<Pencir> (
				string.Concat (metadataPath, "/pencir.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				new RoomCategory[]{RoomCategory.Hall});

			List<Sprite> pencilSprites = new List<Sprite> ();

			Texture2D spriteAtlas3 = AssetLoader.TextureFromFile (path + "/NPCs/pencil_sprites.png");

			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < 4; j++) {
					Rect eRECTion = new Rect (((float)j * 256f), ((float)i * 256f), 256f, 256f);
					Sprite newSprite = Sprite.Create(spriteAtlas3, 
						eRECTion, 
						new Vector2(0.5f, 0.5f), 25 );
					pencilSprites.Add (newSprite);
				}
			}

			penisc.allSprites = pencilSprites.ToArray ();

			penisc.moveAud = ObjectFunctions.CreateSound ("NPCs/pnc_move", path2, genericMeta, "PNC_Move", "wav");
			penisc.audStab = ObjectFunctions.CreateSound ("NPCs/stabsound", path2, genericMeta, "PNC_Stab", "wav");



			Debug.Log ("Loading Klutz");

			Crabby crabb = (Crabby)ObjectFunctions.CreateCustomNPC<Crabby> (
				string.Concat (metadataPath, "/klutz.json"),
				base.Info,
				modContents,
				NPCFlags.HasPhysicalAppearence,
				roomz);

			crabb.audActivate = ObjectFunctions.CreateSound ("NPCs/ktz_activate", path2, klutzSub, "KTZ_Activate", "ogg");
			crabb.audTeleport = ObjectFunctions.CreateSound ("NPCs/ktz_teleport", path2, klutzSub, "KTZ_Teleport", "ogg");


			Debug.Log ("Loading Wildcard Willy");

			WildcardWilly campfireWilly = (WildcardWilly)ObjectFunctions.CreateCustomNPC<WildcardWilly>(
				string.Concat (metadataPath, "/willy.json"),
				base.Info,
				modContents,
				NPCFlags.Standard,
				new RoomCategory[]{RoomCategory.Special});

			campfireWilly.bangAudio = ObjectFunctions.CreateSound ("NPCs/wc_bang", modContents.audioPath, genericMeta, "WIL_Bang", "ogg");
			campfireWilly.drummerMode = ObjectFunctions.CreateSound ("NPCs/wc_drumloop", modContents.audioPath, genericMeta, "WIL_Drum", "ogg");

			campfireWilly.switchAud = ObjectFunctions.CreateSound ("NPCs/wc_switch", modContents.audioPath, genericMeta, "WIL_Drum", "ogg", false);

			campfireWilly.ringAudio = ObjectFunctions.CreateSound ("NPCs/wc_ring", modContents.audioPath, genericMeta, "WIL_Ring", "ogg");

			campfireWilly.soupAccept = ObjectFunctions.CreateSound ("NPCs/wc_lines_soupEnjoy", modContents.audioPath, genericMeta, "WIL_SoupEnjoy", "ogg");

			campfireWilly.hammerAnnouncement = ObjectFunctions.CreateSound ("NPCs/wc_hammer", modContents.audioPath, genericMeta, "WIL_SwitchPercussion", "ogg");

			campfireWilly.wanderSoup = ObjectFunctions.CreateSoundArray("NPCs/wc_soupIdle", modContents.audioPath, 3, genericMeta, "WIL_SoupIdle", "ogg");

			campfireWilly.wanderWhistle = ObjectFunctions.CreateSoundArray("NPCs/wc_whistleIdle", modContents.audioPath, 3, genericMeta, "WIL_AlertIdle", "ogg");


			WeightedNPC carmNpc = new WeightedNPC
			{
				selection = carmella,
				weight = 96
			};
			WeightedNPC iceNpc = new WeightedNPC
			{
				selection = icesled,
				weight = 84
			};
			WeightedNPC billNpc = new WeightedNPC
			{
				selection = billberty,
				weight = 89
			};
			WeightedNPC skelNPC = new WeightedNPC
			{
				selection = skelly,
				weight = 66
			};
			WeightedNPC rtmNPC = new WeightedNPC
			{
				selection = rhythmdude,
				weight = 80
			};
			WeightedNPC kimNPC = new WeightedNPC
			{
				selection = kimChar,
				weight = 85
			};
			WeightedNPC clrNPC = new WeightedNPC
			{
				selection = clrChr,
				weight = 73
			};
			WeightedNPC placeNPC = new WeightedNPC
			{
				selection = placerfacer,
				weight = 60
			};
			WeightedNPC fritzNPC = new WeightedNPC
			{
				selection = fritzDude,
				weight = 82
			};
			WeightedNPC zarkNPC = new WeightedNPC
			{
				selection = zarkstuu,
				weight = 73
			};
			WeightedNPC pencirNPC = new WeightedNPC
			{
				selection = penisc,
				weight = 67
			};

			WeightedNPC crabNPC = new WeightedNPC
			{
				selection = crabb,
				weight = 60
			};

			WeightedNPC wildNPC = new WeightedNPC
			{
				selection = campfireWilly,
				weight = 74
			};


			modContents.newNpcs.Add ("Carmella", carmNpc);
			modContents.newNpcs.Add ("Ice Sled", iceNpc);
			modContents.newNpcs.Add ("Billbert", billNpc);
			modContents.newNpcs.Add ("Skeleton", skelNPC);
			modContents.newNpcs.Add ("Rhythm", rtmNPC);
			modContents.newNpcs.Add ("Kim", kimNPC);
			modContents.newNpcs.Add ("Coloury", clrNPC);
			modContents.newNpcs.Add ("Placeface", placeNPC);
			modContents.newNpcs.Add ("Fritz", fritzNPC);
			modContents.newNpcs.Add ("Zarkstur", zarkNPC);
			modContents.newNpcs.Add ("Pencir", pencirNPC);
			modContents.newNpcs.Add ("Klutz", crabNPC);
			modContents.newNpcs.Add ("Wildcard", wildNPC);
		}


		public void SetupItems(string path, string path2)
		{
			Sprite Garbage = ObjectFunctions.FindResourceOfName<Sprite> ("PlaceHolder");

			Sprite ch1 = ObjectFunctions.CreateSprite ("items/Cheat_HUD", path, 0.5f, 16f);
			Sprite ch2 = ObjectFunctions.CreateSprite ("items/Cheat_Pickup", path, 0.5f, 26f);

			ItemBuilder cheatItem = new ItemBuilder (base.Info).SetEnum ("CheatDevice").SetNameAndDescription ("ITM_Cheat", "ITM_Cheat_Desc").SetShopPrice (800).SetSprites (ch1, ch2).SetGeneratorCost (50).SetMeta(ItemFlags.None, new string[]
				{"CheatDevice"});
			ItemObject chheatITM = cheatItem.Build ();

			ITM_CheatCode citm = ItemFunctions.CreateItemObject<ITM_CheatCode> ("ITM_CheatCode", false, true).GetComponent<ITM_CheatCode>();
			chheatITM.item = citm;

			GameObject WHISTLE_patch = new GameObject ("ITM_WHistleFix");
			UnityEngine.Object.DontDestroyOnLoad(WHISTLE_patch);
			WHISTLE_patch.SetActive (true);
			WHISTLE_patch.AddComponent<ITM_WhistleImproved> ();
			ITM_WhistleImproved iwi = WHISTLE_patch.GetComponent<ITM_WhistleImproved> ();
			iwi.audWhistle = ObjectFunctions.FindResourceOfName<SoundObject>("PriWhistle");

			ObjectFunctions.FindResourceOfName<ItemObject>("PrincipalWhistle").item = iwi;

			Sprite p1 = ObjectFunctions.CreateSprite ("items/Percussion_Large", path, 0.5f, 16f);
			Sprite p2 = ObjectFunctions.CreateSprite ("items/Percussion_Small", path, 0.5f, 32f);

			ItemBuilder periufjdk = new ItemBuilder (base.Info).SetEnum ("PercussionHammer").SetNameAndDescription ("ITM_Perc", "ITM_Perc_Desc").SetShopPrice (740).SetSprites (p1, p2).SetGeneratorCost (61).SetMeta(ItemFlags.None, new string[]
				{"PercussionHam"});

			ItemObject percussionITM = periufjdk.Build ();

			ITM_Perucssion perc = ItemFunctions.CreateItemObject<ITM_Perucssion> ("ITM_Percussion", false, true).GetComponent<ITM_Perucssion> ();
			perc.bang = ObjectFunctions.FindResourceOfName<SoundObject> ("DrR_Hammer");
			percussionITM.item = perc;

			Sprite r1 = ObjectFunctions.CreateSprite ("items/REMOTE_HUD", path, 0.5f, 16f);
			Sprite r2 = ObjectFunctions.CreateSprite ("items/REMOTE_PICKUP", path, 0.5f, 36f);

			ItemObject remoteITM = new ItemBuilder (base.Info)
				.SetEnum("RemoteControl")
				.SetGeneratorCost (60)
				.SetNameAndDescription ("ITM_Remote", "ITM_Remote_Desc")
				.SetShopPrice (575)
				.SetSprites (r1, r2)
				.SetMeta (ItemFlags.None, new string[]{ "Remote" }).Build ();//ObjectCreators.CreateItemObject ("ITM_Remote", "ITM_Remote_Desc", r1, r2, Items.Football, 225, 60);

			ITM_Remote remo = ItemFunctions.CreateItemObject<ITM_Remote> ("ITM_Remote", false, true).GetComponent<ITM_Remote>();
			remo.audUse = ObjectFunctions.CreateSound ("Items/remote_use", path2, genericMeta, "Remote_Use");
			remo.audClick = ObjectFunctions.CreateSound ("Items/remote_error", path2, genericMeta, "Remote_NotHere");
			remoteITM.item = remo;


			Sprite b1 = ObjectFunctions.CreateSprite ("items/Butter_Pickup", path, 0.47f, 26f);
			Sprite b2 = ObjectFunctions.CreateSprite ("items/Butter_HUD", path, 0.5f, 36f);


			ItemObject butterITM = new ItemBuilder (base.Info)
				.SetEnum("Butter")
				.SetGeneratorCost (42)
				.SetNameAndDescription ("ITM_Butter", "ITM_Butter_Desc")
				.SetShopPrice (220)
				.SetSprites (b1, b2)
				.SetMeta (ItemFlags.None, new string[]{ "Butter" }).Build ();//ObjectCreators.CreateItemObject ("ITM_Butter", "ITM_Butter_Desc", b1, b2, Items.NanaPeel, 90, 42);

			ITM_Butter butITM = ItemFunctions.CreateItemObject<ITM_Butter> ("ITM_Butter", true, true).GetComponent<ITM_Butter>();

			butITM.gameObject.AddComponent<CapsuleCollider> ();
			CapsuleCollider sp = butITM.GetComponent<CapsuleCollider> ();
			sp.radius = 2f;
			sp.height = 1f;

			butITM.gameObject.AddComponent<SphereCollider> ();
			SphereCollider bcol = butITM.GetComponent<SphereCollider> ();
			bcol.isTrigger = true;
			bcol.radius = 2f;

			sp.gameObject.AddComponent<Rigidbody> ();
			sp.gameObject.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;

			butITM.gameObject.layer = 16;

			Entity refEntity = ObjectFunctions.FindResourceOfName<Entity> ("Gum");

			butITM.butterSprite = ObjectFunctions.CreateSprite ("ButterProjectile", path, 0.5f, 48f);
			butITM.entity = ObjectFunctions.CreateEntity (butITM.gameObject, refEntity.BaseHeight, true, sp, bcol, 
				butITM.gameObject.AddComponent<ActivityModifier> (), butITM.transform);//butterr.AddComponent<Entity> ();
			
			butITM.SetupVisuals ();
			butITM.audSplat = ObjectFunctions.FindResourceOfName<SoundObject> ("Ben_Splat");

			butITM.gameObject.ConvertToPrefab (true);

			butterITM.item = butITM;

			EnumExtensions.ExtendEnum<Items> ("Orange");

			Sprite o1 = ObjectFunctions.CreateSprite ("items/Orange_HUD", path, 0.47f, 22f);
			Sprite o2 = ObjectFunctions.CreateSprite ("items/Orange_Pickup", path, 0.5f, 32f);


			ItemObject orangeITM = new ItemBuilder (base.Info)
				.SetEnum("Orange")
				.SetGeneratorCost (45)
				.SetNameAndDescription ("ITM_Orange", "ITM_Orange_Desc")
				.SetShopPrice (350)
				.SetSprites (o1, o2)
				.SetMeta (ItemFlags.NoUses, new string[]{ "Orange" }).Build ();//ObjectCreators.CreateItemObject ("ITM_Orange", "ITM_Orange_Desc", o1, o2, EnumExtensions.GetFromExtendedName<Items>("Orange"), 75, 45);

			ItemObject giftboxItem = new ItemBuilder (base.Info).SetEnum ("Giftbox").SetGeneratorCost (20).SetItemComponent<ITM_Giftbox> ().SetNameAndDescription ("ITM_Giftbox", "ITM_Giftbox_Desc").SetShopPrice (100).
				SetSprites (ObjectFunctions.CreateSprite("items/Giftbox_Small", path, 0.5f, 16f), ObjectFunctions.CreateSprite("items/Giftbox_Large", path, 0.5f, 36f))
				.SetMeta (ItemFlags.None, new string[]{ "Giftbox" }).Build();
			string lootPath = Path.Combine(AssetLoader.GetModPath(this), "LootTabels");

			ItemObject sledItem = new ItemBuilder (base.Info).SetEnum ("Sled").SetGeneratorCost (60).SetItemComponent<ITM_Sled> ().SetNameAndDescription ("ITM_Sled", "ITM_Sled_Desc").SetShopPrice (450).
				SetSprites (ObjectFunctions.CreateSprite("items/Sled_HUD", path, 0.5f, 16f), ObjectFunctions.CreateSprite("items/Sled_Pickup", path, 0.5f, 32f)).SetMeta (ItemFlags.None, new string[]{ "Sled" }).Build();



			ItemObject soupItem = new ItemBuilder (base.Info)
				.SetEnum("WillySoup")
				.SetGeneratorCost (40)
				.SetNameAndDescription ("ITM_Soup", "ITM_Soup_Desc")
				.SetShopPrice (999)
				.SetSprites (ObjectFunctions.CreateSprite("items/Soup_Small", path, 0.5f, 16f), ObjectFunctions.CreateSprite("items/Soup_Large", path, 0.5f, 32f))
				.SetMeta (ItemFlags.None, new string[]{ "Soup" }).Build ();//ObjectCreators.CreateItemObject ("ITM_Remote", "ITM_Remote_Desc", r1, r2, Items.Football, 225, 60);

			ITM_Soup sowp = ItemFunctions.CreateItemObject<ITM_Soup> ("ITM_Soup", false, true).GetComponent<ITM_Soup>();

			soupItem.item = sowp;

			WeightedItemObject rdfsjkddas = new WeightedItemObject
			{
				selection = chheatITM,
				weight = 60
			};
			assetMan.Add<WeightedItemObject> ("Cheat Device", rdfsjkddas);
			WeightedItemObject percc = new WeightedItemObject
			{
				selection = percussionITM,
				weight = 60
			};
			assetMan.Add<WeightedItemObject> ("Percussion Hammer", percc);
			WeightedItemObject remmo = new WeightedItemObject
			{
				selection = remoteITM,
				weight = 80
			};
			assetMan.Add<WeightedItemObject> ("Remote Control", remmo);
			WeightedItemObject buter = new WeightedItemObject
			{
				selection = butterITM,
				weight = 120
			};
			assetMan.Add<WeightedItemObject> ("Butter Item", buter);


			WeightedItemObject orag = new WeightedItemObject
			{
				selection = orangeITM,
				weight = 100
			};
			assetMan.Add<WeightedItemObject> ("Ripe Orange", orag);

			WeightedItemObject gigiigigigigigig = new WeightedItemObject
			{
				selection = giftboxItem,
				weight = 100
			};
			assetMan.Add<WeightedItemObject> ("Mysterious Giftbox", gigiigigigigigig);

			WeightedItemObject sleeeeee = new WeightedItemObject
			{
				selection = sledItem,
				weight = 70
			};
			assetMan.Add<WeightedItemObject> ("Backup Sled", sleeeeee);


			WeightedItemObject storeSOup = new WeightedItemObject
			{
				selection = soupItem,
				weight = 50
			};
			assetMan.Add<WeightedItemObject> ("Willy's Soup", storeSOup);


			assetMan.Add<ItemObject> ("CheatingDevice", chheatITM);
			assetMan.Add<ItemObject> ("PercussionHammer", percussionITM);
			assetMan.Add<ItemObject> ("RemoteControl", remoteITM);
			assetMan.Add<ItemObject> ("Butter", butterITM);
			assetMan.Add<ItemObject> ("RipeOrange", orangeITM);
			assetMan.Add<ItemObject> ("MysteryGift", giftboxItem);
			assetMan.Add<ItemObject> ("BackupSled", sledItem);
			assetMan.Add<ItemObject> ("WillysSoup", soupItem);

			modContents.newItems.Add ("CheatingDevice", rdfsjkddas);
			modContents.newItems.Add ("PercussionHammer", percc);
			modContents.newItems.Add ("RemoteControl", remmo);
			modContents.newItems.Add ("ButteryButter", buter);
			modContents.newItems.Add ("RipeOrange", orag);
			modContents.newItems.Add ("MysteryGift", gigiigigigigigig);
			modContents.newItems.Add ("BackupSled", sleeeeee);
			modContents.newItems.Add ("WillysSoup", storeSOup);
		}

		public WeightedItemObject[] giftloottable;

		public void SetupRooms(string path, string path2, string path3)
		{

			StandardDoorMats testDoors = ObjectCreators.CreateDoorDataObject ("TestDoor", ObjectFunctions.FindResourceOfName<Texture2D> ("DoorTexture_Open"), ObjectFunctions.FindResourceOfName<Texture2D> ("DoorTexture_Closed"));
			assetMan.Add<StandardDoorMats> ("Test Doors", testDoors);
			StandardDoorMats vendingDoors = ObjectCreators.CreateDoorDataObject ("VendingDoor", AssetLoader.TextureFromFile (path + "/Tiles/Door_VendingOpen.png"), AssetLoader.TextureFromFile (path + "/Tiles/Door_VendingClosed.png"));
			assetMan.Add<StandardDoorMats> ("Vending Doors", vendingDoors);
			StandardDoorMats bandDoors = ObjectCreators.CreateDoorDataObject ("BandDoor", AssetLoader.TextureFromFile (path + "/Tiles/Door_BandOpen.png"), AssetLoader.TextureFromFile (path + "/Tiles/Door_BandClosed.png"));
			assetMan.Add<StandardDoorMats> ("Band Doors", bandDoors);
			StandardDoorMats partDoors = ObjectCreators.CreateDoorDataObject ("PartsDoor", AssetLoader.TextureFromFile (path + "/Tiles/Door_PartsOpen.png"), AssetLoader.TextureFromFile (path + "/Tiles/Door_PartsClosed.png"));
			assetMan.Add<StandardDoorMats> ("PaS Doors", partDoors);
			StandardDoorMats lockerDoors = ObjectCreators.CreateDoorDataObject ("LockerDoor", AssetLoader.TextureFromFile (path + "/Tiles/Door_LockerOpen.png"), AssetLoader.TextureFromFile (path + "/Tiles/Door_LockerClosed.png"));
			assetMan.Add<StandardDoorMats> ("Locker Doors", lockerDoors);
			StandardDoorMats artDoors = ObjectCreators.CreateDoorDataObject ("ArtDoor", AssetLoader.TextureFromFile (path + "/Tiles/Door_ArtOpen.png"), AssetLoader.TextureFromFile (path + "/Tiles/Door_ArtClosed.png"));
			assetMan.Add<StandardDoorMats> ("Art Doors", artDoors);
			StandardDoorMats trashDoors = ObjectCreators.CreateDoorDataObject ("TrashDoor", AssetLoader.TextureFromFile (path + "/Tiles/Door_TrashOpen.png"), AssetLoader.TextureFromFile (path + "/Tiles/Door_TrashClosed.png"));
			assetMan.Add<StandardDoorMats> ("Garbage Doors", trashDoors);


			MTM101BaldiDevAPI.roomAssetMeta.Get ("Room_ReflexOffice_0").value.color = new Color (0f, 0.5f, 1f);

			EnumExtensions.ExtendEnum<RoomCategory> ("Vending");
			EnumExtensions.ExtendEnum<RoomCategory> ("Music");
			EnumExtensions.ExtendEnum<RoomCategory> ("Trash");
			EnumExtensions.ExtendEnum<RoomCategory> ("Art");
			EnumExtensions.ExtendEnum<RoomCategory> ("Parts");
			EnumExtensions.ExtendEnum<RoomCategory> ("Locker");

			Transform light = MTM101BaldiDevAPI.roomAssetMeta.Get ("Room_ReflexOffice_0").value.lightPre;

			//Soda Room

			RoomAsset roomAsset = RoomParser.CreateRoomAsset (path3, "SmallRoom", "SodaRoom", "SodaMeta", light, EnumExtensions.GetFromExtendedName<RoomCategory> ("Vending")
				, new Texture2D[] {
					assetMan.Get<WeightedTexture2D> ("Floor_Metal").selection,
					assetMan.Get<WeightedTexture2D> ("Wall_BlueSaloon").selection,
					ObjectFunctions.FindResourceOfName<Texture2D> ("PlasticTable")
				}, vendingDoors, assetMan);


			roomAsset.basicObjects = getVendingData ();
			roomAsset.basicSwaps = getSwapsV (roomAsset.basicObjects);

			ItemData ifshufd = new ItemData ();
			ifshufd.item = ObjectFunctions.FindResourceOfName<ItemObject> ("Quarter");
			ifshufd.position = new Vector2 (15f, 10f);
			roomAsset.items.Add (ifshufd);
			assetMan.Add<RoomAsset> ("Vending_Room", roomAsset);


			RoomAsset roomAsset2 = RoomParser.CreateRoomAsset (path3, "MediumRoom", "SodaRoom", "MusicMeta", light, 
				EnumExtensions.GetFromExtendedName<RoomCategory> ("Music"), new Texture2D[] {
					ObjectFunctions.FindResourceOfName<Texture2D> ("ActualTileFloor"),
					ObjectFunctions.FindResourceOfName<Texture2D> ("SaloonWall"),
					ObjectFunctions.FindResourceOfName<Texture2D> ("PlasticTable")
				}, bandDoors, assetMan);


			Transform stage = ObjectFunctions.CreateBasicMesh (PrimitiveType.Cube, new Vector3 (30f, 3f, 5f), ObjectFunctions.FindResourceOfName<Material> ("WoodSimple"), new Vector3 (1.1f, 3f, 1.1f));
			Vector3 stagePos = new Vector3 (20f, 1f, 25f);

			stage.gameObject.ConvertToPrefab (false);

			Sprite pianoSprite = ObjectFunctions.CreateSprite("Decorative/Piano", path, 0.5f, 27f);

			Transform piano = new GameObject ("Piano").transform;
			DontDestroyOnLoad (piano);
			piano.gameObject.AddComponent<SpriteRenderer> ();
			piano.GetComponent<SpriteRenderer> ().sprite = pianoSprite;
			piano.gameObject.AddComponent<BoxCollider> ();
			piano.GetComponent<BoxCollider> ().size = new Vector3(10f, 10f, 0.1f);

			piano.gameObject.ConvertToPrefab (false);

			BasicObjectData stageData = new BasicObjectData ();
			stageData.position = stagePos;
			stageData.prefab = stage;

			BasicObjectData pianoData = new BasicObjectData ();
			pianoData.position = new Vector3(39.5f, 4f, 15f);
			pianoData.prefab = piano;
			pianoData.rotation = new Quaternion (0f, 0.7071068f, 0f, 0.7071068f);

			roomAsset2.basicObjects.Add (stageData);
			roomAsset2.basicObjects.Add (pianoData);

			assetMan.Add<RoomAsset> ("Music_Room", roomAsset2);

			Texture2D partsWall = AssetLoader.TextureFromFile (path + "/Tiles/Wall_Parts.png");
			Texture2D partsRoof = AssetLoader.TextureFromFile (path + "/Tiles/Ceiling_Parts.png");

			assetMan.Add<Texture2D> ("PartsWall", partsWall);
			assetMan.Add<Texture2D> ("PartsCeiling", partsRoof);

			RoomAsset partsRoom = RoomParser.CreateRoomAsset (path3, "MediumRoom", "PartsRoom", "PartsMeta", light, EnumExtensions.GetFromExtendedName<RoomCategory> ("Parts"), new Texture2D[] {
				ObjectFunctions.FindResourceOfName<Texture2D> ("Carpet"),
				partsWall,
				partsRoof
			}, partDoors, assetMan);

			//RoomFunction marine = ObjectFunctions.CreateRoomFunction<PartsRoomFunction> ("PartsFunction", true);
			//RoomFunctionContainer secks = ObjectFunctions.CreateRoomFunctionContainer("PartsFunctionContainer", true);

			//secks.AddFunction (marine);

			//partsRoom.roomFunctionContainer = secks;

			//marine.gameObject.ConvertToPrefab (true);
			//secks.gameObject.ConvertToPrefab (true);


			assetMan.Add<RoomAsset> ("Parts_Room", partsRoom);


			Material compactMateral = new Material (ObjectFunctions.FindResourceOfName<Material> ("WoodSimple").shader);
			compactMateral.mainTexture = AssetLoader.TextureFromFile (path + "/GarbageDisposial.png");

			Transform garbageCompact = ObjectFunctions.CreateBasicMesh (PrimitiveType.Cube, new Vector3 (9.5f, 10f, 8f), compactMateral, new Vector3 (1f, 1f, 1f));
			GarbageCompactor gcc = garbageCompact.gameObject.AddComponent<GarbageCompactor> ();
			gcc.useAud = ObjectFunctions.FindResourceOfName<SoundObject> ("ChipCrunch");
			gcc.grabAud = ObjectFunctions.CreateSound ("compactorBurp", path2, genericMeta, "COMPACT_Burp");
			garbageCompact.gameObject.ConvertToPrefab (false);
			assetMan.Add<Transform> ("Garbage Disposal", garbageCompact);

			Texture2D garbageWall = AssetLoader.TextureFromFile (path + "/Tiles/Wall_Trash.png");
			assetMan.Add<Texture2D> ("TrashWall", garbageWall);

			RoomAsset trashRoom = RoomParser.CreateRoomAsset (path3,
				"TrashRoomCells", "TrashRoom", "TrashMeta", light, EnumExtensions.GetFromExtendedName<RoomCategory> ("Trash"),
				new Texture2D[] {
					ObjectFunctions.FindResourceOfName<Texture2D> ("Carpet"),
					garbageWall,
					partsRoof
				}, trashDoors, assetMan);

			assetMan.Add<RoomAsset> ("Trash_Disposal", trashRoom);

			RoomAsset classref = ObjectFunctions.FindResourceOfName<RoomAsset> ("Room_Class_MathMachine_4");

			RoomAsset artRoom = RoomParser.CreateRoomAsset (path3, "ArtClass", "ArtDecor", "ArtMeta", light,
				EnumExtensions.GetFromExtendedName<RoomCategory> ("Art"), new Texture2D[] {
					AssetLoader.TextureFromFile (path + "/Tiles/Floor_Art.png"),
					assetMan.Get <WeightedTexture2D> ("Wall_Sandpaper").selection,
					AssetLoader.TextureFromFile (path + "/Tiles/Ceiling_Parts.png")
				}, artDoors, assetMan);
			artRoom.type = RoomType.Room;
			artRoom.roomFunctionContainer = classref.roomFunctionContainer;
			assetMan.Add<RoomAsset> ("Art_Room", artRoom);

			RoomAsset lockerRoom = RoomParser.CreateRoomAsset (path3, "LockerRoom_Tiles", "LockerRoom", "LockerMeta",
				light, EnumExtensions.GetFromExtendedName<RoomCategory> ("Locker"), new Texture2D[] {
					assetMan.Get<WeightedTexture2D> ("Floor_Wood").selection,
					assetMan.Get<WeightedTexture2D> ("Wall_Bricks").selection,
					ObjectFunctions.FindResourceOfName<Texture2D> ("PlasticTable")
				}, lockerDoors, assetMan);
			lockerRoom.basicSwaps = getSwapsL (lockerRoom.basicObjects);

			assetMan.Add<RoomAsset> ("Locker_Room", lockerRoom);


			RoomAsset debugRoom = RoomParser.CreateRoomAsset (path3,
				"PlacefaceRoom",
				"SodaRoom",
				"RGBmeta", light, RoomCategory.Test, new Texture2D[] {
					AssetLoader.TextureFromFile (path + "/Tiles/GridRed.png"),
					AssetLoader.TextureFromFile (path + "/Tiles/GridGreen.png"),
					AssetLoader.TextureFromFile (path + "/Tiles/GridBlue.png")
				}, testDoors, assetMan);
			assetMan.Add<RoomAsset> ("Debug_Room", lockerRoom);


			modContents.newRooms.Add ("VendingRoom", new WeightedRoomAsset {
				selection = roomAsset,
				weight = 10
			});
			modContents.newRooms.Add ("TrashRoom", new WeightedRoomAsset {
				selection = trashRoom,
				weight = 80
			});
			modContents.newRooms.Add ("LockerRoom", new WeightedRoomAsset {
				selection = lockerRoom,
				weight = 100
			});
			modContents.newRooms.Add ("PartsService", new WeightedRoomAsset {
				selection = partsRoom,
				weight = 50
			});

			//Character-Specific Rooms (all weights are 100)
			modContents.newRooms.Add ("MusicRoom", new WeightedRoomAsset {
				selection = roomAsset2,
				weight = 100
			});

			modContents.newRooms.Add ("ArtClass", new WeightedRoomAsset {
				selection = artRoom,
				weight = 100
			});

			modContents.newRooms.Add ("RGBRoom", new WeightedRoomAsset {
				selection = debugRoom,
				weight = 100
			});
		}

		List<string> earlyGameNames = new List<string>()
		{
			"F1",
			"END",
			"NSM"
		};
		List<string> midGameNames = new List<string>()
		{
			"F2",
			"END",
			"NSM"
		};
		List<string> lateGameNames = new List<string>()
		{
			"F3",
			"END",
			"NSM"
		};


		void BenerationFuck(string floorName, int floorNumber, CustomLevelObject floorObject)
		{
			List<WeightedRoomAsset> myRooms = new List<WeightedRoomAsset> ();
			List<WeightedItemObject> myItems = new List<WeightedItemObject> ();
			List<WeightedItemObject> myItemsStoreExc = new List<WeightedItemObject> ();

			List<WeightedObjectBuilder> myStructures = new List<WeightedObjectBuilder> ();

			List<WeightedRoomAsset> myOffices = new List<WeightedRoomAsset> ();

			WeightedTransform[] refLight = floorObject.roomGroup [0].light;

			WeightedTexture2D[] placeLaLoder = new WeightedTexture2D[] {
				new WeightedTexture2D {
					selection = assetMan.Get<Texture2D> ("PartsWall"),
					weight = 100
				}
			};

			RoomGroup garbageGroup = new RoomGroup (); 
			garbageGroup.stickToHallChance = 1f;
			garbageGroup.minRooms = 1;
			garbageGroup.maxRooms = 2;
			garbageGroup.potentialRooms = new WeightedRoomAsset[] {
				modContents.newRooms ["TrashRoom"]
			};
			garbageGroup.light = refLight;
			garbageGroup.name = "Trash Rooms";
			garbageGroup.ceilingTexture = placeLaLoder;
			garbageGroup.floorTexture = placeLaLoder;
			garbageGroup.wallTexture = placeLaLoder;

			RoomGroup lockerGroup = new RoomGroup (); 
			lockerGroup.stickToHallChance = 1f;
			lockerGroup.minRooms = 1;
			lockerGroup.maxRooms = 2;
			lockerGroup.potentialRooms = new WeightedRoomAsset[] {
				modContents.newRooms ["LockerRoom"]
			};
			lockerGroup.light = refLight;
			lockerGroup.name = "Locker Rooms";
			lockerGroup.ceilingTexture = placeLaLoder;
			lockerGroup.floorTexture = placeLaLoder;
			lockerGroup.wallTexture = placeLaLoder;

			RoomGroup partsGroup = new RoomGroup ();
			partsGroup.stickToHallChance = 1f;
			partsGroup.minRooms = 1;
			partsGroup.maxRooms = 2;
			partsGroup.potentialRooms = new WeightedRoomAsset[] {
				modContents.newRooms ["PartsService"]
			};
			partsGroup.light = refLight;
			partsGroup.name = "Parts and Service";
			partsGroup.ceilingTexture = placeLaLoder;
			partsGroup.floorTexture = placeLaLoder;
			partsGroup.wallTexture = placeLaLoder;
			/*stickToHallChance = 1f,
				minRooms = 1,PartsService
				maxRooms = 2,
				potentialRooms = new WeightedRoomAsset[]
				{
					modContents.newRooms["PartsService"]
				},
				light = refLight,
				name = "Parts and Service"
			};*/

			Debug.Log (partsGroup.potentialRooms);

			List<RoomGroup> groups = new List<RoomGroup> ();


			if (earlyGameNames.Contains (floorName)) {
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Carmella"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Ice Sled"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Billbert"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Kim"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Fritz"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Zarkstur"]);

				groups.Add (garbageGroup);

				myItems.Add (modContents.newItems ["ButteryButter"]);
				myItems.Add (modContents.newItems ["MysteryGift"]);

				myStructures.Add (newStructures ["LaserDoor"]);

				floorObject.randomEvents.Add (modContents.newEvents ["CloneEvent"]);
			

				floorObject.maxExtraRooms += 1;
				floorObject.extraStickToHallChance = 1f;
				floorObject.additionalNPCs++;
			}

			if (midGameNames.Contains (floorName)) {
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Rhythm"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Coloury"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Klutz"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Wildcard"]);
				floorObject.randomEvents.Add (modContents.newEvents ["MathEvent"]);
				floorObject.randomEvents.Add (modContents.newEvents ["DoorEvent"]);

				groups.Add (lockerGroup);
				groups.Add (partsGroup);

				myItems.Add (modContents.newItems ["CheatingDevice"]);
				myItems.Add (modContents.newItems ["PercussionHammer"]);
				myItems.Add (modContents.newItems ["RipeOrange"]);
				myItems.Add (modContents.newItems ["BackupSled"]);


				myItemsStoreExc.Add (modContents.newItems ["WillysSoup"]);

				floorObject.additionalNPCs++;
				floorObject.posterChance += 1f;
				myStructures.Add (newStructures ["Fan"]);
				myOffices.Add (new WeightedRoomAsset{
					selection = assetMan.Get<RoomAsset> ("Office_Custom"),
					weight = 75
				});
				floorObject.maxExtraRooms += 1;
			}
			if (lateGameNames.Contains (floorName)) {
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Skeleton"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Placeface"]);
				floorObject.potentialNPCs.Add (modContents.newNpcs ["Pencir"]);
				floorObject.randomEvents.Add (modContents.newEvents ["ItemEvent"]);
				myRooms.Add (modContents.newRooms ["VendingRoom"]);
				myItems.Add (modContents.newItems ["RemoteControl"]);
				floorObject.additionalNPCs++;
				myOffices.Add (new WeightedRoomAsset{
					selection = assetMan.Get<RoomAsset> ("Office_Custom2"),
					weight = 100
				});
				floorObject.maxExtraRooms += 2;
				floorObject.posterChance += 2f;
			}
			if (floorName == "END") {
				floorObject.posterChance += 2f;
			}

			floorObject.specialHallBuilders = ObjectFunctions.AddToArray<WeightedObjectBuilder> (myStructures, floorObject.specialHallBuilders);


			floorObject.potentialOffices = ObjectFunctions.AddToArray<WeightedRoomAsset> (myOffices, floorObject.potentialOffices);

			floorObject.potentialItems = ObjectFunctions.AddToArray<WeightedItemObject> (myItems, floorObject.potentialItems);

			myItemsStoreExc.AddRange (myItems);

			floorObject.shopItems = ObjectFunctions.AddToArray<WeightedItemObject> (myItemsStoreExc, floorObject.shopItems);


			floorObject.posters = ObjectFunctions.AddToArray<WeightedPosterObject> (newPosters, floorObject.posters);


			floorObject.roomGroup = ObjectFunctions.AddToArray<RoomGroup>(groups, floorObject.roomGroup);

			floorObject.hallFloorTexs = getNewTex (additionalFloors, floorObject.hallFloorTexs);
			floorObject.hallWallTexs = getNewTex (additionalWALLS, floorObject.hallWallTexs);

			floorObject.MarkAsNeverUnload();
		}

		public static SceneObject challengeScene;

		WeightedTexture2D[] getNewTex(List<WeightedTexture2D> gaye, WeightedTexture2D[] og)
		{
			
			List<WeightedTexture2D> news = new List<WeightedTexture2D> ();

			for (int i = 0; i < og.Length; i++) {
				news.Add (og [i]);
			}
			for (int j = 0; j < gaye.Count; j++) {
				news.Add (gaye [j]);
			}
			return news.ToArray ();
		}
	

	
		public WeightedTexture2D GenerateFloorTex(string name, string path)
		{
			Texture2D tex = AssetLoader.TextureFromFile (path + "/Tiles/Floor_" + name + ".png");
			WeightedTexture2D output = new WeightedTexture2D{
				selection = tex,
				weight = 25
			};
			return output;
		}
		public WeightedTexture2D GenerateWallTex(string name, string path)
		{
			Texture2D tex =AssetLoader.TextureFromFile (path + "/Tiles/Wall_" + name + ".png");
			WeightedTexture2D output = new WeightedTexture2D{
				selection = tex,
				weight = 25
			};
			return output;
		}

	}



	[HarmonyPatch(typeof(MainMenu), "Start")]
	public class TitlePatch //hehe get cucked pixedude
	{
		// Token: 0x06000055 RID: 85 RVA: 0x00005110 File Offset: 0x00003310
		private static void Prefix(MainMenu __instance)
		{
			Sprite referenceSprite = ObjectFunctions.FindResourceOfName<Sprite> ("TempMenu_Low");
			//Texture2D titleLol = AssetLoader.TextureFromFile (string.Concat (Path.Combine (AssetLoader.GetModPath (BasePlugin.plugin), "Textures"), "/SugakuTitle.png"));


			//Sprite sugakuMenu =  Sprite.Create(titleLol, referenceSprite.rect, referenceSprite.pivot, referenceSprite.pixelsPerUnit);
			Sprite sugakuMenu = ObjectFunctions.CreateSprite("SugakuTitle", Path.Combine (AssetLoader.GetModPath (BasePlugin.plugin), "Textures"), referenceSprite.pivot.y, referenceSprite.pixelsPerUnit);
			Sprite referenceSprite2 = ObjectFunctions.FindResourceOfName<Sprite> ("About_Unlit");


			__instance.gameObject.transform.Find("Image").GetComponent<Image>().sprite = sugakuMenu;

			__instance.gameObject.transform.Find ("Version").GetComponent<TMPro.TMP_Text> ().text = "V" + BasePlugin.plugin.Info.Metadata.Version.ToString ();
		

		}
	}

	public class GUITools
	{
		public static Canvas CreateCanvas(string name = "New Canvas", float width = 640f, float height = 360f)
		{
			
			Canvas component = new GameObject(name, new Type[]
				{
					typeof(Canvas),
					typeof(CanvasScaler),
					typeof(GraphicRaycaster)
				}).GetComponent<Canvas>();
			component.renderMode = RenderMode.ScreenSpaceOverlay;
			component.gameObject.layer = 5;

			CanvasScaler scaler = component.gameObject.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(width, height);



			return component;
		}

		public static RawImage CreateImage(string name, string fileName, Vector2 position, Vector2 scale, int layer, Transform parent)
		{
			RawImage component = new GameObject (name, new System.Type[] {
				typeof(RawImage)
			}).GetComponent<RawImage> ();
			component.gameObject.SetActive (true);
			component.transform.SetParent (parent);
			component.texture = BasePlugin.plugin.assetMan.Get<Texture2D> (fileName);
			component.rectTransform.anchoredPosition = position;
			component.rectTransform.sizeDelta = scale;
			component.gameObject.layer = layer;
			return component;
		}
	}


}
