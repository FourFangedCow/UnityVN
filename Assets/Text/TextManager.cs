using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextManager {
	// VARIABLES
	bool Active;
	Text Chat;
	Text Name;
	TextNode CurNode;
	SceneManager SM;

	// WRITING
	int WritingIndex = 0;
	float WritingTimer = 0f;
	float WritingTime = 0.05f;
	bool WritingText = false;

	

	// CONSTRUCTOR
	public TextManager(SceneManager sm) {
		Initialize(sm);
	}

	// Use this for initialization
	void Initialize (SceneManager sm) {
		SM = sm;
		Active = false;
		Name = GameObject.Find("Text_Name").GetComponent<Text>();
		Chat = GameObject.Find("Text_Chat").GetComponent<Text>();
	}
	
	// Update is called once per frame
	public void Update () {
		if (WritingText && Active) {
			WritingTimer += Time.deltaTime;
			if (WritingTimer > WritingTime) {
				WritingTimer -= WritingTime;
				Chat.text = string.Concat(Chat.text, NextChar());
			}
		}
	}

	// Input Function
	public void InputPressed() {
		if (WritingText && Active) {
			WritingText = false;
			Chat.text = CurNode.Text;
		} else if (Active) {
			Active = false;
			SM.NextSceneNode ();
		}
	}

	// Given new text node
	public void Activate(TextNode tn) {
		CurNode = tn;
		Active = true;
		WritingText = true;
		Chat.text = "";
		Name.text = CurNode.Name;
		WritingIndex = 0;
	}

	string NextChar () {
		if (WritingIndex >= CurNode.Text.Length - 1)
			WritingText = false;
		return CurNode.Text [WritingIndex++].ToString();
	}
}
