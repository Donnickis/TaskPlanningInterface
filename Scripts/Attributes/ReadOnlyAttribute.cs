using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// 
    /// This Attribute allows Unity to lock a variable in the Inspector, therefore set it to a "ReadOnly" state. <see href="https://gist.github.com/LotteMakesStuff/c0a3b404524be57574ffa5f8270268ea">Source</see>
    ///
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }

}