using UnityEngine;
using System.Collections;

public class FOVManager : MonoBehaviour
{

	public float targetFOV = 60f;
	public float baseFOV = 60f;

	float curFOV = 60f;

	public static FOVManager instance;

	// Use this for initialization
	void Start ()
	{
		instance = this;
		targetFOV = 60f;
	}
	
	// Update is called once per frame
	public void UpdateCams (Camera[] cams)
	{
		curFOV = Mathf.Lerp (curFOV, targetFOV, 0.06f);
		foreach (Camera cam in cams) {
			cam.fieldOfView = curFOV;
		}
	}

	public void SetFov(float fov)
	{
		targetFOV = fov;
	}
}

