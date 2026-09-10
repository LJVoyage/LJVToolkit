using System.IO;
using UnityEditor;
using UnityEngine;

namespace VoyageForge.Depot.Editor
{
    /// <summary>
    /// 监听 Unity 资产移动/删除事件，自动同步伴随文件（.forge~）。
    /// </summary>
    public class ForgeMetaAssetHandler : AssetModificationProcessor
    {
        private static AssetMoveResult OnWillMoveAsset(string oldPath, string newPath)
        {
            if (oldPath.EndsWith(".forge~") || newPath.EndsWith(".forge~"))
                return AssetMoveResult.DidNotMove;

            MoveSingleForgeFile(oldPath, newPath);
            return AssetMoveResult.DidNotMove;
        }

        private static void MoveSingleForgeFile(string oldAssetPath, string newAssetPath)
        {
            string oldForge = GetForgeFilePathFromAssetPath(oldAssetPath);
            string newForge = GetForgeFilePathFromAssetPath(newAssetPath);

            if (File.Exists(oldForge))
            {
                string newDir = Path.GetDirectoryName(newForge);
                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);

                if (File.Exists(newForge))
                    File.Delete(newForge);

                File.Move(oldForge, newForge);
                AssetDatabase.Refresh();
            }
        }

        private static string GetForgeFilePathFromAssetPath(string assetPath)
        {
            string directory = Path.GetDirectoryName(assetPath);
            string fileName = Path.GetFileName(assetPath);
            string forgeFileName = fileName + ".forge~";
            return Path.Combine(directory, forgeFileName).Replace("\\", "/");
        }

        public class ForgeMetaPostprocessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                foreach (string deletedAsset in deletedAssets)
                {
                    if (deletedAsset.EndsWith(".forge~"))
                        continue;

                    string forgePath = GetForgeFilePathFromAssetPath(deletedAsset);
                    TryDeleteFile(forgePath);
                }

                if (deletedAssets.Length > 0)
                    AssetDatabase.Refresh();
            }

            private static void TryDeleteFile(string path)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            
            
        }
    }
}