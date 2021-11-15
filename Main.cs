#region
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace PlayspaceMover
{
    public static class ModInfo
    {
        public const string Name = "OculusPlayspaceMover";
        public const string Description = "A SteamVR's Playspace clone for VRChat from Oculus Store";
        public const string Author = "Rafa";
        public const string Company = "RBX";
        public const string Version = "1.2.0";
        public const string DownloadLink = "https://github.com/Rafacasari/Playerspace-Mover/releases/latest/download/PlayspaceMover.dll";
    }

    public class Main : MelonMod
    {
        #region Settings
        private readonly string Category = "PlayspaceMover";
        private bool Enabled = true;
        private float Strength = 1f;
        private float DoubleClickTime = 0.25f;
        private bool DisableDoubleClick;
        private bool DisableLeftHand;
        private bool DisableRightHand;
        #endregion
        
        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory(Category, "Oculus Playspace Mover");
            MelonPreferences.CreateEntry(Category, nameof(Enabled), Enabled, "Enabled");
            MelonPreferences.CreateEntry(Category, nameof(Strength), Strength, "Strength");
            MelonPreferences.CreateEntry(Category, nameof(DoubleClickTime), DoubleClickTime, "Double Click Time");
            MelonPreferences.CreateEntry(Category, nameof(DisableDoubleClick), DisableDoubleClick, "Disable Double Click");
            MelonPreferences.CreateEntry(Category, nameof(DisableLeftHand), DisableLeftHand, "Disable Left Hand");
            MelonPreferences.CreateEntry(Category, nameof(DisableRightHand), DisableRightHand, "Disable Right Hand");

            ApplySettings();

            MelonCoroutines.Start(WaitInitialization());
        }

        private void ApplySettings()
        {
            Enabled = MelonPreferences.GetEntryValue<bool>(Category, nameof(Enabled));
            Strength = MelonPreferences.GetEntryValue<float>(Category, nameof(Strength));
            DoubleClickTime = MelonPreferences.GetEntryValue<float>(Category, nameof(DoubleClickTime));
            DisableDoubleClick = MelonPreferences.GetEntryValue<bool>(Category, nameof(DisableDoubleClick));
            DisableLeftHand = MelonPreferences.GetEntryValue<bool>(Category, nameof(DisableLeftHand));
            DisableRightHand = MelonPreferences.GetEntryValue<bool>(Category, nameof(DisableRightHand));
        }

        public override void OnPreferencesSaved() => ApplySettings();
        
        private OVRCameraRig Camera;
        private OVRInput.Controller LastPressed; 
        private Vector3 startingOffset;
        private Vector3 StartPosition;

        private IEnumerator WaitInitialization()
        {
            // Wait for the VRCUiManager
            while (VRCUiManager.prop_VRCUiManager_0 == null)
            {
                yield return new WaitForFixedUpdate();
            }
            
            var objects = Object.FindObjectsOfType(UnhollowerRuntimeLib.Il2CppType.Of<OVRCameraRig>());
            if (objects != null && objects.Length > 0)
            {
                Camera = objects[0].TryCast<OVRCameraRig>();
                StartPosition = Camera.trackingSpace.localPosition;
                yield break;
            }

            MelonLogger.Error("OVRCameraRig not found, this mod only work on Oculus! If u are using SteamVR, use the OVR Advanced Settings!");
        }

        public override void OnUpdate()
        {
            if (!Enabled || Camera == null)
            {
                return;
            }

            if (!DisableDoubleClick && (HasDoubleClicked(OVRInput.Button.Three, DoubleClickTime) || HasDoubleClicked(OVRInput.Button.One, DoubleClickTime)))
            {
                Camera.trackingSpace.localPosition = StartPosition;
                return;
            }

            bool isLeftPressed = IsKeyJustPressed(OVRInput.Button.Three);
            bool isRightPressed = IsKeyJustPressed(OVRInput.Button.One);

            if (isLeftPressed || isRightPressed)
            {
                startingOffset = OVRInput.GetLocalControllerPosition(isLeftPressed ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);

                if (isLeftPressed)
                {
                    LastPressed = OVRInput.Controller.LTouch;
                }
                else if (isRightPressed)
                {
                    LastPressed = OVRInput.Controller.RTouch;
                }
            }

            bool leftTrigger = OVRInput.Get(OVRInput.Button.Three, OVRInput.Controller.Touch);
            bool rightTrigger = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.Touch);

            if (leftTrigger && LastPressed == OVRInput.Controller.LTouch && !DisableLeftHand)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                Vector3 calculatedOffset = (currentOffset - startingOffset) * -Strength;
                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;
            }

            if (rightTrigger && LastPressed == OVRInput.Controller.RTouch && !DisableRightHand)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                Vector3 calculatedOffset = (currentOffset - startingOffset) * -Strength;
                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;
            }
        }

        private static readonly Dictionary<OVRInput.Button, bool> PreviousStates = new Dictionary<OVRInput.Button, bool>
        {
            { OVRInput.Button.Three, false }, { OVRInput.Button.One, false }
        };

        private static bool IsKeyJustPressed(OVRInput.Button key)
        {
            if (!PreviousStates.ContainsKey(key))
            {
                PreviousStates.Add(key, false);
            }

            return PreviousStates[key] = OVRInput.Get(key, OVRInput.Controller.Touch) && !PreviousStates[key];
        }

        private static readonly Dictionary<OVRInput.Button, float> lastTime = new Dictionary<OVRInput.Button, float>();

        // Thanks to Psychloor!
        // https://github.com/Psychloor/DoubleTapRunner/blob/master/DoubleTapSpeed/Utilities.cs#L30
        public static bool HasDoubleClicked(OVRInput.Button keyCode, float threshold)
        {
            if (!OVRInput.GetDown(keyCode, OVRInput.Controller.Touch))
            {
                return false;
            }

            if (!lastTime.ContainsKey(keyCode))
            {
                lastTime.Add(keyCode, Time.time);
            }

            if (Time.time - lastTime[keyCode] <= threshold)
            {
                lastTime[keyCode] = threshold * 2;
                return true;
            }

            lastTime[keyCode] = Time.time;
            return false;
        }
    }
}
