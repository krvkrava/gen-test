using UnityEditor;
using UnityEngine;

namespace Editor.MenuItems
{
    public static class MenuItems
    {
        private const string CLEAR_PLAYER_PREFS_PATH = "Utils/Clear player prefs"; 
        
        [MenuItem(CLEAR_PLAYER_PREFS_PATH)]
        public static void ShowWindow()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}