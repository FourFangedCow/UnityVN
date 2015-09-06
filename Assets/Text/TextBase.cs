using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextNode {
	public string Name_ = "DEFAULT NAME";
	public string Text_ = "DEFAULT TEXT";

	public TextNode(string name, string text) {
		Name_ = name;
		Text_ = text;
	}
}

public class Scene {
	List<TextNode> Tree_ = new List<TextNode>();
	int CurrentIndex_ = 0;

	public virtual void LoadText() {}

	public TextNode GetNextText() {
		if (CurrentIndex_ < Tree_.Count)
			return Tree_[CurrentIndex_++];
		else
			return new TextNode(" " , " ");
	}

	public void Add(string name, string text) {
		Tree_.Add(new TextNode(name, text));
	}
}
