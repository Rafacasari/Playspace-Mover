#region Usings
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace Playerspace_Mover
{
    public static class ModInfo
    {
        public const string Name = "PlayerspaceMover";
        public const string Description = "A SteamVR Playerspace clone for Oculus Users";
        public const string Author = "Rafa";
        public const string Company = "RBX";
        public const string Version = "1.0.1";
        public const string DownloadLink = null;
    }

    public class PlayerspaceMover : MelonMod
    {
        public override void OnApplicationStart() => MelonCoroutines.Start(WaitInitialization());

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

            MelonLogger.LogError("VRCVrCameraOculus has not found, this mod only work in Oculus for now!");
            yield break;
        }


        public override void OnUpdate()
        {
            if (Camera == null) return;

            if (HasDoubleClicked(OVRInput.Button.Three, DoubleClickTime) || HasDoubleClicked(OVRInput.Button.One, DoubleClickTime))
            {
                Camera.cameraLiftTransform.localPosition = StartPosition;
                return;
            }

            isLeftPressed = IsKeyJustPressed(OVRInput.Button.Three);
            isRightPressed = IsKeyJustPressed(OVRInput.Button.One);

            if (isLeftPressed || isRightPressed) startingOffset = OVRInput.GetLocalControllerPosition(isLeftPressed ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);

            bool leftTrigger = OVRInput.Get(OVRInput.Button.Three, OVRInput.Controller.Touch);
            bool rightTrigger = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.Touch);

            if (leftTrigger)
            {

                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                Vector3 calculatedOffset = (currentOffset - startingOffset) * -1.0f;
                startingOffset = currentOffset;
                Camera.cameraLiftTransform.localPosition += calculatedOffset;
            }

            if (rightTrigger)
            {
                Vector3 currentOffset = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                Vector3 calculatedOffset = (currentOffset - startingOffset) * -1.0f;
                startingOffset = currentOffset;
                Camera.cameraLiftTransform.localPosition += calculatedOffset;
            }
        }

        private static Dictionary<OVRInput.Button, bool> PreviousStates = new Dictionary<OVRInput.Button, bool>()
        {
            { OVRInput.Button.Three, false }, { OVRInput.Button.One, false }
        };

        private static bool IsKeyJustPressed(OVRInput.Button key)
        {
            if (!PreviousStates.ContainsKey(key)) PreviousStates.Add(key, false);

            if (OVRInput.Get(key, OVRInput.Controller.Touch) && !PreviousStates[key]) return PreviousStates[key] = true;
            else return PreviousStates[key] = false;
        }

        private readonly float DoubleClickTime = 0.25f;
        private static Dictionary<OVRInput.Button, float> lastTime = new Dictionary<OVRInput.Button, float>();

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
