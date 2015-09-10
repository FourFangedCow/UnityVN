//------------------------------------------------------------------------------
/* The manager for the VN scene.
 * 
 * This should be fed a text file with the proper formatting (tbd)
 * 
 * TO ADD:
 *  - Scene Event Queue
 * 		- Animation Queue
 *      - Make input not buggy
 */
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/* The scene manager maintains the scene node queue and makes sure events
 * are fired correctly and in a timely manner. Takes a text file and
 * generates all the characters/nodes accordingly.
 */
public class SceneManager : MonoBehaviour {

	public bool Active;
	public TextManager TM;
	public AnimManager AM;
	GameObject VisualElements;
	GameObject TextCanvas;
	Queue<SceneNode> SceneNodeQueue;								// The scene node queue
	Dictionary<string,GameObject> CharacterObjects;	// List of character objects by ID
	Dictionary<string,string> CharacterNames;				// List of character names by ID

	float SceneTimer = 0.0f;
	float SceneDelay = -1.0f;	// The time delay until the next node.

	public GameObject Background;

	// STATIC SHIT
	public static string SceneToLoad = "TEST";

	public SceneManager () {

	}

	void Start () {
		//CreateScene();
		TM = new TextManager(this);
		AM = new AnimManager(this);

	}

	// Activated the next scene node in the queue
	public void NextSceneNode () {
		SceneTimer = 0.0f;
		SceneDelay = SceneNodeQueue.Dequeue().Activate(this);
	}	

	// Update is called once per frame
	void Update() {
		if(Input.GetKeyUp(KeyCode.A)) { // INPUT SHOULD BE CHANGED
			CreateScene("TEST");
		}
		if(Input.GetKeyUp(KeyCode.S)) { // INPUT SHOULD BE CHANGED
			EndScene();
		}
		if(Active) {
			if(Input.GetKeyUp(KeyCode.Space)) { // INPUT SHOULD BE CHANGED
				TM.InputPressed();
				AM.InputPressed();
			}
			TM.Update();
			AM.Update();
		
			// Updating the scene node queue
			SceneTimer += Time.deltaTime;
			if(SceneDelay != -1.0f && SceneTimer > SceneDelay) {
				NextSceneNode();
			}
		}
	}

	// Make Scene
	void CreateScene(string sceneName) {
		SceneToLoad = sceneName;
		Active = true;
		SceneNodeQueue = new Queue<SceneNode>();								// The scene node queue
		CharacterObjects = new Dictionary<string,GameObject>();	// List of character objects by ID
		CharacterNames = new Dictionary<string,string>();				// List of character names by ID
		VisualElements = (GameObject)Instantiate(Resources.Load("VN_VisualElements"), new Vector3(0, 0, 0), new Quaternion());
		TextCanvas = (GameObject)Instantiate(Resources.Load("VN_TextCanvas"), new Vector3(0, 0, 0), new Quaternion());
		Background = GameObject.Find("Background");
		AM.Initialize();
		TM.Initialize();

		LoadScene ("Assets//VisualNovel//Text//Story//" + SceneToLoad + ".txt"); // TEMP
		NextSceneNode();
	}

	// End Scene
	void EndScene() {
		SceneNodeQueue = null;
		Dictionary<string, GameObject>.ValueCollection values = CharacterObjects.Values;
		foreach(GameObject character in values)
			Destroy(character);
		CharacterObjects = null;
		CharacterNames = null;
		Destroy(VisualElements);
		Destroy(TextCanvas);
		Background = null;
		AM.CleanUp();
		TM.CleanUp();
	}


	// --- ! GET FOR CHARACTERS ! --- //
	public GameObject GetCharObj (string ID) {
		GameObject obj;
		CharacterObjects.TryGetValue (ID, out obj);
		return obj;
	}


	// --- ! READING IN TEXT ! --- //
	void LoadScene (string fileName) {
		string line;
		StreamReader sr = new StreamReader(fileName);
		using (sr) {
			do {
				line = sr.ReadLine ();
				if(line != null) {
					ProcessString (sr, line);
				}
			} while (line != null);
			sr.Close();
		}
	}

	void ProcessString(StreamReader sr, string line) {
		string[] args = line.Split ('|');
		switch(args[0].Trim()) {
		case "#CHARACTER":
			CreateCharacter(sr);
			break;
		case "#T":
			string cname = "error!";
			CharacterNames.TryGetValue(args[1].Trim(), out cname);
			CreateTextNode(sr, cname);
			break;
		case "#A":
			SceneNodeQueue.Enqueue(new AnimNode(args[1].Trim(), args[2].Trim()));
			break;
		case "#S":
			SceneNodeQueue.Enqueue(new SpriteNode(args[1].Trim(), args[2].Trim()));
			break;
		case "#N":
			SceneNodeQueue.Enqueue(new NewSceneNode(args[1].Trim()));
			break;
		default:
			break;
		}
	}

	void CreateTextNode (StreamReader sr, string cname) {
		string line = sr.ReadLine();
		string mainText = "";
		do {
			mainText += " " + line.Trim();
			line = sr.ReadLine();
		} while(line.ToCharArray()[0] != '#');
		SceneNodeQueue.Enqueue(new TextNode(cname, mainText.Trim()));
		ProcessString(sr, line);
	}

	void CreateCharacter (StreamReader sr) {
		string[] args;
		string id = "default";
		GameObject character = (GameObject)Instantiate(Resources.Load("VN_Character"), new Vector3(100, 0, 0), new Quaternion());
		do {
			args = sr.ReadLine().Split('|');
			switch(args[0].Trim()) {
			case "ID":
				id = args[1].Trim();
				break;
			case "Name":
				CharacterNames.Add(id, args[1].Trim());
				break;
			case "Sprite":
				Sprite[] sprites = Resources.LoadAll<Sprite>(args[1].Trim());
				foreach(var sp in sprites) {
					character.GetComponent<CharacterBase>().Sprites.Add(sp.name, sp);
				}
				character.GetComponent<CharacterBase>().SetSprite("Idle");
				break;
			case "PosVec":
				args = args[1].Split(',');
				var trans = character.GetComponent<Transform>();
				trans.position = new Vector3(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()), float.Parse(args[2].Trim()));
				if(args[3].Trim() == "true")
					trans.rotation = new Quaternion(0, 180, 0, 0);
				break;
			default:
				break;
			}
		} while(args[0] != "#END");
		CharacterObjects.Add(id, character);
	}
	public void PrintTest(string s) {
		print (s);
	}
}

