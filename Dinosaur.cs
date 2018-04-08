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
    // States for the finite state machine
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

    // Get the rest of the nearby dinosaurs of the same species
	public abstract ArrayList getPack();
	

    public static void ResetDinoObjectivesOnRespawn()
    {
        foreach (Dinosaur d in allDinos)
        {
            
            // Set each dinosaur to idling
            d.state = FSMState.FSM_PASSTIME;
            
            
        }
    }

	public void Awake() {
		if (allDinos==null)
			allDinos = new List<Dinosaur>();
        // Account for the dinosaur and get it ready to interact with player
		allDinos.Add (this);
		smr = GetComponentInChildren<SkinnedMeshRenderer> ();
		thePlayer = GameObject.FindGameObjectWithTag ("Player");
	}

    // Make a flat/2D vector of the dinosaur's position
	public Vector3 planarPoint(Vector3 p) {
		return new Vector3 (p.x, 0f, p.z);
	}

    // Toggle visibility on and off to avoid animating dinosaurs not being seen
	public void OnBecameVisible() {
		//print ("just became visible");
		bVisible = true;
	}

	public void OnBecameInvisible() {
		bVisible = false;
	}

    // Get the 2D distance between 2 points
	float planarDistance(Vector3 a, Vector3 b) {
		return (((new Vector2 (a.x, a.z)) - (new Vector2 (b.x, b.z))).magnitude);
	}

    // Helper function to normalize vector
	public Vector3 normVector(Vector3 source) {
		source.Normalize();
		return source;
	}

    // Look toward the player
	public virtual void lookToward(Vector3 target) {

		// TODO see if using nonplanar normalized vector subtraction fixes quaternion error
		// Rotate model if its backwards, otherwise point it toward its target
		if (backwardsModel) {
						targetAngle = Quaternion.LookRotation (normVector(planarPoint(transform.position) - planarPoint(target)));
				} else {
						targetAngle = Quaternion.LookRotation (normVector(planarPoint(target) - planarPoint(transform.position)));
				}

	
	}
    
    // Get the distance to the player
	protected float playerDistance() {
		return (thePlayer.transform.position - transform.position).magnitude;
	}

    // Steering toward the player and move toward them
	protected void moveToward(Vector3 point, float requestedSpeed, AudioClip movementSound) {

        // Face them
		lookToward (point);
        // Determine speed
		float speed = requestedSpeed;

        // Play the proper sound and proper animation
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


        // Compute a point to move to proportional to their movement speed and the time its taking
		Vector3 angleVelocity = new Vector3 (0f, 1f, 0f);
		Vector3 destination = (point - transform.position);
		destination.Normalize();
		destination.Scale(new Vector3(Time.deltaTime * speed,Time.deltaTime * runSpeed,Time.deltaTime * speed));

        // Move the dinosaur toward the player and rotate them if they're close enough to be seen
		GetComponent<Rigidbody>().MovePosition (transform.position + destination);//.AddRelativeForce(Vector3.forward * GetComponent<Rigidbody>().mass*5*Time.deltaTime);
		if (bVisible && (thePlayer.transform.position - transform.position).magnitude < 400) {
			GetComponent<Rigidbody> ().MoveRotation (currAngle);
		}
	}
    
    // If they're in the chase state and are close to the player, move to attack and handle updating
    // the path they're following
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
	
    // Attack and if the player moves out of range, switch to chase state after finishing attack animation
	protected virtual void updateAttackState() {
		lookToward (focusCreature.transform.position);
		GetComponent<Rigidbody> ().MoveRotation (currAngle);
		if ((transform.position - focusCreature.transform.position).magnitude < attackDistance) {
			GetComponent<Animation>().Play (attackAnimation);
		} else if (!GetComponent<Animation>().IsPlaying (attackAnimation)) {
			moveToward (focusCreature.transform.position,runSpeed,footstepRun);
			if ((transform.position - focusCreature.transform.position).magnitude > attackDistance)
				state = FSMState.FSM_CHASE;
		}
		
	}
	
    // Update the escape state by following an escape path and finding a new path if needed
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

    // Get nearby dinosaurs of the same species to attack this dinosaur's target
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
        // Set that it's not dead and set it's damage actuator
		bDead = false;
		if (inflictsDamage)
			damageAct = GetComponentInChildren<DamageActuator> ();
	}

	// Update is called once per frame
	public virtual void FixedUpdate () {

        // Only allow its damage actuator to be live if it's attacking
		if (inflictsDamage)
			damageAct.enabled = (state == FSMState.FSM_ATTACK);

        // If the player is too far away, don't animate (to speed up performance)
		if (playerDistance () > updateDistanceThreshold) {
				GetComponent<Animation>().Stop();
				return;
			}
	
        // The time it must walk, e.g. if it's just been hit
		mustWalkTimer -= Time.deltaTime;
		if (mustWalkTimer < 0f)
				mustWalkTimer = 0f;

        // Calculate the dinosaur's rotation angle as a time-proportional movement from its current
        // angle toward the angle it should be at
		currAngle = Quaternion.Slerp (currAngle, targetAngle, Time.deltaTime*3f);//4f*Time.deltaTime);//Time.deltaTime * (2f - Quaternion.Angle (transform.rotation, targetAngle) / 120));

	    // Update the timer that it uses to periodically scan its field of view for the player and act if it is time to scan
		lookTimer += Time.deltaTime;
		if (lookTimer > lookDelay) {
			lookTimer = 0;
            // If it's a predator, check to see if it can see the player
			if (predator)
				sense ();
		
		}
		
        // Make it dead if it needs to be
		if (health < 0) {
			state = FSMState.FSM_DEAD; 
		}
		
        // Handle the different states' updates
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
                // If it's dead, have it ignore relevant physics layers, play the death animation, and let it fall over
			    if (!bDead) {
				    Physics.IgnoreLayerCollision(9,10,false);
				    GetComponent<Animation>().Play ("die");
				    bDead=true;
				    GetComponent<Rigidbody>().isKinematic = true;
				    GetComponent<Rigidbody>().freezeRotation = false;
				    //transform.Translate (0,.5f,0);
			    } else if (!GetComponent<Animation>().IsPlaying("die")) {
				    // Disable physics being able to affect the dinosaur and make sure it can't
                    // "push" itself out of the ground fast if it has animated any part of itself into the ground
				    GetComponent<Rigidbody>().isKinematic = false;
				    GetComponent<Rigidbody>().maxDepenetrationVelocity = 3.0f;
				
				
			    }

			    break;
		    }
			
		}
	}
}
