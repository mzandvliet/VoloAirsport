using UnityEngine;

/*
 * Todo:
 * - Auto-get layers from physics api
 * - Easy method for or-ing multiple layers
 * - caching
 * - easy method for inverting and combining masks
 */
public static class LayerMasks
{
	public static int NameToLayerMask(string name)
	{
		return 1 << LayerMask.NameToLayer(name);
	}

    public static int NamesToLayerMask(params string[] names) {
        int mask = 0;
        for (int i = 0; i < names.Length; i++) {
            mask |= NameToLayerMask(names[i]);
        }
        return mask;
    }
}