using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UGCE
{
    public static class Constants
    {
        public const string ScriptPattern = @"<script(?![^>]*\bsrc\b)[^>]*>(.*?)<\/script>";
        public const string GameJsFileName = "game.js";
        public const string ManifestFileName = "manifest.json";
        public const string ManifestTemplatePath = "/Editor/BuildGamesForBrowserExtension/manifest_template.json";
        public const string ExtensionFilesPath = "/Editor/BuildGamesForBrowserExtension/ExtensionFiles";
        public const string MetaFileExtension = ".meta";
    }

    public class PostBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (!EditorSettings.Instance.makeExtensionBuild)
            {
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                return;
            }

            ProcessIndexHTML(report);
            PlaceConfiguredManifestAtBuild(report);
            CopyFilesToBuildPath(report);
            CopyIconToDestination(report);
        }

        static void ProcessIndexHTML(BuildReport report)
        {
            string buildPath = report.summary.outputPath;

            string sourceFile = $"{buildPath}/index.html";

            string outputFilePath = $"{buildPath}/{Constants.GameJsFileName}";
            ExtractScriptContent(sourceFile, outputFilePath);
        }
        static void ExtractScriptContent(string htmlFilePath, string outputFilePath)
        {
            if (!File.Exists(htmlFilePath))
            {
                Debug.Log("HTML file not found.");
                return;
            }

            string htmlContent = File.ReadAllText(htmlFilePath);
            MatchCollection matches = Regex.Matches(htmlContent, Constants.ScriptPattern, RegexOptions.Singleline);

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (Match match in matches)
                {
                    string scriptContent = match.Groups[1].Value;
                    writer.WriteLine(scriptContent);
                }
            }

            string updatedHtmlContent = Regex.Replace(htmlContent, Constants.ScriptPattern, $"<script src=\"{Constants.GameJsFileName}\"></script>", RegexOptions.Singleline);
            File.WriteAllText(htmlFilePath, updatedHtmlContent);

            Debug.Log("Script content extracted to " + outputFilePath);

        }
        public static void PlaceConfiguredManifestAtBuild(BuildReport report)
        {
            string destinationPath = report.summary.outputPath;
            destinationPath = $"{destinationPath}/{Constants.ManifestFileName}";
            string jsonFilePath = $"{Application.dataPath}{Constants.ManifestTemplatePath}";

            Dictionary<string, string> replacements = new Dictionary<string, string>();
            replacements.Add("GAME_NAME", EditorSettings.Instance.extensionName);
            replacements.Add("BUILD_VERSION", EditorSettings.Instance.extensionVersion);

            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"JSON file not found: {jsonFilePath}");
            }

            string jsonContent = File.ReadAllText(jsonFilePath);
            foreach (var replacement in replacements)
            {
                jsonContent = jsonContent.Replace(replacement.Key, replacement.Value);
            }

            File.WriteAllText(destinationPath, jsonContent);
        }

        public static void CopyFilesToBuildPath(BuildReport report)
        {
            string sourcePath = $"{Application.dataPath}{Constants.ExtensionFilesPath}";
            string destinationPath = report.summary.outputPath;

            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");
            }

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            string[] files = Directory.GetFiles(sourcePath);

            foreach (string file in files)
            {
                if (file.Contains(Constants.MetaFileExtension))
                {
                    continue;
                }
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destinationPath, fileName);
                File.Copy(file, destFile, true);
            }
        }

        public void CopyIconToDestination(BuildReport report)
        {
            var icon = EditorSettings.Instance.icon;
            var destinationPath = report.summary.outputPath;

            if (icon == null)
            {
                Debug.LogError("Icon is not set in the EditorSettings.");
                return;
            }
            
            var srcpath = AssetDatabase.GetAssetPath(icon);
            destinationPath = $"{destinationPath}/{Path.GetFileName(srcpath)}";
            Debug.Log(destinationPath);
            File.Copy(srcpath, destinationPath, true);
        }
    }
}