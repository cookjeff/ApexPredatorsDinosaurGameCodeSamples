using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Dinosaur : MonoBehaviour {
	Vector3 eyes;
	public float health;
	// 0-maxhealth, sets the point at which they run away; a rex might
	// be 0 and never run away till he dies, whereas a parasaur might
	// run as soon as he or she is hit and warn the others 
	protected float panicPoint;
	public bool inflictsDamage = true;
	public DamageActuator damageAct;
	public float viewDistance = 120;
	public float rallyDistance;
	public float viewAngle = 45;
	private float lookTimer;
	private float lookDelay = 0.1f;
	protected bool newPath = false;
	protected bool bDead = false;
	protected bool rallied = false;
	protected bool backwardsModel = false;
	protected bool backwardsFixed = false;
	protected bool bVisible = false;
	protected bool movesWhileAttacking = false;
	public bool loneWolf = false;
	protected bool predator = false;
	protected bool wonkedleft = false;
	// the chance that a dino goes for fight instead of flight when they 
	// reach their panic point
	protected float berserkerChance;
	protected float attackDistance;
	protected GameObject focusCreature;
//	protected Animation animation;
//	protected Rigidbody rigidbody;
	public AudioClip die;
	public AudioClip warn;
	public AudioClip breathe;
	public AudioClip growl;
	public AudioClip footstepWalk;
	public AudioClip footstepRun;
 	public List<astarpathfind.Node> path;
	protected float rotateSpeed = 1f;
	protected float criticalDistance = 7f;
	protected int pathLimit = 250;
 	Vector3 objective;
	int pathIndex;
	protected float currTaskTimer = -1;
	protected float mustWalkTimer = 0;
	protected float timeMustWalkAfterHit = 0.17f;
	protected float walkSpeed;
	protected float runSpeed;
	protected string attackAnimation;
	protected string runAnimation = "run";
	protected string walkAnimation = "walk";
	protected Quaternion targetAngle;
	protected Quaternion currAngle;
	protected float timeSinceAStar = 0f;
	protected float aStarCooldown = 1f;
	protected SkinnedMeshRenderer smr;
	public enum FSMState { FSM_DEAD, FSM_FLEE, FSM_ATTACK, FSM_FEED, FSM_CHASE, FSM_HUNT, FSM_COORDINATE, FSM_PASSTIME };
	protected GameObject thePlayer;
	public float updateDistanceThreshold = 333;
	private static GameObject[] eyeballs;
	public GameObject eyeball;
	public static List<Dinosaur> allDinos;

	public FSMState state;
	// Use this for initialization
	//protected abstract void Start ();
	//public astarpathfind pathFinder;

	public abstract ArrayList getPack();
	
    public static void ResetDinoObjectivesOnRespawn()
    {
        foreach (Dinosaur d in allDinos)
        {
            //if (d.state == FSMState.FSM_ATTACK || d.state == FSMState.FSM_CHASE)
            //{
                d.state = FSMState.FSM_PASSTIME;
            //}
            
        }
    }

	public void Awake() {
		if (allDinos==null)
			allDinos = new List<Dinosaur>();

		allDinos.Add (this);
		//pathFinder = new astarpathfind();
		//Physics.IgnoreLayerCollision(8,9,true);
		//Physics.IgnoreCollision(GetComponent<Collider>(),Terrain.activeTerrain.GetComponent<Collider>());
		//GetComponent<Rigidbody>().isKinematic = true;
		smr = GetComponentInChildren<SkinnedMeshRenderer> ();
		thePlayer = GameObject.FindGameObjectWithTag ("Player");
	}

	public Vector3 planarPoint(Vector3 p) {
		return new Vector3 (p.x, 0f, p.z);
		}

	public void OnBecameVisible() {
		//print ("just became visible");
		bVisible = true;
	}

	public void OnBecameInvisible() {
		bVisible = false;
	}

	float planarDistance(Vector3 a, Vector3 b) {
		return (((new Vector2 (a.x, a.z)) - (new Vector2 (b.x, b.z))).magnitude);
	}

	public Vector3 normVector(Vector3 source) {
		source.Normalize();
		return source;

	}

	public virtual void lookToward(Vector3 target) {

		// TODO see if using nonplanar normalized vector subtraction fixes quaternion error
		//Quaternion.LookRotation(target);
		if (backwardsModel) {
						targetAngle = Quaternion.LookRotation (normVector(planarPoint(transform.position) - planarPoint(target)));
				} else {
						targetAngle = Quaternion.LookRotation (normVector(planarPoint(target) - planarPoint(transform.position)));
				}

	
		//backwardsFixed = false;
		//if (backwardsModel)
		//		targetAngle.y += 180f;
		//transform.LookAt(new Vector3(target.x,transform.position.y,target.z));
		//targetAngle = Quaternion.LookRotation (target - transform.position);
		
	}
	protected float playerDistance() {
		return (thePlayer.transform.position - transform.position).magnitude;
	}
	protected void moveToward(Vector3 point, float requestedSpeed, AudioClip movementSound) {
		lookToward (point);
		float speed = requestedSpeed;
		if (GetComponent<AudioSource>().clip!=movementSound)
			GetComponent<AudioSource>().clip = movementSound;
		if (GetComponent<AudioSource>().isPlaying==false)
			GetComponent<AudioSource>().Play();
		if (!GetComponent<Animation>().IsPlaying(attackAnimation))
			if ((mustWalkTimer > 0f) || (requestedSpeed==walkSpeed)) {
				speed = walkSpeed;
				if (!GetComponent<Animation>().IsPlaying(walkAnimation))
					GetComponent<Animation>().Play (walkAnimation);
			} else {
				GetComponent<Animation>().Play (runAnimation);
			}


		Vector3 angleVelocity = new Vector3 (0f, 1f, 0f);
		Vector3 destination = (point - transform.position);
		destination.Normalize();
		destination.Scale(new Vector3(Time.deltaTime * speed,Time.deltaTime * runSpeed,Time.deltaTime * speed));
		//destination.Scale (new Vector3(runSpeed,runSpeed,runSpeed));
		//destination.Normalize();

		//Quaternion deltaRotation = Quaternion.Euler (angleVelocity * Time.deltaTime);

		GetComponent<Rigidbody>().MovePosition (transform.position + destination);//.AddRelativeForce(Vector3.forward * GetComponent<Rigidbody>().mass*5*Time.deltaTime);
		if (bVisible && (thePlayer.transform.position - transform.position).magnitude < 400) {
			//if (backwardsModel) 
			//		transform.Rotate (0f, -180f, 0f);
			//transform.rotation = currAngle;
			//if (backwardsModel) 
			//
			GetComponent<Rigidbody> ().MoveRotation (currAngle);
			//if (backwardsModel) 
			//	transform.Rotate (0f, 180f, 0f);
		}
	}
	
	protected void followPath(List<astarpathfind.Node> path) {
	
	}

	protected virtual void updateChaseState() {
		
		//transform.LookAt(((astarpathfind.Node)path[pathIndex]).position);
		//transform.rotation.y = 
		if ((transform.position-focusCreature.transform.position).magnitude < attackDistance) {
			state = FSMState.FSM_ATTACK;
			if (GetComponent<AudioSource>().clip!= growl)
				GetComponent<AudioSource>().clip = growl;
			if (GetComponent<AudioSource>().isPlaying==false)
				GetComponent<AudioSource>().Play();
			return; //break;
		}
			
			
		if (path != null) {

						//print ("index is " + pathIndex + " of " + path.Count + "-1");
						if (pathIndex > path.Count - 1) {
				
								// TODO check for line of sight
								if ((focusCreature.transform.position - transform.position).magnitude < criticalDistance) {
									if (!path.Contains (astarpathfind.GridManager.GetNearestNode(focusCreature.transform.position)))
										path.Add (astarpathfind.GridManager.GetNearestNode(focusCreature.transform.position));
								}
								//		moveToward (focusCreature.transform.position,runSpeed);
								//	print ("going straight for player");
								//	pathIndex = 0;
								//	path = null;
								//} else {
									pathIndex = 0;	
									path = null;
								//}				
				
						} else {
							lookToward(path[pathIndex].position);
							if (planarDistance(path [pathIndex].position,transform.position) < 3f) {  //Mathf.Sqrt (2*Mathf.Pow (astarpathfind.GridManager.gridCellSize,2f))+0.5f) {
								
								pathIndex ++;
								
							} else {
								moveToward (path [pathIndex].position, runSpeed,footstepRun);
							}
							//if (pathIndex%3==0)
								if ((path[path.Count-1].position - focusCreature.transform.position).magnitude > (transform.position - focusCreature.transform.position).magnitude) {
									path.Remove (path[path.Count-1]);
									//path.Add(astarpathfind.GridManager.GetNearestNode(focusCreature.transform.position));
									if (path.Count==0)
										path = null;
									
								}

						}

						
			
				if ((transform.position-focusCreature.transform.position).magnitude < attackDistance) {
				state = FSMState.FSM_ATTACK;
					//path = null;
					//pathIndex = 0;
					return; //break;
				}
				//moveToward (path [pathIndex].position, runSpeed);
			} else {
				path = astarpathfind.findPath(this.transform.position,focusCreature.transform.position);
				pathIndex = 0;
		
			}

		
	}
	
	protected virtual void updateAttackState() {
		//TODO make sure to deal with if it's chasing an animal or player
		lookToward (focusCreature.transform.position);
		GetComponent<Rigidbody> ().MoveRotation (currAngle);
		if ((transform.position - focusCreature.transform.position).magnitude < attackDistance) {
			//if (movesWhileAttacking)
			//	moveToward(focusCreature.transform.position,walkSpeed,GetComponent<AudioSource>().clip);
			GetComponent<Animation>().Play (attackAnimation);
		} else if (!GetComponent<Animation>().IsPlaying (attackAnimation)) {
			moveToward (focusCreature.transform.position,runSpeed,footstepRun);
			if ((transform.position - focusCreature.transform.position).magnitude > attackDistance)
				state = FSMState.FSM_CHASE;
			//path=new List<astarpathfind.Node>();
			//path.Add (new astarpathfind.Node(focusCreature.transform.position));
			//pathIndex = 0;
		}
		
	}
	
	protected virtual void updateFleeState() {

		if (path!= null && !newPath && pathIndex < path.Count) {

			moveToward (path[pathIndex].position,runSpeed,footstepRun);

			if ( (path[pathIndex].position - new Vector3(transform.position.x,Terrain.activeTerrain.SampleHeight(transform.position),transform.position.z)).magnitude < astarpathfind.GridManager.gridCellSize*3f) {
				lookToward (path[pathIndex].position);
				pathIndex ++;

				if (pathIndex >= path.Count) //|| (transform.position-focusCreature.transform.position).magnitude < (((astarpathfind.Node)path[path.Count-1]).position - transform.position).magnitude  ) 
					//if ((focusCreature.transform.position - transform.position).magnitude < 5)
				{
					newPath = true;
					if ((transform.position-objective).magnitude < 15f) {
						state = FSMState.FSM_PASSTIME;
						updateIdleState();
						return;
					}
				}
			} else {
				moveToward (path[pathIndex].position,runSpeed,footstepRun);
				//if ( (((astarpathfind.Node)path[pathIndex]).position - transform.position).magnitude > (astarpathfind.GridManager.gridCellSize*1.5f)) {
				//	path = null;
				//	pathIndex = 0;
				//}
			}
		} else {

			path = astarpathfind.findPath(transform.position,objective);
			pathIndex = 0;
			newPath = false;
			//moveToward (objective,runSpeed);
			
			//if (Vector3.Distance(transform.position,objective)<runSpeed*2f)
			//	state = FSMState.FSM_PASSTIME;
		}
	}
	
	
	protected virtual void updateIdleState() {
		currTaskTimer -= Time.deltaTime;
		
		if (currTaskTimer < 0.0f) {
			objective = GrazingArea.getCloset(transform.position).getWanderPoint();
			//while (Vector3.Distance(objective,transform.position) < 15.0f) 
			//	objective = GrazingArea.getCloset(transform.position).getWanderPoint();
			
			objective.y = Terrain.activeTerrain.SampleHeight(objective);
			lookToward(objective);
			Debug.DrawLine(objective,objective+Vector3.up*20.0f);
			currTaskTimer = Random.Range (5.0f,15.0f);
		} else {
			objective.y = Terrain.activeTerrain.SampleHeight(objective);
			if (Vector3.Distance(objective,transform.position) > walkSpeed) {
				moveToward (new Vector3(objective.x,transform.position.y,objective.z),walkSpeed,footstepWalk);
			} else {
				//if (GetComponent<Animation>().IsPlaying(walkAnimation))
					GetComponent<Animation>().Play ("idle");
			}
		}
		////////
		// TODO this might f it up
		if (eyes == null) {
			//eyes = new Vector3(transform.position.x,transform.position.y+1f,transform.position.z);
			objective = GrazingArea.getCloset (transform.position).getWanderPoint ();
			//while (Vector3.Distance(objective,transform.position) < 15.0f) 
			//	objective = GrazingArea.getCloset(transform.position).getWanderPoint();
			
			objective.y = Terrain.activeTerrain.SampleHeight (objective);

		} else if (eyeball!=null) {

			eyes = eyeball.transform.position;
			Ray collisionCheck = new Ray (eyes, new Vector3 (objective.x, objective.y + 0.1f, objective.z));
			RaycastHit hit;

/////////Debug.DrawLine (eyeball.transform.position,objective);
			Debug.DrawLine (eyeball.transform.position, transform.position);

			if (Physics.Raycast (collisionCheck, out hit, 1f)) {
				objective = GrazingArea.getCloset (transform.position).getWanderPoint ();
				//while (Vector3.Distance(objective,transform.position) < 15.0f) 
				//	objective = GrazingArea.getCloset(transform.position).getWanderPoint();
			
				objective.y = Terrain.activeTerrain.SampleHeight (objective);
			}

		}


	}
	
	public virtual void justGotHit() {
		mustWalkTimer = timeMustWalkAfterHit;
		if (health < panicPoint) {
			if (state!=FSMState.FSM_FLEE) {
				//path = null;
				//pathIndex = 0;
			objective = GrazingArea.getEscape(transform.position,Random.Range (80.0f,250.0f)).transform.position;
			objective.y = Terrain.activeTerrain.SampleHeight(objective);
			//lookToward(objective);
			state = FSMState.FSM_FLEE;
			rallyEscape (path,100f,objective);
			}
		} else if (state!=FSMState.FSM_CHASE) {
			state = FSMState.FSM_CHASE;
			// = null;
			//pathIndex = 0;
		}
		if (focusCreature==null)
			focusCreature = thePlayer;
		
	
	}

	// This is to handle if the poor lil dino gets shot :(
	public void OnCollisionEnter (Collision collision) {
		//print ("HIT!");
		//print (collision.gameObject.tag);
		if (collision.gameObject.tag=="projectile") {
			Bullet theBullet  = collision.gameObject.GetComponent<Bullet>();
			// TODO maybe error-producing code
			// Oh noes! Our cute lil dinosaur just got SHOT!!!
			if (theBullet != null) {
				health -= theBullet.baseDamage;
				justGotHit();
				Destroy(theBullet);
			}
		}
		
		
		

	}

	public void OnDrawGizmos() {
		//if (path != null)
						
		//foreach (astarpathfind.Node spot in path)
		//	Gizmos.DrawCube (spot.position,new Vector3(1f,1f,1f));
		//if (objective!=null)
		//	Gizmos.DrawCube (objective,new Vector3(5f,5f,5f));

        //if (astarpathfind.GridManager.nodes == null)
        //    return;

	}

	protected virtual void rallyHelp(GameObject target, float range) {
		foreach (Dinosaur dino in getPack ()) {
			if (Vector3.Distance(transform.position,dino.transform.position) < range) {
				dino.focusCreature = this.focusCreature;
				dino.state = FSMState.FSM_CHASE;
				//dino.path = null;
				//pathIndex = 0;
				dino.rallied = true;
				//WaitForSeconds(Random.Range (0.2f,1.5f));
			}
			
		}
	}
	
	
	private bool IsTargetVisible(GameObject lookObjective)
	{
		RaycastHit ObstacleHit;
		if (lookObjective!=null)    // make sure we have an objective first or we get a dirty error.
			return (Physics.Raycast(transform.position, lookObjective.transform.position - transform.position, out ObstacleHit, viewDistance) &&  ObstacleHit.transform != transform && ObstacleHit.transform == lookObjective.transform);
		else
			return false;
	} 
	
	
	protected virtual void sense () {
	
		if (Vector3.Angle ((thePlayer.transform.position-transform.position),transform.forward)<viewAngle) {
			if (Vector3.Distance( thePlayer.transform.position,transform.position) < viewDistance) {
				
				//if (IsTargetVisible(GameObject.FindGameObjectWithTag("Player")))
				//{
					state=FSMState.FSM_CHASE;
					//path = null;
					focusCreature = thePlayer;
					rallyHelp (focusCreature,rallyDistance);
				//}
			}
		}
	}
	
	protected virtual void rallyEscape(List<astarpathfind.Node> path, float range, Vector3 objective) {
		foreach (Dinosaur dino in getPack ())
		if (Vector3.Distance(transform.position,dino.transform.position) < range) {
			dino.path = path;
			dino.pathIndex = 0;
			dino.state = FSMState.FSM_FLEE;
			dino.objective = objective;
			
			dino.rallied = true;
			//WaitForSeconds(Random.Range (0.2f,1.5f));
		}
	}

	protected void Start() {
		bDead = false;
		if (inflictsDamage)
			damageAct = GetComponentInChildren<DamageActuator> ();

        /*
		eyeball = GameObject.FindWithTag("eyeball");
		eyeballs = GameObject.FindGameObjectsWithTag ("eyeball");

		print ("here!");
		print (eyeballs.Length);

		if (eyeballs == null || eyeballs.Length == 0)
			return;

		eyeball = eyeballs [0];
		for (int i = 0; i < eyeballs.Length; i++) {
			if (Vector3.Distance(transform.position,eyeballs[i].transform.position) < Vector3.Distance(transform.position,eyeball.transform.position))
			    eyeball = eyeballs[i];
		}

		if (Vector3.Distance (eyeball.transform.position, this.transform.position) > 3f)
			eyeball = null;

		//eyeball = (transform.FindChild ("eyeball"));
		//if (eyeball != null) 
			*/
	}

	// Update is called once per frame
	public virtual void FixedUpdate () {
			if (inflictsDamage)
				damageAct.enabled = (state == FSMState.FSM_ATTACK);

			if (playerDistance () > updateDistanceThreshold) {
					GetComponent<Animation>().Stop();
					return;
				}
				//transform.position.Set (transform.position.x,Terrain.activeTerrain.SampleHeight(transform.position),transform.position.z);
	
			mustWalkTimer -= Time.deltaTime;
			if (mustWalkTimer < 0f)
					mustWalkTimer = 0f;
	
				//if (!bDead) GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().mass * Vector3.down);
	
	

				// Rotate toward the angle they're supposed to be looking at
				//Quaternion.Angle(transform.rotation,targetAngle)
				//if (Mathf.Abs (Mathf.DeltaAngle(transform.rotation.eulerAngles.y,targetAngle.eulerAngles.y)) > 6.0f)
				//if (Mathf.Abs(transform.rotation.eulerAngles.y-targetAngle.y)>5f)

		currAngle = Quaternion.Slerp (currAngle, targetAngle, Time.deltaTime*3f);//4f*Time.deltaTime);//Time.deltaTime * (2f - Quaternion.Angle (transform.rotation, targetAngle) / 120));
			//Quaternion.
			if (bVisible && (thePlayer.transform.position - transform.position).magnitude < 400)
			{	
					//	transform.Rotate (0f, 180f, 0f);
			}
		//print (transform.rotation.eulerAngles.y - targetAngle.y);

			//transform.rotation = Quaternion.Lerp(transform.rotation,targetAngle,Time.deltaTime * (2f - Quaternion.Angle(transform.rotation,targetAngle)/120));
			//transform.rotation.eulerAngles.Set(0f,transform.eulerAngles.y,0f);
			//transform.rotation.eulerAngles.z = 0f;

	
		//GetComponent<Rigidbody>().freezeRotation = true;
	
		lookTimer += Time.deltaTime;
		if (lookTimer > lookDelay) {
			lookTimer = 0;
			if (predator)
				sense ();
		
		}
		
		if (health < 0) {
			state = FSMState.FSM_DEAD; 
		}
		
		switch (state) 
		{
		case (FSMState.FSM_CHASE):
		{
			updateChaseState();
			break;
		}
		case (FSMState.FSM_PASSTIME):
		{
			
			updateIdleState();
			break;
		}
		case (FSMState.FSM_HUNT):
		{
			
			break;
		}
		case (FSMState.FSM_ATTACK):
		{
			updateAttackState();
			break;
		}
		case (FSMState.FSM_FLEE): {
			updateFleeState();
			break;
			
		}
		case (FSMState.FSM_DEAD): 
		{
			if (!bDead) {
				Physics.IgnoreLayerCollision(9,10,false);
				GetComponent<Animation>().Play ("die");
				bDead=true;
				GetComponent<Rigidbody>().isKinematic = true;
				GetComponent<Rigidbody>().freezeRotation = false;
				//transform.Translate (0,.5f,0);
			} else if (!GetComponent<Animation>().IsPlaying("die")) {
				//GetComponent<Rigidbody>().mass = GetComponent<Rigidbody>().mass * GetComponent<Rigidbody>().mass;
				GetComponent<Rigidbody>().isKinematic = false;
				GetComponent<Rigidbody>().maxDepenetrationVelocity = 3.0f;
				GetComponent<Rigidbody>().freezeRotation = false;
				
			}
			break;
		}
			
		}
	}
}
