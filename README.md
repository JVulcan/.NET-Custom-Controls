# .NET Custom Controls
Custom controls I have made

## .NET FormV2
Modified version of the normal Windows Forms in .NET

### What's the difference with the original one?
- When it shows is with a fade-in and fade-out effect (can be disabled too).
- Well, you can change the title bar color along with its font.
- You can adjust the border width and its color too.
- You can change the Minimize, Maximize and Exit colors buttons (Idle and Over)
- Adjust the height of the title bar (minimum 22 pixels)
- Add an EDGE color of the form

### What is missing
- The new standard (since Win 7) behavior of normal Forms: When you drag the Form along with your cursor pointer to an edge of the screen.

### How to use it
- Add the FormV2 class to your project
- Create a new Form from your Visual Studio env.
- Go to the .design file your new Form has.
- Change the inheritance code saying your new Form inherites this class
- Profit

### Screenshots
![alt tag](https://github.com/JVulcan/.NET-Custom-Controls/blob/master/images/FormV2_1.jpg)

**along with the Form, you can spot a BooleanControl (right of "Must Evaluate") and a CustomGroupBox (the container of the BooleanControl).**
![alt tag](https://github.com/JVulcan/.NET-Custom-Controls/blob/master/images/FormV2_2.jpg)

- - -
## .NET Descriptive Button
A button that shows a description when the mouse is hover, the transition to show and hide the description is full animated!

### How to use it
- Add the DescButton class to your project
- Build your project
- Now the control will be visible in the Components (YourProjectName) section of your Tools box tab (its at first)
- Drag the control to your Form
- Customize it
- Profit

### Screenshots
![alt tag](https://github.com/JVulcan/.NET-Custom-Controls/blob/master/images/DescButton_1.jpg)
![alt tag](https://github.com/JVulcan/.NET-Custom-Controls/blob/master/images/DescButton_2.jpg)
The transition IS ANIMATED!
- - -
## .NET Custom Groupbox
I was bored of use that un-customizable container, so I made a Customizable GroupBox

### How to use it
- Add the CustomGroupBox class to your project
- Build your project
- Now the control will be visible in the Components (YourProjectName) section of your Tools box tab (its at first)
- Drag the control to your Form
- Customize it
- Profit

### Screenshots
![alt tag](https://github.com/JVulcan/.NET-Custom-Controls/blob/master/images/CustomGroupbox_1.jpg)
![alt tag](https://github.com/JVulcan/.NET-Custom-Controls/blob/master/images/CustomGroupbox_2.jpg)

**Border width and color are customizable.**
- - - 
## .NET Boolean Control
Like a checkbox but with a completly different design. It has 2 types: Rounded and Squared. Rounded is bugged because the .NET "Engine" doesn't stand the quantity of re-draws (uses an animation), but I decided to let it be just in case you are curious.
JUST USE THE SQUARE TYPE!! xd

### How to use it
- Same as CustomGroupBox

### Screenshots
![alt tag](https://github.com/JVulcan/.NET-Custom-Controls/blob/master/images/BooleanControl_1.jpg)
![alt tag](https://github.com/JVulcan/.NET-Custom-Controls/blob/master/images/BooleanControl_2.jpg)

**The button is slightly highlighted when the mouse is hover it, like in the second screenshot.**
- - -

- - -
# TODO
