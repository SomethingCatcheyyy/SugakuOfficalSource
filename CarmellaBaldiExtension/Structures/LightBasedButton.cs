using UnityEngine;
using System.Collections;

public class LightBasedButton : MonoBehaviour, IButtonReceiver
{
	public EnvironmentController ec;
	public void ButtonPressed(bool val)
	{
		ec.SetAllLights (val);
	}
}

