using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Contains functions which can be useful when working with the 'Application' class.
    /// </summary>
    public static class ApplicationHelper
    {
        #region Public Static Functions
        /// <summary>
        /// Quits the application. When the application is running in 'Editor' mode, the
        /// application will be paused. When running a build, the application will quit.
        /// </summary>
        public static void Quit()
        {
            if (Application.isEditor) Debug.Break();
            else Application.Quit();
        }
        #endregion
    }

}