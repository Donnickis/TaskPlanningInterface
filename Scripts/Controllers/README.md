# Content

This folder contains all the controllers that are used by the TPI. Each controller is responsible for a different area of the TPI.

| Component                      | Description of the Options                                                                                                      |
|--------------------------------|---------------------------------------------------------------------------------------------------------------------------------|
| TPI_DialogMenuController      | Options concerning the dialog menus                                                                                             |
| TPI_HandGestureController     | Options concerning the hand gestures                                                                                            |
| TPI_MainController            | Options concerning the TPI in general and the 'Workflow Hand Menu', i.e. icons and references to the different parts of the TPI |
| TPI_ObjectPlacementController | Options concerning the grid algorithm that strategically places the menus in the environment around the operator |
| TPI_RobotController           | Options concerning the digital twin of the robot                                                                                |
| TPI_ROSController             | Options concerning ROS and the 'ROS Status Menu'                                                                                |
| TPI_SequenceMenuController    | Options concerning the 'Sequence Menu' and the sequence functions (e.g. 'StartSequence()')                                      |
| TPI_TutorialController        | Options concerning the tutorial feature                                                                                         |
| TPI_WorkflowConfigurationController        | Options concerning the 'Sequence Menu' and 'Building Blocks Menu', i.e. categories, snippet options, constraint options |

Each controller script contains summaries for all the functions, sometimes also a tutorial on how to use them.

**To make any changes to the TPI, you need to alter the specific controller scripts.**