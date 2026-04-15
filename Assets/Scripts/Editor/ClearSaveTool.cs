#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace YesChef.EditorTools
{
    public static class ClearSaveTool
    {
        [MenuItem("YesChef/Clear Saved Data", false, 50)]
        private static void ClearSaveFile()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "gameData.json");

            if (!File.Exists(savePath))
            {
                EditorUtility.DisplayDialog("No Save Data", 
                    "No saved game data found to delete.", "OK");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog("Clear Saved Data", 
                $"Are you sure you want to delete the save file?\n\n{savePath}", 
                "Delete", "Cancel");

            if (confirm)
            {
                try
                {
                    File.Delete(savePath);
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Success", 
                        "Saved game data has been cleared.", "OK");
                    Debug.Log($"Save file deleted: {savePath}");
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("Error", 
                        $"Failed to delete save file:\n{e.Message}", "OK");
                    Debug.LogError($"Clear save failed: {e.Message}");
                }
            }
        }

        [MenuItem("YesChef/Clear Saved Data", true)]
        private static bool ValidateClearSaveFile()
        {
            // Always show the menu item, but you could conditionally enable it
            return true;
        }
    }
}
#endif