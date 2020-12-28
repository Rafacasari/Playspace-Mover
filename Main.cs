#region Usings
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
//using System;
//using UIExpansionKit.API;
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
        public const string Version = "1.1.1";
        public const string DownloadLink = null;
    }

    public class Main : MelonMod
    {
        #region Settings
        private bool Enabled = true;
        private float Strength = 1f;
        private float DoubleClickTime = 0.25f;
        private bool DisableDoubleClick = false;
        private bool DisableLeftHand = false;
        private bool DisableRightHand = false;
        #endregion

        private readonly string Category = "PlayspaceMover";
        public override void OnApplicationStart()
        {
            MelonPrefs.RegisterCategory(Category, "Oculus Playspace Mover");
            MelonPrefs.RegisterBool(Category, nameof(Enabled), Enabled, "Enabled");
            MelonPrefs.RegisterFloat(Category, nameof(Strength), Strength, "Strength");
            MelonPrefs.RegisterFloat(Category, nameof(DoubleClickTime), DoubleClickTime, "Double Click Time");
            MelonPrefs.RegisterBool(Category, nameof(DisableDoubleClick), DisableDoubleClick, "Disable Double Click");
            MelonPrefs.RegisterBool(Category, nameof(DisableLeftHand), DisableLeftHand, "Disable Left Hand");
            MelonPrefs.RegisterBool(Category, nameof(DisableRightHand), DisableRightHand, "Disable Right Hand");

            ApplySettings();

            MelonCoroutines.Start(WaitInitialization());
            
            //if (MelonHandler.Mods.Any(x => x.Info.Name == "UI Expansion Kit"))
            //{
            //    BindManager.Initialize();

            //    var playspaceSettings = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);
            //    playspaceSettings.AddSimpleButton("Left Hand", new Action(() => BindManager.Show("Left Hand Spacedrag", new Action<KeyCode>(key =>
            //        {

            //        }), null)
            //    ));

            //    playspaceSettings.AddSimpleButton("Right Hand", new Action(() => 
            //        BindManager.Show("Right Hand Spacedrag", new Action<KeyCode>(key => {

            //        }), null)
            //    ));

            //    ExpansionKitApi.GetExpandedMenu(ExpandedMenu.SettingsMenu).AddSimpleButton("Oculus Playspace", new Action(() => playspaceSettings.Show()));
            //} 
        }

        private void ApplySettings()
        {
            Enabled = MelonPrefs.GetBool(Category, nameof(Enabled));
            Strength = MelonPrefs.GetFloat(Category, nameof(Strength));
            DoubleClickTime = MelonPrefs.GetFloat(Category, nameof(DoubleClickTime));
            DisableDoubleClick = MelonPrefs.GetBool(Category, nameof(DisableDoubleClick));
            DisableLeftHand = MelonPrefs.GetBool(Category, nameof(DisableLeftHand));
            DisableRightHand = MelonPrefs.GetBool(Category, nameof(DisableRightHand));
        }

        public override void OnModSettingsApplied() => ApplySettings();

        private VRCVrCameraOculus Camera;
        private bool isLeftPressed, isRightPressed = false;
        private Vector3 startingOffset;
        private Vector3 StartPosition;

        private IEnumerator WaitInitialization()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return new WaitForFixedUpdate();
            var objects = UnityEngine.Object.FindObjectsOfType<VRCVrCameraOculus>();
            if (objects != null && objects.Length > 0)
            {
                Camera = objects[0];
                StartPosition = Camera.cameraLiftTransform.localPosition;
                yield break;
            }

            MelonLogger.LogError("VRCVrCameraOculus not found, this mod only work in Oculus for now!");
            yield break;
        }

        public override void OnUpdate()
        {
            if (!Enabled || Camera == null) return;

            if (!DisableDoubleClick && (HasDoubleClicked(OVRInput.Button.Three, DoubleClickTime) || HasDoubleClicked(OVRInput.Button.One, DoubleClickTime)))
            {
                Camera.cameraLiftTransform.localPosition = StartPosition;
                return;
            }

            isLeftPressed = IsKeyJustPressed(OVRInput.Button.Three);
            isRightPressed = IsKeyJustPressed(OVRInput.Button.One);

            if (isLeftPressed || isRightPressed) startingOffset = OVRInput.GetLocalControllerPosition(isLeftPressed ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);

            bool leftTrigger = OVRInput.Get(OVRInput.Button.Three, OVRInput.Controller.Touch);
            bool rightTrigger = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.Touch);

            if (leftTrigger && !DisableLeftHand)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                Vector3 calculatedOffset = (currentOffset - startingOffset) * -Strength;
                startingOffset = currentOffset;
                Camera.cameraLiftTransform.localPosition += calculatedOffset;
            }

            if (rightTrigger && !DisableRightHand)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                Vector3 calculatedOffset = (currentOffset - startingOffset) * -Strength;
                startingOffset = currentOffset;
                Camera.cameraLiftTransform.localPosition += calculatedOffset;
            }
        }

        private static readonly Dictionary<OVRInput.Button, bool> PreviousStates = new Dictionary<OVRInput.Button, bool>()
        {
            { OVRInput.Button.Three, false }, { OVRInput.Button.One, false }
        };

        private static bool IsKeyJustPressed(OVRInput.Button key)
        {
            if (!PreviousStates.ContainsKey(key)) PreviousStates.Add(key, false);

            if (OVRInput.Get(key, OVRInput.Controller.Touch) && !PreviousStates[key]) return PreviousStates[key] = true;
            else return PreviousStates[key] = false;
        }

        private static readonly Dictionary<OVRInput.Button, float> lastTime = new Dictionary<OVRInput.Button, float>();

        // Thanks to Psychloor!
        // https://github.com/Psychloor/DoubleTapRunner/blob/master/DoubleTapSpeed/Utilities.cs#L30
        public static bool HasDoubleClicked(OVRInput.Button keyCode, float threshold)
        {
            if (!OVRInput.GetDown(keyCode, OVRInput.Controller.Touch)) return false;
            if (!lastTime.ContainsKey(keyCode)) lastTime.Add(keyCode, Time.time);

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
