using UnityEditor;
using UnityEngine;

namespace UGCE
{
    public class EditorSettings : ScriptableObject
    {
        public Texture2D icon;
        public bool makeExtensionBuild;
        public string extensionName;
        public string extensionVersion;

        private static EditorSettings instance;

        private const string AssetPath = "Assets/Editor/BuildGamesForBrowserExtension/EditorSettings.asset";

        public static EditorSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<EditorSettings>(AssetPath);
                    if (instance == null)
                    {
                        instance = CreateInstance<EditorSettings>();
                        AssetDatabase.CreateAsset(instance, AssetPath);
                        AssetDatabase.SaveAssets();
                    }
                }
                return instance;
            }
        }
    }

    public class EditorSettingsWindow : EditorWindow
    {
        private EditorSettings settings;

        [MenuItem("Window/BroswserExt/Editor Settings")]
        public static void ShowWindow()
        {
            GetWindow<EditorSettingsWindow>("Editor Settings");
        }

        private void OnEnable()
        {
            settings = EditorSettings.Instance;

            if (string.IsNullOrEmpty(settings.extensionName))
            {
                settings.extensionName = PlayerSettings.productName;
            }
            if (string.IsNullOrEmpty(settings.extensionVersion))
            {
                settings.extensionVersion = PlayerSettings.bundleVersion;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Editor Settings", EditorStyles.boldLabel);

            settings.icon = (Texture2D)EditorGUILayout.ObjectField("Icon", settings.icon, typeof(Texture2D), false);
            settings.makeExtensionBuild = EditorGUILayout.Toggle("Make Extension Build", settings.makeExtensionBuild);
            settings.extensionName = EditorGUILayout.TextField("Extension Name", settings.extensionName);
            settings.extensionVersion = EditorGUILayout.TextField("Extension Version", settings.extensionVersion);
            
            if (settings.makeExtensionBuild)
            {
                EditorGUILayout.HelpBox("Next WebGL build will generate Browser Extension Build.", MessageType.Info);
            }

            if (GUILayout.Button("Save Settings"))
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                this.Close();
            }
        }
    }
}