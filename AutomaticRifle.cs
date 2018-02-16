using UnityEngine;
using System.Collections;

public class AutomaticRifle : Weapon {

// Inherited cuties
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
		
		//currShell.GetComponent<Rigidbody>().AddForce (1.5f*(currWeapon.ejector.transform.position - wherePlayerWas));
		//currShell.transform.Translate (ejector.transform.position - wherePlayerWas);
		currShell.GetComponent<Rigidbody>().AddForce(Random.Range (1.9f,2.4f)*(ejectVector.transform.position-ejector.transform.position));
		//new WaitForSeconds(0.1f);
		//transform.position += recoilOffset;
		
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
				//transform.localRotation.eulerAngles.Set (rotation3.x,rotation3.y,rotation3.z+currRecoil*5.0f);
				//GetComponentInParent<Camera>().transform.localRotation.eulerAngles.Set (position3.x-currRecoil*currRecoil,position3.y,position3.z);
				//transform.localPosition = position3;
		
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

		/*
		//transform.localRotation.eulerAngles.Set (rotation3.x,rotation3.y,rotation3.z+currRecoil*5.0f);
		//GetComponentInParent<Camera>().transform.localRotation.eulerAngles.Set (position3.x-currRecoil*currRecoil,position3.y,position3.z);
		//transform.localPosition = position3;


		currRecoil -= Time.fixedDeltaTime*1.3f; //Random.Range (0.2f,0.25f);
		
		if (currRecoil < 0)
			currRecoil = 0;
			

			flashtimer -= Time.deltaTime;
			
			if (flashtimer < 0) {
				MuzzleFlash.GetComponent<Light>().enabled = false;
				MuzzFlashText.GetComponent<Renderer>().enabled = false;
			}


			if(isShootingInput ())
			{
				if (shooting==false)
					GetComponent<AudioSource>().clip = fireSound;
				shooting = true;
			}
			
			if (!isShootingInput ()) {
				if (shooting)
					if (Weapon.bulletsThisFrame < 1.0f)
						Weapon.bulletsThisFrame = 1.0f;
				
				shooting = false;
				MuzzleFlash.GetComponent<Light> ().enabled = false;
			}

			if (loadedRounds > 0 && !(GetComponent<AudioSource>().clip==reloadSound && GetComponent<AudioSource>().isPlaying)) {
						ray = Camera.main.ScreenPointToRay (new Vector3 (Screen.width / 2, Screen.height / 2, 0));

						
			
						if (stillReloading ())
								return;

						if (shooting) {
								// Find out how many we need to shoot this time
								Weapon.bulletsThisFrame += fireRate * Time.deltaTime;
				
								if (!GetComponent<AudioSource> ().isPlaying)
										GetComponent<AudioSource> ().Play ();
				
								MuzzleFlash.GetComponent<Light> ().enabled = !MuzzleFlash.GetComponent<Light> ().enabled;
								flashtimer = 0.1f;
								Vector3 point = ray.origin + (ray.direction * 10000f);
				
								MuzzFlashText.GetComponent<Renderer> ().enabled = true;
								MuzzFlashText.transform.eulerAngles = new Vector3 (MuzzFlashText.transform.eulerAngles.x, MuzzFlashText.transform.eulerAngles.y, Random.Range (0.0f, 360.0f));
				
								while (Weapon.bulletsThisFrame >= 1.0f) {
										Weapon.bulletsThisFrame -= 1.0f;
										if (Weapon.bulletsThisFrame < 0f)
												Weapon.bulletsThisFrame = 0f;
					
										fire (point);
					
					
					
					
								}
						} else {
								GetComponent<AudioSource> ().Stop ();
						}
						//wherePlayerWas = currWeapon.ejector.transform.position;	
				} else if (reserveRounds>0) {
					reload ();
				} else if (shooting) {
					GetComponent<AudioSource>().clip = emptySound;
					GetComponent<AudioSource>().Play ();
				}
		transform.localPosition = new Vector3(position3.x,position3.y,position3.z-currRecoil);
	}*/
}
