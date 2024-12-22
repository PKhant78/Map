# Custom Environment Builder

The purpose of the Custom Environment Builder is to allow users to build virtual environments/scenes for the [MRCane](https://dl.acm.org/doi/abs/10.1145/3565970.3568189) application. While not a part of the MRCane application, this is an addition to the application. Currently, MRCane has a fixed environment made by developers. This places a great limitation on the blind users using MRCane as they would only be able to explore one environment. The Custom Environment Builder will help increase the amount of environments the users can explore.

The Custom Environment Builder enables the user to create environments composed of objects based on the object selection hotbar. Furthermore, the user will be able to save current progress and load the environments they have made.

This application was developed using Unity 3D and C#.
 
## Demo

Click [here](https://play.unity.com/en/games/9b656255-4b94-45e2-83d2-f1892d04732b/environment-builder-for-mrcane) to try out the demo.

## Design
### 1. Tilemap System
- A grid system that allows the user to place objects and maintain consistent spacing.
- A boundary is set so the user cannot place objects outside of this boundary.

### 2. Object Placement
- Place objects by drag-and-drop.
- Cancel object placement.
- Rotate objects.
- Delete objects.
- Confirm object placement.
- Double-click to reselect an object.

### 3. Object Selection
- Select different objects from various categories.
- Change the scale of objects.

### 4. Save/Load the Environment
- Save the environment to a JSON file.
- Load the environment from a JSON file.

## User Interface
- Navigate between four categories of objects (Exterior, Desks, Shapes)
- Select objects from these categories and click-and-drag them to position them about the tilemap within a boundary
- Delete, place, and rotate the currently selected by 90 degress. Once an object is placed, it can be double-click to be moved again
- Select another object while currently selecting one to swap currently selected objects
- Click on the Trash icon to begin using the Delete Tool. Use the delete tool to delete objects along the tilemap
- Scale the width of the currently selected object with the draggable scale hud which appears on object selection
- Object width can be scaled up to 10 times it original width or shrunk to half its original width

## Limitations

- The Save/Load feature does not work with the demo version because of security issues
- Performance issues due to Unity WebGL

We are actively working to resolve these issues in future updates. Thank you for your patience.


## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

Please make sure to update tests as appropriate.

## Acknowledegments

- MRCane Team
- StandaloneFileBrowser (plugin)
