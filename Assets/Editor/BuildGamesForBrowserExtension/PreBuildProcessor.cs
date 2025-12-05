
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace UGCE
{
    class PreBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!EditorSettings.Instance.makeExtensionBuild)
            {
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                return;
            }

            RunPreBuildActions();
        }

        private const string ERROR_MSG_DATA_CACING_ENABLED = "Data caching is not supported in chromium extension builds. Please disable Build Profiles -> Publishing Setting -> Data Caching";
        private const string ERROR_MSG_COMPRESSION = "Compression is not supported in chromium extension builds. Please disable compression from Build Profiles -> Publishing Setting -> Compression Format";

        public static void RunPreBuildActions()
        {
            bool isCachingEnabled = PlayerSettings.WebGL.dataCaching;
            if (isCachingEnabled)
            {
                throw new BuildFailedException(ERROR_MSG_DATA_CACING_ENABLED);
            }

            var hasCompression = PlayerSettings.WebGL.compressionFormat != WebGLCompressionFormat.Disabled;
            if (hasCompression)
            {
                throw new BuildFailedException(ERROR_MSG_COMPRESSION);

            }
        }
    }
}