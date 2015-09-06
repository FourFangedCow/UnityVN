using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextManager : MonoBehaviour {
	// VARIABLES
	Text Chat_;
	Text Name_;
	TextNode CurNode_;

	// Writing shit
	int WritingIndex_ = 0;
	float WritingTimer_ = 0f;
	float WritingTime_ = 0.05f;
	bool WritingText_ = false;

	Scene Scene_ = new Scene1();

	// Use this for initialization
	void Start () {
		Chat_ = GameObject.Find("Text_Chat").GetComponent<Text>();
		Name_ = GameObject.Find("Text_Name").GetComponent<Text>();
		Scene_.LoadText ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.Space)) {
			NextText ();
		}
		if (WritingText_) {
			WritingTimer_ += Time.deltaTime;
			if (WritingTimer_ > WritingTime_) {
				WritingTimer_ -= WritingTime_;
				Chat_.text = string.Concat(Chat_.text, NextChar());
			}
		}
	}

	void NextText () {
		if (WritingText_ == true) {
			WritingText_ = false;
			Chat_.text = CurNode_.Text_;
		} else {
			CurNode_ = Scene_.GetNextText ();
			Chat_.text = "";
			Name_.text = CurNode_.Name_;
			WritingText_ = true;
			WritingIndex_ = 0;
		}
	}

	string NextChar () {
		if (WritingIndex_ >= CurNode_.Text_.Length - 1)
			WritingText_ = false;
		return CurNode_.Text_ [WritingIndex_++].ToString();
	}
}
