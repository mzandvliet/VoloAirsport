using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Contains useful functions which can be used when working with layers.
    /// </summary>
    public static class LayerHelper
    {
        #region Public Static Functions
        /// <summary>
        /// Returns the minimum layer number;
        /// </summary>
        public static int GetMinLayerNumber()
        {
            return 0;
        }

        /// <summary>
        /// Returns the maximum layer number.
        /// </summary>
        public static int GetMaxLayerNumber()
        {
            return 31;
        }

        /// <summary>
        /// Checks if the 'layerNumber' bit is set inside 'layerBits'.
        /// </summary>
        public static bool IsLayerBitSet(int layerBits, int layerNumber)
        {
            return (layerBits & (1 << layerNumber)) != 0;
        }

        /// <summary>
        /// Sets the layer bit 'layerNumber' inside 'layerBits'.
        /// </summary>
        public static int SetLayerBit(int layerBits, int layerNumber)
        {
            return layerBits | (1 << layerNumber);
        }

        /// <summary>
        /// Clears the layer bit 'layerNumber' inside 'layerBits'.
        /// </summary>
        public static int ClearLayerBit(int layerBits, int layerNumber)
        {
            return layerBits & (~(1 << layerNumber));
        }

        /// <summary>
        /// Returns true if the specified layer number is valid.
        /// </summary>
        public static bool IsLayerNumberValid(int layerNumber)
        {
            return layerNumber >= GetMinLayerNumber() && layerNumber <= GetMaxLayerNumber();
        }

        /// <summary>
        /// Returns the names of all layers.
        /// </summary>
        /// <remarks>
        /// The function returns only the names of the layers which have been given a
        /// name inside the Unity Editor.
        /// </remarks>
        public static List<string> GetAllLayerNames()
        {
            // Loop through each layer
            var layerNames = new List<string>();
            for (int layerIndex = 0; layerIndex <= 31; ++layerIndex)
            {
                // Retrieve the name and if it is valid, store it inside the layer name list
                string layerName = LayerMask.LayerToName(layerIndex);
                if (!string.IsNullOrEmpty(layerName)) layerNames.Add(layerName);
            }

            // Return the layer name list
            return layerNames;
        }
        #endregion
    }
}
