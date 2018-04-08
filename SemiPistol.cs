using UnityEngine;
using System.Collections;

public class SemiPistol : Weapon {

	
	// Inherited cuties
	/*
	public float fireRate;
	public float avgDamagePerProjectile;
	public float accuracySpread;
	public string weaponName;
	public GameObject shellToEject;
	public GameObject bulletToShoot;
	public GameObject barrel;
	public GameObject ejector;
	public GameObject ejectVector;
	public GameObject MuzzleFlash;
	public GameObject MuzzFlashText;
	public AudioClip fireSound;
	public AudioClip reloadSound;
	public AudioClip emptySound;
	public Vector3 scale3;
	public Vector3 position3;
	public Vector3 rotation3;
	public Vector3 recoilOffset;
*/
	//public override void fire (Vector3 point)
	//{
		

	// Use this for initialization
	void Start () {
		base.Start();
		//fireRate = 10.0f;
		allWeapons.Add(this);
		scale3 = new Vector3(0,0,0);
		clipSize = 20;
		maxRounds = 200;
		position3 = transform.localPosition;
		rotation3 = GetComponentInParent<Camera>().transform.localRotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {


		handlePositionRecoilAndMuzzleFlash ();
				
		if (stillReloading ())
				return;
		//else if (bulletsThisFrame == -1) 
		//		bulletsThisFrame = 0;

		shooting = isShooting ();

		if (shooting) 
			// Find out how many we need to shoot this time
			Weapon.bulletsThisFrame += fireRate * Time.deltaTime;
		if (loadedRounds > 0) {
			handleFiring();
		} else if (reserveRounds > 0 && !(isSetAndPlaying (fireSound))) {
			reload ();
						
		} else if (shooting) {
			handleDryFiring();
					
		}

	    if (loadedRounds == 0 && reserveRounds == 0) 
		    reserveRounds += 10;
			


		
				
	}
	
}
