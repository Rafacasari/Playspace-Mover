//#region
//using System;
//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UIExpansionKit.API;
//#endregion

//namespace PlayspaceMover
//{
//    public static class BindManager
//    {
//        public static ICustomShowableLayoutedMenu Page;
//        private static GameObject titleObject;
//        private static GameObject textObject;
//        private static bool Initialized = false;

//        public static void Initialize()
//        {
//            Page = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);

//            Page.AddLabel("Title", new Action<GameObject>((obj) => { titleObject = obj; }));
//            Page.AddLabel("Waiting for key...", new Action<GameObject>((obj) => { textObject = obj; }));

//            Page.AddSimpleButton("Accept", new Action(() =>
//            {
//                AcceptAction?.Invoke(selectedKey);
//                fetchingKeys = false;
//                Page.Hide();
//            }));

//            Page.AddSimpleButton("Cancel", new Action(() =>
//            {
               
//                CancelAction?.Invoke();
//                fetchingKeys = false;
//                Page.Hide();
//            }));

//            Initialized = true;
//        }


//        public static void Show(string title, Action<KeyCode> acceptAction, Action cancelAction)
//        {
//            if (!Initialized) return;
//            selectedKey = KeyCode.None;
//            AcceptAction = acceptAction;
//            CancelAction = cancelAction;
//            Page.Show();

//            if (titleObject != null && titleObject.GetComponentInChildren<Text>() != null) titleObject.GetComponentInChildren<Text>().text = title;

//            fetchingKeys = true;
//            MelonLoader.MelonCoroutines.Start(WaitForKey());
//        }

//        private static Action<KeyCode> AcceptAction;
//        private static Action CancelAction;

//        private static bool fetchingKeys = false;
//        public static IEnumerator WaitForKey()
//        {
//            while (fetchingKeys && textObject != null)
//            {
//                foreach (KeyCode inputKey in Enum.GetValues(typeof(KeyCode)))
//                {
//                    if ((int)inputKey < 330) continue;

//                    if (Input.GetKeyDown(inputKey)) selectedKey = inputKey;
//                }

//                if (textObject != null && selectedKey != KeyCode.None)
//                    textObject.GetComponentInChildren<Text>().text = "Waiting for key...";
//                else if (textObject != null)
//                    textObject.GetComponentInChildren<Text>().text = selectedKey.ToString();
                

//                yield return new WaitForEndOfFrame();
//            }
//            yield break;
//        }

//        private static KeyCode selectedKey = KeyCode.None;
//    }
//}
