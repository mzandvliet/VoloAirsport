using UnityEngine;
using System.Threading;

namespace RTEditor
{
    public abstract class SilentJob
    {
        #region Private Variables
        private bool _isRunning = false;
        private Thread _thread = null;
        private object _lockHandle = new object();
        #endregion

        #region Public Properties
        public bool IsRunning
        {
            get
            {
                bool temp;
                lock(_lockHandle)
                {
                    temp = _isRunning;
                }
                return temp;
            }
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            if (IsRunning) return;

            _thread = new Thread(JobThread);
            _thread.Start();
        }

        public void Abort()
        {
            if (IsRunning) _thread.Abort();
        }
        #endregion

        #region Protected Abstract Methods
        protected abstract void DoJob();
        #endregion

        #region Private Methods
        private void JobThread()
        {
            SetIsRunning(true);
            DoJob();
            SetIsRunning(false);
        }

        private void SetIsRunning(bool isRunning)
        {
            lock(_lockHandle)
            {
                _isRunning = isRunning;
            }
        }
        #endregion
    }
}
