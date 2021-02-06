using System.IO;
using System.Threading;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using UnityEngine;
using UnityEngine.Networking;

using pdxpartyparrot.Core.Network;

namespace pdxpartyparrot.Core.Editor
{
    // TODO: some of this should move to Core.Util.EditorUtils so game code can use it in editor mode

    public static class Util
    {
        public const string ProjectSettingsAssetPath = "ProjectSettings/ProjectSettings.asset";

        #region Assets

        // creates the asset folder only if it doesn't already exist
        public static void CreateAssetFolder(string parentFolder, string newFolderName)
        {
            if(!Directory.Exists($"{parentFolder}/{newFolderName}")) {
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
        }

        // creates the given asset
        // removes the asset first if it already exists
        public static void CreateAsset(Object obj, string path)
        {
            if(File.Exists(path)) {
                File.Delete(path);
            }

            AssetDatabase.CreateAsset(obj, path);
        }

        public static bool DownloadAssetToFile(string url, string path)
        {
            Debug.Log($"Downloading asset from {url} to {path}...");

            UnityWebRequest www = UnityWebRequest.Get(url);
            AsyncOperation asyncOp = www.SendWebRequest();
            while(!asyncOp.isDone) {
                Thread.Sleep(500);
            }

            if(www.IsHttpError()) {
                Debug.LogError($"Failed to download asset from {url}: {www.error}");
                return false;
            }

            FileInfo fileInfo = new FileInfo(path);
            if(fileInfo.Exists) {
                fileInfo.Delete();
            }

            DirectoryInfo dirInfo = fileInfo.Directory;
            if(null != dirInfo && !dirInfo.Exists) {
                dirInfo.Create();
            }

            File.WriteAllBytes(path, www.downloadHandler.data);

            AssetDatabase.ImportAsset(path);

            return true;
        }

        public static bool DownloadTextureToFile(string url, string path, TextureImporterType type)
        {
            if(!DownloadAssetToFile(url, path)) {
                return false;
            }

            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
            textureImporter.textureType = type;
            textureImporter.SaveAndReimport();

            return true;
        }

        public static bool DownloadMusicToFile(string url, string path)
        {
            if(!DownloadAssetToFile(url, path)) {
                return false;
            }

            AudioImporter audioImporter = (AudioImporter)AssetImporter.GetAtPath(path);
            audioImporter.preloadAudioData = true;
            audioImporter.loadInBackground = false;

            AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;
            sampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            sampleSettings.loadType = AudioClipLoadType.Streaming;
            sampleSettings.quality = 1.0f;
            audioImporter.defaultSampleSettings = sampleSettings;

            audioImporter.SaveAndReimport();

            return true;
        }

        public static bool DownloadStingerToFile(string url, string path)
        {
            if(!DownloadAssetToFile(url, path)) {
                return false;
            }

            AudioImporter audioImporter = (AudioImporter)AssetImporter.GetAtPath(path);
            audioImporter.preloadAudioData = true;
            audioImporter.loadInBackground = false;

            AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;
            sampleSettings.compressionFormat = AudioCompressionFormat.Vorbis;
            sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
            sampleSettings.quality = 1.0f;
            audioImporter.defaultSampleSettings = sampleSettings;

            audioImporter.SaveAndReimport();

            return true;
        }

        public static bool DownloadSFXToFile(string url, string path)
        {
            if(!DownloadAssetToFile(url, path)) {
                return false;
            }

            AudioImporter audioImporter = (AudioImporter)AssetImporter.GetAtPath(path);
            audioImporter.preloadAudioData = true;
            audioImporter.loadInBackground = false;

            AudioImporterSampleSettings sampleSettings = audioImporter.defaultSampleSettings;
            sampleSettings.compressionFormat = AudioCompressionFormat.PCM;
            sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
            sampleSettings.quality = 1.0f;
            audioImporter.defaultSampleSettings = sampleSettings;

            audioImporter.SaveAndReimport();

            return true;
        }

        #endregion

        #region Packages

        public static void AddPackage(string identifier)
        {
            Debug.Log($"Adding package {identifier}...");

            AddRequest request = Client.Add(identifier);
            while(!request.IsCompleted) {
                Thread.Sleep(500);
            }

            if(StatusCode.Failure == request.Status) {
                Debug.Log($"Failed to add package {identifier}: {request.Error.message}");
                return;
            }
        }

        public static void RemovePackage(string packageName)
        {
            Debug.Log($"Removing package {packageName}...");

            RemoveRequest request = Client.Remove(packageName);
            while(!request.IsCompleted) {
                Thread.Sleep(500);
            }

            if(StatusCode.Failure == request.Status) {
                Debug.Log($"Failed to add package {packageName}: {request.Error.message}");
                return;
            }
        }

        #endregion

        // TODO: this should move to the core filesystem utils
        public static void CreateEmptyFile(string path)
        {
            File.WriteAllText(path, "");
        }
    }
}
