﻿using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
// Imports Jint

namespace GenieClient.Genie
{
    public class Command
    {
        public event EventReconnectEventHandler EventReconnect;

        public delegate void EventReconnectEventHandler();

        public event EventConnectEventHandler EventConnect;

        public delegate void EventConnectEventHandler(string sAccountName, string sPassword, string sCharacter, string sGame);

        public event EventDisconnectEventHandler EventDisconnect;

        public delegate void EventDisconnectEventHandler();

        public event EventEchoTextEventHandler EventEchoText;

        public delegate void EventEchoTextEventHandler(string sText, string sWindow);

        public event EventLinkTextEventHandler EventLinkText;

        public delegate void EventLinkTextEventHandler(string sText, string sLink, string sWindow);

        public event EventEchoColorTextEventHandler EventEchoColorText;

        public delegate void EventEchoColorTextEventHandler(string sText, Color oColor, Color oBgColor, string sWindow);

        public event EventSendTextEventHandler EventSendText;

        public delegate void EventSendTextEventHandler(string sText, bool bUserInput, string sOrigin);

        public event EventRunScriptEventHandler EventRunScript;

        public delegate void EventRunScriptEventHandler(string sText);

        public event EventClearWindowEventHandler EventClearWindow;

        public delegate void EventClearWindowEventHandler(string sWindow);

        public event EventVariableChangedEventHandler EventVariableChanged;

        public delegate void EventVariableChangedEventHandler(string sVariable);

        public event EventParseLineEventHandler EventParseLine;

        public delegate void EventParseLineEventHandler(string sText);

        public event EventStatusBarEventHandler EventStatusBar;

        public delegate void EventStatusBarEventHandler(string sText, int iIndex);

        public event EventCopyDataEventHandler EventCopyData;

        public delegate void EventCopyDataEventHandler(string sDestination, string sData);

        public event EventListScriptsEventHandler EventListScripts;

        public delegate void EventListScriptsEventHandler(string sFilter);

        public event EventScriptTraceEventHandler EventScriptTrace;

        public delegate void EventScriptTraceEventHandler(string sScript);

        public event EventScriptAbortEventHandler EventScriptAbort;

        public delegate void EventScriptAbortEventHandler(string sScript);

        public event EventScriptPauseEventHandler EventScriptPause;

        public delegate void EventScriptPauseEventHandler(string sScript);

        public event EventScriptPauseOrResumeEventHandler EventScriptPauseOrResume;

        public delegate void EventScriptPauseOrResumeEventHandler(string sScript);

        public event EventScriptResumeEventHandler EventScriptResume;

        public delegate void EventScriptResumeEventHandler(string sScript);

        public event EventScriptDebugEventHandler EventScriptDebug;

        public delegate void EventScriptDebugEventHandler(int iDebugLevel, string sScript);

        public event EventScriptVariablesEventHandler EventScriptVariables;

        public delegate void EventScriptVariablesEventHandler(string sScript, string sFilter);

        public event EventPresetChangedEventHandler EventPresetChanged;

        public delegate void EventPresetChangedEventHandler(string sPreset);

        public event EventShowScriptExplorerEventHandler EventShowScriptExplorer;

        public delegate void EventShowScriptExplorerEventHandler();

        public event EventLoadLayoutEventHandler EventLoadLayout;

        public delegate void EventLoadLayoutEventHandler(string sFile);

        public event EventSaveLayoutEventHandler EventSaveLayout;

        public delegate void EventSaveLayoutEventHandler(string sFile);

        public event EventLoadProfileEventHandler EventLoadProfile;

        public delegate void EventLoadProfileEventHandler();

        public event EventSaveProfileEventHandler EventSaveProfile;

        public delegate void EventSaveProfileEventHandler();

        public event EventFlashWindowEventHandler EventFlashWindow;

        public delegate void EventFlashWindowEventHandler();

        public event EventClassChangeEventHandler EventClassChange;

        public delegate void EventClassChangeEventHandler();

        public event EventMapperCommandEventHandler EventMapperCommand;

        public delegate void EventMapperCommandEventHandler(string cmd);

        public event EventAddWindowEventHandler EventAddWindow;

        public delegate void EventAddWindowEventHandler(string sWindow);

        public event EventRemoveWindowEventHandler EventRemoveWindow;

        public delegate void EventRemoveWindowEventHandler(string sWindow);

        public event EventCloseWindowEventHandler EventCloseWindow;

        public delegate void EventCloseWindowEventHandler(string sWindow);

        public event EventChangeWindowTitleEventHandler EventChangeWindowTitle;

        public delegate void EventChangeWindowTitleEventHandler(string sWindow, string sComment);

        public event EventRawToggleEventHandler EventRawToggle;

        public delegate void EventRawToggleEventHandler(string sToggle);

        public event EventSendRawEventHandler EventSendRaw;

        public delegate void EventSendRawEventHandler(string sText);

        public event EventChangeIconEventHandler EventChangeIcon;

        public delegate void EventChangeIconEventHandler(string sPath);

        public event LoadPluginEventHandler LoadPlugin;

        public delegate void LoadPluginEventHandler(string filename);

        public event UnloadPluginEventHandler UnloadPlugin;

        public delegate void UnloadPluginEventHandler(string filename);

        public event EnablePluginEventHandler EnablePlugin;

        public delegate void EnablePluginEventHandler(string filename);

        public event DisablePluginEventHandler DisablePlugin;

        public delegate void DisablePluginEventHandler(string filename);

        public event ReloadPluginsEventHandler ReloadPlugins;

        public delegate void ReloadPluginsEventHandler();

        public event ListPluginsEventHandler ListPlugins;

        public delegate void ListPluginsEventHandler();

        private Script.Eval m_oEval = new Script.Eval();
        private Script.MathEval m_oMathEval = new Script.MathEval();
        private Globals oGlobals;

        public Command(ref Globals cl)
        {
            oGlobals = cl;
        }

        public string ParseCommand(string sText, bool bSendToGame = false, bool bUserInput = false, string sOrigin = "", bool bParseQuickSend = true)
        {
            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            string sResult = string.Empty;
            if (sText.ToLower().StartsWith("bug ") | sText.ToLower().StartsWith("sing "))
            {
                if (bSendToGame == true)
                {
                    // Send Text To Game
                    string argsText = oGlobals.ParseGlobalVars(sText);
                    SendTextToGame(argsText, bUserInput, sOrigin);
                }

                return sText;
            }

            foreach (string stemp in Utility.SafeSplit(sText, oGlobals.Config.cSeparatorChar))
            {
                var sStringTemp = stemp;
                if (oGlobals.AliasList.ContainsKey(GetKeywordString(sStringTemp).ToLower()) == true) // Alias
                {
                    sStringTemp = ParseAlias(sStringTemp);
                }

                foreach (string row in Utility.SafeSplit(sStringTemp, oGlobals.Config.cSeparatorChar))
                {
                    var sRow = row;
                    // Quick #send
                    if (bParseQuickSend)
                    {
                        if (sRow.StartsWith(Conversions.ToString(oGlobals.Config.cQuickSendChar)))
                        {
                            sRow = "#send " + sRow.Substring(1);
                        }
                    }

                    sResult = string.Empty;
                    if (sRow.Trim().StartsWith(Conversions.ToString(oGlobals.Config.cCommandChar)))
                    {
                        // Get result from function then send result to game
                        var oArgs = new ArrayList();
                        oArgs = Utility.ParseArgs(sRow);
                        if (oArgs.Count > 0)
                        {
                            if (Conversions.ToString(oArgs[0]).Length > 0)
                            {
                                var switchExpr = Conversions.ToString(oArgs[0]).Substring(1).ToLower();
                                switch (switchExpr)
                                {
                                    case "echo":
                                        {
                                            // #echo <color> >window text
                                            string sOutputWindow = string.Empty;
                                            int iColorIndex = 0;
                                            Color oColor = default;
                                            Color oBgcolor = default;
                                            if (oArgs.Count > 1 && oArgs[1].ToString().StartsWith(">"))
                                            {
                                                sOutputWindow = oGlobals.ParseGlobalVars(oArgs[1].ToString().Substring(1));
                                                oArgs[1] = null;
                                                if (oArgs.Count > 2)
                                                {
                                                    iColorIndex = 2;
                                                }
                                            }
                                            else if (oArgs.Count > 2 && oArgs[2].ToString().StartsWith(">"))
                                            {
                                                sOutputWindow = oGlobals.ParseGlobalVars(oArgs[2].ToString().Substring(1));
                                                oArgs[2] = null;
                                                iColorIndex = 1;
                                            }
                                            else if (oArgs.Count > 1)
                                            {
                                                iColorIndex = 1;
                                            }

                                            if (iColorIndex > 0)
                                            {
                                                string sColorName = oArgs[iColorIndex].ToString();
                                                if (sColorName.Contains(",") == true && sColorName.EndsWith(",") == false)
                                                {
                                                    string sColor = sColorName.Substring(0, sColorName.IndexOf(",")).Trim();
                                                    string sBgColor = sColorName.Substring(sColorName.IndexOf(",") + 1).Trim();
                                                    oColor = ColorCode.StringToColor(sColor);
                                                    oBgcolor = ColorCode.StringToColor(sBgColor);
                                                }
                                                else
                                                {
                                                    oColor = ColorCode.StringToColor(sColorName);
                                                    oBgcolor = Color.Transparent;
                                                }

                                                if (!Information.IsNothing(oColor) && oColor != Color.Empty)
                                                {
                                                    oArgs[iColorIndex] = null;
                                                }
                                            }

                                            if (!Information.IsNothing(oColor) && oColor != Color.Empty)
                                            {
                                                string argsText1 = oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 1, false) + Constants.vbNewLine);
                                                EchoColorText(argsText1, oColor, oBgcolor, sOutputWindow);
                                            }
                                            else
                                            {
                                                EchoText(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 1, false) + Constants.vbNewLine), sOutputWindow);
                                            }

                                            sResult = "";
                                            break;
                                        }
                                    // Case "jint"
                                    // Dim jint As New JintEngine()
                                    // EchoText(jint.Run("var blah = 1;return blah + 10;").ToString())
                                    case "link":
                                        {
                                            string sWindow = string.Empty;
                                            string sLinkText = string.Empty;
                                            string sLinkCommand = string.Empty;
                                            if (oArgs.Count > 3 && oArgs[1].ToString().StartsWith(">"))
                                            {
                                                sWindow = oGlobals.ParseGlobalVars(oArgs[1].ToString().Substring(1));
                                                sLinkText = oGlobals.ParseGlobalVars(oArgs[2].ToString());
                                                sLinkCommand = Utility.ArrayToString(oArgs, 3);
                                            }
                                            else if (oArgs.Count > 2)
                                            {
                                                sLinkText = oGlobals.ParseGlobalVars(oArgs[1].ToString());
                                                sLinkCommand = Utility.ArrayToString(oArgs, 2);
                                            }

                                            if (sLinkText.Length > 0)
                                            {
                                                EventLinkText?.Invoke(sLinkText, sLinkCommand, sWindow);
                                            }

                                            break;
                                        }

                                    case "icon":
                                        {
                                            EventChangeIcon?.Invoke(oGlobals.ParseGlobalVars(oArgs[1].ToString()));
                                            break;
                                        }

                                    case "log":
                                        {
                                            if (oArgs.Count > 0)
                                            {
                                                string sLogText = string.Empty;
                                                if (oArgs[1].ToString().StartsWith(">"))
                                                {
                                                    string sFileName = oGlobals.ParseGlobalVars(oArgs[1].ToString().Substring(1));
                                                    if (oArgs.Count > 2)
                                                    {
                                                        sLogText = oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2, false));
                                                        oGlobals.Log.LogLine(sLogText, sFileName);
                                                    }
                                                }
                                                else
                                                {
                                                    string sTargetChar = Conversions.ToString(oGlobals.VariableList["charactername"]);
                                                    string sGameName = Conversions.ToString(oGlobals.VariableList["game"]);
                                                    sLogText = oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 1, false));
                                                    oGlobals.Log.LogText(sLogText + Constants.vbNewLine, sTargetChar, sGameName);
                                                }
                                            }

                                            break;
                                        }

                                    case "connect":
                                        {
                                            if (oArgs.Count == 1)
                                            {
                                                // EchoText("Reconnect Command Received" & vbNewLine)
                                                EventReconnect?.Invoke();
                                            }
                                            else if (oArgs.Count == 5)
                                            {
                                                // EchoText("Connect Command Received" & vbNewLine);
                                                var arg1 = oGlobals.ParseGlobalVars(oArgs[1].ToString());
                                                var arg2 = oGlobals.ParseGlobalVars(oArgs[2].ToString());
                                                var arg3 = oGlobals.ParseGlobalVars(oArgs[3].ToString());
                                                var arg4 = oGlobals.ParseGlobalVars(oArgs[4].ToString());

                                                EventConnect?.Invoke(arg1, arg2, arg3, arg4);
                                            }
                                            else if (oArgs.Count == 2)
                                            {
                                                var arg1 = oGlobals.ParseGlobalVars(oArgs[1].ToString());
                                                var argEmpty = "";
                                                EventConnect?.Invoke(arg1, argEmpty, argEmpty, argEmpty);
                                            }
                                            else
                                            {
                                                EchoText("Invalid number of arguments in #connect command. Syntax: #connect account password character game" + Constants.vbNewLine);
                                            }

                                            break;
                                        }

                                    case "disconnect":
                                        {
                                            EventDisconnect?.Invoke();
                                            break;
                                        }

                                    case "clear":
                                        {
                                            ClearWindow(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 1)));
                                            break;
                                        }

                                    case "save":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                for (int I = 1, loopTo = oArgs.Count - 1; I <= loopTo; I++)
                                                {
                                                    var switchExpr1 = oArgs[I].ToString().ToLower();
                                                    switch (switchExpr1)
                                                    {
                                                        case "var":
                                                        case "vars":
                                                        case "variable":
                                                        case "setvar":
                                                        case "setvariable":
                                                            {
                                                                EchoText("Variables Saved" + Constants.vbNewLine);
                                                                oGlobals.VariableList.Save();
                                                                break;
                                                            }

                                                        case "alias":
                                                        case "aliases":
                                                            {
                                                                EchoText("Aliases Saved" + Constants.vbNewLine);
                                                                oGlobals.AliasList.Save();
                                                                break;
                                                            }

                                                        case "class":
                                                        case "classes":
                                                            {
                                                                EchoText("Classes Saved" + Constants.vbNewLine);
                                                                oGlobals.ClassList.Save();
                                                                break;
                                                            }

                                                        case "trigger":
                                                        case "triggers":
                                                        case "action":
                                                        case "actions":
                                                            {
                                                                EchoText("Triggers Saved" + Constants.vbNewLine);
                                                                oGlobals.TriggerList.Save(oGlobals.Config.ConfigDir + @"\triggers.cfg");
                                                                break;
                                                            }

                                                        case "config":
                                                        case "set":
                                                        case "setting":
                                                        case "settings":
                                                            {
                                                                EchoText("Settings Saved" + Constants.vbNewLine);
                                                                oGlobals.Config.Save(oGlobals.Config.ConfigDir + @"\settings.cfg");
                                                                break;
                                                            }

                                                        case "macro":
                                                        case "macros":
                                                            {
                                                                EchoText("Macros Saved" + Constants.vbNewLine);
                                                                oGlobals.MacroList.Save();
                                                                break;
                                                            }

                                                        case "sub":
                                                        case "subs":
                                                        case "substitute":
                                                            {
                                                                EchoText("Substitutes Saved" + Constants.vbNewLine);
                                                                oGlobals.SubstituteList.Save(oGlobals.Config.ConfigDir + @"\substitutes.cfg");
                                                                break;
                                                            }

                                                        case "gag":
                                                        case "gags":
                                                        case "squelch":
                                                        case "ignore":
                                                        case "ignores":
                                                            {
                                                                EchoText("Gags Saved" + Constants.vbNewLine);
                                                                oGlobals.GagList.Save();
                                                                break;
                                                            }

                                                        case "highlight":
                                                        case "highlights":
                                                            {
                                                                EchoText("Highlights Saved" + Constants.vbNewLine);
                                                                oGlobals.SaveHighlights(oGlobals.Config.ConfigDir + @"\highlights.cfg");
                                                                break;
                                                            }

                                                        case "name":
                                                        case "names":
                                                            {
                                                                EchoText("Names Saved" + Constants.vbNewLine);
                                                                oGlobals.NameList.Save(oGlobals.Config.ConfigDir + @"\names.cfg");
                                                                break;
                                                            }

                                                        case "preset":
                                                        case "presets":
                                                            {
                                                                EchoText("Presets Saved" + Constants.vbNewLine);
                                                                oGlobals.PresetList.Save(oGlobals.Config.ConfigDir + @"\presets.cfg");
                                                                break;
                                                            }

                                                        case "layout":
                                                            {
                                                                EchoText("Layout Saved" + Constants.vbNewLine);
                                                                var tmp = string.Empty;
                                                                EventSaveLayout?.Invoke(tmp);
                                                                break;
                                                            }

                                                        case "profile":
                                                            {
                                                                EchoText("Profile Saved" + Constants.vbNewLine);
                                                                EventSaveProfile?.Invoke();
                                                                break;
                                                            }

                                                        case "all":
                                                            {
                                                                EchoText("Variables Saved" + Constants.vbNewLine);
                                                                oGlobals.VariableList.Save();
                                                                EchoText("Aliases Saved" + Constants.vbNewLine);
                                                                oGlobals.AliasList.Save();
                                                                EchoText("Classes Saved" + Constants.vbNewLine);
                                                                oGlobals.ClassList.Save();
                                                                EchoText("Triggers Saved" + Constants.vbNewLine);
                                                                oGlobals.TriggerList.Save(oGlobals.Config.ConfigDir + @"\triggers.cfg");
                                                                EchoText("Settings Saved" + Constants.vbNewLine);
                                                                oGlobals.Config.Save(oGlobals.Config.ConfigDir + @"\settings.cfg");
                                                                EchoText("Macros Saved" + Constants.vbNewLine);
                                                                oGlobals.MacroList.Save();
                                                                EchoText("Substitutes Saved" + Constants.vbNewLine);
                                                                oGlobals.SubstituteList.Save(oGlobals.Config.ConfigDir + @"\substitutes.cfg");
                                                                EchoText("Gags Saved" + Constants.vbNewLine);
                                                                oGlobals.GagList.Save();
                                                                EchoText("Highlights Saved" + Constants.vbNewLine);
                                                                oGlobals.SaveHighlights(oGlobals.Config.ConfigDir + @"\highlights.cfg");
                                                                EchoText("Names Saved" + Constants.vbNewLine);
                                                                oGlobals.NameList.Save(oGlobals.Config.ConfigDir + @"\names.cfg");
                                                                EchoText("Presets Saved" + Constants.vbNewLine);
                                                                oGlobals.PresetList.Save(oGlobals.Config.ConfigDir + @"\presets.cfg");
                                                                break;
                                                            }
                                                    }
                                                }
                                            }

                                            break;
                                        }

                                    case "load":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                for (int I = 1, loopTo1 = oArgs.Count - 1; I <= loopTo1; I++)
                                                {
                                                    var switchExpr2 = oArgs[I].ToString().ToLower();
                                                    switch (switchExpr2)
                                                    {
                                                        case "var":
                                                        case "vars":
                                                        case "variable":
                                                        case "setvar":
                                                        case "setvariable":
                                                            {
                                                                EchoText("Variables Loaded" + Constants.vbNewLine);
                                                                oGlobals.VariableList.ClearUser();
                                                                oGlobals.VariableList.Load();
                                                                break;
                                                            }

                                                        case "alias":
                                                        case "aliases":
                                                            {
                                                                EchoText("Aliases Loaded" + Constants.vbNewLine);
                                                                oGlobals.AliasList.Clear();
                                                                oGlobals.AliasList.Load();
                                                                break;
                                                            }

                                                        case "class":
                                                        case "classes":
                                                            {
                                                                EchoText("Classes Loaded" + Constants.vbNewLine);
                                                                oGlobals.ClassList.Clear();
                                                                oGlobals.ClassList.Load();
                                                                EventClassChange?.Invoke();
                                                                break;
                                                            }

                                                        case "trigger":
                                                        case "triggers":
                                                        case "action":
                                                        case "actions":
                                                            {
                                                                EchoText("Triggers Loaded" + Constants.vbNewLine);
                                                                oGlobals.TriggerList.Clear();
                                                                oGlobals.TriggerList.Load(oGlobals.Config.ConfigDir + @"\triggers.cfg");
                                                                break;
                                                            }

                                                        case "config":
                                                        case "set":
                                                        case "setting":
                                                        case "settings":
                                                            {
                                                                EchoText("Settings Loaded" + Constants.vbNewLine);
                                                                oGlobals.Config.Load(oGlobals.Config.ConfigDir + @"\settings.cfg");
                                                                break;
                                                            }

                                                        case "macro":
                                                        case "macros":
                                                            {
                                                                EchoText("Macros Loaded" + Constants.vbNewLine);
                                                                oGlobals.MacroList.Clear();
                                                                oGlobals.MacroList.Load();
                                                                break;
                                                            }

                                                        case "sub":
                                                        case "subs":
                                                        case "substitute":
                                                            {
                                                                EchoText("Substitutes Loaded" + Constants.vbNewLine);
                                                                oGlobals.SubstituteList.Clear();
                                                                oGlobals.SubstituteList.Load(oGlobals.Config.ConfigDir + @"\substitutes.cfg");
                                                                break;
                                                            }

                                                        case "gag":
                                                        case "gags":
                                                        case "squelch":
                                                        case "ignore":
                                                        case "ignores":
                                                            {
                                                                EchoText("Gags Loaded" + Constants.vbNewLine);
                                                                oGlobals.GagList.Clear();
                                                                oGlobals.GagList.Load();
                                                                break;
                                                            }

                                                        case "highlight":
                                                        case "highlights":
                                                            {
                                                                oGlobals.HighlightList.Clear();
                                                                oGlobals.HighlightRegExpList.Clear();
                                                                oGlobals.HighlightBeginsWithList.Clear();
                                                                EchoText("Highlights Loaded" + Constants.vbNewLine);
                                                                oGlobals.LoadHighlights(oGlobals.Config.ConfigDir + @"\highlights.cfg");
                                                                break;
                                                            }

                                                        case "name":
                                                        case "names":
                                                            {
                                                                EchoText("Names Loaded" + Constants.vbNewLine);
                                                                oGlobals.NameList.Clear();
                                                                oGlobals.NameList.Load(oGlobals.Config.ConfigDir + @"\names.cfg");
                                                                break;
                                                            }

                                                        case "preset":
                                                        case "presets":
                                                            {
                                                                EchoText("Presets Loaded" + Constants.vbNewLine);
                                                                oGlobals.PresetList.Clear();
                                                                oGlobals.PresetList.Load(oGlobals.Config.ConfigDir + @"\presets.cfg");
                                                                break;
                                                            }

                                                        case "layout":
                                                            {
                                                                EchoText("Layout Loaded" + Constants.vbNewLine);
                                                                var tmp = string.Empty;
                                                                EventLoadLayout?.Invoke(tmp);
                                                                break;
                                                            }

                                                        case "profile":
                                                            {
                                                                EchoText("Profile Loaded" + Constants.vbNewLine);
                                                                EventLoadProfile?.Invoke();
                                                                break;
                                                            }

                                                        case "all":
                                                            {
                                                                EchoText("Variables Loaded" + Constants.vbNewLine);
                                                                oGlobals.VariableList.ClearUser();
                                                                oGlobals.VariableList.Load();
                                                                EchoText("Aliases Loaded" + Constants.vbNewLine);
                                                                oGlobals.AliasList.Clear();
                                                                oGlobals.AliasList.Load();
                                                                EchoText("Classes Loaded" + Constants.vbNewLine);
                                                                oGlobals.ClassList.Clear();
                                                                oGlobals.ClassList.Load();
                                                                EchoText("Triggers Loaded" + Constants.vbNewLine);
                                                                oGlobals.TriggerList.Clear();
                                                                oGlobals.TriggerList.Load(oGlobals.Config.ConfigDir + @"\triggers.cfg");
                                                                EchoText("Settings Loaded" + Constants.vbNewLine);
                                                                oGlobals.Config.Load();
                                                                EchoText("Macros Loaded" + Constants.vbNewLine);
                                                                oGlobals.MacroList.Clear();
                                                                oGlobals.MacroList.Load();
                                                                EchoText("Substitutes Loaded" + Constants.vbNewLine);
                                                                oGlobals.SubstituteList.Clear();
                                                                oGlobals.SubstituteList.Load(oGlobals.Config.ConfigDir + @"\substitutes.cfg");
                                                                EchoText("Gags Loaded" + Constants.vbNewLine);
                                                                oGlobals.GagList.Clear();
                                                                oGlobals.GagList.Load();
                                                                oGlobals.HighlightList.Clear();
                                                                oGlobals.HighlightRegExpList.Clear();
                                                                oGlobals.HighlightBeginsWithList.Clear();
                                                                EchoText("Highlights Loaded" + Constants.vbNewLine);
                                                                oGlobals.LoadHighlights(oGlobals.Config.ConfigDir + @"\highlights.cfg");
                                                                EchoText("Names Loaded" + Constants.vbNewLine);
                                                                oGlobals.NameList.Clear();
                                                                oGlobals.NameList.Load(oGlobals.Config.ConfigDir + @"\names.cfg");
                                                                EchoText("Presets Loaded" + Constants.vbNewLine);
                                                                oGlobals.PresetList.Clear();
                                                                oGlobals.PresetList.Load(oGlobals.Config.ConfigDir + @"\presets.cfg");
                                                                break;
                                                            }
                                                    }
                                                }
                                            }

                                            break;
                                        }

                                    case "nop": // Do nothing
                                        {
                                            sResult = "";
                                            break;
                                        }

                                    case "cr":
                                        {
                                            sResult = Constants.vbNewLine;
                                            break;
                                        }

                                    case "send":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                string s = oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 1));
                                                if ((s.ToLower() ?? "") == "clear")
                                                {
                                                    oGlobals.CommandQueue.Clear();
                                                }
                                                else
                                                {
                                                    double dPauseTime = 0;
                                                    string sNumber = string.Empty;
                                                    foreach (char c in s.ToCharArray())
                                                    {
                                                        if (Information.IsNumeric(c) | c == '.')
                                                        {
                                                            sNumber += Conversions.ToString(c);
                                                        }
                                                        else
                                                        {
                                                            break;
                                                        }
                                                    }

                                                    if (sNumber.Length > 0 & (sNumber ?? "") != ".")
                                                    {
                                                        s = s.Substring(sNumber.Length).Trim();
                                                        dPauseTime = double.Parse(sNumber);
                                                    }

                                                    if (s.Trim().Length > 0)
                                                    {
                                                        // Put it in queue
                                                        oGlobals.CommandQueue.AddToQueue(dPauseTime, true, s);
                                                    }
                                                }
                                            }

                                            sResult = "";
                                            break;
                                        }

                                    case "put":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                // Send Text To Game
                                                string argsText2 = oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 1));
                                                bool argbUserInput = false;
                                                SendTextToGame(argsText2, argbUserInput, sOrigin);
                                            }

                                            sResult = "";
                                            break;
                                        }

                                    case "push":
                                        {
                                            // If oArgs.Count > 2 Then
                                            // RaiseEvent EventCopyData(oGlobals.ParseGlobalVars(oArgs.Item(1).ToString).ToLower, oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2)))
                                            // End If
                                            sResult = "";
                                            break;
                                        }

                                    case "var":
                                    case "vars":
                                    case "variable":
                                    case "setvar":
                                    case "setvariable":
                                        {
                                            if (oArgs.Count < 3)
                                            {
                                                if (oArgs.Count < 2)
                                                {
                                                    ListVariables("");
                                                }
                                                else
                                                {
                                                    var switchExpr3 = oArgs[1].ToString().ToLower();
                                                    switch (switchExpr3)
                                                    {
                                                        case "load":
                                                            {
                                                                EchoText("Variables Loaded" + Constants.vbNewLine);
                                                                oGlobals.VariableList.Load();
                                                                break;
                                                            }

                                                        case "save":
                                                            {
                                                                EchoText("Variables Saved" + Constants.vbNewLine);
                                                                oGlobals.VariableList.Save();
                                                                break;
                                                            }

                                                        case "clear":
                                                            {
                                                                EchoText("Variables Cleared" + Constants.vbNewLine);
                                                                oGlobals.VariableList.ClearUser();
                                                                break;
                                                            }

                                                        case "edit":
                                                            {
                                                                Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigProfileDir + @"\variables.cfg""", AppWinStyle.NormalFocus, false);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                ListVariables(Utility.ArrayToString(oArgs, 1));
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Add
                                                string argkey = oGlobals.ParseGlobalVars(oArgs[1].ToString());
                                                string argvalue = oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2));
                                                oGlobals.VariableList.Add(argkey, argvalue);
                                                string argsVariable = "$" + oArgs[1].ToString();
                                                VariableChanged(argsVariable);
                                            }

                                            break;
                                        }

                                    case "tvar":
                                    case "tempvar":
                                    case "tempvariable":
                                        {
                                            if (oArgs.Count < 3)
                                            {
                                                ListVariables("");
                                            }
                                            else
                                            {
                                                // Add
                                                string argkey1 = oGlobals.ParseGlobalVars(oArgs[1].ToString());
                                                string argvalue1 = oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2));
                                                oGlobals.VariableList.Add(argkey1, argvalue1, Globals.Variables.VariableType.Temporary);
                                                string argsVariable1 = "$" + oArgs[1].ToString();
                                                VariableChanged(argsVariable1);
                                            }

                                            break;
                                        }

                                    case "svar":
                                    case "servervar":
                                    case "servervariable":
                                        {
                                            if (oArgs.Count < 3)
                                            {
                                                ListVariables("");
                                            }
                                            else
                                            {
                                                // Add
                                                string sName = oGlobals.ParseGlobalVars(oArgs[1].ToString()).ToLower();
                                                string sValue = oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2));
                                                string sCmdRaw = "<stgupd>";
                                                if (oGlobals.VariableList.ContainsKey(sName))
                                                {
                                                    if (oGlobals.VariableList.get_GetVariable(sName).oType == Globals.Variables.VariableType.Server)
                                                    {
                                                        sCmdRaw += "<vars><<d><k name=\"" + sName + "\" value=\"" + oGlobals.VariableList[sName] + "\"/></<d></vars>";
                                                    }
                                                }

                                                sCmdRaw += "<vars><<a><k name=\"" + sName + "\" value=\"" + sValue + "\"/></<a></vars>";
                                                oGlobals.VariableList.Add(sName, sValue, Globals.Variables.VariableType.Server);
                                                string argsVariable2 = "$" + oArgs[1].ToString();
                                                VariableChanged(argsVariable2);
                                                /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                                                EventSendRaw?.Invoke(sCmdRaw + Constants.vbLf);
                                            }

                                            break;
                                        }

                                    case "unvar":
                                    case "unvariable":
                                    case "unsetvar":
                                    case "unsetvariable":
                                        {
                                            if (oArgs.Count >= 1)
                                            {
                                                string sName = oArgs[1].ToString().ToLower();
                                                if (oGlobals.VariableList.ContainsKey(sName))
                                                {
                                                    if (oGlobals.VariableList.get_GetVariable(sName).oType == Globals.Variables.VariableType.Server)
                                                    {
                                                        string sCmdRaw = Conversions.ToString("<stgupd><vars><<d><k name=\"" + sName + "\" value=\"" + oGlobals.VariableList[sName] + "\"/></<d></vars>");
                                                        /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                                                        EventSendRaw?.Invoke(sCmdRaw + Constants.vbLf);
                                                    }

                                                    oGlobals.VariableList.Remove(sName);
                                                }
                                            }

                                            break;
                                        }

                                    case "eval":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                string argsText3 = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                                sResult = Eval(argsText3);
                                                // EchoText("Eval Result: " & sResult & vbNewLine)
                                            }

                                            break;
                                        }

                                    case "math":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                double dValue;
                                                if (!double.TryParse(Conversions.ToString(oGlobals.VariableList[oArgs[1].ToString()]), out dValue))
                                                    dValue = 0;
                                                try
                                                {
                                                    string argsExpression = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2));
                                                    dValue = Utility.MathCalc(dValue, argsExpression);
                                                    oGlobals.VariableList.Add(oArgs[1].ToString(), dValue.ToString());
                                                    string argsVariable3 = "$" + oArgs[1].ToString();
                                                    VariableChanged(argsVariable3);
                                                }
                                                catch (Exception ex)
                                                {
                                                    EchoText("Invalid #math expression: " + Utility.ArrayToString(oArgs, 1));
                                                }
                                            }

                                            break;
                                        }

                                    case "evalmath":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                string argsText4 = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                                sResult = EvalMath(argsText4);
                                                // EchoText("Math Result: " & sResult & vbNewLine)
                                            }

                                            break;
                                        }

                                    case "if":
                                        {
                                            if (oArgs.Count > 2)
                                            {
                                                if (oArgs[1].ToString().StartsWith("#"))
                                                {
                                                    oArgs[1] = ParseCommand(oGlobals.ParseGlobalVars(oArgs[1].ToString()));
                                                }

                                                if (EvalIf(oGlobals.ParseGlobalVars(oArgs[1])) == true)
                                                {
                                                    if (oArgs[2].ToString().StartsWith("#"))
                                                    {
                                                        sResult = ParseCommand(oGlobals.ParseGlobalVars(oArgs[2].ToString()));
                                                    }
                                                    else
                                                    {
                                                        sResult = ParseCommand(Utility.ArrayToString(oArgs, 2));
                                                    }
                                                }
                                                else if (oArgs.Count > 3)
                                                {
                                                    if (oArgs[3].ToString().StartsWith("#"))
                                                    {
                                                        sResult = ParseCommand(oGlobals.ParseGlobalVars(oArgs[3].ToString()));
                                                    }
                                                    else
                                                    {
                                                        sResult = ParseCommand(Utility.ArrayToString(oArgs, 3));
                                                    }
                                                }
                                            }

                                            break;
                                        }

                                    case "event":
                                    case "events":
                                        {
                                            if (oArgs.Count > 2)
                                            {
                                                string sAction = Utility.ArrayToString(oArgs, 2);
                                                if (sAction.Trim().Length > 0)
                                                {
                                                    double argdSeconds = Utility.StringToDouble(oArgs[1].ToString());
                                                    oGlobals.Events.AddToQueue(argdSeconds, sAction);
                                                }
                                            }
                                            else if (oArgs.Count == 2)
                                            {
                                                if ((oArgs[1].ToString().ToLower() ?? "") == "clear")
                                                {
                                                    oGlobals.Events.EventList.Clear();
                                                }
                                                else
                                                {
                                                    ListEvents(Utility.ArrayToString(oArgs, 1));
                                                }
                                            }
                                            else
                                            {
                                                ListEvents(Utility.ArrayToString(oArgs, 1));
                                            }

                                            break;
                                        }

                                    case "que":
                                    case "queue":
                                        {
                                            if (oArgs.Count > 2)
                                            {
                                                string sAction = Utility.ArrayToString(oArgs, 2);
                                                if (sAction.Trim().Length > 0)
                                                {
                                                    double argdDelay = Utility.StringToDouble(oArgs[1].ToString());
                                                    oGlobals.CommandQueue.AddToQueue(argdDelay, true, sAction);
                                                }
                                            }
                                            else if (oArgs.Count == 2)
                                            {
                                                if ((oArgs[1].ToString().ToLower() ?? "") == "clear")
                                                {
                                                    // EchoText("Queue Cleared" & vbNewLine)
                                                    oGlobals.CommandQueue.Clear();
                                                }
                                                else
                                                {
                                                    ListCommandQueue(Utility.ArrayToString(oArgs, 1));
                                                }
                                            }
                                            else
                                            {
                                                ListCommandQueue(Utility.ArrayToString(oArgs, 1));
                                            }

                                            break;
                                        }

                                    case "alias":
                                    case "aliases":
                                        {
                                            if (oArgs.Count < 3)
                                            {
                                                if (oArgs.Count < 2)
                                                {
                                                    ListAliases("");
                                                }
                                                else // 2 Arguments
                                                {
                                                    var switchExpr4 = oArgs[1].ToString().ToLower();
                                                    switch (switchExpr4)
                                                    {
                                                        case "load":
                                                            {
                                                                EchoText("Aliases Loaded" + Constants.vbNewLine);
                                                                oGlobals.AliasList.Load();
                                                                break;
                                                            }

                                                        case "save":
                                                            {
                                                                EchoText("Aliases Saved" + Constants.vbNewLine);
                                                                oGlobals.AliasList.Save();
                                                                break;
                                                            }

                                                        case "clear":
                                                            {
                                                                EchoText("Aliases Cleared" + Constants.vbNewLine);
                                                                oGlobals.AliasList.Clear();
                                                                break;
                                                            }

                                                        case "edit":
                                                            {
                                                                Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigProfileDir + @"\aliases.cfg""", AppWinStyle.NormalFocus, false);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                ListAliases(Utility.ArrayToString(oArgs, 1));
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // TODO: #alias {asdf asdf} {asdf} does not work with GetArg/GetArg(sRow)
                                                oGlobals.AliasList.Add(oArgs[1].ToString(), oArgs[2].ToString());
                                            }

                                            break;
                                        }

                                    case "unalias":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                oGlobals.AliasList.Remove(oArgs[1].ToString());
                                            }

                                            break;
                                        }

                                    case "class":
                                    case "classes":
                                        {
                                            if (oArgs.Count == 1)
                                            {
                                                ListClasses("");
                                            }
                                            else if (oArgs.Count > 1)	// 2 Arguments or more
                                            {
                                                if (oArgs[1].ToString().StartsWith("+") | oArgs[1].ToString().StartsWith("-"))
                                                {
                                                    foreach (string sItem in oArgs)
                                                    {
                                                        if (sItem.StartsWith("+") & sItem.Length > 1)
                                                        {
                                                            if ((sItem.Substring(1).ToLower() ?? "") == "all")
                                                            {
                                                                EchoText("All Classes Activated" + Constants.vbNewLine);
                                                                oGlobals.ClassList.ActivateAll();
                                                            }
                                                            else
                                                            {
                                                                string argsValue = "True";
                                                                oGlobals.ClassList.Add(sItem.Substring(1).ToLower(), argsValue);
                                                            }
                                                        }
                                                        else if (sItem.StartsWith("-") & sItem.Length > 1)
                                                        {
                                                            if ((sItem.Substring(1).ToLower() ?? "") == "all")
                                                            {
                                                                EchoText("All Classes InActivated" + Constants.vbNewLine);
                                                                oGlobals.ClassList.InActivateAll();
                                                            }
                                                            else
                                                            {
                                                                string argsValue1 = "False";
                                                                oGlobals.ClassList.Add(sItem.Substring(1).ToLower(), argsValue1);
                                                            }
                                                        }
                                                    }

                                                    EventClassChange?.Invoke();
                                                }
                                                else if (oArgs.Count < 3)
                                                {
                                                    var switchExpr6 = oArgs[1].ToString().ToLower();
                                                    switch (switchExpr6)
                                                    {
                                                        case "load":
                                                            {
                                                                EchoText("Classes Loaded" + Constants.vbNewLine);
                                                                oGlobals.ClassList.Load();
                                                                EventClassChange?.Invoke();
                                                                break;
                                                            }

                                                        case "save":
                                                            {
                                                                EchoText("Classes Saved" + Constants.vbNewLine);
                                                                oGlobals.ClassList.Save();
                                                                break;
                                                            }

                                                        case "clear":
                                                            {
                                                                EchoText("Classes Cleared" + Constants.vbNewLine);
                                                                oGlobals.ClassList.Clear();
                                                                break;
                                                            }

                                                        case "edit":
                                                            {
                                                                Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigProfileDir + @"\classes.cfg""", AppWinStyle.NormalFocus, false);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                ListClasses(Utility.ArrayToString(oArgs, 1));
                                                                EventClassChange?.Invoke();
                                                                break;
                                                            }
                                                    }
                                                }
                                                else
                                                {
                                                    if ((oArgs[1].ToString().ToLower() ?? "") == "all")
                                                    {
                                                        var switchExpr5 = oArgs[2].ToString().ToLower();
                                                        switch (switchExpr5)
                                                        {
                                                            case "true":
                                                            case "on":
                                                            case "1":
                                                                {
                                                                    EchoText("All Classes Activated" + Constants.vbNewLine);
                                                                    oGlobals.ClassList.ActivateAll();
                                                                    break;
                                                                }

                                                            case "false":
                                                            case "off":
                                                            case "0":
                                                                {
                                                                    EchoText("All Classes InActivated" + Constants.vbNewLine);
                                                                    oGlobals.ClassList.InActivateAll();
                                                                    break;
                                                                }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        oGlobals.ClassList.Add(oArgs[1].ToString(), oArgs[2].ToString());
                                                    }

                                                    EventClassChange?.Invoke();
                                                }
                                            }

                                            break;
                                        }

                                    case "unclass":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                oGlobals.AliasList.Remove(oArgs[1].ToString());
                                            }

                                            break;
                                        }

                                    case "trigger":
                                    case "triggers":
                                    case "action":
                                    case "actions":
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListTriggers("");
                                            }
                                            else if (oArgs.Count == 2)	// 2 Arguments
                                            {
                                                var switchExpr7 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr7)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Triggers Loaded" + Constants.vbNewLine);
                                                            oGlobals.TriggerList.Load(oGlobals.Config.ConfigDir + @"\triggers.cfg");
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Triggers Saved" + Constants.vbNewLine);
                                                            oGlobals.TriggerList.Save(oGlobals.Config.ConfigDir + @"\triggers.cfg");
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Triggers Cleared" + Constants.vbNewLine);
                                                            oGlobals.TriggerList.Clear();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigDir + @"\triggers.cfg""", AppWinStyle.NormalFocus, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            ListTriggers(Utility.ArrayToString(oArgs, 1));
                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                            {
                                                string sClass = string.Empty;
                                                if (oArgs.Count > 3)
                                                {
                                                    sClass = oArgs[3].ToString().Trim();
                                                }

                                                if (oGlobals.AddTrigger(oArgs[1].ToString(), oArgs[2].ToString(), false, false, sClass) == false)
                                                {
                                                    EchoText("Invalid regexp in trigger: " + oArgs[1].ToString() + Constants.vbNewLine);
                                                }
                                            }

                                            break;
                                        }

                                    case "untrigger":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                oGlobals.TriggerList.Remove(oArgs[1].ToString());
                                            }

                                            break;
                                        }

                                    case "random":
                                        {
                                            if (oArgs.Count > 2)
                                            {
                                                string argsValue2 = ParseCommand(oGlobals.ParseGlobalVars(oArgs[1].ToString()));
                                                string argsValue3 = ParseCommand(oGlobals.ParseGlobalVars(oArgs[2].ToString()));
                                                sResult = Utility.RandomNumber(Utility.StringToInteger(argsValue2), Utility.StringToInteger(argsValue3)).ToString();
                                            }
                                            else
                                            {
                                                sResult = Utility.RandomNumber(1, 1000).ToString();
                                            }

                                            break;
                                        }

                                    case "config":
                                    case "set":
                                    case "setting":
                                    case "settings":
                                        {
                                            if (oArgs.Count <= 1)
                                            {
                                                ListSettings();
                                            }
                                            else if (oArgs.Count == 2)
                                            {
                                                var switchExpr8 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr8)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Settings Loaded" + Constants.vbNewLine);
                                                            oGlobals.Config.Load(oGlobals.Config.ConfigDir + @"\settings.cfg");
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Settings Saved" + Constants.vbNewLine);
                                                            oGlobals.Config.Save(oGlobals.Config.ConfigDir + @"\settings.cfg");
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigDir + @"\settings.cfg""", AppWinStyle.NormalFocus, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            oGlobals.Config.SetSetting(oArgs[1].ToString());
                                                            break;
                                                        }
                                                }
                                            }
                                            else if (oArgs.Count > 2)
                                            {
                                                try
                                                {
                                                    oGlobals.Config.SetSetting(oArgs[1].ToString(), Utility.ArrayToString(oArgs, 2));
                                                }
                                                catch (Exception ex)
                                                {
                                                    EchoText("Invalid syntax: " + sRow + Constants.vbNewLine);
                                                    EchoText(ex.Message + Constants.vbNewLine);
                                                }
                                            }

                                            break;
                                        }

                                    case "parse":
                                        {
                                            string argsText5 = oGlobals.ParseGlobalVars(GetArgumentString(sRow));
                                            ParseLine(argsText5);
                                            sResult = "";
                                            break;
                                        }

                                    case "beep":
                                    case "bell":
                                        {
                                            if (oGlobals.Config.bPlaySounds == true)
                                            {
                                                Interaction.Beep();
                                            }

                                            sResult = "";
                                            break;
                                        }

                                    case "play":
                                    case "playwave":
                                    case "playsound":
                                        {
                                            if (oGlobals.Config.bPlaySounds == true)
                                            {
                                                string sSound = GetArgumentString(sRow);
                                                if ((sSound.ToLower() ?? "") == "stop")
                                                {
                                                    Sound.StopPlaying();
                                                }
                                                else if (sSound.Length > 0)
                                                {
                                                    Sound.PlayWaveFile(sSound);
                                                }
                                            }

                                            break;
                                        }

                                    case "playsystem":
                                        {
                                            if (oGlobals.Config.bPlaySounds == true)
                                            {
                                                string sSound = GetArgumentString(sRow);
                                                if (sSound.Length > 0)
                                                {
                                                    Sound.PlayWaveSystem(sSound);
                                                }
                                            }

                                            break;
                                        }

                                    case "macro":
                                    case "macros":
                                        {
                                            if (oArgs.Count < 3)
                                            {
                                                if (oArgs.Count < 2)
                                                {
                                                    ListMacros("");
                                                }
                                                else // 2 Arguments
                                                {
                                                    var switchExpr9 = oArgs[1].ToString().ToLower();
                                                    switch (switchExpr9)
                                                    {
                                                        case "load":
                                                            {
                                                                EchoText("Macros Loaded" + Constants.vbNewLine);
                                                                oGlobals.MacroList.Load();
                                                                break;
                                                            }

                                                        case "save":
                                                            {
                                                                EchoText("Macros Saved" + Constants.vbNewLine);
                                                                oGlobals.MacroList.Save();
                                                                break;
                                                            }

                                                        case "clear":
                                                            {
                                                                EchoText("Macros Cleared" + Constants.vbNewLine);
                                                                oGlobals.MacroList.Clear();
                                                                break;
                                                            }

                                                        case "edit":
                                                            {
                                                                Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigProfileDir + @"\macros.cfg""", AppWinStyle.NormalFocus, false);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                ListMacros(Utility.ArrayToString(oArgs, 1));
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            // Add
                                            else if (oGlobals.MacroList.Add(oArgs[1].ToString(), oArgs[2].ToString()) == false)
                                            {
                                                EchoText("Unknown key combination: " + oArgs[1].ToString() + Constants.vbNewLine);
                                            }

                                            break;
                                        }

                                    case "unmacro":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                if (oGlobals.MacroList.Remove(oArgs[1].ToString()) == -1)
                                                {
                                                    EchoText("Unknown key combination: " + oArgs[1].ToString() + Constants.vbNewLine);
                                                }
                                            }

                                            break;
                                        }

                                    case "keys":
                                        {
                                            ListKeys();
                                            break;
                                        }

                                    case "sub":
                                    case "subs":
                                    case "substitute":
                                        {
                                            if (oArgs.Count < 3)
                                            {
                                                if (oArgs.Count < 2)
                                                {
                                                    ListSubstitutes("");
                                                }
                                                else // 2 Arguments
                                                {
                                                    var switchExpr10 = oArgs[1].ToString().ToLower();
                                                    switch (switchExpr10)
                                                    {
                                                        case "load":
                                                            {
                                                                EchoText("Substitutes Loaded" + Constants.vbNewLine);
                                                                oGlobals.SubstituteList.Load(oGlobals.Config.ConfigDir + @"\substitutes.cfg");
                                                                break;
                                                            }

                                                        case "save":
                                                            {
                                                                EchoText("Substitutes Saved" + Constants.vbNewLine);
                                                                oGlobals.SubstituteList.Save(oGlobals.Config.ConfigDir + @"\substitutes.cfg");
                                                                break;
                                                            }

                                                        case "clear":
                                                            {
                                                                EchoText("Substitutes Cleared" + Constants.vbNewLine);
                                                                oGlobals.SubstituteList.Clear();
                                                                break;
                                                            }

                                                        case "edit":
                                                            {
                                                                Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigDir + @"\substitutes.cfg""", AppWinStyle.NormalFocus, false);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                ListSubstitutes(Utility.ArrayToString(oArgs, 1));
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Add
                                                string sClass = string.Empty;
                                                if (oArgs.Count > 3)
                                                {
                                                    sClass = oGlobals.ParseGlobalVars(oArgs[3].ToString());
                                                }

                                                string argsText6 = oGlobals.ParseGlobalVars(oArgs[1].ToString());
                                                string argReplaceBy = oGlobals.ParseGlobalVars(oArgs[2].ToString());
                                                if (oGlobals.SubstituteList.Add(argsText6, argReplaceBy, false, sClass, true) == false)
                                                {
                                                    EchoText("Invalid regexp in substitute: " + oGlobals.ParseGlobalVars(oArgs[1].ToString()) + Constants.vbNewLine);
                                                }
                                            }

                                            break;
                                        }

                                    case "unsub":
                                    case "unsubs":
                                    case "unsubstitute":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                string argText = oGlobals.ParseGlobalVars(oArgs[1].ToString());
                                                oGlobals.SubstituteList.Remove(argText);
                                            }

                                            break;
                                        }

                                    case "gag":
                                    case "gags":
                                    case "squelch":
                                    case "ignore":
                                    case "ignores":
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListGags("");
                                            }
                                            else // 1 Arguments
                                            {
                                                var switchExpr11 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr11)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Gags Loaded" + Constants.vbNewLine);
                                                            oGlobals.GagList.Load();
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Gags Saved" + Constants.vbNewLine);
                                                            oGlobals.GagList.Save();
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Gags Cleared" + Constants.vbNewLine);
                                                            oGlobals.GagList.Clear();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigProfileDir + @"\gags.cfg""", AppWinStyle.NormalFocus, false);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            // Add
                                                            string argsText7 = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                                            if (oGlobals.GagList.Add(argsText7) == false)
                                                            {
                                                                EchoText("Invalid regexp in gag: " + oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1)) + Constants.vbNewLine);
                                                            }

                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }

                                    case "preset":
                                    case "presets":
                                        {
                                            if (oArgs.Count < 3)
                                            {
                                                if (oArgs.Count < 2)
                                                {
                                                    ListPresets("");
                                                }
                                                else // 2 Arguments
                                                {
                                                    var switchExpr12 = oArgs[1].ToString().ToLower();
                                                    switch (switchExpr12)
                                                    {
                                                        case "load":
                                                            {
                                                                EchoText("Presets Loaded" + Constants.vbNewLine);
                                                                oGlobals.PresetList.Load(oGlobals.Config.ConfigDir + @"\presets.cfg");
                                                                var loadVar = "all";
                                                                EventPresetChanged?.Invoke(loadVar);
                                                                break;
                                                            }

                                                        case "save":
                                                            {
                                                                EchoText("Presets Saved" + Constants.vbNewLine);
                                                                oGlobals.PresetList.Save(oGlobals.Config.ConfigDir + @"\presets.cfg");
                                                                break;
                                                            }

                                                        case "clear":
                                                            {
                                                                EchoText("Presets Cleared" + Constants.vbNewLine);
                                                                oGlobals.PresetList.Clear();
                                                                var clearVar = "all";
                                                                EventPresetChanged?.Invoke(clearVar);
                                                                break;
                                                            }

                                                        case "edit":
                                                            {
                                                                Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigDir + @"\presets.cfg""", AppWinStyle.NormalFocus, false);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                ListPresets(Utility.ArrayToString(oArgs, 1));
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            else if (oGlobals.PresetList.ContainsKey(oArgs[1].ToString().ToLower()))
                                            {
                                                oGlobals.PresetList.Add(oArgs[1].ToString().ToLower(), oArgs[2].ToString());
                                                EventPresetChanged?.Invoke(oArgs[1].ToString().ToLower());
                                            }
                                            else
                                            {
                                                EchoText("Invalid #preset keyword.");
                                            }

                                            break;
                                        }

                                    case "ungag":
                                    case "unsquelch":
                                    case "unignore":
                                    case "unignores":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                string argText1 = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                                oGlobals.GagList.Remove(argText1);
                                            }

                                            break;
                                        }

                                    case "highlight":
                                    case "highlights":
                                        {
                                            if (oArgs.Count < 2)
                                            {
                                                ListHighlights("");
                                            }
                                            else // 1 Arguments
                                            {
                                                var switchExpr13 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr13)
                                                {
                                                    case "load":
                                                        {
                                                            EchoText("Highlights Loaded" + Constants.vbNewLine);
                                                            oGlobals.LoadHighlights(oGlobals.Config.ConfigDir + @"\highlights.cfg");
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EchoText("Highlights Saved" + Constants.vbNewLine);
                                                            oGlobals.SaveHighlights(oGlobals.Config.ConfigDir + @"\highlights.cfg");
                                                            break;
                                                        }

                                                    case "clear":
                                                        {
                                                            EchoText("Highlights Cleared" + Constants.vbNewLine);
                                                            oGlobals.HighlightList.Clear();
                                                            oGlobals.HighlightRegExpList.Clear();
                                                            oGlobals.HighlightBeginsWithList.Clear();
                                                            oGlobals.HighlightList.RebuildLineIndex();
                                                            break;
                                                        }

                                                    case "edit":
                                                        {
                                                            Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigDir + @"\highlights.cfg""", AppWinStyle.NormalFocus, false);
                                                            break;
                                                        }

                                                    case "line":
                                                    case "lines":
                                                        {
                                                            if (oArgs.Count > 3)
                                                            {
                                                                string argsKey = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                                bool argbHighlightWholeRow = true;
                                                                oGlobals.HighlightList.Add(argsKey, argbHighlightWholeRow, oGlobals.ParseGlobalVars(oArgs[2].ToString()));
                                                                oGlobals.HighlightList.RebuildLineIndex();
                                                            }

                                                            break;
                                                        }

                                                    case "string":
                                                    case "strings":
                                                        {
                                                            if (oArgs.Count > 3)
                                                            {
                                                                string argsKey1 = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                                bool argbHighlightWholeRow1 = false;
                                                                oGlobals.HighlightList.Add(argsKey1, argbHighlightWholeRow1, oGlobals.ParseGlobalVars(oArgs[2].ToString()));
                                                                oGlobals.HighlightList.RebuildStringIndex();
                                                            }

                                                            break;
                                                        }

                                                    case "beginswith":
                                                        {
                                                            if (oArgs.Count > 3)
                                                            {
                                                                string argsKey2 = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                                string argsColorName = oGlobals.ParseGlobalVars(oArgs[2].ToString());
                                                                oGlobals.HighlightBeginsWithList.Add(argsKey2, argsColorName);
                                                            }

                                                            break;
                                                        }

                                                    case "regexp":
                                                    case "regex":
                                                        {
                                                            if (oArgs.Count > 3)
                                                            {
                                                                string argsRegExp = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                                if (Utility.ValidateRegExp(argsRegExp) == true)
                                                                {
                                                                    string argsKey3 = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3));
                                                                    string argsColorName1 = oGlobals.ParseGlobalVars(oArgs[2].ToString());
                                                                    oGlobals.HighlightRegExpList.Add(argsKey3, argsColorName1);
                                                                }
                                                                else
                                                                {
                                                                    EchoText("Invalid RegExp in highlight: " + oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 3)) + Constants.vbNewLine);
                                                                }
                                                            }

                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            ListHighlights(Utility.ArrayToString(oArgs, 1));
                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }

                                    case "colors":
                                    case "colours":
                                        {
                                            ListColors();
                                            break;
                                        }

                                    case "name":
                                    case "names":
                                        {
                                            if (oArgs.Count < 3)
                                            {
                                                if (oArgs.Count < 2)
                                                {
                                                    ListNames("");
                                                }
                                                else // 2 Arguments
                                                {
                                                    var switchExpr14 = oArgs[1].ToString().ToLower();
                                                    switch (switchExpr14)
                                                    {
                                                        case "load":
                                                            {
                                                                EchoText("Names Loaded" + Constants.vbNewLine);
                                                                oGlobals.NameList.Load(oGlobals.Config.ConfigDir + @"\names.cfg");
                                                                break;
                                                            }

                                                        case "save":
                                                            {
                                                                EchoText("Names Saved" + Constants.vbNewLine);
                                                                oGlobals.NameList.Save(oGlobals.Config.ConfigDir + @"\names.cfg");
                                                                break;
                                                            }

                                                        case "clear":
                                                            {
                                                                EchoText("Names Cleared" + Constants.vbNewLine);
                                                                oGlobals.NameList.Clear();
                                                                break;
                                                            }

                                                        case "edit":
                                                            {
                                                                Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + oGlobals.Config.ConfigDir + @"\names.cfg""", AppWinStyle.NormalFocus, false);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                ListNames(Utility.ArrayToString(oArgs, 1));
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // Add
                                                for (int I = 2, loopTo2 = oArgs.Count - 1; I <= loopTo2; I++)
                                                    oGlobals.NameList.Add(oArgs[I].ToString(), oArgs[1].ToString());
                                                oGlobals.NameList.RebuildIndex();
                                            }

                                            break;
                                        }

                                    case "unname":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                for (int I = 1, loopTo3 = oArgs.Count - 1; I <= loopTo3; I++)
                                                    oGlobals.NameList.Remove(oArgs[I].ToString());
                                            }

                                            break;
                                        }

                                    case "edit":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                string sFile = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1));
                                                if (sFile.ToLower().EndsWith(".cmd") == false & sFile.ToLower().EndsWith(".js") == false)
                                                {
                                                    sFile += ".cmd";
                                                }

                                                if (sFile.IndexOf(@"\") == -1)
                                                {
                                                    string sLocation = oGlobals.Config.ScriptDir;
                                                    if (sLocation.EndsWith(@"\"))
                                                    {
                                                        sFile = sLocation + sFile;
                                                    }
                                                    else
                                                    {
                                                        sFile = sLocation + @"\" + sFile;
                                                    }
                                                }

                                                Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + sFile + "\"", AppWinStyle.NormalFocus, false);
                                            }

                                            break;
                                        }

                                    case "help":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                if ((oArgs[1].ToString().ToLower() ?? "") == "edit")	// New topic
                                                {
                                                    string sTemp = "index.txt";
                                                    if (oArgs.Count > 2)
                                                    {
                                                        sTemp = oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 2));
                                                    }

                                                    if (sTemp.ToLower().EndsWith(".txt") == false)
                                                    {
                                                        sTemp += ".txt";
                                                    }

                                                    if (sTemp.IndexOf(@"\") == -1)
                                                    {
                                                        sTemp = LocalDirectory.Path + @"\Help\" + sTemp;
                                                    }

                                                    Interaction.Shell("\"" + oGlobals.Config.sEditor + "\" \"" + sTemp + "\"", AppWinStyle.NormalFocus, false);
                                                }
                                                else
                                                {
                                                    ShowHelp(oGlobals.ParseGlobalVars(Utility.ArrayToString(oArgs, 1)));
                                                }
                                            }
                                            else
                                            {
                                                ShowHelp();
                                            }

                                            break;
                                        }

                                    case "flash":
                                        {
                                            EventFlashWindow?.Invoke();
                                            break;
                                        }

                                    case "script":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                var switchExpr15 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr15)
                                                {
                                                    case "abort":
                                                        {
                                                            EventScriptAbort?.Invoke(Utility.ArrayToString(oArgs, 2));
                                                            break;
                                                        }

                                                    case "pause":
                                                        {
                                                            EventScriptPause?.Invoke(Utility.ArrayToString(oArgs, 2));
                                                            break;
                                                        }

                                                    case "pauseorresume":
                                                        {
                                                            EventScriptPauseOrResume?.Invoke(Utility.ArrayToString(oArgs, 2));
                                                            break;
                                                        }

                                                    case "resume":
                                                        {
                                                            EventScriptResume?.Invoke(Utility.ArrayToString(oArgs, 2));
                                                            break;
                                                        }

                                                    case "trace":
                                                        {
                                                            EventScriptTrace?.Invoke(Utility.ArrayToString(oArgs, 2));
                                                            break;
                                                        }

                                                    case "vars":
                                                    case "variables":
                                                        {
                                                            if (oArgs.Count > 3)
                                                            {
                                                                EventScriptVariables?.Invoke(oArgs[2].ToString(), oArgs[3].ToString());
                                                            }
                                                            else if (oArgs.Count > 2)
                                                            {
                                                                EventScriptVariables?.Invoke(oArgs[2].ToString(), "");
                                                            }
                                                            else
                                                            {
                                                                EventScriptVariables?.Invoke("", "");
                                                            }

                                                            break;
                                                        }

                                                    case "debug":
                                                    case "debuglevel":
                                                        {
                                                            if (oArgs.Count > 2)
                                                            {
                                                                EventScriptDebug?.Invoke(Conversions.ToInteger(Utility.StringToDouble(oArgs[2].ToString())), Utility.ArrayToString(oArgs, 3));
                                                            }

                                                            break;
                                                        }

                                                    case "explorer":
                                                        {
                                                            EventShowScriptExplorer?.Invoke();
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            EventListScripts?.Invoke(Utility.ArrayToString(oArgs, 2));
                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                            {
                                                EventListScripts?.Invoke("");
                                            }

                                            if (oArgs.Count < 2)
                                            {
                                            }
                                            // ListScripts("")
                                            else
                                            {
                                            } // 2 Arguments

                                            break;
                                        }

                                    case "status":
                                    case "statusbar":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                var switchExpr16 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr16)
                                                {
                                                    case "1":
                                                    case "2":
                                                    case "3":
                                                    case "4":
                                                    case "5":
                                                    case "6":
                                                    case "7":
                                                    case "8":
                                                    case "9":
                                                    case "10":
                                                        {
                                                            StatusBar(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)), int.Parse(oArgs[1].ToString()));
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            StatusBar(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 1)));
                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }

                                    case "layout":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                var switchExpr17 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr17)
                                                {
                                                    case "load":
                                                        {
                                                            EventLoadLayout?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EventSaveLayout?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                            {
                                                EventLoadLayout?.Invoke("@windowsize@");
                                            }

                                            break;
                                        }

                                    case "window":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                var switchExpr18 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr18)
                                                {
                                                    case "add":
                                                    case "show":
                                                        {
                                                            EventAddWindow?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "remove":
                                                        {
                                                            EventRemoveWindow?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "close":
                                                    case "hide":
                                                        {
                                                            EventCloseWindow?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "load":
                                                        {
                                                            EventLoadLayout?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EventLoadLayout?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }

                                    case "comment":
                                        {
                                            if (oArgs.Count > 2)
                                            {
                                                EventChangeWindowTitle?.Invoke(oArgs[1].ToString(), oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                            }

                                            break;
                                        }

                                    case "profile":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                var switchExpr19 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr19)
                                                {
                                                    case "load":
                                                        {
                                                            EventLoadProfile?.Invoke();
                                                            break;
                                                        }

                                                    case "save":
                                                        {
                                                            EventSaveProfile?.Invoke();
                                                            break;
                                                        }
                                                }
                                            }

                                            break;
                                        }

                                    case "goto":
                                    case "walk":
                                        {
                                            EventMapperCommand?.Invoke(sRow.Replace("#", "#mapper "));
                                            break;
                                        }

                                    case "mapper":
                                    case "map":
                                        {
                                            EventMapperCommand?.Invoke(sRow);
                                            break;
                                        }

                                    case "plugin":
                                        {
                                            if (oArgs.Count > 1)
                                            {
                                                var switchExpr20 = oArgs[1].ToString().ToLower();
                                                switch (switchExpr20)
                                                {
                                                    case "enable":
                                                        {
                                                            EnablePlugin?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "disable":
                                                        {
                                                            DisablePlugin?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "load":
                                                        {
                                                            LoadPlugin?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "unload":
                                                        {
                                                            UnloadPlugin?.Invoke(oGlobals.ParseGlobalVars(ParseAllArgs(oArgs, 2)));
                                                            break;
                                                        }

                                                    case "reload":
                                                        {
                                                            ReloadPlugins?.Invoke();
                                                            break;
                                                        }
                                                }
                                            }
                                            else
                                            {
                                                ListPlugins?.Invoke();
                                            }

                                            break;
                                        }

                                    case "xml":
                                        {
                                            EventRawToggle?.Invoke(Utility.ArrayToString(oArgs, 1));
                                            break;
                                        }

                                    case "raw":
                                        {
                                            EventSendRaw?.Invoke(Utility.ArrayToString(oArgs, 1) + Constants.vbLf);
                                            break;
                                        }

                                    default:
                                        {
                                            if (Conversions.ToString(oArgs[0]).Trim().StartsWith("#") == true && ColorCode.IsHexString(Conversions.ToString(oArgs[0]).Trim()) == true)
                                            {
                                                // Hex Code
                                                sResult = sRow;
                                            }
                                            else
                                            {
                                                EchoText("Unknown command: " + sRow + Constants.vbNewLine);
                                            }

                                            break;
                                        }
                                }
                            }
                        }
                    }
                    else if (sRow.StartsWith(Conversions.ToString(oGlobals.Config.ScriptChar)))
                    {
                        RunScript(sRow);
                    }
                    else
                    {
                        if (bSendToGame == true)
                        {
                            // Send Text To Game
                            string argsText8 = oGlobals.ParseGlobalVars(sRow);
                            SendTextToGame(argsText8, bUserInput, sOrigin);
                        }

                        if (sResult.Length == 0)
                        {
                            sResult = sRow;
                        }
                    }
                }
            }

            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
            return sResult;
        }

        public string Eval(string sText)
        {
            string s = m_oEval.EvalString(sText, oGlobals);
            return s;
        }

        private void EchoText(string sText, string sWindow = "")
        {
            EventEchoText?.Invoke(sText, sWindow);
        }

        private void EchoColorText(string sText, Color oColor, Color oBgColor, string sWindow = "")
        {
            EventEchoColorText?.Invoke(sText, oColor, oBgColor, sWindow);
        }

        private void SendText(string sText, [Optional, DefaultParameterValue(false)] bool bUserInput, [Optional, DefaultParameterValue("")] string sOrigin)
        {
            EventSendText?.Invoke(sText, bUserInput, sOrigin);
        }

        private void RunScript(string sText)
        {
            EventRunScript?.Invoke(sText);
        }

        private void ClearWindow(string sWindow = "")
        {
            EventClearWindow?.Invoke(sWindow);
        }

        private void VariableChanged(string sVariable)
        {
            EventVariableChanged?.Invoke(sVariable);
        }

        private void ParseLine(string sText)
        {
            EventParseLine?.Invoke(sText);
        }

        private void StatusBar(string sText, int iIndex = 1)
        {
            EventStatusBar?.Invoke(sText, iIndex);
        }

        private string GetKeywordString(string strRow)
        {
            strRow = strRow.Trim();
            if (strRow.IndexOf(" ") > -1)
            {
                return strRow.Substring(0, strRow.IndexOf(" "));
            }
            else
            {
                return strRow;
            }
        }

        private string GetArgumentString(string strRow)
        {
            strRow = strRow.Trim();
            if (strRow.IndexOf(" ") > -1)
            {
                return strRow.Substring(strRow.IndexOf(" ") + 1);
            }
            else
            {
                return string.Empty;
            }
        }

        private string ParseAlias(string sText)
        {
            string sResult = "";
            var oArgs = new ArrayList();
            oArgs = Utility.ParseArgs(sText);
            string sKey = GetKeywordString(sText);
            if (oGlobals.AliasList.ContainsKey(sKey) == true)
            {
                sResult = Conversions.ToString(oGlobals.AliasList[sKey]);
                if (sResult.Contains("$") == true)
                {
                    sResult = sResult.Replace("$0", GetArgumentString(sText).Replace("\"", ""));
                    for (int i = 1, loopTo = oGlobals.Config.iArgumentCount - 1; i <= loopTo; i++)
                    {
                        if (i > oArgs.Count - 1)
                        {
                            sResult = sResult.Replace("$" + i.ToString(), "");
                        }
                        else
                        {
                            sResult = sResult.Replace("$" + i.ToString(), oArgs[i].ToString().Replace("\"", ""));
                        }
                    }
                }
                else
                {
                    sResult += " " + GetArgumentString(sText);
                }
            }

            return sResult;
        }

        private void ShowHelp(string sFile = "index.txt")
        {
            if (sFile.IndexOf(@"\") == -1)
            {
                if (sFile.ToLower().EndsWith(".txt") == false)
                {
                    sFile += ".txt";
                }

                sFile = LocalDirectory.Path + @"\Help\" + sFile.Replace(" ", "_"); // Replace with "_" for sub categories
            }

            try
            {
                var fi = new FileInfo(sFile);
                if (fi.Exists & fi.Length > 0)
                {
                    var objFile = new FileStream(sFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var objReader = new StreamReader(objFile);
                    int I = 0;
                    string strLine = string.Empty;
                    while (objReader.Peek() > -1)
                    {
                        I += 1;
                        strLine = objReader.ReadLine().TrimStart();
                        if (strLine.Length > 0)
                        {
                            EchoText(strLine + Constants.vbNewLine);
                        }
                    }

                    objReader.Close();
                    objFile.Close();
                }
            }
            catch (FileNotFoundException ex)
            {
                EchoText("Topic does not exist.");
            }
            catch (FileLoadException ex)
            {
                EchoText("File Load Exception: " + ex.Message);
            }
        }

        private void SendTextToGame(string sText, [Optional, DefaultParameterValue(false)] bool bUserInput, [Optional, DefaultParameterValue("")] string sOrigin)
        {
            if (sText.Contains(@"\"))
            {
                sText = sText.Replace(@"\\", "¤");
                sText = sText.Replace(@"\x", "¤x");
                sText = sText.Replace(@"\@", "¤@");
                sText = sText.Replace(@"\", "");
                sText = sText.Replace("¤", @"\");
            }

            // EchoText("Send: " & sText & vbNewLine)
            SendText(sText, bUserInput, sOrigin);
        }

        // Private Function ParseAllArgs(ByoList As ArrayList, Optional ByVal iStartIndex As Integer = 1) As String
        // Dim sResult As String = String.Empty

        // For i As Integer = iStartIndex To oList.Count - 1
        // If Not IsNothing(oList.Item(i)) Then
        // sResult &= " " & ParseCommand(oList.Item(i).ToString)
        // End If
        // Next

        // If sResult.Length > 0 Then
        // sResult = sResult.Substring(1) ' Remove first space
        // End If

        // Return sResult
        // End Function

        private string ParseAllArgs(ArrayList oList, int iStartIndex = 1, bool bParseQuickSend = true)
        {
            string sResult = string.Empty;
            string sCommand = string.Empty;
            for (int i = iStartIndex, loopTo = oList.Count - 1; i <= loopTo; i++)
            {
                if (!Information.IsNothing(oList[i]))
                {
                    sCommand += " " + oList[i].ToString();
                }
            }

            if (sCommand.Length > 0)
            {
                if (sCommand.StartsWith(" .") == false)
                {
                    sResult = ParseCommand(sCommand.Substring(1), false, false, "", bParseQuickSend); // Remove first space
                }
                else
                {
                    sResult = sCommand.Substring(1);
                }	// Remove first space
            }

            return sResult;
        }

        private bool EvalIf(string sText)
        {
            return m_oEval.DoEval(sText, oGlobals);
        }

        private string EvalMath(string sText)
        {
            if ((System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator ?? "") != ".")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            }

            double d = m_oMathEval.Evaluate(sText);
            return d.ToString();
        }

        private void ListSettings()
        {
            EchoText(Constants.vbNewLine + "Active settings: " + Constants.vbNewLine);
            EchoText("abortdupescript=" + oGlobals.Config.bAbortDupeScript.ToString() + Constants.vbNewLine);
            EchoText("autolog=" + oGlobals.Config.bAutoLog.ToString() + Constants.vbNewLine);
            EchoText("automapper=" + oGlobals.Config.bAutoMapper.ToString() + Constants.vbNewLine);
            EchoText("commandchar=" + oGlobals.Config.cCommandChar.ToString() + Constants.vbNewLine);
            EchoText("configdir=" + oGlobals.Config.sConfigDir + Constants.vbNewLine);
            EchoText("connectstring=" + oGlobals.Config.sConnectString.ToString() + Constants.vbNewLine);
            EchoText("editor=" + oGlobals.Config.sEditor + Constants.vbNewLine);
            EchoText("ignoreclosealert=" + oGlobals.Config.bIgnoreCloseAlert.ToString() + Constants.vbNewLine);
            EchoText("ignorescriptwarnings=" + oGlobals.Config.bIgnoreScriptWarnings.ToString() + Constants.vbNewLine);
            EchoText("keepinputtext=" + oGlobals.Config.bKeepInput.ToString() + Constants.vbNewLine);
            EchoText("logdir=" + oGlobals.Config.sLogDir + Constants.vbNewLine);
            EchoText("maxgosubdepth=" + oGlobals.Config.iMaxGoSubDepth + Constants.vbNewLine);
            EchoText("maxrowbuffer=" + oGlobals.Config.iBufferLineSize.ToString() + Constants.vbNewLine);
            EchoText("monstercountignorelist=" + oGlobals.Config.sIgnoreMonsterList + Constants.vbNewLine);
            EchoText("muted=" + (!oGlobals.Config.bPlaySounds).ToString() + Constants.vbNewLine);
            EchoText("mycommandchar=" + oGlobals.Config.cMyCommandChar.ToString() + Constants.vbNewLine);
            EchoText("parsegameonly=" + oGlobals.Config.bParseGameOnly.ToString() + Constants.vbNewLine);
            EchoText("prompt=" + oGlobals.Config.sPrompt + Constants.vbNewLine);
            EchoText("reconnect=" + oGlobals.Config.bReconnect.ToString() + Constants.vbNewLine);
            EchoText("roundtimeoffset=" + oGlobals.Config.dRTOffset + Constants.vbNewLine);
            EchoText("showlinks=" + oGlobals.Config.bShowLinks.ToString() + Constants.vbNewLine);
            EchoText("scriptchar=" + oGlobals.Config.ScriptChar.ToString() + Constants.vbNewLine);
            EchoText("scriptdir=" + oGlobals.Config.sScriptDir + Constants.vbNewLine);
            EchoText("scripttimeout=" + oGlobals.Config.iScriptTimeout.ToString() + Constants.vbNewLine);
            EchoText("separatorchar=" + oGlobals.Config.cSeparatorChar.ToString() + Constants.vbNewLine);
            EchoText("spelltimer=" + oGlobals.Config.bShowSpellTimer.ToString() + Constants.vbNewLine);
            EchoText("triggeroninput=" + oGlobals.Config.bTriggerOnInput.ToString() + Constants.vbNewLine);
            EchoText("servertimeout=" + oGlobals.Config.iServerActivityTimeout.ToString() + Constants.vbNewLine);
            EchoText("servertimeoutcommand=" + oGlobals.Config.sServerActivityCommand.ToString() + Constants.vbNewLine);
            EchoText("usertimeout=" + oGlobals.Config.iUserActivityTimeout.ToString() + Constants.vbNewLine);
            EchoText("usertimeoutcommand=" + oGlobals.Config.sUserActivityCommand.ToString() + Constants.vbNewLine);
        }

        private void ListColors()
        {
            EchoText("Available colors: " + Constants.vbNewLine);
            KnownColor c;
            foreach (string s in Enum.GetNames(typeof(KnownColor)))
            {
                c = (KnownColor)Enum.Parse(typeof(KnownColor), s);
                if (c > KnownColor.Transparent & c < KnownColor.ButtonFace)
                {
                    string argsText = s + Constants.vbNewLine;
                    var argoColor = Color.FromKnownColor(c);
                    var argoBgColor = Color.Transparent;
                    EchoColorText(argsText, argoColor, argoBgColor);
                }
            }
        }

        private void ListKeys()
        {
            EchoText("Available keycodes: " + Constants.vbNewLine);
            foreach (string s in Enum.GetNames(typeof(KeyCode.Keys)))
                EchoText(s + Constants.vbNewLine);
        }

        private void ListVariables(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active variables: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.VariableList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    foreach (DictionaryEntry de in oGlobals.VariableList)
                    {
                        if (bUsePattern == false | de.Key.ToString().Contains(sPattern))
                        {
                            if (((Globals.Variables.Variable)de.Value).oType == Globals.Variables.VariableType.SaveToFile)
                            {
                                EchoText("$" + de.Key.ToString() + "=" + ((Globals.Variables.Variable)de.Value).sValue + Constants.vbNewLine);
                                I += 1;
                            }
                        }
                    }

                    foreach (DictionaryEntry de in oGlobals.VariableList)
                    {
                        if (bUsePattern == false | de.Key.ToString().Contains(sPattern))
                        {
                            if (((Globals.Variables.Variable)de.Value).oType == Globals.Variables.VariableType.Temporary)
                            {
                                EchoText("(temporary) $" + de.Key.ToString() + "=" + ((Globals.Variables.Variable)de.Value).sValue + Constants.vbNewLine);
                                I += 1;
                            }
                        }
                    }

                    foreach (DictionaryEntry de in oGlobals.VariableList)
                    {
                        if (bUsePattern == false | de.Key.ToString().Contains(sPattern))
                        {
                            if (((Globals.Variables.Variable)de.Value).oType == Globals.Variables.VariableType.Reserved)
                            {
                                EchoText("(reserved) $" + de.Key.ToString() + "=" + ((Globals.Variables.Variable)de.Value).sValue + Constants.vbNewLine);
                                I += 1;
                            }
                        }
                    }

                    foreach (DictionaryEntry de in oGlobals.VariableList)
                    {
                        if (bUsePattern == false | de.Key.ToString().Contains(sPattern))
                        {
                            if (((Globals.Variables.Variable)de.Value).oType == Globals.Variables.VariableType.Server)
                            {
                                EchoText("(server) $" + de.Key.ToString() + "=" + ((Globals.Variables.Variable)de.Value).sValue + Constants.vbNewLine);
                                I += 1;
                            }
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.VariableList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListVariables", "Unable to aquire reader lock.");
            }
        }

        private void ListSubstitutes(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active substitutes: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.SubstituteList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    var myEnumeratorWords = oGlobals.SubstituteList.GetEnumerator();
                    while (myEnumeratorWords.MoveNext())
                    {
                        Globals.SubstituteRegExp.Substitute o = (Globals.SubstituteRegExp.Substitute)myEnumeratorWords.Current;
                        EchoText(o.sText + " => " + o.sReplaceBy + Constants.vbNewLine);
                        I += 1;
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.SubstituteList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListSubstitutes", "Unable to aquire reader lock.");
            }
        }

        private void ListGags(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active gags: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.GagList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    var myEnumeratorWords = oGlobals.GagList.GetEnumerator();
                    while (myEnumeratorWords.MoveNext())
                    {
                        Globals.GagRegExp.Gag o = (Globals.GagRegExp.Gag)myEnumeratorWords.Current;
                        EchoText(o.Text + Constants.vbNewLine);
                        I += 1;
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.GagList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListGags", "Unable to aquire reader lock.");
            }
        }

        private void ListNames(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active names: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.NameList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    foreach (DictionaryEntry de in oGlobals.NameList)
                    {
                        if (bUsePattern == false | de.Value.ToString().Contains(sPattern))
                        {
                            string argsText = Conversions.ToString(de.Key) + Constants.vbNewLine;
                            EchoColorText(argsText, ((Names.Name)de.Value).FgColor, ((Names.Name)de.Value).BgColor);
                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.NameList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListNames", "Unable to aquire reader lock.");
            }
        }

        private void ListPresets(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active presets: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.PresetList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    foreach (DictionaryEntry de in oGlobals.PresetList)
                    {
                        if (bUsePattern == false | de.Value.ToString().Contains(sPattern))
                        {
                            string argsText = Conversions.ToString(de.Key) + Constants.vbNewLine;
                            EchoColorText(argsText, ((Globals.Presets.Preset)de.Value).FgColor, ((Globals.Presets.Preset)de.Value).BgColor);
                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.PresetList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListPresets", "Unable to aquire reader lock.");
            }
        }

        private void ListHighlights(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active highlights: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.HighlightList.AcquireReaderLock())
            {
                try
                {
                    EchoText("Highlight Strings: " + Constants.vbNewLine);
                    /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                    int I = 0;
                    foreach (DictionaryEntry de in oGlobals.HighlightList)
                    {
                        if (bUsePattern == false | de.Value.ToString().Contains(sPattern))
                        {
                            if (((Highlights.Highlight)de.Value).HighlightWholeRow == false)
                            {
                                string argsText = Conversions.ToString("[" + ((Highlights.Highlight)de.Value).ClassName + ":" + Interaction.IIf(((Highlights.Highlight)de.Value).IsActive, "ON", "OFF") + "] " + Conversions.ToString(de.Key) + Constants.vbNewLine);
                                EchoColorText(argsText, ((Highlights.Highlight)de.Value).FgColor, ((Highlights.Highlight)de.Value).BgColor);
                            }

                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.HighlightList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListHighlights", "Unable to aquire reader lock.");
            }

            if (oGlobals.HighlightList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    EchoText("Highlight Lines: " + Constants.vbNewLine);
                    /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */
                    foreach (DictionaryEntry de in oGlobals.HighlightList)
                    {
                        if (bUsePattern == false | de.Value.ToString().Contains(sPattern))
                        {
                            if (((Highlights.Highlight)de.Value).HighlightWholeRow == true)
                            {
                                string argsText1 = Conversions.ToString("[" + ((Highlights.Highlight)de.Value).ClassName + ":" + Interaction.IIf(((Highlights.Highlight)de.Value).IsActive, "ON", "OFF") + "] " + Conversions.ToString(de.Key) + Constants.vbNewLine);
                                EchoColorText(argsText1, ((Highlights.Highlight)de.Value).FgColor, ((Highlights.Highlight)de.Value).BgColor);
                            }

                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.HighlightList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListHighlights", "Unable to aquire reader lock.");
            }

            if (oGlobals.HighlightBeginsWithList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    EchoText("Highlight BeginsWith: " + Constants.vbNewLine);
                    foreach (DictionaryEntry de in oGlobals.HighlightBeginsWithList)
                    {
                        if (bUsePattern == false | de.Value.ToString().Contains(sPattern))
                        {
                            Globals.HighlightLineBeginsWith.Highlight oHighlight = (Globals.HighlightLineBeginsWith.Highlight)de.Value;
                            string argsText2 = Conversions.ToString("[" + oHighlight.ClassName + ":" + Interaction.IIf(oHighlight.IsActive, "ON", "OFF") + "] " + Conversions.ToString(de.Key) + Constants.vbNewLine);
                            EchoColorText(argsText2, oHighlight.FgColor, oHighlight.BgColor);
                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.HighlightBeginsWithList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListHighlights", "Unable to aquire reader lock.");
            }

            if (oGlobals.HighlightRegExpList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    EchoText("Highlight RegExp: " + Constants.vbNewLine);
                    foreach (DictionaryEntry de in oGlobals.HighlightRegExpList)
                    {
                        if (bUsePattern == false | de.Value.ToString().Contains(sPattern))
                        {
                            Globals.HighlightRegExp.Highlight oHighlight = (Globals.HighlightRegExp.Highlight)de.Value;
                            string argsText3 = Conversions.ToString("[" + oHighlight.ClassName + ":" + Interaction.IIf(oHighlight.IsActive, "ON", "OFF") + "] " + Conversions.ToString(de.Key) + Constants.vbNewLine);
                            EchoColorText(argsText3, oHighlight.FgColor, oHighlight.BgColor);
                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.HighlightRegExpList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListHighlights", "Unable to aquire reader lock.");
            }
        }

        private void ListMacros(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active macros: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.MacroList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    foreach (DictionaryEntry de in oGlobals.MacroList)
                    {
                        if (bUsePattern == false | de.Value.ToString().Contains(sPattern))
                        {
                            EchoText(((Keys)Conversions.ToInteger(de.Key)).ToString() + "=" + ((Macros.Macro)de.Value).sAction + Constants.vbNewLine);
                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.MacroList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListMacros", "Unable to aquire reader lock.");
            }
        }

        private void ListTriggers(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active triggers: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.TriggerList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    foreach (DictionaryEntry de in oGlobals.TriggerList)
                    {
                        if (bUsePattern == false | de.Key.ToString().Contains(sPattern))
                        {
                            EchoText(de.Key.ToString() + "=" + ((Globals.Triggers.Trigger)de.Value).sAction + Constants.vbNewLine);
                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.TriggerList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListTriggers", "Unable to aquire reader lock.");
            }
        }

        private void ListAliases(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active aliases: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.AliasList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    foreach (DictionaryEntry de in oGlobals.AliasList)
                    {
                        if (bUsePattern == false | de.Key.ToString().Contains(sPattern))
                        {
                            EchoText(de.Key.ToString() + "=" + de.Value.ToString() + Constants.vbNewLine);
                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.AliasList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListAliases", "Unable to aquire reader lock.");
            }
        }

        private void ListClasses(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active classes: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.ClassList.AcquireReaderLock())
            {
                try
                {
                    int I = 0;
                    foreach (DictionaryEntry de in oGlobals.ClassList)
                    {
                        if (bUsePattern == false | de.Key.ToString().Contains(sPattern))
                        {
                            EchoText(de.Key.ToString() + "=" + Conversions.ToBoolean(de.Value).ToString() + Constants.vbNewLine);
                            I += 1;
                        }
                    }

                    if (I == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.ClassList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListClasses", "Unable to aquire reader lock.");
            }
        }

        private void ListEvents(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active event queue: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.Events.EventList.AcquireReaderLock())
            {
                try
                {
                    int J = 0;
                    for (int I = 0, loopTo = oGlobals.Events.EventList.Count - 1; I <= loopTo; I++)
                    {
                        if (bUsePattern == false | ((Events.Queue.EventItem)oGlobals.Events.EventList.get_Item(I)).sAction.Contains(sPattern))
                        {
                            EchoText("(" + ((Events.Queue.EventItem)oGlobals.Events.EventList.get_Item(I)).oDate + ") " + ((Events.Queue.EventItem)oGlobals.Events.EventList.get_Item(I)).sAction + Constants.vbNewLine);
                            J += 1;
                        }
                    }

                    if (J == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.Events.EventList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListEvents", "Unable to aquire reader lock.");
            }
        }

        private void ListCommandQueue(string sPattern)
        {
            EchoText(Constants.vbNewLine + "Active command queue: " + Constants.vbNewLine);
            bool bUsePattern = false;
            if (sPattern.Length > 0)
            {
                bUsePattern = true;
                EchoText("Filter: " + sPattern + Constants.vbNewLine);
            }

            if (oGlobals.CommandQueue.EventList.AcquireReaderLock())
            {
                try
                {
                    int J = 0;
                    for (int I = 0, loopTo = oGlobals.CommandQueue.EventList.Count - 1; I <= loopTo; I++)
                    {
                        if (bUsePattern == false | ((CommandQueue.Queue.EventItem)oGlobals.CommandQueue.EventList.get_Item(I)).sAction.Contains(sPattern))
                        {
                            EchoText("(" + ((CommandQueue.Queue.EventItem)oGlobals.CommandQueue.EventList.get_Item(I)).dDelay + ") " + ((CommandQueue.Queue.EventItem)oGlobals.CommandQueue.EventList.get_Item(I)).sAction + Constants.vbNewLine);
                            J += 1;
                        }
                    }

                    if (J == 0)
                    {
                        EchoText("None." + Constants.vbNewLine);
                    }
                }
                finally
                {
                    oGlobals.CommandQueue.EventList.ReleaseReaderLock();
                }
            }
            else
            {
                GenieError.Error("ListCommandQueue", "Unable to aquire reader lock.");
            }
        }
    }
}