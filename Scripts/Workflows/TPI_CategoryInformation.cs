using TaskPlanningInterface.EditorAndInspector;
using UnityEngine;

namespace TaskPlanningInterface.Workflow {

    /// <summary>
    /// <para>
    /// The aim of this script is to store the information of a Category and thereby allowing it to be saved and passed between different C# scripts.
    /// </para>
    /// 
    /// <para>
    /// To access this script from another script, you must include the namespace (using statements) at the top,
    /// e.g. "using TaskPlanningInterface.Workflow" without the quotes.
    /// </para>
    /// 
    /// <para>
    /// Generally speaking, if you only want to use the TPI and do not want to alter its behavior,
    /// you do not need to make any changes in this script.
    /// </para>
    /// 
    /// <para>
    /// @author
    /// <br></br>Yannick Huber (yannhuber@student.ethz.ch)
    /// </para>
    /// </summary>
    [System.Serializable]
    public class TPI_CategoryInformation {

        [Tooltip("Name of the category")]
        public string categoryName;
        [Tooltip("Description of the content of the category")]
        [TextArea]
        public string categoryDescription; // optional
        [Tooltip("Icon displayed alongside the category button (optional)")]
        [Rename("Category Icon (optional)")]
        public Texture2D categoryIcon; // optional
        [UniqueID(true)]
        [Tooltip("Category ID is automatically setup by the system")]
        [HideInInspector]
        public string categoryID;

        public TPI_CategoryInformation(string _categoryName, string _categoryDescription, Texture2D _categoryIcon) {
            this.categoryName = _categoryName;
            this.categoryDescription = _categoryDescription;
            this.categoryIcon = _categoryIcon;
        }

        public TPI_CategoryInformation(string _categoryName, string _categoryDescription) {
            this.categoryName = _categoryName;
            this.categoryDescription = _categoryDescription;
            this.categoryIcon = null;
        }
    }

}