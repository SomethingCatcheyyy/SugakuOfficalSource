using System.Collections;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using CarmellaBaldiExtension;
using Gemu;

public class MenuPatch : MonoBehaviour
{
	StandardMenuButton menuButton;
	// Use this for initialization
	void Start ()
	{
		GameObject refButton = GameObject.Find ("Endless");
		GameObject challengeButton = GameObject.Instantiate( refButton, refButton.transform.parent);
		if(GameObject.Find ("MainContinue") != null)
		{
			GameObject.Find ("MainContinue").transform.localPosition = new Vector3(-120f, 112f, 0f);
		}
		if(GameObject.Find ("MainNew") != null)
		{
			GameObject.Find ("MainNew").transform.localPosition = new Vector3(-120f, 112f, 0f);
		}
		challengeButton.transform.localPosition = new Vector3 (120f, 112f, 0f);
		menuButton = challengeButton.GetComponent<StandardMenuButton> ();

		//menuButton.text.text = "My Challenge (NEW)";
		//menuButton.GetComponent<TextLocalizer>().GetLocalizedText("CHL_Name");

		menuButton.OnPress = new UnityEvent ();
		menuButton.OnPress.AddListener (delegate() {
			GameLoader[] lol = Resources.FindObjectsOfTypeAll<GameLoader> ();
			GameLoader gameLoader = lol[0];
			gameLoader.gameObject.SetActive (true);
			gameLoader.CheckSeed();
			gameLoader.Initialize(0);
			gameLoader.SetMode(0);
			Singleton<CoreGameManager>.Instance.AddPoints(1300, 0, false);
			Singleton<CursorManager>.Instance.LockCursor();

			ElevatorScreen component5 = ObjectFunctions.findObjectInScene("ElevatorScreen").GetComponent<ElevatorScreen>();
			gameLoader.AssignElevatorScreen(component5);
			component5.gameObject.SetActive(true);
			gameLoader.LoadLevel(BasePlugin.challengeScene);
			component5.Initialize();
			component5.QueueShop();
			gameLoader.SetSave(false);
		});
		menuButton.OnHighlight = new UnityEvent ();
		Transform modeText = base.gameObject.transform.Find("ModeText");
		menuButton.OnHighlight.AddListener(delegate()
			{
				modeText.gameObject.GetComponent<TextLocalizer>().GetLocalizedText("CHL_Desc");
			});
		CursorController.Instance.transform.SetAsLastSibling();
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		menuButton.GetComponent<TextLocalizer>().GetLocalizedText("CHL_Name");
	}
}

