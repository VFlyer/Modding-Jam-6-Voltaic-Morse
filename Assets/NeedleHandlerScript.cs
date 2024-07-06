using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleHandlerScript : MonoBehaviour {

	public Transform affectedTransform;
	public Vector3 minRot, maxRot;
	[Range(0f, 1f)]
	public float nextProg = 0f;
	public float speed = 1f;
	public float progress { get { return curProg; }  }
	float curProg = 0;
	// Update is called once per frame
	void Update () {
		if (curProg < nextProg)
		{
			curProg += Time.deltaTime * speed;
			if (curProg > nextProg)
				curProg = nextProg;
		}
		else if (curProg > nextProg)
		{
			curProg -= Time.deltaTime * speed;
			if (curProg < nextProg)
				curProg = nextProg;
		}
		if (affectedTransform != null)
			affectedTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(minRot), Quaternion.Euler(maxRot), curProg);
	}
}
