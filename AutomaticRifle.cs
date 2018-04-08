using UnityEngine;
using System.Collections;

public class AutomaticRifle : Weapon {

// Inherited 
/*
	public bool shooting;
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
	protected override void fire (Vector3 point)
	{
	
		point.x += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
		point.y += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
		point.z += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
	
		GameObject currShell = (GameObject)Instantiate(shellToEject,ejector.transform.position, Random.rotation);//Quaternion.identity);
		
		currShell.GetComponent<Rigidbody>().AddForce(Random.Range (1.9f,2.4f)*(ejectVector.transform.position-ejector.transform.position));
		
		Destroy (currShell,5.0f);
	

		shootBullet (point);
		--loadedRounds;
		//if (!allBullets.Contains(bullet))
		//	allBullets.Add (bullet);
		
		currRecoil += 0.26f;//Random.Range (0.2f,0.3f);
		if (currRecoil>maxRecoil)
			currRecoil = maxRecoil-Random.Range (0.06f,0.095f);
		


	}

	// Use this for initialization
	void Start () {
		base.Start();
		//fireRate = 10.0f;
		allWeapons.Add(this);
		scale3 = new Vector3(0,0,0);
		clipSize = 30;
		maxRounds = 480;
		position3 = transform.localPosition;
		rotation3 = GetComponentInParent<Camera>().transform.localRotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		
        // Standard weapon update functionality per the Unity3D engine's need for an update function
		handlePositionRecoilAndMuzzleFlash ();
		
		if (stillReloading ())
				return;
		
		shooting = isShooting ();
		
		if (shooting) 
			// Find out how many we need to shoot this time
			Weapon.bulletsThisFrame += fireRate * Time.deltaTime;
		else if (isSetAndPlaying (fireSound))
				GetComponent<AudioSource> ().Stop ();

		if (loadedRounds > 0) {
				handleFiring ();
		} else if (reserveRounds > 0 && !(isSetAndPlaying (fireSound))) {
				reload ();
			
		} else if (shooting) {
				handleDryFiring ();
		}
		
		
		
		}

}
