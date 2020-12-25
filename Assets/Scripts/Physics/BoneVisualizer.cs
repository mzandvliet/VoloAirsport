using UnityEngine;

[ExecuteInEditMode()]
public class BoneVisualizer : MonoBehaviour
{
	void Update()
	{
		DrawBones(this.transform);
	}
	
	void DrawBones(Transform parent)
	{
		if (parent.childCount == 0)
			return;
		
		for (int i = 0; i < parent.childCount; i++) {
			Transform child = parent.GetChild(i);
			Debug.DrawLine(parent.position, child.position, Color.green);
			DrawBones(child);
		}
	}
}
