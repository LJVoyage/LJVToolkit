using System;
using UnityEngine;
using UnityEngine.UIElements;
using VoyageForge.Depot.Editor.Utilities;
using System.IO;

namespace VoyageForge.Depot.Editor.ProjectBrowser
{
    public sealed class PathInputField : VFVisualElement
    {
        public string Value
        {
            get => _value;
            set
            {
                if (Analysis(value))
                {
                    _value = value;
                }
                else
                {
                    RelativePath = RelativePath;
                }
            }
        }

        /// <summary>
        /// 相对路径
        /// </summary>
        public string RelativePath
        {
            get => _relativePath;
            set
            {
                _relativePath = value;
                _textField.value = value;
            }
        }


        private VisualElement _container;

        /// <summary>
        /// 相对路径
        /// </summary>
        private string _relativePath = defaultValue;


        private string _value = defaultValue;

        private const string defaultValue = "./";

        private TextField _textField;

        // ---------- UxmlFactory 和 UxmlTraits（支持 UI Builder 和 UXML 序列化） ----------
        public new class UxmlFactory : UxmlFactory<PathInputField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }


        public PathInputField()
        {
            name = "path-input-field";
            AddToClassList("path-input-field");
            _container = TreeAsset.InstantiateWithFillAndAddTo(this);

            _textField = new TextField
            {
                style =
                {
                    flexGrow = 1
                },
                value = Value
            };

            _textField.RegisterCallback<NavigationSubmitEvent>(OnSubmit);
            _textField.RegisterCallback<FocusOutEvent>(OnFocusOut);

            _container.Add(_textField);
        }

        /// <summary>
        /// 输入框失焦时 检查路径
        /// </summary>
        /// <param name="evt"></param>
        private void OnFocusOut(FocusOutEvent evt)
        {
            if (_textField.value == Value)
            {
                _textField.value = RelativePath;
                return;
            }


            Value = _textField.value;
        }

        /// <summary>
        /// 提交时 检查
        /// </summary>
        /// <param name="evt"></param>
        private void OnSubmit(NavigationSubmitEvent evt)
        {
            if (_textField.value == Value)
            {
                _textField.value = RelativePath;
                return;
            }

            Value = _textField.value;
        }


        /// <summary>
        /// 验证输入的路径
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private bool Analysis(string path)
        {
            Debug.Log("Analysis");

            //先判断是不是空
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // 判断是不是绝对路径，不是则拼接成绝对路径 统一处理
            if (!IsFullyAbsolute(path))
            {
                path = Path.GetFullPath(Path.Combine(Application.dataPath, path));
            }


            switch (GetPathType(path))
            {
                case PathType.NotFound:
                    return false;
                case PathType.Directory:
                {
                    if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                        path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    break;
                }
                case PathType.File:
                    path = Path.GetDirectoryName(path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var (isSubPath, relativePath) = IsSubPathOf(path);

            // 判断路径是不是在 Assets下
            if (isSubPath)
            {
                string webPath = relativePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (webPath == ".")
                {
                    RelativePath = webPath + "/";
                }
                else
                {
                    RelativePath = defaultValue + webPath;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///  是否是绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsFullyAbsolute(string path)
        {
            return Path.IsPathRooted(path);
        }

        /// <summary>
        /// child 是否是 parent 的子路径
        /// </summary>
        /// <param name="childPath"></param>
        /// <param name="parentPath"></param>
        /// <returns></returns>
        public (bool, string) IsSubPathOf(string childPath, string parentPath = "")
        {
            if (string.IsNullOrEmpty(parentPath))
            {
                parentPath = Application.dataPath;
            }

            // 先规范化为绝对路径（防止传入相对路径）
            string fullChild = Path.GetFullPath(childPath);
            string fullParent = Path.GetFullPath(parentPath);

            // 计算相对路径
            string relative = Path.GetRelativePath(fullParent, fullChild);

            // 如果相对路径不以 ".." 开头，且不是 "." 或 ""，说明子路径在父路径之下
            // 注意：如果 child == parent，relative 为 "."，我们通常也认为它属于该目录下（或相等）
            return (!relative.StartsWith("..") && !Path.IsPathRooted(relative), relative);
        }

        public enum PathType
        {
            /// <summary>
            ///  路径不存在或无效
            /// </summary>
            NotFound,

            /// <summary>
            /// 是一个文件
            /// </summary>
            File,

            /// <summary>
            /// 是一个目录
            /// </summary>
            Directory
        }

        public static PathType GetPathType(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return PathType.NotFound;

            try
            {
                FileAttributes attr = File.GetAttributes(path);
                return attr.HasFlag(FileAttributes.Directory) ? PathType.Directory : PathType.File;
            }
            catch (FileNotFoundException)
            {
                return PathType.NotFound;
            }
            catch (DirectoryNotFoundException)
            {
                return PathType.NotFound;
            }
            catch (IOException)
            {
                // 可能是访问被拒绝等，无法确定
                return PathType.NotFound;
            }
            catch (UnauthorizedAccessException)
            {
                return PathType.NotFound;
            }
            catch (ArgumentException)
            {
                return PathType.NotFound;
            }
        }
    }
}