# Content

This folder contains all the helper classes that were used throughout the TPI framework.

Currently, there are the following helper classes:

- **TPI_DebugTools**: holds the functions for the debug buttons
- **TPI_ObjectIdentifier**: allows the TPI to assign a unique identifier to a GameObject (GUID)
- **TPI_Photo**: allows you to take a picture with the HoloLens webcam and send it to ROS
- **TPI_SequenceMenuButton**: controlls what happens if the operator clicks on or holds a snippet or constraint in the sequence menu

**Generally speaking, you do not need to make any changes to the files in this folder except if you would like to for example change what happens if the operator clicks on a snippet in the sequence menu.**