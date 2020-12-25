using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RagdollEditor : EditorWindow
{
    private Transform _skeletonRoot;
    private Transform _ragdollRoot;

	[MenuItem("Window/Character/Ragdoll Editor")]
	private static void Initialize()
	{
	    var window = GetWindow<RagdollEditor>("Ragdoll Editor");
        window.minSize = new Vector2(256f, 256f);
	}

    private void OnGUI()
    {
        _skeletonRoot = EditorGUILayout.ObjectField("Root Bone", _skeletonRoot, typeof (Transform), true) as Transform;

        if (!_skeletonRoot)
            return;

        if (!_ragdollRoot)
        {
            if (GUILayout.Button("Create"))
                _ragdollRoot = DuplicateSkeleton(_skeletonRoot);
        }
        else
        {
            if (GUILayout.Button("Delete"))
                DestroyImmediate(_ragdollRoot.gameObject);
        }
    }

    private static Transform DuplicateSkeleton(Transform root)
    {
        Transform rootCopy = DuplicateTransform(root, "_ragdoll");
        DuplicateChildrenRecursively(root, rootCopy);

        return rootCopy;
    }

    private static void DuplicateChildrenRecursively(Transform parent, Transform parentCopy)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Transform childCopy = DuplicateTransform(child, "_ragdoll");
            childCopy.parent = parentCopy;
            
            DuplicateChildrenRecursively(child, childCopy);
        }
    }

    private static Transform DuplicateTransform(Transform source, string namePostfix)
    {
        Transform copy = new GameObject(source.name + namePostfix).transform;
        copy.position = source.position;
        copy.rotation = source.rotation;
        copy.localScale = source.localScale;
        return copy;
    }

    private void OnFocus()
    {
        SceneView.onSceneGUIDelegate += OnSceneViewRender;
    }

    void OnLostFocus()
    {
        SceneView.onSceneGUIDelegate -= OnSceneViewRender;
    }

    private void OnSceneViewRender(SceneView window)
    {
        if (_skeletonRoot)
            DrawSkeleton(_skeletonRoot, Color.blue);

        if (_ragdollRoot)
            DrawSkeleton(_ragdollRoot, Color.green);
    }

    private static readonly Vector3 Normal = new Vector3(0f, 0f, 0.05f);

    private static void DrawSkeleton(Transform root, Color color)
    {
        Stack<Transform> bones = new Stack<Transform>();
        bones.Push(root);

        while (bones.Count > 0)
        {
            Transform current = bones.Pop();
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                bones.Push(child);

                Debug.DrawLine(current.position, child.position, color);
                Debug.DrawRay(current.position, Normal, Color.white);
            }
        }
    }
}