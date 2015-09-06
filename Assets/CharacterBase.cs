using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterBase : MonoBehaviour {

	//public IList<Sprite> Emotions = new IList<Sprite>();
	public Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetSprite(string sName) {
		Sprite newSprite = null;
		Sprites.TryGetValue(sName, out newSprite);
		if (newSprite == null)
			print ("ERROR: " + sName + " was not found on the character."); 
		else this.gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
	}
}
