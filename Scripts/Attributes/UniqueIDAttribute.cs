using UnityEngine;

namespace TaskPlanningInterface.EditorAndInspector {

    /// <summary>
    /// 
    /// This Attribute allows Unity to automatically set a string to a uniquely generated identifier (GUID). <see href="https://answers.unity.com/questions/487121/automatically-assigning-gameobjects-a-unique-and-c.html">Source</see>
    ///
    /// </summary>
    public class UniqueIDAttribute : PropertyAttribute {
        public bool locked { get; private set; }
        public UniqueIDAttribute(bool shouldBeLocked)
        {
            locked = shouldBeLocked;
        }
    }

}