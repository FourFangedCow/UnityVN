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

/* Scene nodes are the base for all scene events. This includes character
 * animation, text, special effects, etc.
 */
public abstract class SceneNode {
	public SceneNode () {
	}

	// Activate function. Returns the delay if timed. Returns -1 if infinite.
	public virtual float Activate(SceneManager SM) {
		return -1;
	}

}
/* Text nodes are for displaying text. Yup.
 */
public class TextNode : SceneNode {
	
	public string Name = "DEFAULT NAME";
	public string Text = "DEFAULT TEXT";
	
	public TextNode(string name, string text) {
		Name = name;
		Text = text;
	}
	public override float Activate(SceneManager SM) {
		SM.TM.Activate(this);
		return -1.0f;
	}
}

/* Anim nodes are for moving and manipulating characters using an animation
 * node queue.
 */
/*public class AnimNode : SceneNode {
	
	public string Name = "DEFAULT NAME";
	public string Text = "DEFAULT TEXT";
	
	public AnimNode(string name, string text) {
		Name = name;
		Text = text;
	}
	public override float Activate(SceneManager SM) {
		SM.TM.Activate(this);
		return -1.0f;
	}
}*/

public class SpriteNode : SceneNode {
	public string ID = "Default";
	public string Name = "Idle";
	public SpriteNode(string id, string name) {
		ID = id;
		Name = name;
	}
	public override float Activate(SceneManager SM) {
		SM.GetCharObj(ID).GetComponent<CharacterBase>().SetSprite(Name);
		SM.NextSceneNode ();
		return -1.0f;
	}
}


/* The scene manager maintains the scene node queue and makes sure events
 * are fired correctly and in a timely manner. Takes a text file and
 * generates all the characters/nodes accordingly.
 */
public class SceneManager : MonoBehaviour {

	public TextManager TM;
	Queue<SceneNode> SceneNodeQueue = new Queue<SceneNode>();								// The scene node queue
	Dictionary<string,GameObject> CharacterObjects = new Dictionary<string,GameObject>();	// List of character objects by ID
	Dictionary<string,string> CharacterNames = new Dictionary<string,string>();				// List of character names by ID

	float SceneTimer = 0.0f;
	float SceneDelay = -1.0f;	// The time delay until the next node.

	public SceneManager () {

	}

	void Start () {
		TM = new TextManager(this);
		LoadScene ("Assets//Text//Story//TEST.txt"); // TEMP
		NextSceneNode();
	}

	// Activated the next scene node in the queue
	public void NextSceneNode () {
		SceneTimer = 0.0f;
		SceneDelay = SceneNodeQueue.Dequeue().Activate(this);
	}	

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Space)) { // INPUT SHOULD BE CHANGED
			TM.InputPressed();
		}

		TM.Update();
	
		// Updating the scene node queue
		//SceneTimer += Time.deltaTime;
		//if (SceneDelay != -1.0f && SceneTimer > SceneDelay) {
		//	NextSceneNode ();
		//}
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
			break;
		case "#S":
			SceneNodeQueue.Enqueue(new SpriteNode(args[1].Trim(), args[2].Trim()));
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
		GameObject character = (GameObject)Instantiate(Resources.Load("Character"), new Vector3(100, 0, 0), new Quaternion());
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
}

