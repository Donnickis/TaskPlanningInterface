using UnityEngine;
using TaskPlanningInterface.EditorAndInspector;

namespace TaskPlanningInterface.Helper
{

    /// <summary>
    /// <para>
    /// This script is used as a helper function to distinguish between different objects and to assign them a Globally Unique Identifier (GUID).
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top, e.g. "using TaskPlanningInterface.Helper" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior, you do not need to make any changes in this script.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    public class TPI_ObjectIdentifier : MonoBehaviour {

        [ReadOnly][Tooltip("This globally unique identifier (GUID) is used to distinguish between objects.")][SerializeField]
        private string _GUID;

        /// <summary>
        /// This globally unique identifier (GUID) is used to distinguish between objects.
        /// </summary>
        public string GUID {
            get { return _GUID; }
            set { _GUID = value; }
        }

    }

}