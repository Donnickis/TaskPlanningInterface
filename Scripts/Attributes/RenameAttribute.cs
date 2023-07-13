using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// 
    /// This Attribute allows unity to show a different name for a variable in the Unity Inspector than was assigned in the C# script. <see href="https://answers.unity.com/questions/1487864/change-a-variable-name-only-on-the-inspector.html">Source</see>
    ///
    /// </summary>
    public class RenameAttribute : PropertyAttribute {

        public string NewName { get; private set; }
        public RenameAttribute(string name) {
            NewName = name;
        }

    }

}