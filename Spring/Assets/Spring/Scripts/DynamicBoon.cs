using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 여기서 참고함
// https://forum.unity.com/threads/rubber-simulation-script.45148/

public class DynamicBoon : MonoBehaviour 
{
	private Vector3 targetPos = new Vector3 ();
	//private Vector3 targetPos_start = new Vector3 ();
	private Vector3 dynamicPos = new Vector3 ();
	//private Vector3 dynamicPos_start = new Vector3 ();

	[Header ("방향 설정")]
	public Vector3 boneAxis = Vector3.forward;
	public Vector3 boneUpAxis = Vector3.up;
	public Vector3 boneOffset = Vector3.zero;

	
	[Header ("물리 설정")]
	public float targetDistance = 2.0f;
	public float stiffness = 0.1f;
	public float mass = 1f;
	[Range (0, 1)]
	public float damping = 0.7f;
	[Range (0.001f, 2)]
	public float gravity = 0.75f;

	private Vector3 startLocalPosition;
	private Quaternion startLocalRotation;
	private Vector3 startLocalScale;

	private Vector3 force = Vector3.zero;
	private Vector3 acc = Vector3.zero;
	private Vector3 vel = Vector3.zero;

	[Header ("스케일 설정")]

	public bool squashNStretch = true;
	public float sideStretch = 0.15f;
	public float frontStretch = 0.2f;

	protected Quaternion curtRot;
	protected Vector3 forwardVec;
	protected Vector3 upVec;

	protected bool isNorFirstFrame = false;

	void LateUpdate ()
	{
		if (isNorFirstFrame == false)
		{
			dynamicPos = transform.position;
			targetPos = transform.position;

			isNorFirstFrame = true;
			return;
		}

		curtRot = transform.localRotation;
		transform.localRotation = startLocalRotation;

		forwardVec = gameObject.transform.TransformDirection (boneAxis * targetDistance);
		upVec = gameObject.transform.TransformDirection (boneUpAxis);
		targetPos = gameObject.transform.position + gameObject.transform.TransformDirection (boneAxis * targetDistance);

		transform.localRotation = curtRot;

		force = (targetPos - dynamicPos) * stiffness;
		force.y -= gravity / 10;

		if (mass != 0)
			acc = force / mass;
			
		vel += acc * (1 - damping);

		dynamicPos += vel + force;


		transform.LookAt (dynamicPos, upVec);
		transform.rotation *= Quaternion.Euler (boneOffset);

		if (squashNStretch)
		{
			Vector3 _dynamicVec = dynamicPos - targetPos;

			float stretchMag = _dynamicVec.magnitude;

			Vector3 stretch;

			if (boneAxis.x == 0)
				stretch.x = 1 + (-stretchMag * sideStretch);
			else
				stretch.x = 1 + (stretchMag * frontStretch);

			if (boneAxis.y == 0)
				stretch.y = 1 + (-stretchMag * sideStretch);
			else
				stretch.y = 1 + (stretchMag * frontStretch);

			if (boneAxis.z == 0)
				stretch.z = 1 + (-stretchMag * sideStretch);
			else
				stretch.z = 1 + (stretchMag * frontStretch);

			transform.localScale = stretch;
		}
	}
}
