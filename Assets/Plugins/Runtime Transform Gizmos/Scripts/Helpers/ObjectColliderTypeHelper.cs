using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RTEditor
{
    /// <summary>
    /// Static class which can be used when working with the 'ObjectColliderType' enum.
    /// </summary>
    public static class ObjectColliderTypeHelper
    {
        #region Public Static Functions
        /// <summary>
        /// Returns a 'List' of all possible object collider types.
        /// </summary>
        public static List<ObjectCollider3DType> GetPossibleObjectCollderTypes()
        {
            // Retrieve an array of all possible collider types
            Array allColliderTypes = Enum.GetValues(typeof(ObjectCollider3DType));
            var outputColliderTypeList = new List<ObjectCollider3DType>();

            // Add the values to the list
            foreach(ObjectCollider3DType colliderType in allColliderTypes)
            {
                outputColliderTypeList.Add(colliderType);
            }

            return outputColliderTypeList;
        }

        /// <summary>
        /// Returns a string array which contains the names of all possible object collider types.
        /// </summary>
        /// <param name="ignoreTypes">
        /// The returned string array will not include the names of the collider types stored
        /// in this array. This parameter is only taken into consideration if it is not null
        /// and not empty.
        /// </param>
        public static string[] GetPossibleObjectColliderTypeNames(ObjectCollider3DType[] ignoreTypes = null)
        {
            List<ObjectCollider3DType> allColliderTypes = GetPossibleObjectCollderTypes();

            if (ignoreTypes != null && ignoreTypes.Length != 0) return (from colliderType in allColliderTypes where !ignoreTypes.Contains(colliderType) select colliderType.ToString()).ToArray();
            else return (from colliderType in allColliderTypes select colliderType.ToString()).ToArray();
        }

        /// <summary>
        /// Returns a member of the 'ObjectColliderType' enum based on the supplied name. 
        /// </summary>
        /// <remarks>
        /// A match will be found only when 'name' is the same as the result of 'ToString()' applied
        /// to one of the 'ObjectColliderType' members. That is the memebr which will be returned.
        /// </remarks>
        /// <param name="name">
        /// The name of the 'ObjectColliderType' enum member which must be returned.
        /// </param>
        /// <param name="outputColliderType">
        /// On function return, this will hold the member of the 'ObjectColliderType' enum which
        /// matches the input name. If the function returns false, this will be seto to 'Box'.
        /// </param>
        /// <returns>
        /// True if a match is found and false otherwise. If the method returns false, the value
        /// of 'outputColliderType' should be ignored.
        /// </returns>
        public static bool GetObjectColliderTypeByName(string name, out ObjectCollider3DType outputColliderType)
        {
            outputColliderType = ObjectCollider3DType.Box;

            // Get a list of all collider types
            List<ObjectCollider3DType> allColliderTypes = GetPossibleObjectCollderTypes();
            foreach(ObjectCollider3DType colliderType in allColliderTypes)
            {
                // If we have a name match, we can store the value in the ouput parameter and return true
                if(colliderType.ToString() == name)
                {
                    outputColliderType = colliderType;
                    return true;
                }
            }

            // No match was found
            return false;
        }
        #endregion
    }
}
