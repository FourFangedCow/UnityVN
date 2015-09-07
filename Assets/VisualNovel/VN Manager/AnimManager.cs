using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// TYPES: Move, Scale, Flip, Visible?
class ANode {
	public float Duration;
	public ANode () {
	}
	
	// Activate function. Returns the delay if timed. Returns -1 if infinite.
	public virtual void Activate(GameObject target, Vector3 originalPos, float time) {
	}
	public virtual void Complete(GameObject target, Vector3 originalPos) {
	}

}

/* Absolute movement node
 */
class MoveNode : ANode {
	bool UseStart = false;
	Vector3 StartPos;
	Vector3 EndPos;
	public MoveNode (Vector3 startPos, Vector3 endPos, float  dur) {
		StartPos = startPos;
		EndPos = endPos;
		Duration = dur;
		UseStart = true;
	}
	public MoveNode (Vector3 endPos, float  dur) {
		EndPos = endPos;
		Duration = dur;
	}
	public override void Activate(GameObject target, Vector3 originalPos, float time) {
		if(UseStart)
			target.GetComponent<Transform>().position = Vector3.Lerp(StartPos, EndPos, time / Duration);
		else
			target.GetComponent<Transform>().position = Vector3.Lerp(originalPos, EndPos, time / Duration);
	}
	public override void Complete(GameObject target, Vector3 originalPos) {
		target.GetComponent<Transform>().position = EndPos;
	}
}

/* Rel movement node
 */
class ShiftNode : ANode {
	Vector3 ShiftPos;
	public ShiftNode (Vector3 shiftPos, float  dur) {
		ShiftPos = shiftPos;
		Duration = dur;
	}
	public override void Activate(GameObject target, Vector3 originalPos, float time) {
		target.GetComponent<Transform>().position = Vector3.Lerp(originalPos, originalPos + ShiftPos, time / Duration);
	}
	public override void Complete(GameObject target, Vector3 originalPos) {
		target.GetComponent<Transform>().position = originalPos + ShiftPos;
	}
}

/* Animation list actually animates the sprites
 * keeps timing, etc
 */
class AnimationList {
	float Timer;
	float Duration;

	GameObject Target;
	Vector3 OriginalPos;

	LinkedList<ANode> AList;
	public LinkedListNode<ANode> CurNode;
	
	public bool Complete = false;
	public string ID;

	public AnimationList(LinkedList<ANode> list, GameObject target, string id) {
		ID = id;
		Target = target;
		AList = list;
		CurNode = AList.First;
		OriginalPos = Target.GetComponent<Transform>().position;
	}

	public void Update() {
		Timer += Time.deltaTime;
		if (Timer > Duration) {
			Timer = 0;
			CurNode.Value.Complete(Target, OriginalPos);
			CurNode = CurNode.Next;
			OriginalPos = Target.GetComponent<Transform>().position;
			if (CurNode == null) {
				Complete = true; return;
			} else
				Duration = CurNode.Value.Duration;
		} else {
			CurNode.Value.Activate(Target, OriginalPos, Timer);
		}
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
	public AnimManager (SceneManager sm) {
		Initialize(sm);
	}
	
	// Use this for initialization
	void Initialize (SceneManager sm) {
		SM = sm;
		Active = true;
		AnimationLibrary = new Dictionary<string, LinkedList<ANode>>();
		AnimationLists = new Dictionary<string, AnimationList>();
		MarkedForRemoval = new List<string>();
		LoadAnimations ();
	}
	
	// Update is called once per frame
	public void Update () {
		if (Active) {
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
						ProcessString (sr, line, newAnim);
					}
				} while (line != null);
				sr.Close();
			}
			string[] findName = path.Split('\\');
			string id = findName[findName.Length - 1].Substring(0, findName[findName.Length - 1].Length - 4);
			AnimationLibrary.Add(id, newAnim);
		}
	}

	void ProcessString(StreamReader sr, string line, LinkedList<ANode> anim) {
		string[] args = line.Split ('|');
		switch(args[0].Trim()) {
		case "#M":
			anim.AddLast(CreateMoveNode(args));
			break;
		case "#S":
			string[] vec = args[1].Split(',');
			anim.AddLast(new ShiftNode(new Vector3(float.Parse(vec[0].Trim()), float.Parse(vec[1].Trim()), float.Parse(vec[2].Trim())), float.Parse(args[2].Trim())));
			break;
		default:
			break;
		}
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
}
