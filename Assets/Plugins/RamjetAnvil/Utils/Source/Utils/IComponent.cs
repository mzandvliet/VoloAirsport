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

namespace RamjetAnvil.Unity.Utility {
    public interface IComponent {
        bool enabled { get; set; }
	    GameObject gameObject { get; }
	    Transform transform { get; }
	
	    T GetComponent<T>();
    }
}
