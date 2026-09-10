using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoyageForge.Depot.Editor
{
    /// <summary>
    /// 轻量级 YAML 序列化器，将 ForgeMetadata 对象写入/读取为 YAML 格式文本。
    /// 支持嵌套字典，缩进使用两个空格。序列化采用迭代（栈）方式，避免递归。
    /// </summary>
    internal static class ForgeMetaSerializer
    {
        /// <summary>将元数据序列化到指定文件路径（迭代实现）</summary>
        public static void Serialize(string filePath, ForgeMetadata data)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            // 写入版本和 GUID 作为顶层键
            writer.WriteLine($"v: {data.version}");
            writer.WriteLine($"guid: {data.guid ?? ""}");

            // 使用栈进行深度优先遍历
            var stack = new Stack<(int indent, string key, object value)>();
            PushDictionaryItems(stack, data.fields, 0);

            while (stack.Count > 0)
            {
                var (indent, key, value) = stack.Pop();

                if (value is Dictionary<string, object> nested)
                {
                    writer.WriteLine($"{new string(' ', indent * 2)}{key}:");
                    PushDictionaryItems(stack, nested, indent + 1);
                }
                else
                {
                    writer.WriteLine($"{new string(' ', indent * 2)}{key}: {value}");
                }
            }
        }

        /// <summary>将字典的所有键值对按倒序压入栈，以保证顺序</summary>
        private static void PushDictionaryItems(Stack<(int indent, string key, object value)> stack,
            Dictionary<string, object> dict, int indent)
        {
            var items = new List<(string key, object value)>(dict.Count);
            foreach (var kv in dict)
                items.Add((kv.Key, kv.Value));

            for (int i = items.Count - 1; i >= 0; i--)
            {
                var (key, value) = items[i];
                stack.Push((indent, key, value));
            }
        }

        /// <summary>从文件反序列化元数据（迭代实现）</summary>
        public static ForgeMetadata Deserialize(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            var data = new ForgeMetadata();
            var root = data.fields;
            var stack = new Stack<Dictionary<string, object>>();
            stack.Push(root);
            var indentLevels = new Stack<int>();
            indentLevels.Push(-1);

            bool versionParsed = false, guidParsed = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                int indent = 0;
                while (indent < line.Length && line[indent] == ' ')
                    indent++;

                string trimmed = line.TrimStart();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                int colonIndex = trimmed.IndexOf(':');
                if (colonIndex == -1)
                    continue;

                string key = trimmed.Substring(0, colonIndex).Trim();
                string value = trimmed.Substring(colonIndex + 1).Trim();

                if (!versionParsed && key == "v")
                {
                    int.TryParse(value, out data.version);
                    versionParsed = true;
                    continue;
                }

                if (!guidParsed && key == "guid")
                {
                    data.guid = value;
                    guidParsed = true;
                    continue;
                }

                while (stack.Count > 1 && indentLevels.Peek() >= indent)
                {
                    stack.Pop();
                    indentLevels.Pop();
                }

                var currentDict = stack.Peek();

                if (string.IsNullOrEmpty(value))
                {
                    bool hasChildren = false;
                    int currentIndex = System.Array.IndexOf(lines, line);
                    for (int i = currentIndex + 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                            continue;
                        int nextIndent = 0;
                        while (nextIndent < lines[i].Length && lines[i][nextIndent] == ' ')
                            nextIndent++;
                        if (nextIndent > indent)
                        {
                            hasChildren = true;
                            break;
                        }
                        else
                            break;
                    }

                    if (hasChildren)
                    {
                        var nested = new Dictionary<string, object>();
                        currentDict[key] = nested;
                        stack.Push(nested);
                        indentLevels.Push(indent);
                    }
                    else
                    {
                        currentDict[key] = "";
                    }
                }
                else
                {
                    currentDict[key] = value;
                }
            }

            return data;
        }
    }
}