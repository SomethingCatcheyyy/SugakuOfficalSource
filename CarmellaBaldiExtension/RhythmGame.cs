using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class RhythmGame : MonoBehaviour
{
	public TMP_Text infomatrion;
	// Use this for initialization
	void Start ()
	{
		infomatrion.text = "Placeholder";
		infomatrion.rectTransform.sizeDelta = new Vector2 (300f, 400f);
		infomatrion.fontSize = 48f;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}

