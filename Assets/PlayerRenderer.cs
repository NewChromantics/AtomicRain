using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour {

	public RectTransform	Cannon;

	public void SetAngle(float Angle)
	{
		var Rot = Cannon.localRotation;
		Rot = Quaternion.Euler( 0, 0, Angle );
		Cannon.localRotation = Rot;
	}

}
