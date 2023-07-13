# Content

This folder contains all the editor scripts, which are only active in the Unity editor.

Alongside the attributes scripts, Unity also needs an editor script to determine what should be displayed in the Unity inspector.

Furthermore, editor scripts can also be used for entire classes (they need to inherit MonoBehaviour so that they can be attached to GameObjects) in order to determine what should be displayed in the Unity inspector.

**Generally speaking, you do not need to change anything in this folder as long as you do not want to change the displayable content. However, if you change any variables, the editor scripts also have to be updated.**
