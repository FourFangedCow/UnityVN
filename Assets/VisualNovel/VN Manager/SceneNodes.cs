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
public class AnimNode : SceneNode {
	
	public string ID = "DEFAULT";
	public string AnimName = "";
	
	public AnimNode(string id, string anim) {
		ID = id;
		AnimName = anim;
	}
	public override float Activate(SceneManager SM) {
		SM.AM.AddOverwriteAnimation(ID, AnimName);
		return 0.0f;
	}
}
/* Sprite nodes change the character's sprite.
 */
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

/* Sprite nodes change the character's sprite.
 */
public class BackgroundNode : SceneNode {
	string Name;
	public BackgroundNode(string name) {
		Name = name;
	}
	public override float Activate(SceneManager SM) {
		SM.Background.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(Name);
		SM.NextSceneNode();
		return -1.0f;
	}
}

/* Changes to a new scene
 */
public class NewSceneNode : SceneNode {
	string Scene = "TEST";
	public NewSceneNode(string scene) {
		Scene = scene;
	}
	public override float Activate(SceneManager SM) {
		SceneManager.SceneToLoad = Scene;
		Application.LoadLevel(Application.loadedLevel);
		return -1.0f;
	}
}