﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using PasteIntoFile.Properties;

namespace PasteIntoFile
{
    public class RegistryUtil
    {
        // Please note that registry keys are also created by installer and removed upon uninstall
        // Always keep the "Installer/PasteIntoFile.wxs" up to date with the keys used below!
        

        private static string PRIMARY_KEY_NAME = "PasteIntoFile";
        
        /// <summary>
        /// Opens a number of class sub keys according to the requested type.
        /// Passing type=null (default) will return all keys for all types
        /// </summary>
        /// <param name="type">Class type, one of "Directory", "*" or null</param>
        /// <returns>List of registry keys</returns>
        private static IEnumerable<RegistryKey> OpenClassKeys(string type = null) {
	        var classes = Registry.CurrentUser.CreateSubKey(@"Software\Classes");
	        var keys = new List<RegistryKey>();
	        if (type == null || type == "Directory")
		        keys.AddRange(new [] {classes.CreateSubKey(@"Directory\shell"), classes.CreateSubKey(@"Directory\Background\shell")});
	        if (type == null || type == "*")
		        keys.Add(classes.CreateSubKey(@"*\shell"));
	        return keys;
        }


        public static bool IsDarkMode()
        {
	        try
	        {
		        var v = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
		        return v != null && v.ToString() == "0";
	        }
	        catch
	        {
		        // ignored
	        }

	        return false;
        }
        
        
        /// <summary>
        /// Checks if context menu entry is registered
        /// </summary>
        /// <returns>context menu entry registration status (true/false)</returns>
        public static bool IsContextMenuEntryRegistered()
        {
	        foreach (var classKey in OpenClassKeys())
	        {
		        if (classKey == null || !classKey.GetSubKeyNames().Contains(PRIMARY_KEY_NAME)) return false;
	        }
	        return true;
        }
        
        /// <summary>
        /// Remove context menu entry
        /// </summary>
        public static void UnRegisterContextMenuEntry()
        {
	        foreach (var classKey in OpenClassKeys())
	        {
		        classKey.DeleteSubKeyTree(PRIMARY_KEY_NAME);
	        }
        }

        /// <summary>
        /// Create context menu entry
        /// </summary>
        public static void RegisterContextMenuEntry(bool silent = false)
        {
	        // Documentation:
	        // https://docs.microsoft.com/en-us/windows/win32/shell/context
	        // https://docs.microsoft.com/en-us/windows/win32/shell/context-menu
	        
	        // register "paste into file" for directory context menu
	        foreach (var classKey in OpenClassKeys("Directory"))
	        {
		        var key = classKey.CreateSubKey(PRIMARY_KEY_NAME);
		        key.SetValue("", Resources.str_contextentry);
		        key.SetValue("Icon", "\"" + Application.ExecutablePath + "\",0");
		        key = key.CreateSubKey("command");
		        key.SetValue("", "\"" + Application.ExecutablePath + "\" paste \"%V\"");

	        }

	        // register "copy from file" for file context menu (any extension)
	        foreach (var classKey in OpenClassKeys("*"))
	        {
		        var key = classKey.CreateSubKey(PRIMARY_KEY_NAME);
		        key.SetValue("", Resources.str_contextentry_copyfromfile);
		        key.SetValue("Icon", "\"" + Application.ExecutablePath + "\",0");
		        key = key.CreateSubKey("command");
		        key.SetValue("", "\"" + Application.ExecutablePath + "\" copy \"%V\"");

	        }

        }

        /// <summary>
        /// Checks if autostart is registered
        /// </summary>
        /// <returns>autostart registration status (true/false)</returns>
        public static bool IsAutostartRegistered() {
	        return Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)?
		        .GetValue(PRIMARY_KEY_NAME) != null;
        }
        
        /// <summary>
        /// Register autostart
        /// </summary>
        public static void RegisterAutostart() {
	        // The path to the key where Windows looks for startup applications
	        Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)?
		        .SetValue(PRIMARY_KEY_NAME, "\"" + Application.ExecutablePath + "\" tray");
        }
        
        /// <summary>
        /// Remove autostart registration
        /// </summary>
        public static void UnRegisterAutostart() {
	        // The path to the key where Windows looks for startup applications
	        Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)?
		        .DeleteValue(PRIMARY_KEY_NAME, false);
        }

    }
}