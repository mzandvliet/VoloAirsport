/**
 * Created by Martijn Zandvliet, 2014
 * 
 * Use of the source code in this document is governed by the terms
 * and conditions described in the Ramjet Anvil End-User License Agreement.
 * 
 * A copy of the Ramjet Anvil EULA should have been provided in the purchase
 * of the license to this source code. If you do not have a copy, please
 * contact me.
 * 
 * For any inquiries, contact: martijn@ramjetanvil.com
 */

using UnityEngine;
using RamjetAnvil.Unity.Utility;

public static class InputUtilities {
	/* Normally when you hold the stick at 45deg angle, input for both axes reads .72, we need
     * that to read 1, otherwise the player will not be able to pan full range effectively.
     * For this we convert the input to polar coordinates and work from there. */
	
	public static Vector2 CircularizeInput(Vector2 stickInput) {
		float stickInputAngle = Mathf.Atan2(stickInput.x, stickInput.y);
		float stickInputMagnitude = Mathf.Clamp(Mathf.Sqrt(stickInput.x * stickInput.x + stickInput.y * stickInput.y), 0f, 1f);
		
		float factorY = ((Mathf.Abs(stickInputAngle) - MathUtils.HalfPi) / MathUtils.HalfPi) * 2f;
		float factorX = Mathf.Sign(stickInput.x) * (2f - Mathf.Abs(factorY));
		factorX = Mathf.Clamp(factorX, -1f, 1f);
		factorY = Mathf.Clamp(factorY, -1f, 1f);
		
		return new Vector2(factorX * stickInputMagnitude, -factorY * stickInputMagnitude);
	}
}
