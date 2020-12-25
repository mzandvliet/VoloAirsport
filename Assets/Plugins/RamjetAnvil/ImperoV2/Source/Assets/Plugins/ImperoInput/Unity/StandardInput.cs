using System;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Impero.StandardInput {
    public enum ButtonState { Released, Pressed }
    public enum ButtonEvent { Nothing, Down, Up }

    public static class Adapters {

        public static ButtonState Bool2ButtonState(bool isPressed) {
            return isPressed ? ButtonState.Pressed : ButtonState.Released;
        }

        public static bool ButtonState2Bool(ButtonState b) {
            return b == ButtonState.Pressed;
        }

        public static float Button2Axis(ButtonState source) {
            return source == ButtonState.Pressed ? 1.0f : 0.0f;
        }

        /// <summary>
        ///     Warning, only works on positive axis input
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="gravity"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Func<ButtonState, float> Button2AccumulatedAxis(float speed, float gravity, Func<float> deltaTime) {
            float currentValue = 0.0f;
            return buttonState => {
                if (buttonState == ButtonState.Pressed) {
                    currentValue += speed*deltaTime();
                } else {
                    currentValue -= gravity*deltaTime();
                }
                currentValue = Mathf.Clamp(currentValue, 0f, 1f);
                return currentValue;
            };
        }

        public static Func<float, ButtonState> Axis2Button(float threshold) {
            return axisState => Mathf.Abs(axisState) > threshold ? ButtonState.Pressed : ButtonState.Released;
        }

        public static ButtonState Invert(ButtonState buttonState) {
            return buttonState == ButtonState.Pressed ? ButtonState.Released : ButtonState.Pressed;
        }

        public static bool Invert(bool b) { return !b; }

        public static float Invert(float f) { return -f; }

        public static float Abs(float axisState) {
            return Mathf.Abs(axisState);
        }

        public static float FilterNegativeInput(float axisState) {
            return Mathf.Min(0.0f, axisState);
        }

        public static float FilterPositiveInput(float axisState) {
            return Mathf.Max(0.0f, axisState);
        }

        public static Func<float, float> ApplySensitivity(float sensitivity, float range = float.PositiveInfinity) {
            return axisState => Mathf.Clamp(axisState * sensitivity, -range, range);
        }

        public static Func<float, float> Deadzone(float deadzone) {
            return axisState => {
                float scalingFactor = 1 - deadzone;
                if (Mathf.Abs(axisState) < deadzone) {
                    return 0f;
                }

                return ((Mathf.Abs(axisState) - deadzone) / scalingFactor) * Mathf.Sign(axisState);
            };
        }

        public static Vector2 ApplyDeadzone(float deadzone, Vector2 stickInput) {
            float inputMagnitude = stickInput.magnitude;

            if (inputMagnitude < deadzone) {
                return Vector2.zero;
            }

            // rescale the clipped input vector into the non-dead zone space
            stickInput *= (inputMagnitude - deadzone) / ((1f - deadzone) * inputMagnitude);
            return stickInput;
        }

        /// <summary>
        ///     Taken from: http://www.third-helix.com/2013/04/doing-thumbstick-dead-zones-right/
        ///     , and https://gist.github.com/stfx/5372176
        /// </summary>
        public static Func<Vector2, Vector2> StickDeadzone(float deadzone) {
            return stickInput => {
                float inputMagnitude = stickInput.magnitude;

                if (inputMagnitude < deadzone) {
                    return Vector2.zero;
                }

                // rescale the clipped input vector into the non-dead zone space
                stickInput *= (inputMagnitude - deadzone) / ((1f - deadzone) * inputMagnitude);
                return stickInput;
            };
        }

        // Combine functions

        public static Vector2 MergeAxes(Vector2 axisState1, Vector2 axisState2) {
            return axisState1.magnitude > axisState2.magnitude ? axisState1 : axisState2;
        }

        public static Vector2 MergeAxes(params Vector2[] axesState) {
            var mergedAxis = Vector2.zero;
            for (int i = 0; i < axesState.Length; i++) {
                var axisState = axesState[i];
                mergedAxis = axisState.magnitude > mergedAxis.magnitude ? axisState : mergedAxis;
            }
            return mergedAxis;
        }

        /// <summary>
        ///     Returns the axis state that has the greatest input value (either negative or positive)
        /// </summary>
        public static float MergeAxes(float axisState1, float axisState2) {
            return Mathf.Abs(axisState1) > Mathf.Abs(axisState2) ? axisState1 : axisState2;
        }

        public static float MergeAxes(params float[] axesState) {
            var mergedAxis = 0f;
            for (int i = 0; i < axesState.Length; i++) {
                var axisState = axesState[i];
                mergedAxis = Mathf.Abs(axisState) > Mathf.Abs(mergedAxis) ? axisState : mergedAxis;
            }
            return mergedAxis;
        }

        public static float CombineAxes(float axis1, float axis2) {
            return Mathf.Clamp(axis1 + axis2, -1f, 1f);
        }

        public static float CombineAxes(params float[] axesState) {
            var mergedAxis = 0f;
            for (int i = 0; i < axesState.Length; i++) {
                var axisState = axesState[i];
                mergedAxis = Mathf.Clamp(axisState + mergedAxis, -1f, 1f);
            }
            return mergedAxis;
        }

        public static Vector2 CombineAxes(Vector2 axis1, Vector2 axis2) {
            return Vector2.ClampMagnitude(axis1 + axis2, 1f);
        }

        public static Vector2 CombineAxes(params Vector2[] axesState) {
            var mergedAxis = Vector2.zero;
            for (int i = 0; i < axesState.Length; i++) {
                var axisState = axesState[i];
                mergedAxis = Vector2.ClampMagnitude(axisState + mergedAxis, 1f);
            }
            return mergedAxis;
        }

        public static ButtonState MergeButtons(ButtonState s1, ButtonState s2) {
            return s1 == ButtonState.Pressed || s2 == ButtonState.Pressed
                ? ButtonState.Pressed
                : ButtonState.Released;
        }

        public static ButtonState MergeButtons(params ButtonState[] buttonStates) {
            var mergedButtonState = ButtonState.Released;
            for (int i = 0; i < buttonStates.Length; i++) {
                var buttonState = buttonStates[i];
                mergedButtonState = buttonState == ButtonState.Pressed || mergedButtonState == ButtonState.Pressed
                    ? ButtonState.Pressed
                    : ButtonState.Released;
            }
            return mergedButtonState;
        }

        

        public static Func<ButtonState, ButtonEvent> ButtonEvents(Func<int> frameCount) {
            return MergePreviousAndCurrent<ButtonState, ButtonEvent>(
                 mergeFn: (previous, current) => {
                     if (previous == ButtonState.Released && current == ButtonState.Pressed) {
                         return ButtonEvent.Down;
                     } else if (previous == ButtonState.Pressed && current == ButtonState.Released) {
                         return ButtonEvent.Up;
                     } else {
                         return ButtonEvent.Nothing;
                     }
                 },
                 currentFrame: frameCount,
                 defaultOutput: ButtonEvent.Nothing);
        }
        
        public static Func<TInput, TOutput> MergePreviousAndCurrent<TInput, TOutput>(
            Func<TInput, TInput, TOutput> mergeFn, 
            Func<int> currentFrame, 
            TOutput defaultOutput = default(TOutput)) {

            var previousState = default(TInput);
            var previousFrame = -2;
            Func<TInput, TOutput> adapter = currentState => {
                var isNextFrame = previousFrame + 1 == currentFrame();
                TOutput output = isNextFrame ? mergeFn(previousState, currentState) : defaultOutput;
                previousState = currentState;
                previousFrame = currentFrame();
                return output;
            };
            return ImperoCore.CacheByFrame(adapter, currentFrame);
        }

        public static Func<TInput, TOutput> StatefulAdapter<TInput, TOutput, TState>(
            Func<TState, TInput, Tuple<TOutput, TState>> adapter, 
            TState initialState) {

            TState state = initialState;
            return input => {
                var output = adapter(state, input);
                state = output._2;
                return output._1;
            };
        }

        public static Func<ButtonState, ButtonState> MinHoldDuration(Func<TimeSpan> deltaTime, TimeSpan holdDuration) {
            return StatefulAdapter<ButtonState, ButtonState, TimeSpan>(
                (buttonPressDuration, input) => {
                    if (input == ButtonState.Pressed) {
                        buttonPressDuration += deltaTime();
                    } else {
                        buttonPressDuration = TimeSpan.Zero;
                    }

                    if (buttonPressDuration >= holdDuration) {
                        return new Tuple<ButtonState, TimeSpan>(ButtonState.Pressed, buttonPressDuration);
                    }
                    return new Tuple<ButtonState, TimeSpan>(ButtonState.Released, buttonPressDuration);
                },
                TimeSpan.Zero);
        }
        

        public static Func<ButtonState, ButtonState> MaxHoldDuration(Func<TimeSpan> currentTime, TimeSpan maxholdDuration) {
            return StatefulAdapter<ButtonState, ButtonState, Maybe<TimeSpan>>(
                (holdTimestamp, input) => {
                    // TODO Simplify!

                    if (input == ButtonState.Pressed) {
                        return new Tuple<ButtonState, Maybe<TimeSpan>>(ButtonState.Released, holdTimestamp.IsJust ? holdTimestamp : Maybe.Just(currentTime()));
                    } else if (input == ButtonState.Released) {
                        Maybe<TimeSpan> holdDuration;
                        if (holdTimestamp.IsJust) {
                            holdDuration = Maybe.Just(currentTime() - holdTimestamp.Value);
                        } else {
                            holdDuration = Maybe<TimeSpan>.Nothing;
                        }

                        if (holdDuration.IsJust && holdDuration.Value <= maxholdDuration) {
                            return new Tuple<ButtonState, Maybe<TimeSpan>>(ButtonState.Pressed, Maybe<TimeSpan>.Nothing);
                        }
                        return new Tuple<ButtonState, Maybe<TimeSpan>>(ButtonState.Released, Maybe<TimeSpan>.Nothing);
                    }
                    return new Tuple<ButtonState, Maybe<TimeSpan>>(ButtonState.Released, Maybe<TimeSpan>.Nothing);
                },
                Maybe.Nothing<TimeSpan>());
        }


        public static Func<Vector2, Vector2> DiscreteAxisInput(
            float lowerAxisThreshold = 0.2f,
            float upperAxisThreshold = 0.6f) {
            var isNewInputAllowed = true;
            return currentInput => {
                Vector2 discreteInput = Vector2.zero;

                var currentInputMagnitude = currentInput.magnitude;
                if (isNewInputAllowed && currentInputMagnitude > upperAxisThreshold) {
                    discreteInput = currentInput;
                    isNewInputAllowed = false;
                } else if (currentInputMagnitude < lowerAxisThreshold) {
                    isNewInputAllowed = true;
                }

                return discreteInput;
            };
        }

    }
}