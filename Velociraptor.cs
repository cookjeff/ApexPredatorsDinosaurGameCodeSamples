using UnityEngine;
using System.Collections;

public class Velociraptor : Dinosaur {

	
	public static ArrayList pack = new ArrayList(); 
	private Vector3 attackVector;

	// Use this for initialization
	public void Start () {
		base.Start();
		pack.Add (this);
		health = 36;
		//dieSound = Resources.Load("
		state = FSMState.FSM_PASSTIME;
		focusCreature = null;
		path = null;
		predator = true;
		//pathFinder = new astarpathfind();
		GetComponent<Rigidbody>().freezeRotation = true;
		attackDistance = 6.3f;
		walkSpeed = 5.0f;
		runSpeed = 13.67f;
		attackAnimation = "bite";
		panicPoint = 0f;
		rallyDistance = 100f;
		movesWhileAttacking = true;
		//rotateSpeed = 0.1f;
	}
	
	public override ArrayList getPack ()
	{
		return pack;
	}
	
	public override void justGotHit ()
	{
		focusCreature = GameObject.FindGameObjectWithTag("Player");
		state = FSMState.FSM_CHASE;

		if (!rallied & !loneWolf) {
			rallyHelp(focusCreature,100);
			rallied = true;
		}
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		base.FixedUpdate ();
	}
}
