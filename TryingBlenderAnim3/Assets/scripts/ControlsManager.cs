using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class ControlsManager
{

    public enum ButtonType
    {
        Horizontal, Vertical, MouseX, MouseY, Jump, Walk, Climb, ResetCam, Interact, Pause, Back, Attack
    }

    public enum InputDevice
    {
        keyboard,
        controller
    }

    public InputValues keyboard, controller;
    public InputValues[] values;

    private ButtonType[] axes = { ButtonType.Horizontal, ButtonType.Vertical, ButtonType.MouseX, ButtonType.MouseY };
    private ButtonType[] buttons = { ButtonType.Jump, ButtonType.Walk, ButtonType.Climb, ButtonType.ResetCam, ButtonType.Interact, ButtonType.Pause, ButtonType.Back, ButtonType.Attack };
    private string[] inputNames = { "Horizontal", "Vertical", "MouseX", "MouseY", "Jump", "Walk", "Climb", "ResetCam", "Interact", "Pause", "Back" , "Attack"};
    private string keyboardAppend = "K";
    private string controllerWinAppend = "Win";
    private string controllerMacAppend = "Mac";

    private InputDevice recentDevice = InputDevice.keyboard;

    public InputDevice RecentDevice
    {
        get
        {
            return recentDevice;
        }
    }

    public void updateRecentInputDevice()
    {
        foreach (ButtonType axis in axes)
        {
            if (Mathf.Abs(controller.GetAxis(axis)) > 0.3f)
            {
                recentDevice = InputDevice.controller;
                return;
            }
            if (Mathf.Abs(keyboard.GetAxis(axis)) > 0.3f)
            {
                recentDevice = InputDevice.keyboard;
                return;
            }
        }


        foreach (ButtonType button in buttons)
        {
            if (controller.GetButton(button) || controller.GetButtonDown(button) || controller.GetButtonUp(button))
            {
                recentDevice = InputDevice.controller;
                return;
            }

            if (keyboard.GetButton(button) || keyboard.GetButtonDown(button) || keyboard.GetButtonUp(button))
            {
                recentDevice = InputDevice.keyboard;
                return;
            }
        }
    }

    public void Init()
    {

        keyboard = new InputValues(keyboardAppend, inputNames[0], inputNames[1], inputNames[2], inputNames[3], inputNames[4], inputNames[5], inputNames[6], inputNames[7], inputNames[8], inputNames[9], inputNames[10], inputNames[11]);

        bool windows = (Application.platform.Equals(RuntimePlatform.WindowsPlayer) || Application.platform.Equals(RuntimePlatform.WindowsEditor));
        string platformExtension = windows ? controllerWinAppend : controllerMacAppend;
        controller = new InputValues(platformExtension, inputNames[0], inputNames[1],
            inputNames[2], inputNames[3], inputNames[4], inputNames[5], inputNames[6], inputNames[7], inputNames[8], inputNames[9], inputNames[10], inputNames[11]);

        values = new InputValues[2];
        values[0] = keyboard;
        values[1] = controller;
    }

    public float GetAxis(ButtonType type)
    {
        float max = 0f;
        foreach (InputValues inputValue in values)
        {
            float val = inputValue.GetAxis(type);
            if (Mathf.Abs(val) > Mathf.Abs(max))
                max = val;
        }
        return max;
    }

    public bool GetButton(ButtonType type)
    {
        return keyboard.GetButton(type) || controller.GetButton(type);
    }

    public bool GetButtonDown(ButtonType type)
    {
        return keyboard.GetButtonDown(type) || controller.GetButtonDown(type);
    }
    public bool GetButtonUp(ButtonType type)
    {
        return keyboard.GetButtonUp(type) || controller.GetButtonUp(type);
    }

    public class InputValues
    {
        Dictionary<ButtonType, string> buttonToName;

        public InputValues(string append, string Horizontal, string Vertical, string MouseX, string MouseY,
            string Jump, string Walk, string Climb, string ResetCam, string Interact, string Pause, string Back, string Attack)
        {
            buttonToName = new Dictionary<ButtonType, string>();

            buttonToName[ButtonType.Horizontal] = Horizontal + append;
            buttonToName[ButtonType.Vertical] = Vertical + append;
            buttonToName[ButtonType.MouseX] = MouseX + append;
            buttonToName[ButtonType.MouseY] = MouseY + append;
            buttonToName[ButtonType.Jump] = Jump + append;
            buttonToName[ButtonType.Walk] = Walk + append;
            buttonToName[ButtonType.Climb] = Climb + append;
            buttonToName[ButtonType.ResetCam] = ResetCam + append;
            buttonToName[ButtonType.Interact] = Interact + append;
            buttonToName[ButtonType.Pause] = Pause + append;
            buttonToName[ButtonType.Back] = Back + append;
            buttonToName[ButtonType.Attack] = Attack + append;
        }

        public float GetAxis(ButtonType type)
        {
            return CrossPlatformInputManager.GetAxis(buttonToName[type]);
        }

        public bool GetButton(ButtonType type)
        {
            return CrossPlatformInputManager.GetButton(buttonToName[type]);
        }

        public bool GetButtonDown(ButtonType type)
        {
            return CrossPlatformInputManager.GetButtonDown(buttonToName[type]);
        }
        public bool GetButtonUp(ButtonType type)
        {
            return CrossPlatformInputManager.GetButtonUp(buttonToName[type]);
        }

    }
}
