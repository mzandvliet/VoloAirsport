using UnityEngine;

public class Treefab : MonoBehaviour
{
	static int numTrees = 0;
	static bool printed = false;
	
	void Awake()
	{
		numTrees++;
	}
	
	void Start()
	{
		if (!printed) {
			Debug.Log("Number of active trees: " + numTrees);
			printed = true;
		}
	}
}
