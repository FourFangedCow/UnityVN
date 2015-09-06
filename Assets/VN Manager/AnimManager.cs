using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

class ANode {

}

public class AnimManager {
	// VARIABLES
	bool Active;

	ANode CurrentNode;
	Queue<ANode> AnimationQueue = new Queue<ANode>();

	SceneManager SM;

	
	
	// CONSTRUCTOR
	public AnimManager (SceneManager sm) {
		Initialize(sm);
	}
	
	// Use this for initialization
	void Initialize (SceneManager sm) {
		SM = sm;
		Active = false;
	}
	
	// Update is called once per frame
	public void Update () {
		if (Active) {

		}
	}
	
	// Input Function
	public void InputPressed() {

	}
	
	// Given new text node
	public void Activate(TextNode tn) {

	}
}
