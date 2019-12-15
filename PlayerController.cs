using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;

public class PlayerController : MonoBehaviour 
{
	Vector2 target;
	[Header("Set up player value")]
	[Tooltip("speed of player")]
	public float speed = 0.07f;

	[Tooltip("Jump force of player")]
	public float jumpForce = 391f;

	[Tooltip("Gravity of player when is jumping in the air")]
	public float gravityJump = 1.2f;

	[Tooltip("Gravity of player when sliding")]
	public float gravitySlide = 5f;

	[Tooltip("The force of bullet when player throw it")]
	public float throwForce = 300f;

	[Header("JetPack")]
	public GameObject JetPack;
	[Tooltip("Force of jetpack when user hold the Jump button")]
	public float jetPackForce = 50f;

	[Tooltip("The fire fx of Jet Pack")]
	public ParticleSystem JetPackFire;

	[Tooltip("The position of bullet")]
	public Transform throwPoint;

	[Tooltip("Smoke position when player jump, slide")]
	public Transform smokePoint;

	[Tooltip("Place Bullet prefab here")]
	public GameObject Bullet;

	[Tooltip("Place Smoke fx prefab here")]
	public GameObject smokeFx;

	[Tooltip("Place Jump fx prefab here")]
	public GameObject jumpFx;

	[Tooltip("Place the magnet from player here, this object will be set on and off during game")]
	public GameObject Magnet;

	[Tooltip("Time allow the magnet work")]
	public float magnetTimer = 10f;

	public AudioClip soundJump;
	public AudioClip soundThrow;
	public AudioClip soundCollectBullet;
	public AudioClip soundEatFruit;

	[Tooltip("The Box Collider of upper body, this will be disabled when player sliding")]
	public BoxCollider2D boxColl1;
	public BoxCollider2D boxColl2;
	[Tooltip("Check ground point, this must be under player feet ")]
	public Transform checkGround;
	[Tooltip("The layers that are considered is the ground")]
	public LayerMask LayerGround;
	[Header("Animator Controller")]
	[Tooltip("Place the paremeters of Animator in here, useful to custom player")]


	//public
	public string walkTrigger = "Walk";
	public string isGroundBool = "isGround";
	public string slideBool = "Slide";
	public string thrownTrigger = "Thrown";
	public string dieTrigger = "Die";
	

	//private 
	private Animator anim;
	private Rigidbody2D rig;
	private bool play = false;
	private bool die = false;
	private bool isGrounded = true;

	//[HideInInspector] //ocultar variables publicas en el inspector
	public bool isUsingJetPack = false;
	private bool isJumpHold = false;
	private float gravityNormal;
	private bool isCannonFiring = false; 	//está disparando cañones
	private bool isBoost = false;
	private float timeStuck = 0.1f;
	
	//====================================================
	private Estatus actual; //variable para crear y almacenar en el registro de aprendisaje
	private double posx, posy; 	//posicion del jugador
	private string accion; 		//estado del jugador
	public string nombreDelMundo; //obteniendo el nombre del mundo para guardar registro
	public Text textDelMundo;	//obteniendo textmundo
	private bool isslide = false;
	public bool autonomo = false;   //activador modo autonomo
	public bool guardoArchivo = false; //informar cuando se guardo el archivo
	public Vector3 posicionFinal;
    public GameObject castillo;
	List < List <Estatus> > AllRE;
	public List<Estatus> registro;
	public List<Estatus> movimientosPredeterminados;
	//=======================================================================================

	void Awake(){
		Magnet.SetActive (false);		//Apague el imán cuando comience el juego
	}

	
	// Use this for initialization
	void Start () 
	{
		textDelMundo = GameObject.Find("world").GetComponent<Text>(); //obteniendo el text del mundo
		nombreDelMundo = textDelMundo.text; //obteniendo el valor del texto
		registro = new List<Estatus>(); 	//lista con el registro de aprendizaje
		castillo = GameObject.Find("Castle Gate Black"); //obteniendo object castillo
		posicionFinal = castillo.transform.position; //obteniendo la posicion del castillo
		movimientosPredeterminados = new List<Estatus>();
		AllRE = new List< List<Estatus> >(); //lista de lista
		cargarPredeterminado();
		//cargarRegistro();
		//==================================================================================

		//Configurar variables
		target = transform.position;			// guardas la posicion inicial
		rig = GetComponent<Rigidbody2D> ();

		gravityNormal = rig.gravityScale;		//guardar la escala de gravedad normal
		anim = GetComponent<Animator> ();

		JetPack.SetActive (false);				//deshabilitar objeto Jet pack

		if(GlobalValue.isUsingJetpack) 			//activacion del jetpack
		{
			isUsingJetPack = true;
			rig.velocity = Vector2.zero;
			JetPack.SetActive (true);
		}
	}
	
	void cargarPredeterminado()
	{
		Estatus pre;
		
		pre = new Estatus(5.310004, -1.689712, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(5.450004, -1.391039, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(5.590004, -1.1112, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(5.660005, -0.9783444, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(5.800005, -0.7267588, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(5.800005, -0.7267588, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(24.28409, -2.505139, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(24.49409, -2.064192, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(24.56409, -1.926627, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(24.56409, -1.926627, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(24.63409, -1.793771, "jump"); movimientosPredeterminados.Add(pre);
		pre = new Estatus(24.70409, -1.665624, "jump"); movimientosPredeterminados.Add(pre);
	}

	void cargarRegistro()
    {
		if(File.Exists(nombreDelMundo + ".txt"))
		{
			print("leyo el archivo");
            string allText = File.ReadAllText(nombreDelMundo + ".txt");
            string [] allLine = allText.Split('\n');
            string [] estadosDeLinea; //iterador de extraccion de lineas

            List <Estatus> nuevoApredizaje;

            foreach (string line in allLine)
            {
                //nuevoApredizaje = new List<Estatus>();
                estadosDeLinea = line.Split('|');

                foreach (string estado in estadosDeLinea)
                {
                    try
                    {
                        registro.Add(new Estatus(estado)); //agregando estado a la lista
                    }catch(IndexOutOfRangeException)
                    {
                    }
                }

                //AllRE.Add(nuevoApredizaje);//agregando lista al registro de aprendizaje
            }
			print("leyo el archivo");
            /* AllRE.ForEach(delegate(List<Estatus> elem){
                Console.WriteLine(elem.Count);
            }); */
		}
    }





	// La actualización se llama una vez por fotograma
	void Update () 
	{
		posx = transform.position.x;
		posy = transform.position.y;
		
		//Este es el controlador para PC
		if (!die) //deja de hacer nada cuando el jugador está muerto
		{		
			if(Input.GetKeyDown(KeyCode.Space))
			{
				autonomo = !autonomo;
			}
			
			//modo autonomo
			if(autonomo == true)
			{
				//movimientosPredeterminados.ForEach(delegate(Estatus e)
				movimientosPredeterminados.ForEach(delegate(Estatus e)
				{
					if(e.Equals(posx, posy))
					{
						print("accion");
						if(e.Qn.Equals("jump"))
						{
							Jump();
							anim.SetBool("Jump", true);
						} 	
						if(e.Qn.Equals("slide"))
						{
						 	Slide(true);
							// anim.SetBool("Slide", true); //no es necesario ya esta en la funcion Slide(true); 
						}
						if(e.Qn.Equals("offjump")) 	
						{
							JumpOff();
							
						}
						if(e.Qn.Equals("offslide"))
						{
							Slide(false);
							// anim.SetBool("Slide", false); //no es necesario ya esta en la funcion Slide(true); 
						} 
					}
					anim.SetBool("Jump", false);
				});
			}

			// modo normal / aprendisaje
			else if(autonomo == false)
			{
				if (Input.GetKeyDown (KeyCode.UpArrow)){ //Solo salta cuando el jugador está en el suelo
					Jump();
				}
				if (Input.GetKeyUp (KeyCode.UpArrow)) {
					JumpOff ();
					actual = new Estatus(posx, posy, "offjump");
					registro.Add(actual);
					anim.SetBool("Jump", false);
				}
				if (Input.GetKeyDown (KeyCode.RightArrow)) {
					Attack ();
				}
				if (Input.GetKeyDown (KeyCode.DownArrow)) {
					Slide (true);
				} 
				if (Input.GetKeyUp (KeyCode.DownArrow)) {
					Slide (false);
					actual = new Estatus(posx, posy, "offslide");
					registro.Add(actual);
					// anim.SetBool("Slide", false); //no es necesario ya esta en la funcion Slide(true);
				}


				//=======================================================
				if(isJumpHold == true)
				{
					actual = new Estatus(posx, posy, "jump");
					registro.Add(actual);
					anim.SetBool("Jump", true);
				}

				if(isslide == true)
				{
					actual = new Estatus(posx, posy, "slide");
					registro.Add(actual);
					// anim.SetBool("Slide", true); //no es necesario ya esta en la funcion Slide(true);
				}


				//guardando el archivo, cuando llega al castillo
				if(((int) posicionFinal.x == (int) posx) && guardoArchivo == false)
				{
					//Debug.Log("Paso el mundo");
					registro.ForEach(delegate(Estatus e)
					{
						// Debug.Log(e.ToString()+ "|");
						File.AppendAllText(nombreDelMundo + ".txt", e.ToString() + "|");
					});

					File.AppendAllText(nombreDelMundo + ".txt", "\n");
					guardoArchivo = true;
				}
			}
			
			anim.SetFloat ("Height", rig.velocity.y);	
		}
		else
		{
			/* registro.ForEach(delegate(Estatus e)
			{
				Debug.Log(e);
			}); */
			//que hacer cuando se muere xd
		}
	}

	void FixedUpdate()
	{
		if (!die) //stop doing anything when player dead
		{		
			if (play && !isCannonFiring) {		//only moving when play varible is true and not fire by the Big Cannon
				transform.Translate (new Vector3 (speed, 0, 0));		//moving the player with speed 
			}
			if (isUsingJetPack && isJumpHold) {		//if player got JetPack mode, the Jump button will be used to raise the player up
				rig.AddForce (new Vector2 (0, jetPackForce));
			}

			//check if the player grounded
			if (Physics2D.OverlapCircle (checkGround.transform.position, 0.2f, LayerGround)) {
				anim.SetBool (isGroundBool, true);		//set animator
				isGrounded = true;
				isCannonFiring = false;	//if player fired out of the Cannon and hit the ground, allow moving
			} else {
				anim.SetBool (isGroundBool, false);		//set animator
				isGrounded = false;
			}

			if (rig.velocity.y == 0 && !isGrounded && !isUsingJetPack) {
				timeStuck -= Time.fixedDeltaTime;
				if (timeStuck <= 0)
					GameManager.instance.GameOver ();
			} else
				timeStuck = 0.1f;			
		}
	}

	//Llamado por el gran cañón
	public void CannonFire(){
		isCannonFiring = true;
		anim.SetTrigger (walkTrigger);
	}

	//Llamado por la secuencia de comandos GameManager
	public void Play(){
		if (anim != null)
			anim.SetTrigger (walkTrigger);
		play = true;
	}

	//Llamado por la UI del controlador y la PC
	public void Jump()
	{
		if (!die) {		//deja de hacer nada cuando el jugador está muerto
			isJumpHold = true;		//marque este bool para indicar que el usuario mantiene presionados los botones de salto
			if (isUsingJetPack) {		//si usa jet pack
				JetPackFire.emissionRate = 100f;	//aumentar el efecto fx
				JetPack.GetComponent<AudioSource> ().volume = 0.85f;	//aumentar el volumen del sonido del jet pack
				rig.gravityScale = gravityNormal;
			}
			else if (isGrounded) {
				SoundManager.PlaySfx (soundJump);
				rig.gravityScale = gravityJump;
				rig.velocity = Vector2.zero;
				rig.AddForce (new Vector2 (0, jumpForce));
				Instantiate (jumpFx, smokePoint.position, Quaternion.identity);
			}
		}
	}

	//Llamado por la UI del controlador y la PC
	public void JumpOff(){
		isJumpHold = false;
		if (isUsingJetPack) {
			JetPackFire.emissionRate = 25f;		
			JetPack.GetComponent<AudioSource> ().volume = 0.3f;
		} else
			rig.gravityScale = gravityNormal;
	}

	//Slide
	public void Slide(bool slide){
		if (!die) 
		{
			anim.SetBool (slideBool, slide);
			if (slide) {
				boxColl1.enabled = false;				//apague el colisionador del cuerpo cuando se desliza para evitar golpear a otro colisionador
				boxColl2.enabled = false;
				StartCoroutine (CreateSmoke (0.1f));	//crear humo al deslizarse
				rig.gravityScale = gravitySlide;		//aplicar nueva gravedad al deslizarse
				isslide = true;
			} else {
				boxColl1.enabled = true;				//encienda el colisionador de cuerpo nuevamente
				boxColl2.enabled = true;
				StopAllCoroutines ();
				if(!isJumpHold)
					rig.gravityScale = gravityNormal;
				isslide = false;
			}
		}
	}

	IEnumerator CreateSmoke(float time){
		yield return new WaitForSeconds (time);
		if (isGrounded)	//solo crea humo cuando el jugador está en el suelo
			Instantiate (smokeFx, smokePoint.transform.position, Quaternion.identity);
		StartCoroutine (CreateSmoke (0.1f));	//crear humo al deslizarse
	}

	//Llamado por la UI del controlador y la PC
	public void Attack(){
		if (!die && !isCannonFiring) {
			if (GameManager.Bullets > 0) {		//solo permita lanzar la bala cuando la cantidad de bala sea mayor que cero
				GameManager.Bullets--;
				SoundManager.PlaySfx (soundThrow);
				anim.SetTrigger (thrownTrigger);		//establecer gatillo para lanzar
				GameObject obj = Instantiate (Bullet, throwPoint.position, Quaternion.AngleAxis (30, Vector3.forward)) as GameObject;
				obj.GetComponent<Rigidbody2D> ().AddRelativeForce (new Vector2 (throwForce, 0));
			}
		}
	}

	//Llamado por la secuencia de comandos GameManager
	public void Dead(){
		if (!die) {

			die = true;
			anim.SetTrigger (dieTrigger);
			JetPack.SetActive (false);		//esconder Jetpack cuando esté muerto
			StopAllCoroutines ();
			//rig.isKinematic = true;
			rig.velocity = Vector2.zero;
			rig.gravityScale = 0.5f;

			var boxCo = GetComponents<BoxCollider2D> ();
			foreach (var box in boxCo) {
				box.enabled = false;
			}
			var CirCo = GetComponents<CircleCollider2D> ();
			foreach (var cir in CirCo) {
				cir.enabled = false;
			}

		}
	}

	//Detecta los objetos del juego a través de su tag
	void OnTriggerEnter2D(Collider2D other){
		
		if (other.gameObject.CompareTag ("Fruit")) {
			other.GetComponent<CircleCollider2D> ().enabled = false;
			SoundManager.PlaySfx (soundEatFruit, 0.5f);
			GameManager.Hearts++;
			other.gameObject.GetComponent<Animator> ().SetTrigger ("Collected");
		}
		else if (other.gameObject.CompareTag ("Bullet")) {
			SoundManager.PlaySfx (soundCollectBullet);
			GameManager.Bullets += 10;
			Destroy (other.gameObject);
		}
		else if (other.gameObject.CompareTag ("Magnet")) {
			SoundManager.PlaySfx (soundCollectBullet);
			Magnet.SetActive (true);
			StartCoroutine (WaitAndDisableMagnet (magnetTimer));
			Destroy (other.gameObject);
		}
		else if (other.gameObject.CompareTag ("Star")) {
			other.GetComponent<CircleCollider2D> ().enabled = false;
			SoundManager.PlaySfx (soundCollectBullet, 0.7f);
			GameManager.Stars++;
			GameManager.Score += 10;
			other.gameObject.GetComponent<Animator> ().SetTrigger ("Collected");
		}

		else if (other.gameObject.CompareTag ("Bridge")) {
//			Debug.Log ("bridge");
			other.gameObject.SendMessage ("Work", SendMessageOptions.DontRequireReceiver);
		}
		else if (other.gameObject.CompareTag ("JetPack")) {
			isUsingJetPack = !isUsingJetPack;
			rig.velocity = Vector2.zero;
			JetPack.SetActive (isUsingJetPack);
			Destroy (other.gameObject);
		}
		else if (other.gameObject.CompareTag ("SpeedBoost")) {
			if (!isBoost) {
				isBoost = true;
				speed *= 1.45f;
			} else {
				isBoost = false;
				speed /= 1.45f;
			}

			Destroy (other.gameObject);
		}
	}

	//Desactiva el imán después del retraso
	IEnumerator WaitAndDisableMagnet(float time){
		yield return new WaitForSeconds (time);
		Magnet.SetActive (false);
	}

	//Detecta el colisionador enemigo y envía el juego a la secuencia de comandos de GameManager
	void OnCollisionEnter2D(Collision2D other){
		if (other.gameObject.CompareTag ("Enemy")) {
			GameManager.instance.GameOver ();
		}
	}
}
