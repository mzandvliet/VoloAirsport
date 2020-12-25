using UnityEngine;

namespace RTEditor
{
    public class XZGridCell
    {
        #region Private Variables
        private int _cellIndexX;
        private int _cellIndexZ;
        private XZGrid _parentGrid;
        #endregion

        #region Public Properties
        public int CellIndexX { get { return _cellIndexX; } }
        public int CellIndexZ { get { return _cellIndexZ; } }
        public XZGrid ParentGrid { get { return _parentGrid; } }
        #endregion

        #region Public Constructors
        public XZGridCell(int cellIndexX, int cellIndexZ, XZGrid parentGrid)
        {
            _cellIndexX = cellIndexX;
            _cellIndexZ = cellIndexZ;
            _parentGrid = parentGrid;
        }
        #endregion
    }
}