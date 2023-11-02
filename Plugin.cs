using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using System;


namespace ChatUtilities
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "com.metalted.zeepkist.chatutilities";
        public const string pluginName = "Chat Utilities";
        public const string pluginVersion = "1.4";
        
        //Alphanumerical characters.
        public static string[] quickCodes = new string[]
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
            "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z"
        };

        //Alphanumerical keycodes.
        public static KeyCode[] quickKeyCodes = new KeyCode[]
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0,
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
            KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
            KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z
        };

        //This dictionary contains the keydown state for each keycode in the array. This is required because OnGUI is called multiple times per frame, so general Input.GetKeyDown will register multiple frames.
        public static Dictionary<KeyCode, bool> keyDown = new Dictionary<KeyCode, bool>();

        //Reference to the current chat UI.
        public static OnlineChatUI onlineChatUI;
        public static bool emoticonKeyHeld = false;
        public static bool commandKeyHeld = false;
        public static bool clipboardKeyDown = false;
        public static bool clearKeyDown = false;
        public static bool closeKeyDown = false;

        public enum ChatUtilityState { Closed, Emoticon, Command};
        public static ChatUtilityState currentState = ChatUtilityState.Closed;

        private void Awake()
        {
            ConfigManagement.Initialize(Config);

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();

            Logger.LogInfo($"Plugin {pluginName} is loaded!");

            foreach(KeyCode kc in quickKeyCodes)
            {
                keyDown.Add(kc, false);
            }

            TextureManagement.Initialize();
        }

        public void Update()
        {
            emoticonKeyHeld = Input.GetKey((KeyCode)ConfigManagement.emoticonKey.BoxedValue);
            commandKeyHeld = Input.GetKey((KeyCode)ConfigManagement.commandKey.BoxedValue);
            clipboardKeyDown = Input.GetKeyDown((KeyCode)ConfigManagement.clipboardKey.BoxedValue);
            clearKeyDown = Input.GetKeyDown((KeyCode)ConfigManagement.clearKey.BoxedValue);
            closeKeyDown = Input.GetKeyDown((KeyCode)ConfigManagement.closeKey.BoxedValue);

            foreach (KeyCode kc in quickKeyCodes)
            {
                keyDown[kc] = Input.GetKeyDown(kc);
            }
        }

        public void OnGUI()
        {
            //Return if conditions arent met.
            if(onlineChatUI == null)
            {
                currentState = ChatUtilityState.Closed;
                return;
            }

            if (!onlineChatUI.currentlyTyping)
            {
                currentState = ChatUtilityState.Closed;
                return;
            }

            Rect chatRect;
            if(!RectManagement.GetChatRect(out chatRect))
            {
                currentState = ChatUtilityState.Closed;
                return;
            }

            //Show the buttons for the different categories.
            Rect emoticonButtonRect = RectManagement.GetRectRightOfChat(chatRect, 0);
            Rect commandButtonRect = RectManagement.GetRectRightOfChat(chatRect, 1);
            Rect clipboardButtonRect = RectManagement.GetRectRightOfChat(chatRect, 2);
            Rect clearButtonRect = RectManagement.GetRectRightOfChat(chatRect, 3);
            Rect closeButtonRect = RectManagement.GetRectRightOfChat(chatRect, 4);
            
            if(GUI.Button(emoticonButtonRect, ":)") || emoticonKeyHeld)
            {
                currentState = ChatUtilityState.Emoticon;
            }
            
            if(GUI.Button(commandButtonRect, ">_") || commandKeyHeld)
            {
                currentState = ChatUtilityState.Command;
            }

            if(GUI.Button(clipboardButtonRect, "CB") || clipboardKeyDown)
            {
                string clipboard = GUIUtility.systemCopyBuffer;
                if(!string.IsNullOrEmpty(clipboard))
                {
                    ApplyMessage(clipboard);
                }
                clipboardKeyDown = false;
            }

            if(GUI.Button(clearButtonRect, "CE") || clearKeyDown)
            {
                OnlineChatUI.currentMessage = "";
                clearKeyDown = false;
            }

            if (GUI.Button(closeButtonRect, "x") || closeKeyDown)
            {
                currentState = ChatUtilityState.Closed;
                closeKeyDown = false;
            }

            //What message are we adding right now ? 
            string messageToAdd = "";

            switch(currentState)
            {
                case ChatUtilityState.Emoticon:
                    List<Rect> emoticonRects = RectManagement.SubdivideRect(RectManagement.GetRectUnderChat(chatRect), 12, 3);
                    int emoticonCount = TextureManagement.emoticons.Count;
                    for(int i = 0; i < emoticonCount; i++)
                    {
                        if(emoticonKeyHeld)
                        {
                            if(GUI.Button(emoticonRects[i], quickCodes[i]) || keyDown[quickKeyCodes[i]])
                            {
                                messageToAdd += TextureManagement.emoticonCodes[i];
                                keyDown[quickKeyCodes[i]] = false;
                                break;
                            }
                        }
                        else
                        {                            
                            if (GUI.Button(emoticonRects[i], ""))
                            {
                                messageToAdd += TextureManagement.emoticonCodes[i];
                            }
                            GUI.DrawTexture(emoticonRects[i], TextureManagement.emoticons[TextureManagement.emoticonCodes[i]], ScaleMode.ScaleToFit);
                        }
                    }
                    break;
                case ChatUtilityState.Command:
                    List<Rect> cmdRects = RectManagement.SubdivideRect(RectManagement.GetRectUnderChat(chatRect), 3, 3);
                    List<string> cmds = ConfigManagement.GetCommands();
                    for (int i = 0; i < cmds.Count; i++)
                    {
                        if (commandKeyHeld)
                        {
                            if (GUI.Button(cmdRects[i], quickCodes[i]) || keyDown[quickKeyCodes[i]])
                            {
                                messageToAdd += cmds[i];
                                keyDown[quickKeyCodes[i]] = false;
                                break;
                            }
                        }
                        else
                        {
                            string buttonText = cmds[i].Substring(0, Mathf.Min(cmds[i].Length, 16));
                            if(cmds[i].Length > buttonText.Length)
                            {
                                buttonText += "...";
                            }

                            if (GUI.Button(cmdRects[i], buttonText))
                            {
                                messageToAdd += cmds[i];
                            }
                        }
                    }
                    break;
            }

            ApplyMessage(messageToAdd);
        }

        public void ApplyMessage(string message)
        {
            bool autoSend = false;
            if(message.Contains((string)ConfigManagement.autoSendSuffix.BoxedValue))
            {
                message = message.Replace((string)ConfigManagement.autoSendSuffix.BoxedValue, "");
                autoSend = true;
            }

            OnlineChatUI.currentMessage += message;

            if (autoSend && !string.IsNullOrWhiteSpace(OnlineChatUI.currentMessage))
            {
                onlineChatUI.SendChatMessage(OnlineChatUI.currentMessage);
                OnlineChatUI.wasTyping = false;
                onlineChatUI.EnableSmallBox(true);
            }
        }
    }
    
    [HarmonyPatch(typeof(OnlineChatUI), "Awake")]
    public class ChatAwake
    {
        public static void Postfix(OnlineChatUI __instance)
        {
            Plugin.onlineChatUI = __instance;
        }
    }
}
