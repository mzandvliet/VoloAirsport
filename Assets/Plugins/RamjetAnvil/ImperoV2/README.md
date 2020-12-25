#Impero

Helps you deal with the difficulties of supporting a vast number of input devices in your game and avoids the problem of cluttering your game logic with input device specific code.

Impero is a library that deals with all the details of wiring up different input devices into a game and coupling them to in-game actions. For example, the library allows you to wire up a keyboard 'up arrow' key and a joystick's 'left up axis' into one in-game action called 'walk forward'.

##Features

- Allows unification of axis input and button input by converting the one to the other and vice versa.
- Maps an in-game action (e.g. 'jump') to one or more inputs (e.g. 'up arrow', 'left axis, up').
- Serialization of input mapping, e.g. remembering that 'jump' is mapped to 'up arrow' and 'left axis up'.
- Built with extensibility in mind. The library is not limited to buttons and axes or certain peripherals.

##Usage

##Introduction

The library is built around one core principle, which is separating how input is polled from actually polling it. 

Recall that polling input in Unity works like this:

    Input.GetKey(KeyCode.UpArrow);
    
What Impero does is lift out the identifier (the how), in this case KeyCode.UpArrow, and store it separately from the actual polling method, which allows you to do something like this:

    InputMap<KeyCode, bool> keyboard = Impero.Core.ToInputMap(Range(KeyCode.Backspace, KeyCode.Break), Input.GetKey);
    Func<bool> pollUpArrow = keyboard.[KeyCode.UpArrow];
    pollUpArrow();
    
_(Now, this may seem like a lot of code but hold on Impero has shortcuts for this. The goal of this example is just to make you understand on what principles the library is built so that you can get a good understanding of what it is capable of.)_

We can now change this pollUpArrow quite easily into something else. Let's say we don't really like working with booleans because they don't explain much about the domain and we want to change the booleans coming out of pollUpArrow into something more meaningful like a ButtonState: 

    public enum ButtonState { Pressed, Released }
    
    Func<ButtonState> newPollUpArrow = Impero.Core.AdaptInput(
    	adapter: boolean => boolean == true ? ButtonState.Pressed : ButtonState.Released,
    	pollFn: pollUpArrow);
    
See how the adapter we provide here nicely converts any booleans coming in into ButtonStates. Now we can call newPollUpArrow and we get a ButtonState back:

    if(newPollUpArrow() == ButtonState.Pressed)
    {
    	player.MoveForward();
    	...
    }

This UpArrow is still tied to the keyboard but in my game code I don't really care where the input is coming from, let's do something about that:

    public enum InputAction { WalkForward, Jump, Crouch }
    
    InputMap<InputAction, ButtonState> actionMap = new InputMap(InputAction.WalkForward, newPollUpArrow);
    
Now we can poll the button state of this up arrow from the InputAction instead of using the KeyCode for look up:

    InputMap<InputAction, ButtonState> actionMap = ...;
    Func<ButtonState> pollWalkForward = actionMap.[InputAction.WalkForward];
    if(pollWalkForward() == ButtonState.Pressed)
    {
    	player.MoveForward();
    	...
    }
    
And using Impero's Poll shortcut for readability:

    if(actionMap.Poll(InputAction.WalkForward) == ButtonState.Pressed)
    {
    	player.MoveForward();
    	...
    }

With this at the basis of the library we can go a little further by introducing concepts like merging two different input types and dive a little deeper into the meaning of the input map.
