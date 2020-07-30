using Kai.SDK;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class HandController : MonoBehaviour
{
    public float animationSpeed = 5f;

	public GameObject cubePrefab;
	public Transform cubeInstantiationPosition;

	public float cubeSpawnForce = 100f;
	

	private float bulletCreationTimeout = 0.1f;

	private string gameState = "none";
	private Animator animator;
	private GameObject newObject;
	private GameObject heldObject;
	private RaycastHit capturingHit;
	private RaycastHit destroyingHit;
	
	// Start is called before the first frame update
	void Awake()
	{
		KaiSDK.Initialise("superhero-playground", "qwerty");
		KaiSDK.DefaultKai.FingerShortcut += OnDefaultKaiFingerShortcutData;
		KaiSDK.DefaultKai.QuaternionData += OnDefaultKaiQuaternionData;
		KaiSDK.DefaultKai.SetCapabilities(KaiCapabilities.FingerShortcutData | KaiCapabilities.QuaternionData);
		KaiSDK.SetHand(KaiSDK.DefaultKai, Hand.Right);
		KaiSDK.Connect();
	}

	void Start()
	{
		animator = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update()
	{
		if(gameState == "create")
		{
			newObject = Instantiate(cubePrefab, cubeInstantiationPosition.position, Quaternion.Euler(0, 0, 0));
			newObject.transform.localScale = new Vector3();
			newObject.GetComponent<BoxCollider>().enabled = false;
			gameState = "inflating";
		}
		else if(gameState == "inflating")
		{
			var scaleAddition = Time.deltaTime * 2f;
			if(newObject.transform.localScale.x < 5f)
				newObject.transform.localScale += new Vector3(scaleAddition, scaleAddition, scaleAddition);
			newObject.transform.position = cubeInstantiationPosition.position;
			newObject.transform.position -= newObject.transform.right * 3f;
			newObject.transform.rotation = cubeInstantiationPosition.rotation;
		}
		else if(gameState == "capturing" || gameState == "capture-aimed")
		{
			if(Physics.Raycast(transform.position, -transform.right, out capturingHit, Mathf.Infinity, LayerMask.GetMask("Cubes")))
			{
				Debug.DrawRay(transform.position, -transform.right * capturingHit.distance, Color.green);
				heldObject = capturingHit.transform.gameObject;
				heldObject.GetComponent<MeshRenderer>().material.color = Color.green;
				gameState = "capture-aimed";
			}
			else
			{
				Debug.DrawRay(transform.position, -transform.right * 1000, Color.white);
				if(heldObject != null)
				{
					heldObject.GetComponent<MeshRenderer>().material.color = Color.white;
					heldObject = null;
				}
				gameState = "capturing";
			}
		}
		else if(gameState == "held")
		{
			heldObject.transform.position = cubeInstantiationPosition.position;
			heldObject.transform.position -= heldObject.transform.up * 10f;
			heldObject.transform.rotation = cubeInstantiationPosition.rotation;
		}
		else if(gameState == "shooting")
		{
			bulletCreationTimeout -= Time.deltaTime;
			if(bulletCreationTimeout < 0)
			{
				// create bullet and reset timer
				bulletCreationTimeout = 0.1f;
				//Instantiate(bulletPrefab, cubeInstantiationPosition.position, Quaternion.Euler(0, 0, 0));
			}
		}
		else if(gameState == "destroying" || gameState == "destroy-aimed")
		{
			if(Physics.Raycast(transform.position, -transform.right, out destroyingHit, Mathf.Infinity, LayerMask.GetMask("Cubes")))
			{
				Debug.DrawRay(transform.position, -transform.right * destroyingHit.distance, Color.red);
				heldObject = destroyingHit.transform.gameObject;
				heldObject.GetComponent<MeshRenderer>().material.color = Color.red;
				gameState = "destroy-aimed";
			}
			else
			{
				Debug.DrawRay(transform.position, -transform.right * 1000, Color.white);
				if(heldObject != null)
				{
					heldObject.GetComponent<MeshRenderer>().material.color = Color.white;
					heldObject = null;
				}
				gameState = "destroying";
			}
		}
	}

	private void OnDefaultKaiFingerShortcutData(object sender, FingerShortcutEventArgs args)
	{
		AnimateFingers(sender, args);
	}

	private void OnDefaultKaiQuaternionData(object sender, QuaternionEventArgs args)
	{
		var quaternion = new Quaternion();
        quaternion.w = args.Quaternion.w;
        quaternion.x = args.Quaternion.x;
        quaternion.y = -args.Quaternion.z;
        quaternion.z = args.Quaternion.y;
		transform.rotation = quaternion;
		transform.rotation *= Quaternion.Euler(180, 0, 0);
	}

	private void AnimateFingers(object sender, FingerShortcutEventArgs args)
	{
		// Play animation for each finger
		animator.SetBool("Index Finger", args.IndexFinger);
		animator.SetBool("Middle Finger", args.MiddleFinger);
		animator.SetBool("Ring Finger", args.RingFinger);
		animator.SetBool("Little Finger", args.LittleFinger);
		
		if(gameState == "none")
		{
			if (!args.IndexFinger && !args.MiddleFinger && args.RingFinger && args.LittleFinger)
			{
				gameState = "create";
			}
			else if(!args.IndexFinger && args.MiddleFinger && args.RingFinger && !args.LittleFinger)
			{
				gameState = "capturing";
			}
			else if(!args.IndexFinger && args.MiddleFinger && args.RingFinger && args.LittleFinger)
			{
				gameState = "destroying";
			}
		}

		if(gameState == "inflating")
		{
			if(!args.IndexFinger && args.MiddleFinger && args.RingFinger && args.LittleFinger)
			{
				newObject.GetComponent<BoxCollider>().enabled = true;
				newObject.GetComponent<Rigidbody>().AddForce(-newObject.transform.right * cubeSpawnForce);
				gameState = "destroying";
			}
			else if (args.IndexFinger || args.MiddleFinger || !args.RingFinger || !args.LittleFinger)
			{
				newObject.GetComponent<BoxCollider>().enabled = true;
				newObject.GetComponent<Rigidbody>().AddForce(-newObject.transform.right * cubeSpawnForce);
				gameState = "none";
			}
		}

		if(gameState == "capturing")
		{
			if(!args.IndexFinger && args.MiddleFinger && args.RingFinger && args.LittleFinger)
			{
				gameState = "destroying";
			}
			else if(args.IndexFinger || !args.MiddleFinger || !args.RingFinger || args.LittleFinger)
			{
				gameState = "none";
			}
		}

		if(gameState == "capture-aimed")
		{
			if(!args.IndexFinger && args.MiddleFinger && args.RingFinger && args.LittleFinger)
			{
				gameState = "destroying";
			}
			else if(args.IndexFinger && args.MiddleFinger && args.RingFinger && args.LittleFinger)
			{
				heldObject = capturingHit.transform.gameObject;
				heldObject.GetComponent<MeshRenderer>().material.color = Color.white;
				heldObject.GetComponent<Rigidbody>().velocity = new Vector3();
				heldObject.GetComponent<BoxCollider>().enabled = false;
				gameState = "held";
			}
		}

		if(gameState == "held")
		{
			if(!args.IndexFinger || !args.MiddleFinger || !args.RingFinger || !args.LittleFinger)
			{
				heldObject.GetComponent<BoxCollider>().enabled = true;
				heldObject.GetComponent<Rigidbody>().AddForce(-heldObject.transform.right * cubeSpawnForce);
				gameState = "none";
			}
		}

		if(gameState == "destroying")
		{
			if (!args.IndexFinger && !args.MiddleFinger && args.RingFinger && args.LittleFinger)
			{
				gameState = "create";
			}
			else if(!args.IndexFinger && args.MiddleFinger && args.RingFinger && !args.LittleFinger)
			{
				gameState = "capturing";
			}
			else if(args.IndexFinger || !args.MiddleFinger || !args.RingFinger || !args.LittleFinger)
			{
				gameState = "none";
			}
		}

		if(gameState == "destroy-aimed")
		{
			if(args.IndexFinger)
			{
				var destroyObject = destroyingHit.transform.gameObject;
				destroyObject.GetComponent<CubeDestroyer>().Explode();

				gameState = "none";
			}
		}
	}
}
