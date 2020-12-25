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

using System;
using UnityEngine;
using System.Collections.Generic;

public class CollisionEventSource : MonoBehaviour {
	public event Action<CollisionEventSource, Collision> OnCollisionEntered;
	public event Action<CollisionEventSource, Collider> OnTriggerEntered;

	public event Action<CollisionEventSource> OnDestroyed;

	private void Awake() {
		for (int i = 0; i < transform.childCount; i++) {
			CollisionEventSource child = transform.GetChild(i).GetComponent<CollisionEventSource>();
			if (child) {
				OnChildCreated(child);
			}
		}
	}

	/* Bubbling */

	private void OnChildCollisionEntered(CollisionEventSource child, Collision collision) {
		if (OnCollisionEntered != null) {
			OnCollisionEntered(child, collision);
		}
	}

	private void OnChildTriggerEntered(CollisionEventSource child, Collider other) {
		if (OnTriggerEntered != null) {
			OnTriggerEntered(child, GetComponent<Collider>());
		}
	}

	/* Initiation */

	private void OnCollisionEnter(Collision collision) {
		if (OnCollisionEntered != null) {
			OnCollisionEntered(this, collision);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (OnTriggerEntered != null) {
			OnTriggerEntered(this, other);
		}
	}

	/* Lifecycle */

	private void OnChildCreated(CollisionEventSource child) {
		child.OnCollisionEntered += OnChildCollisionEntered;
		child.OnTriggerEntered += OnChildTriggerEntered;
		child.OnDestroyed += OnChildDestroyed;
	}

	private void OnChildDestroyed(CollisionEventSource child) {
		child.OnCollisionEntered -= OnChildCollisionEntered;
		child.OnTriggerEntered -= OnChildTriggerEntered;
		child.OnDestroyed -= OnChildDestroyed;
	}

	private void OnDestroy() {
		if (OnDestroyed != null) {
			OnDestroyed(this);
		}
	}
}
