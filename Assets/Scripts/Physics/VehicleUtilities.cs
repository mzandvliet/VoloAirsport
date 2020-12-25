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
using System.Collections.Generic;
using RamjetAnvil.Unity.Utility;

public static class VehicleUtilities {
    // Todo: Doesn't work with our implementation of ImmutableState (world/local difference)

    //public static Dictionary<Transform, ImmutableTransform> RecordTransformState(Transform root) {
    //    var state = new Dictionary<Transform, ImmutableTransform>();
    //    Transform[] transforms = root.GetComponentsInChildren<Transform>();
    //    for (int i = 0; i < transforms.Length; i++) {
    //        Transform childTransform = transforms[i];
    //        state.Add(childTransform, new ImmutableTransform(childTransform));
    //    }
    //    return state;
    //}

    //public static void RestoreTransformState(Transform root, Dictionary<Transform, ImmutableTransform> state, Vector3 position, Quaternion rotation, Vector3 velocity) {
    //    foreach (KeyValuePair<Transform, ImmutableTransform> pair in state) {
    //        Rigidbody body = pair.Key.GetComponent<Rigidbody>();
    //        if (body) {
    //            body.isKinematic = true;
    //        }
    //        pair.Key.localPosition = pair.Id.Position;
    //        pair.Key.localRotation = pair.Id.Rotation;
			
    //        if (body) {
    //            body.isKinematic = false;
    //            body.velocity = velocity;
    //            body.angularVelocity = Vector3.zero;
    //        }
    //    }
    //    root.position = position;
    //    root.rotation = rotation;
    //}
}
