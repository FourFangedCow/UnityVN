//------------------------------------------------------------------------------
/* The manager for the VN scene.
 * 
 * This should be fed a text file with the proper formatting (tbd)
 * 
 * TO ADD:
 *  - Scene Event Queue
 * 		- Animation Node
 * 		- Text Node
 * 		- ?
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

public class TextNode : SceneNode {

	public string Name = "DEFAULT NAME";
	public string Text = "DEFAULT TEXT";

	public TextNode(string name, string text) {
		Name = name;
		Text = text;
	}
	public override float Activate(SceneManager SM) {
		SM.TM.Activate (this);
		return -1.0f;
	}
}


/* The scene manager maintains the scene node queue and makes sure events
 * are fired correctly and in a timely manner. Should take a text file and
 * generate all the characters accordingly.
 */
public class SceneManager : MonoBehaviour {

	public TextManager TM;
	Queue<SceneNode> SceneNodeQueue = new Queue<SceneNode>();	// The scene node queue

	float SceneTimer = 0.0f;
	float SceneDelay = -1.0f;	// The time delay until the next node.

	public SceneManager () {

	}

	void Start () {
		TM = new TextManager(this);
		LoadScene ("Assets//Text//Story//TEST.txt");
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


	// --- ! READING IN TEXT ! --- //
	void LoadScene (string fileName) {
		string line;
		StreamReader sr = new StreamReader(fileName);
		using (sr) {
			do {
				line = sr.ReadLine ();
				if(line != null) {
					string[] args = line.Split ('|');
					SceneNodeQueue.Enqueue(new TextNode(args[0], args[1]));
				}
			}
			while (line != null);
			sr.Close();
		}
	}
}

