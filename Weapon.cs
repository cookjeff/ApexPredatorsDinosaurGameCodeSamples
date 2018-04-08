using UnityEngine;
using System.Collections;
using TouchControlsKit;
using UnityStandardAssets.CrossPlatformInput;

public abstract class Weapon : MonoBehaviour {
	public static ArrayList allWeapons = new ArrayList();
	protected Ray ray;
	protected float flashtimer;
	protected bool triggerIsReset;
	public float notifyDistance;
	public bool isInInventory = false;
	public bool shooting;
	public static float bulletsThisFrame;
	public static ArrayList allBullets;
	public float fireRate;
	public float avgDamagePerProjectile;
	public float accuracySpread;
	public float currRecoil;
	public float maxRecoil;
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
	public float recoilOffset;
	public float bobOffset;
	public float bobOffsetCounter;
	public float turningOffset;
	protected float inaccuracyMultiplier = 10f;
	public int maxRounds;
	public int loadedRounds = -1;
	public int reserveRounds = 0;
	public int clipSize;
	protected float bulletExpire = 1.67f;
	public Texture bulletTexture;
	public Texture iconTexture;
	public Vector3 lastPosition;
	private float bobAmplitude = .11f;
	private float bobFrequency = 2f;
	public float alertDistance;
	public bool enabled = false;



	// ***** JUST THE ones for handling the double-tap shooting
	const float doubleTapThreshold = 0.2f;
	private static float lastTapTime = 0f;
	private static float currTapTime = 0f;
	private static bool tapped = false;
	public static float touchTimer = 0f;
    private float lastmx, lastmy;

    public void disable() {
		currRecoil = 0f;
		bulletsThisFrame = 0f;
		gameObject.SetActive(false);
	
	}
	
	public void enable() {
		gameObject.SetActive(true);
		enabled = true;
	}
	// Use this for initialization
	protected void Start () {
		// Set the position
		//transform.position.Set(position3.x,position3.y,position3.z);
		allBullets = new ArrayList();
		disable();
		loadedRounds = -1;
		bobOffsetCounter = 0;
		lastPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
		// And the scale
		//transform.
	}

	protected bool isSetAndPlaying(AudioClip clip) {
		return (GetComponent<AudioSource> ().isPlaying && GetComponent<AudioSource> ().clip == clip);
	}

    // This handles if they're hitting the shooting button or tapping the 
    // viewing area on the screen
	protected bool isShootingInput() {

		int shootingAxes;

		// TODO Input.GetAxis ("Fire1") > 0f  ---V
		if (InputManager.GetButton ("Button0") || tapped)
		    shootingAxes = 1;
		else
			shootingAxes = 0;
	
		if (Mathf.Abs (shootingAxes) > 0.05) {
			shootingAxes = 1;
		} else {
			shootingAxes = 0;
		}
		if (shootingAxes == 1)
						return true;
		return false;


	}

	protected virtual void handleFiring () {
		ray = Camera.main.ScreenPointToRay (new Vector3 (Screen.width / 2, Screen.height / 2, 0));
		
		//MuzzleFlash.GetComponent<Light>().enabled = true; //!MuzzleFlash.GetComponent<Light>().enabled;
		
		Vector3 point = ray.origin + (ray.direction * 10000f);

        // Handle as many bullets as we are owed for the time its taken
		while (Weapon.bulletsThisFrame >= 1.0f) {
			flashtimer = 0.1f;
            // Play the sound
			if (!GetComponent<AudioSource> ().isPlaying)
				GetComponent<AudioSource> ().Play ();
			else {
				GetComponent<AudioSource> ().Stop ();
				GetComponent<AudioSource> ().Play ();
				
			}
			
			Weapon.bulletsThisFrame -= 1.0f;
			if (Weapon.bulletsThisFrame < 0f)
				Weapon.bulletsThisFrame = 0f;
			
            // Show the muzzleflash and rotate it
			MuzzFlashText.GetComponent<Renderer> ().enabled = true;
			MuzzFlashText.transform.eulerAngles = new Vector3 (MuzzFlashText.transform.eulerAngles.x, MuzzFlashText.transform.eulerAngles.y, Random.Range (0.0f, 360.0f));
			
			
			fire (point);
			
		}

	}

	protected virtual void handlePositionRecoilAndMuzzleFlash() {
		if ((GameObject.FindGameObjectWithTag("Player").transform.position - lastPosition).magnitude > 0f) {
			bobOffsetCounter += Time.deltaTime;
		} else {
			//bobOffsetCounter = 0f;
		}
        // Set the weapon bob
		bobOffset = (bobAmplitude * Mathf.Sin (Mathf.PI * bobFrequency * bobOffsetCounter))/10f;

        // Handle the recoil
		currRecoil -= Time.deltaTime * 1.3f; //Random.Range (0.2f,0.25f);
		if (currRecoil < 0)
			currRecoil = 0;

        // Set the muzzleflahs if it's still going off
		flashtimer -= 3f * Time.deltaTime;
		MuzzleFlash.GetComponent<Light> ().enabled = MuzzFlashText.GetComponent<Renderer> ().enabled;
		if (flashtimer < 0) {
			MuzzFlashText.GetComponent<Renderer> ().enabled = false;
		}
		transform.localPosition = new Vector3(position3.x,position3.y+bobOffset,position3.z-currRecoil);
		lastPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
	}

    // Returns if the person is trying to shoot and is allowed to per game rules
	protected bool isShooting() {
		if (FireGun.health() < 1)
			return false;
		
		if (isShootingInput ()) {
			if (shooting == false && loadedRounds > 0)
				GetComponent<AudioSource> ().clip = fireSound;
			return true;
		} else {
			if (shooting)
				if (Weapon.bulletsThisFrame < 1.0f && bulletsThisFrame > 0)
					Weapon.bulletsThisFrame = 1.1f;
			
			return false;
			//MuzzleFlash.GetComponent<Light>().enabled = false;
		}
	}

	// Fire the projectile as a physics item
	protected virtual void fire (Vector3 point) {
        // Create inaccuracy artificially
		point.x += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
		point.y += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;
		point.z += Random.Range (0-accuracySpread,accuracySpread)*inaccuracyMultiplier;

        // Eject the shell if applicable
		GameObject currShell = (GameObject)Instantiate(shellToEject,ejector.transform.position, Random.rotation);//Quaternion.identity);

		currShell.GetComponent<Rigidbody>().AddForce(Random.Range (1.9f,2.4f)*(ejectVector.transform.position-ejector.transform.position));
		
		Destroy (currShell,5.0f);

        // Fire the bullet and compute the recoil 
		shootBullet (point);
		--loadedRounds;
		currRecoil += recoilOffset;//Random.Range (0.2f,0.3f);
		if (currRecoil>maxRecoil)
			currRecoil = maxRecoil-Random.Range (0.06f,0.095f);
	}

    // Determine if they're still reloading and set the applicable parameters
	public bool stillReloading() {
		if (GetComponent<AudioSource> ().clip == reloadSound && GetComponent<AudioSource> ().isPlaying) {
			// Can't shoot--reloading
			// TODO handle reloading motion
			bulletsThisFrame = -1;
			return true;
		} else {
			if (loadedRounds > 0) {
				GetComponent<AudioSource>().clip = fireSound;
			}
			if (bulletsThisFrame == -1)
				bulletsThisFrame = 0;
			return false;
		}
	}

    // Play the dry fire sound if needed
	protected virtual void handleDryFiring() {
		if (bulletsThisFrame == -1f)
			bulletsThisFrame = 1f;
		else 
			bulletsThisFrame += Time.deltaTime;
		
		if (bulletsThisFrame >= 1f && !(isSetAndPlaying(emptySound))) {
			GetComponent<AudioSource>().clip = emptySound;
			GetComponent<AudioSource>().Play ();
			bulletsThisFrame--;
		}
	}

	public virtual void reload() {
		GetComponent<AudioSource> ().clip = reloadSound;
		GetComponent<AudioSource> ().Play();
		reserveRounds += loadedRounds;
		if (reserveRounds >= clipSize) {
			loadedRounds = clipSize;
			reserveRounds -= clipSize;
		} else {
			loadedRounds = reserveRounds;
			reserveRounds = 0;
		}
		bulletsThisFrame = -1f;
	}

	protected void shootBullet (Vector3 point)  {
		GameObject bullet = (GameObject)Instantiate(bulletToShoot,barrel.transform.position,Quaternion.identity) as GameObject;
		//bullet.transform.localPosition = new Vector3(0,0,0);
		//bullet.transform.Translate(bulletsThisFrame*Vector3.Normalize(point - bullet.transform.position));
		bullet.GetComponent<Bullet>().FaceTarget(point);//hit.point);
		bullet.GetComponent<Bullet>().Shoot(avgDamagePerProjectile);
		allBullets.Add (bullet);
		//for (int i = 0; i < allBullets.Count; i++ ) 
		//	if (((GameObject)allBullets[i])!=null)
		//		Physics.IgnoreCollision(bullet.GetComponent<Collider>(),((GameObject)allBullets[i]).GetComponent<Collider>());
		Destroy (bullet,bulletExpire);


	}



    // A function to track time between lookpad touches
	public static void lookPadTouched() {
		lastTapTime = currTapTime;
		currTapTime = Time.time;
	}

	// Update is called once per frame
	 void FixedUpdate () {

	
        // Determine if a double tapp has happened
		if ((currTapTime - lastTapTime) < doubleTapThreshold && touchTimer > 0f) {
			tapped = true;
		} else {

			tapped = false;
		}
		touchTimer -= Time.deltaTime;


	}
}
