using System;
using System.Collections.Generic;
using TaskPlanningInterface.EditorAndInspector;
using UnityEngine;

namespace TaskPlanningInterface.Workflow {

    /// <summary>
    /// <para>
    /// The aim of this script is to store the main information of a workflow and thereby allowing it to be saved and passed between different C# scripts.
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
    public class TPI_Workflow {
        [Tooltip("Name of the workflow")][ReadOnly]
        public string workflowName;
        [Tooltip("ID of the workflow")][ReadOnly]
        public string workflowID;
        [Tooltip("List containing the IDs of all the snippets of the workflow")][ReadOnly]
        public List<TPI_WorkflowIDAndType> workflowSnippets;
        [Tooltip("List containing the IDs of all the constraint of the workflow")][ReadOnly]
        public List<TPI_WorkflowIDAndType> workflowConstraints;

        public TPI_Workflow(string _workflowID = "") {
            if (_workflowID == "") {
                Guid guid = Guid.NewGuid();
                workflowID = guid.ToString();
            } else
                this.workflowID = _workflowID;
            workflowName = "";
            workflowSnippets = new List<TPI_WorkflowIDAndType>();
            workflowConstraints = new List<TPI_WorkflowIDAndType>();
        }

        public TPI_Workflow(string workflowName, List<TPI_WorkflowIDAndType> workflowSnippets, List<TPI_WorkflowIDAndType> workflowConstraints) {
            Guid guid = Guid.NewGuid();
            this.workflowID = guid.ToString();
            this.workflowName = workflowName;
            this.workflowSnippets = workflowSnippets;
            this.workflowConstraints = workflowConstraints;
        }
    }

    /// <summary>
    /// <para>
    /// The aim of this script is to allow unity to store the information of the workflows in a JSON file.
    /// <br></br>It allows the Task Planning Interface to only save and load the desired list instead of painfully declaring what should be saved and what not with tags and attributes.
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
    public class TPI_WorkflowSaveData {
        [Tooltip("List of all workflows that were saved during runtime or loaded at startup")]
        public List<TPI_Workflow> workflows;
        public TPI_WorkflowSaveData() {
            workflows = new List<TPI_Workflow>();
        }
    }

    /// <summary>
    /// <para>
    /// This is a helper class, which allows saving and loading of the snippets and constraints.
    /// <br></br> <paramref name="saveDataClassType"/> allows the Task Planning Interface to easily distinguish between the different types of snippet and constraint function classes (classes that derive from TPI_Snippet and TPI_Constraint respectively).
    /// <br></br> Therefore, it is a workaround for the JSON serialization problematic with the allowed data types.
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
    public class TPI_WorkflowIDAndType {

        [Tooltip("Id of the item")][ReadOnly]
        public string GUID;
        [Tooltip("Class Type of the saveData class belonging to this item")][ReadOnly]
        public string saveDataClassType;

        public TPI_WorkflowIDAndType(string ID, string classType) {
            this.GUID = ID;
            this.saveDataClassType = classType;
        }

    }

}
