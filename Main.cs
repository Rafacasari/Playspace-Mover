#region Usings
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
        public const string Version = "1.1.2";
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
            MelonPreferences.CreateCategory(Category, "Oculus Playspace Mover");
            MelonPreferences.CreateEntry(Category, nameof(Enabled), Enabled, "Enabled");
            MelonPreferences.CreateEntry(Category, nameof(Strength), Strength, "Strength");
            MelonPreferences.CreateEntry(Category, nameof(DoubleClickTime), DoubleClickTime, "Double Click Time");
            MelonPreferences.CreateEntry(Category, nameof(DisableDoubleClick), DisableDoubleClick, "Disable Double Click");
            MelonPreferences.CreateEntry(Category, nameof(DisableLeftHand), DisableLeftHand, "Disable Left Hand");
            MelonPreferences.CreateEntry(Category, nameof(DisableRightHand), DisableRightHand, "Disable Right Hand");

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
            Enabled = MelonPreferences.GetEntryValue<bool>(Category, nameof(Enabled));
            Strength = MelonPreferences.GetEntryValue<float>(Category, nameof(Strength));
            DoubleClickTime = MelonPreferences.GetEntryValue<float>(Category, nameof(DoubleClickTime));
            DisableDoubleClick = MelonPreferences.GetEntryValue<bool>(Category, nameof(DisableDoubleClick));
            DisableLeftHand = MelonPreferences.GetEntryValue<bool>(Category, nameof(DisableLeftHand));
            DisableRightHand = MelonPreferences.GetEntryValue<bool>(Category, nameof(DisableRightHand));
        }

        public override void OnPreferencesSaved() => ApplySettings();
        
        private OVRCameraRig Camera;
        private bool isLeftPressed, isRightPressed = false;
        private Vector3 startingOffset;
        private Vector3 StartPosition;

        private IEnumerator WaitInitialization()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return new WaitForFixedUpdate();
            
            var objects = Object.FindObjectsOfType(UnhollowerRuntimeLib.Il2CppType.Of<OVRCameraRig>());
            if (objects != null && objects.Length > 0)
            {
                Camera = objects[0].TryCast<OVRCameraRig>();
                StartPosition = Camera.trackingSpace.localPosition;
                yield break;
            }

            MelonLogger.Error("OVRCameraRig not found, this mod only work in Oculus for now!");
            yield break;
        }

        public override void OnUpdate()
        {
            if (!Enabled || Camera == null) return;

            if (!DisableDoubleClick && (HasDoubleClicked(OVRInput.Button.Three, DoubleClickTime) || HasDoubleClicked(OVRInput.Button.One, DoubleClickTime)))
            {
                Camera.trackingSpace.localPosition = StartPosition;
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
                Camera.trackingSpace.localPosition += calculatedOffset;
            }

            if (rightTrigger && !DisableRightHand)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                Vector3 calculatedOffset = (currentOffset - startingOffset) * -Strength;
                startingOffset = currentOffset;
                Camera.trackingSpace.localPosition += calculatedOffset;
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
