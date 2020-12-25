using System.Xml.Serialization;
using UnityEngine;

namespace RamjetAnvil.Unity.Landmass {
    public enum TreeQuality {
        Low, Medium, High, Extreme
    }

    [System.Serializable]
    public class TerrainConfiguration {
        #region Fields

        [XmlIgnore]
        bool _isDirty;

        [SerializeField]
        private bool _drawTreesAndFoliage = true;
        [SerializeField]
        float _treeDistance = 2000f;
        [SerializeField]
        float _treeBillboardDistance = 50f;
        [SerializeField]
        float _treeCrossFadeLength = 5f;
        [SerializeField]
        int _treeMaximumFullLodCount = 50;
        [SerializeField]
        float _detailObjectDistance = 80f;
        [SerializeField]
        float _detailObjectDensity = 1f;
        [SerializeField]
        float _heightmapPixelError = 5f;
        [SerializeField]
        int _heightmapMaximumLod = 0;
        [SerializeField]
        float _basemapDistance = 1000f;
        [SerializeField]
        bool _castShadows = true;
        [SerializeField] 
        private float _snowAltitude = 2750;
        [SerializeField]
        private float _fogginess;
        [SerializeField]
        private TreeQuality _treeQuality;

        #endregion


        #region Properties

        [XmlIgnore]
        public bool IsDirty {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        public bool DrawTreesAndFoliage
        {
            get { return _drawTreesAndFoliage; }
            set {
                if (_drawTreesAndFoliage != value) {
                    _drawTreesAndFoliage = value;
                    IsDirty = true;
                }
            }
        }

        public float TreeDistance {
            get { return _treeDistance; }
            set {
                if (_treeDistance != value) {
                    _treeDistance = value;
                    IsDirty = true;
                }
            }
        }

        public float TreeBillboardDistance {
            get { return _treeBillboardDistance; }
            set {
                if (_treeBillboardDistance != value) {
                    _treeBillboardDistance = value;
                    IsDirty = true;
                }
            }
        }

        public float TreeCrossFadeLength {
            get { return _treeCrossFadeLength; }
            set {
                if (_treeCrossFadeLength != value) {
                    _treeCrossFadeLength = value;
                    IsDirty = true;
                }
            }
        }

        public int TreeMaximumFullLODCount {
            get { return _treeMaximumFullLodCount; }
            set {
                if (_treeMaximumFullLodCount != value) {
                    _treeMaximumFullLodCount = value;
                    IsDirty = true;
                }
            }
        }

        public float DetailObjectDistance {
            get { return _detailObjectDistance; }
            set {
                if (_detailObjectDistance != value) {
                    _detailObjectDistance = value;
                    IsDirty = true;
                }
            }
        }

        public float DetailObjectDensity {
            get { return _detailObjectDensity; }
            set {
                if (_detailObjectDensity != value) {
                    _detailObjectDensity = value;
                    IsDirty = true;
                }
            }
        }

        public float HeightmapPixelError {
            get { return _heightmapPixelError; }
            set {
                if (_heightmapPixelError != value) {
                    _heightmapPixelError = value;
                    IsDirty = true;
                }
            }
        }

        public int HeightmapMaximumLOD {
            get { return _heightmapMaximumLod; }
            set {
                if (_heightmapMaximumLod != value) {
                    _heightmapMaximumLod = value;
                    IsDirty = true;
                }
            }
        }

        public float BasemapDistance {
            get { return _basemapDistance; }
            set {
                if (_basemapDistance != value) {
                    _basemapDistance = value;
                    IsDirty = true;
                }
            }
        }

        public bool CastShadows {
            get { return _castShadows; }
            set {
                if (_castShadows != value) {
                    _castShadows = value;
                    IsDirty = true;
                }
            }
        }

        public float SnowAltitude {
            get { return _snowAltitude; }
            set {
                if (_snowAltitude != value) {
                    _snowAltitude = value;
                    IsDirty = true;
                }
            }
        }

        public float Fogginess {
            get { return _fogginess; }
            set {
                if (_fogginess != value) {
                    _fogginess = value;
                    IsDirty = true;
                }
            }
        }

        public TreeQuality TreeQuality {
            get { return _treeQuality; }
            set {
                if (_treeQuality != value) {
                    _treeQuality = value;
                    IsDirty = true;
                }
            }
        }

        #endregion
    }
}