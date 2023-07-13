# Content

This folder contains all the building block prefabs that can be used for the dialog menus.

You have to following prefab options:

- **Button**: clickable button with a text field and an icon that can invoke actions. The 'OnClick' Event can be setup in the 'ButtonConfigHelper' component
- **Checkbox**: checkbox button with a text field that can be setup just like a toggle. The 'InteractableOnToggleReceiver' can be set up in the 'Events → Receivers' section of the 'Interactable' Component
- **Dropdown**: dropdown field that can display text (and images)
- **Keyboard Input**: field that receives the text that was manually input via a keyboard by the operator. Automatically opens a keyboard on the HoloLens.
- **Point Selection**: button that spawns a 'Point Selection Sphere', which can be moved to a specific point in order to select said point
- **Text Field**: field that can hold text
- **Toggle**: toggle button with a text field. The 'InteractableOnToggleReceiver' can be set up in the 'Events → Receivers' section of the 'Interactable' Component