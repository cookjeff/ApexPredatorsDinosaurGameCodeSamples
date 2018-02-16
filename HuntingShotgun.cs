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

		for (int b = 0; b < bulletsToShootAtOnce; b++ ) {
			Vector3 altpoint = new Vector3 (point.x, point.y, point.z);
			altpoint.x += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
			altpoint.y += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
			altpoint.z += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
			
			
			
			shootBullet (altpoint);
		}
		--loadedRounds;
		
		GameObject currShell = (GameObject)Instantiate(shellToEject,ejector.transform.position, Random.rotation);//Quaternion.AngleAxis(90f,new Vector3(1f,0f,0f)));//Random.rotation);//Quaternion.identity);
		
		//currShell.GetComponent<Rigidbody>().AddForce (1.5f*(currWeapon.ejector.transform.position - wherePlayerWas));
		//currShell.transform.Translate (ejector.transform.position - wherePlayerWas);
		currShell.GetComponent<Rigidbody>().AddForce(Random.Range (1.9f,2.4f)*(ejectVector.transform.position-ejector.transform.position));
		//new WaitForSeconds(0.1f);
		//transform.position += recoilOffset;
		
		Destroy (currShell,5.0f);
		
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

				handlePositionRecoilAndMuzzleFlash ();
		
				if (stillReloading ())
						return;
		
				shooting = isShooting ();
		
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

		/*
		//transform.localRotation.eulerAngles.Set (rotation3.x,rotation3.y,rotation3.z+currRecoil*5.0f);
		//GetComponentInParent<Camera>().transform.localRotation.eulerAngles.Set (position3.x-currRecoil*currRecoil,position3.y,position3.z);
		//transform.localPosition = position3;
		
		currRecoil -= Time.fixedDeltaTime*1.3f; //Random.Range (0.2f,0.25f);
		
		if (currRecoil < 0)
			currRecoil = 0;
		
		{
			ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2,Screen.height/2,0));
			
			flashtimer -= Time.deltaTime;
			
			if (flashtimer < 0) {
				//MuzzleFlash.GetComponent<Light>().enabled = false;
				MuzzFlashText.GetComponent<Renderer>().enabled = false;
			}
			if(isShootingInput ())
			{
				if (shooting==false) {
					if (GetComponent<AudioSource>().clip != fireSound)
						GetComponent<AudioSource>().clip = fireSound;
					if (bulletsThisFrame <=1.0 && !GetComponent<AudioSource>().isPlaying)
						bulletsThisFrame = 1.1f;	
				}

					shooting = true;
			}



			if (!isShootingInput ()) {
				//if (shooting)
					//if (Weapon.bulletsThisFrame < 1.0f)
					//	Weapon.bulletsThisFrame = 1.0f;
				
				shooting = false;

			}

			//MuzzleFlash.GetComponent<Light>().enabled = MuzzFlashText.GetComponent<Renderer>().enabled;
			// TODO the shotguns' reloading will be f-cky
			if (stillReloading ())
				return;

			if (shooting && !GetComponent<AudioSource>().isPlaying) {
				//if (!GetComponent<AudioSource> ().isPlaying )
				//	Weapon.bulletsThisFrame = 1f;
				// Find out how many we need to shoot this time
				Weapon.bulletsThisFrame = 1f;
				//shooting = false;
				
				
				
				
				while (Weapon.bulletsThisFrame >= 1.0f)
				{
					//MuzzleFlash.GetComponent<Light>().enabled = !MuzzleFlash.GetComponent<Light>().enabled;
					flashtimer = 0.1f;
				
					if (!GetComponent<AudioSource>().isPlaying)
						GetComponent<AudioSource>().Play();
					else {
						GetComponent<AudioSource>().Stop();
						GetComponent<AudioSource>().Play();
						
					}
					
					Vector3 point = ray.origin + (ray.direction * 10000f);
					
					Weapon.bulletsThisFrame -= 1.0f;
					if (Weapon.bulletsThisFrame<0f)
						Weapon.bulletsThisFrame = 0f;
					
					MuzzFlashText.GetComponent<Renderer>().enabled = true;
					MuzzFlashText.transform.eulerAngles = new Vector3(MuzzFlashText.transform.eulerAngles.x,MuzzFlashText.transform.eulerAngles.y,Random.Range(0.0f,360.0f));
					
					
					fire (point);
					
					
					
					
				}
			} else {
				//bulletsThisFrame -= 2f*Time.deltaTime;
				//GetComponent<AudioSource>().Stop ();
			}
			//wherePlayerWas = currWeapon.ejector.transform.position;	
		}
		
		transform.localPosition = new Vector3(position3.x,position3.y,position3.z-currRecoil);
	}*/
	
}
