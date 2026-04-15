#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using YesChef.Core;
namespace YesChef.Encryption
{

    [CustomEditor(typeof(EncryptionConfig))]
    public class EncryptionConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // Draw the default keyBase64 and ivBase64 fields

            EncryptionConfig config = (EncryptionConfig)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Generate New Random Keys"))
            {
                config.GenerateNewKeys();
                // Force the Inspector to refresh
                EditorUtility.SetDirty(config);
                serializedObject.ApplyModifiedProperties();
            }

            GUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "⚠️ These keys are stored as plain text in the asset file.\n" +
                "For production, consider obfuscating the key (e.g., XOR mask or byte array split).",
                MessageType.Warning
            );
        }
    }
}
#endif