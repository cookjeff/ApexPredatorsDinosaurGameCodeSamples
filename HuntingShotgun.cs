using UnityEngine;
using System.Collections;

public class HuntingShotgun : AbstractShotgun {
	
	
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
	public int bulletsToShootAtOnce;
	protected override void fire (Vector3 point)
	{
        // Shoot multiple small projectiles
		for (int b = 0; b < bulletsToShootAtOnce; b++ ) {
			Vector3 altpoint = new Vector3 (point.x, point.y, point.z);
			altpoint.x += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
			altpoint.y += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
			altpoint.z += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
			
			
			
			shootBullet (altpoint);
		}
		--loadedRounds;
		
        // Throw off an empty shell
		GameObject currShell = (GameObject)Instantiate(shellToEject,ejector.transform.position, Random.rotation);//Quaternion.AngleAxis(90f,new Vector3(1f,0f,0f)));//Random.rotation);//Quaternion.identity);

		currShell.GetComponent<Rigidbody>().AddForce(Random.Range (1.9f,2.4f)*(ejectVector.transform.position-ejector.transform.position));
		
		Destroy (currShell,5.0f);
		
        // Control recoil
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
		clipSize = 5;
		maxRounds = 50;
		position3 = transform.localPosition;
		rotation3 = GetComponentInParent<Camera>().transform.localRotation.eulerAngles;
		print ("inacc mult: " + inaccuracyMultiplier + ", spread: " + accuracySpread);
	}
	
	// Update is called once per frame
	void Update () {

        // Standard weapon update functionality per the Unity3D engine's need for an update function
        handlePositionRecoilAndMuzzleFlash();
		
		if (stillReloading ())
				return;
		
		shooting = isShooting ();
		
        // Handle whether they can fire as well as auto-reload and dry fire
		if (shooting && !isSetAndPlaying(fireSound)) 
			// Find out how many we need to shoot this time
			Weapon.bulletsThisFrame = 1.0f;
		if (loadedRounds > 0) {
				if (!GetComponent<AudioSource>().isPlaying)
					handleFiring ();
		} else if (reserveRounds > 0 && !(isSetAndPlaying (fireSound))) {
				reload ();
			
		} else if (shooting) {
				handleDryFiring ();
		}
	}
	
}
