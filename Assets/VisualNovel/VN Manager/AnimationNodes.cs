using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// TYPES: Move, Scale, Flip, Visible?
class ANode {
	public float Duration;
	public ANode () { }
	// Activate function. Returns the delay if timed. Returns -1 if infinite.
	public virtual void Activate(GameObject target, OriginalInfo origin, float time) { }
	public virtual void Complete(GameObject target, OriginalInfo origin) {}
}

/* Absolute movement node
 */
class MoveNode : ANode {
	bool UseStart = false;
	Vector3 StartPos;
	Vector3 EndPos;
	public MoveNode(Vector3 startPos, Vector3 endPos, float  dur) {
		StartPos = startPos;
		EndPos = endPos;
		Duration = dur;
		UseStart = true;
	}
	public MoveNode(Vector3 endPos, float  dur) {
		EndPos = endPos;
		Duration = dur;
	}
	public override void Activate(GameObject target, OriginalInfo origin, float time) {
		if(UseStart)
			target.GetComponent<Transform>().position = Vector3.Lerp(StartPos, EndPos, time / Duration);
		else
			target.GetComponent<Transform>().position = Vector3.Lerp(origin.Pos, EndPos, time / Duration);
	}
	public override void Complete(GameObject target, OriginalInfo origin)
	{ target.GetComponent<Transform>().position = EndPos; }
}

/* Rel movement node
 */
class ShiftNode : ANode {
	Vector3 ShiftPos;
	public ShiftNode (Vector3 shiftPos, float  dur) {
		ShiftPos = shiftPos;
		Duration = dur;
	}
	public override void Activate(GameObject target, OriginalInfo origin, float time)
	{ target.GetComponent<Transform>().position = Vector3.Lerp(origin.Pos, origin.Pos + ShiftPos, time / Duration); }
	public override void Complete(GameObject target, OriginalInfo origin)
	{ target.GetComponent<Transform>().position = origin.Pos + ShiftPos; }
}

/* Alpha color node
 */
class AlphaNode : ANode {
	float Alpha;
	public AlphaNode (float alpha, float  dur) {
		Alpha = alpha;
		Duration = dur;
	}
	public override void Activate(GameObject target, OriginalInfo origin, float time)
	{ target.GetComponent<SpriteRenderer>().color = Vector4.Lerp(origin.Color, new Vector4 (origin.Color.x, origin.Color.y, origin.Color.z, Alpha), time / Duration); }
	public override void Complete(GameObject target, OriginalInfo origin)
	{ target.GetComponent<SpriteRenderer>().color = new Vector4 (origin.Color.x, origin.Color.y, origin.Color.z, Alpha); }
}
/* Rotation and flipping node
 */
class RotationNode : ANode {
	Vector3 Rot;
	public RotationNode (Vector3 rot, float  dur) {
		Rot = rot;
		Duration = dur;
	}
	public override void Activate(GameObject target, OriginalInfo origin, float time)
	{ target.GetComponent<Transform>().rotation = Quaternion.Lerp(origin.Rot, Quaternion.Euler(Rot.x, Rot.y, Rot.z), time / Duration); }
	public override void Complete(GameObject target, OriginalInfo origin)
	{ target.GetComponent<Transform>().rotation = Quaternion.Euler(Rot.x, Rot.y, Rot.z); }
}

/* Scaling and size node
 */
class ScaleNode : ANode {
	Vector2 Scale;
	public ScaleNode (Vector2 scale, float  dur) {
		Scale = scale;
		Duration = dur;
	}
	public override void Activate(GameObject target, OriginalInfo origin, float time)
	{ target.GetComponent<Transform>().localScale = Vector3.Lerp(origin.Scale, new Vector3(Scale.x, Scale.y, 0), time / Duration); }
	public override void Complete(GameObject target, OriginalInfo origin)
	{ target.GetComponent<Transform>().localScale = new Vector3(Scale.x, Scale.y, 0); }
}

/* Combined nodes have multiple ANodes inside them. Crazy shit. 
 */
class CombinedNode : ANode {
	public List<ANode> Nodes = new List<ANode>();
	public CombinedNode(float dur) { Duration = dur; }
	// Activate function. Returns the delay if timed. Returns -1 if infinite.
	public override void Activate(GameObject target, OriginalInfo origin, float time) {
		foreach(ANode node in Nodes)
			node.Activate(target, origin, time);
	}
	public override void Complete(GameObject target, OriginalInfo origin) {
		foreach(ANode node in Nodes)
			node.Complete(target, origin);
	}
}