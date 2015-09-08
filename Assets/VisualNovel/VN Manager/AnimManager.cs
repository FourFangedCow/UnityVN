using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// ORIGIN INFO
class OriginalInfo {
	public Vector3 Pos, Scale;
	public Quaternion Rot;
	public Vector4 Color;
	public OriginalInfo() {}
	public OriginalInfo(Vector3 pos, Quaternion rot, Vector3 scale, Vector4 color)
	{ Pos = pos; Rot = rot; Scale = scale; Color = color; }
	public void Update(GameObject target) {
		Pos = target.GetComponent<Transform>().position;
		Rot = target.GetComponent<Transform>().rotation;
		Scale = target.GetComponent<Transform>().localScale;
		Color = target.GetComponent<SpriteRenderer>().color;
	}
}



/* Animation list actually animates the sprites
 * keeps timing, etc
 */
class AnimationList {
	float Timer;
	float Duration;
	
	GameObject Target;
	OriginalInfo Original = new OriginalInfo();

	LinkedList<ANode> AList;
	public LinkedListNode<ANode> CurNode;
	
	public bool Complete = false;
	public bool Animating = false;
	public string ID;

	public AnimationList(LinkedList<ANode> list, GameObject target, string id) {
		ID = id;
		Target = target;
		AList = list;
		CurNode = AList.First;
		Timer = 0;
		Duration = CurNode.Value.Duration;
		Original.Update(Target);
	}

	public void Update() {
		if(!Animating)
			Animating = true;
		Timer += Time.deltaTime;
		if (Timer > Duration) {
			Timer = 0;
			CurNode.Value.Complete(Target, Original);
			CurNode = CurNode.Next;
			Original.Update(Target);
			if (CurNode == null) {
				Complete = true; return;
			} else Duration = CurNode.Value.Duration;
		} else CurNode.Value.Activate(Target, Original, Timer);
	}

	public void CompleteAnimation() {
		while(CurNode != null) {
			CurNode.Value.Complete (Target, Original);
			Original.Update(Target);
			CurNode = CurNode.Next;
		}
		Complete = true;
	}
}

/* Needs to track the characters
 * Be able to run simultaneous animations
 * cancel and jump between
 */
public class AnimManager {
	// VARIABLES
	bool Active;
	Dictionary<string, AnimationList> AnimationLists;
	Dictionary<string, LinkedList<ANode>> AnimationLibrary;
	

	SceneManager SM;
	List<string> MarkedForRemoval;

	
	// CONSTRUCTOR
	public AnimManager(SceneManager sm) {
		Initialize(sm);
	}
	
	// Use this for initialization
	void Initialize(SceneManager sm) {
		SM = sm;
		Active = true;
		AnimationLibrary = new Dictionary<string, LinkedList<ANode>>();
		AnimationLists = new Dictionary<string, AnimationList>();
		MarkedForRemoval = new List<string>();
		LoadAnimations();
	}
	
	// Update is called once per frame
	public void Update() {
		if(Active) {
			Dictionary<string, AnimationList>.ValueCollection values = AnimationLists.Values;
			foreach(AnimationList list in values) {
				list.Update();
				if(list.Complete)
					MarkedForRemoval.Add(list.ID);
			}
			foreach(string id in MarkedForRemoval)
				AnimationLists.Remove(id);
			MarkedForRemoval.Clear();
		}
	}
	
	// Input Function
	public void InputPressed() {
		Dictionary<string, AnimationList>.ValueCollection values = AnimationLists.Values;
		foreach(AnimationList list in values) {
			if(list.Animating) {
				list.CompleteAnimation();
				MarkedForRemoval.Add(list.ID);
			}
		}
		foreach(string id in MarkedForRemoval)
			AnimationLists.Remove(id);
		MarkedForRemoval.Clear();
	}
	
	public void AddOverwriteAnimation(string id, string anim) {
		AnimationLists.Add(id, GetAnimationList(id, anim));
	}

	AnimationList GetAnimationList(string id, string anim) {
		LinkedList<ANode> animList;
		AnimationLibrary.TryGetValue(anim, out animList);
		return new AnimationList(animList, SM.GetCharObj (id), id);
	}

	// -- ! TEXT READING ! --- //
	void LoadAnimations() {
		string[] animFP = Directory.GetFiles("Assets//VisualNovel//Animations", "*.txt");
		string line;
		foreach (string path in animFP) {
			LinkedList<ANode> newAnim = new LinkedList<ANode>();
			StreamReader sr = new StreamReader(path);
			using (sr) {
				do {
					line = sr.ReadLine ();
					if(line != null) {
						ANode newNode = ProcessString (sr, line);
						if(newNode != null)
							newAnim.AddLast(newNode);
					}
				} while (line != null);
				sr.Close();
			}
			string[] findName = path.Split('\\');
			string id = findName[findName.Length - 1].Substring(0, findName[findName.Length - 1].Length - 4);
			AnimationLibrary.Add(id, newAnim);
		}
	}

	ANode ProcessString(StreamReader sr, string line) {
		string[] args = line.Split('|');
		string[] vec;
		switch(args[0].Trim()) {
		case "#M":
			return CreateMoveNode(args);
		case "#S":
			vec = args[1].Split(',');
			return new ShiftNode(new Vector3(float.Parse(vec[0].Trim()), float.Parse(vec[1].Trim()), float.Parse(vec[2].Trim())), float.Parse(args[2].Trim()));
		case "#R":
			vec = args[1].Split(',');
			return new RotationNode(new Vector3(float.Parse(vec[0].Trim()), float.Parse(vec[1].Trim()), float.Parse(vec[2].Trim())), float.Parse(args[2].Trim()));
		case "#L":
			vec = args[1].Split(',');
			return new ScaleNode(new Vector2(float.Parse(vec[0].Trim()), float.Parse(vec[1].Trim())), float.Parse(args[2].Trim()));
		case "#A":
			return new AlphaNode(float.Parse(args[1].Trim()), float.Parse(args[2].Trim()));
		case "#D":
			return new DelayNode(float.Parse(args[1].Trim()));
		case "#C":
			return CreateCombinedNode(sr);
		default:
			break;
		}
		return null;
	}

	MoveNode CreateMoveNode(string[] args) {
		MoveNode node;
		string[] vec;
		Vector3 start, end;
		switch (args.Length) {
		case 3:
			vec = args[1].Split(',');
			end = new Vector3(float.Parse(vec[0].Trim()), float.Parse(vec[1].Trim()), float.Parse(vec[2].Trim()));
			node = new MoveNode(end, float.Parse(args[2].Trim()));
			break;
		case 4:
			vec = args[1].Split(',');
			start = new Vector3(float.Parse(vec[0].Trim()), float.Parse(vec[1].Trim()), float.Parse(vec[2].Trim()));
			vec = args[2].Split(',');
			end = new Vector3(float.Parse(vec[0].Trim()), float.Parse(vec[1].Trim()), float.Parse(vec[2].Trim()));
			node = new MoveNode(start, end, float.Parse(args[3].Trim()));
			break;
		default:
			node = new MoveNode(new Vector3(0,0,0), 0);
			break;
		}
		return node;
	}

	CombinedNode CreateCombinedNode(StreamReader sr) {
		string line;
		CombinedNode combinedNode = new CombinedNode(0);
		do {
			line = sr.ReadLine ();
			if(line != null) {
				ANode newNode = ProcessString (sr, line);
				if(newNode != null) {
					combinedNode.Nodes.Add(newNode);
					if(newNode.Duration > combinedNode.Duration)
						combinedNode.Duration = newNode.Duration;
				}
			}
		} while(line != "#END");
		return combinedNode;
	}
}
