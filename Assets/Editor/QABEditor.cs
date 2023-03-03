using UnityEditor;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace QFramework.Editor
{
    public class Styles
    {
        public static GUIStyle box;
        public static GUIStyle toolbar;
        public static GUIStyle toolbarButton;
        public static GUIStyle tooltip;
    }

    public class QABEditor : EditorWindow
    {
        /// <summary>
        /// ��Դ·����׺ proto
        /// </summary>
        public const string STR_EXTENSION_PROTO = ".proto";
        /// <summary>
        /// ��Դ·����׺ def
        /// </summary>
        public const string STR_EXTENSION_DEF = ".def";
        /// <summary>
        /// ��Դ·����׺ tsv
        /// </summary>
        public const string STR_EXTENSION_TSV = ".tsv";

        /// <summary>
        /// ���е�bundle �������
        /// </summary>
        private const string allBundleConfig = "Assets/Editor/Build/Config/BundleConfig_all.asset";
        /// <summary>
        /// Ĭ�ϵ�Bundle�����������
        /// </summary>
        private static string savePath = allBundleConfig;

        [MenuItem("QFramework/AB/Build")]

        static void BuildWindow_All()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            //����Bundle������Ϣ
            //UpdateBundleConfig((int)BuildBundleTypeEnum.ALL);

            GetWindowWithRect<QABEditor>(new Rect(0, 0, 1200, 600), true, "Bundle");
        }

        public static void BuildAssetBundle()
        {
            // AB�����·��
            string outPath = Application.streamingAssetsPath + "/QAB";
            // ���·���Ƿ����
            CheckDirAndCreate(outPath);
            BuildPipeline.BuildAssetBundles(outPath, 0, EditorUserBuildSettings.activeBuildTarget);
            // �մ������ļ��к�Ŀ¼��������Project�Ӵ��г���
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// �ж�·���Ƿ����,�������򴴽�
        /// </summary>
        public static void CheckDirAndCreate(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        void UpdateStyles()
        {
            Styles.box = new GUIStyle(GUI.skin.box);
            Styles.box.margin = new RectOffset();
            Styles.box.padding = new RectOffset();
            Styles.toolbar = new GUIStyle(EditorStyles.toolbar);
            Styles.toolbar.margin = new RectOffset();
            Styles.toolbar.padding = new RectOffset();
            Styles.toolbarButton = EditorStyles.toolbarButton;
            Styles.tooltip = GUI.skin.GetStyle("AssetLabel");
        }

        BuildConfig config;
        Vector2 Position;
        private void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            var execBuild = false;
            if (config == null)
            {
                config = AssetDatabase.LoadAssetAtPath<BuildConfig>(savePath);
                if (config == null)
                {
                    config = CreateInstance<BuildConfig>();
                }
            }

            if (config == null)
            {
                return;
            }

            UpdateStyles();

            //tool bar
            GUILayout.BeginHorizontal(Styles.toolbar);
            {
                if (GUILayout.Button("Add", Styles.toolbarButton))
                {
                    config.filters.Add(new BuildAssetFilter());
                }

                if (GUILayout.Button("Save", Styles.toolbarButton))
                {
                    Save();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Build", Styles.toolbarButton))
                {
                    execBuild = true;
                }
            }
            GUILayout.EndHorizontal();
            //context
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Path", GUILayout.Width(400));
                GUILayout.Label("", GUILayout.Width(80));
                GUILayout.Label("Filter(*.xxx)", GUILayout.Width(100));
                GUILayout.Label("ABName", GUILayout.Width(100));
                GUILayout.Label("", GUILayout.Width(60));
                GUILayout.Label("", GUILayout.Width(100));
                GUILayout.Label("", GUILayout.Width(130));
                GUILayout.Label("ɾ��", GUILayout.Width(40));
            }
            GUILayout.EndHorizontal();
            Position = GUILayout.BeginScrollView(Position, GUILayout.Width(Screen.width), GUILayout.ExpandHeight(true));

            //�����ڲ�Ĭ����Դ
            for (int i = 0; i < config.filters.Count; i++)
            {
                var filter = config.filters[i];
                GUILayout.BeginHorizontal();
                {
                    filter.path = GUILayout.TextField(filter.path, GUILayout.Width(400));
                    if (GUILayout.Button("Select", GUILayout.Width(80)))
                    {
                        string dataPath = Application.dataPath;
                        string selectedPath = EditorUtility.OpenFolderPanel("Path", dataPath, "");
                        if (!string.IsNullOrEmpty(selectedPath))
                        {
                            if (selectedPath.StartsWith(dataPath))
                            {
                                filter.path = "Assets/" + selectedPath.Substring(dataPath.Length + 1);
                            }
                            else
                            {
                                ShowNotification(new GUIContent("������AssetsĿ¼֮��!"));
                            }
                        }
                    }
                    if (filter.atlas)
                    {
                        GUILayout.Label("*.spriteatlas", GUILayout.Width(98));
                    }
                    else
                    {
                        filter.filter = GUILayout.TextField(filter.filter, GUILayout.Width(100));
                        if (filter.filter.Contains(".spriteatlas"))
                        {
                            filter.atlas = true;
                            filter.filter = "";
                        }
                    }
                    filter.abname = GUILayout.TextField(filter.abname, GUILayout.Width(100));
                    filter.atlas = GUILayout.Toggle(filter.atlas, "ͼ��", GUILayout.Width(60));
                    filter.subdirs = GUILayout.Toggle(filter.subdirs, "�������ļ���", GUILayout.Width(100));
                    filter.textureMerge = GUILayout.Toggle(filter.textureMerge, "TextureĿ¼��Դ�ϲ�", GUILayout.Width(140));
                    filter.isSubDirPack = GUILayout.Toggle(filter.isSubDirPack, "��Ŀ¼�������", GUILayout.Width(140));
                    if (string.IsNullOrEmpty(filter.abname))
                        filter.isSubDirPack = false;

                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                    {
                        config.filters.RemoveAt(i);
                        i--;
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("DLC��Դ");

            GUILayout.BeginHorizontal(Styles.toolbar);
            {
                if (GUILayout.Button("Add", Styles.toolbarButton))
                {
                    config.dlcFilter.Add(new BuildDlcAssetFilter());
                }

                if (GUILayout.Button("Save", Styles.toolbarButton))
                {
                    SaveDlcAssetFilter();
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Path", GUILayout.Width(1040));
            GUILayout.Label("AssetType", GUILayout.Width(100));
            GUILayout.EndHorizontal();
            //������dlc��Դ��
            for (int i = 0; i < config.dlcFilter.Count; i++)
            {
                var filter = config.dlcFilter[i];
                GUILayout.BeginHorizontal();
                {
                    filter.path = GUILayout.TextField(filter.path, GUILayout.Width(400));
                    filter.filter = GUILayout.TextField(filter.filter, GUILayout.Width(640));
                    filter.assetType = (DlcAssetType)EditorGUILayout.EnumPopup(filter.assetType,
                    GUILayout.Width(100));
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                    {
                        config.dlcFilter.RemoveAt(i);
                        i--;
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            //set dirty
            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
            }

            if (execBuild)
            {
                Build();
            }
        }

        private void Build()
        {
            Save();
            SaveDlcAssetFilter();
            BuildAssetBundles();
        }

        private void Save()
        {
            var nfilters = new List<BuildAssetFilter>();
            foreach (var kv in config.filters)
            {
                if (true)
                {
                    var flts = kv.filter.Split(';');
                    foreach (var flt in flts)
                    {
                        if (!flt.StartsWith("*.") || flt.Length < 3)
                        {
                            kv.filter = string.Empty;
                            break;
                        }
                    }
                }

                bool repeated = false;
                foreach (var nkv in nfilters)
                {
                    if (kv.path == nkv.path && kv.filter == nkv.filter)
                    {
                        repeated = true;
                        break;
                    }
                }
                if (!repeated)
                {
                    nfilters.Add(kv);
                }
            }

            if (nfilters.Count != config.filters.Count)
            {
                config.filters = nfilters;
            }

            if (AssetDatabase.LoadAssetAtPath<BuildConfig>(savePath) == null)
            {
                AssetDatabase.CreateAsset(config, savePath);
            }
            else
            {
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// �洢dlc��Դ������Ϣ
        /// </summary>
        private void SaveDlcAssetFilter()
        {
            var nfilters = new List<BuildDlcAssetFilter>();
            foreach (var kv in config.dlcFilter)
            {


                bool repeated = false;
                foreach (var nkv in nfilters)
                {
                    if (kv.path == nkv.path)
                    {
                        repeated = true;
                        break;
                    }
                }
                if (!repeated)
                {
                    nfilters.Add(kv);
                }
            }

            if (nfilters.Count != config.dlcFilter.Count)
            {
                config.dlcFilter = nfilters;
            }

            if (AssetDatabase.LoadAssetAtPath<BuildConfig>(savePath) == null)
            {
                AssetDatabase.CreateAsset(config, savePath);
            }
            else
            {
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// ��ָ�����͵��ı��ļ�·������.txt��׺
        /// </summary>
        /// <param name="path"></param>
        public static void FixTextAssetExt(ref string path)
        {

            if (string.IsNullOrEmpty(path))
                return;

            var extension = Path.GetExtension(path);

            if (string.Equals(extension, STR_EXTENSION_PROTO)
                || string.Equals(extension, STR_EXTENSION_TSV)
                || string.Equals(extension, STR_EXTENSION_DEF))
            {
                path = string.Format("{0}.txt", path);
            }
        }

        public static void DeleteExtTxt(List<BuildAssetFilter> rules)
        {
            foreach (var rule in rules)
            {
                var patterns = rule.filter.Split(';');
                foreach (var p1 in patterns)
                {
                    var p2 = p1;
                    Helper.FixTextAssetExt(ref p2);
                    if (p2 != p1)
                    {
                        DeleteExtTxt(rule.path, p1);
                    }
                }
            }
        }

        public static void DeleteExtTxt(string file, string pattern)
        {
            if (File.Exists(file))
            {
                File.Delete(file + ".txt");
            }
            else if (Directory.Exists(file))
            {
                DeleteExtTxt(new DirectoryInfo(file), pattern);
            }
            else
            {
                Debug.LogError("Unknown " + file + " -> " + pattern);
            }
        }

        public static void DeleteExtTxt(DirectoryInfo folder, string pattern)
        {
            foreach (var NextFile in folder.GetFiles(pattern + ".txt"))
            {
                NextFile.Delete();
            }

            foreach (DirectoryInfo next in folder.GetDirectories())
            {
                DeleteExtTxt(next, pattern);
            }
        }

        public static void FixExt2Txt(DirectoryInfo folder, string pattern, bool subdirs)
        {
            foreach (var NextFile in folder.GetFiles(pattern))
            {
                File.Copy(NextFile.FullName, NextFile.FullName + ".txt", true);
            }

            if (subdirs)
            {
                foreach (DirectoryInfo next in folder.GetDirectories())
                {
                    FixExt2Txt(next, pattern, subdirs);
                }
            }
        }

        public static void FixExt2Txt(string file, string pattern, bool subdirs)
        {
            if (File.Exists(file))
            {
                File.Copy(file, file + ".txt", true);
            }
            else if (Directory.Exists(file))
            {
                FixExt2Txt(new DirectoryInfo(file), pattern, subdirs);
            }
            else
            {
                Debug.LogError("Unknown " + file + " -> " + pattern);
            }
        }


        public static List<BuildAssetFilter> FixExt2Txt(List<BuildAssetFilter> rules)
        {
            List<BuildAssetFilter> ret = new List<BuildAssetFilter>();
            foreach (var rule in rules)
            {
                var patterns = rule.filter.Split(';');
                var nfilter = string.Empty;
                foreach (var p1 in patterns)
                {
                    if (string.IsNullOrEmpty(nfilter))
                    {
                        nfilter = p1;
                    }
                    else
                    {
                        nfilter += ";" + p1;
                    }

                    var p2 = p1;
                    FixTextAssetExt(ref p2);
                    if (p2 != p1)
                    {
                        FixExt2Txt(rule.path, p1, rule.subdirs);
                        nfilter += ".txt";
                    }
                }
                ret.Add(new BuildAssetFilter()
                {
                    path = rule.path,
                    filter = nfilter,
                    abname = rule.abname,
                    subdirs = rule.subdirs,
                    atlas = rule.atlas,
                    textureMerge = rule.textureMerge,
                    isSubDirPack = rule.isSubDirPack
                });
            }
            return ret;
        }


        public static void BuildAssetBundles(string resource_format = null)
        {
            var savePath = allBundleConfig;

            var config = AssetDatabase.LoadAssetAtPath<BuildConfig>(savePath);
            AssetDatabase.Refresh();
            var filters = FixExt2Txt(config.filters);
            var rules = new List<BuildRule>();
            foreach (var f in filters)
            {
                if (f.atlas)
                {
                    rules.Add(new BuildAssetsSpriteAtlas(f.path, "*.spriteatlas", f.subdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, f.abname, isSubDirPack: f.isSubDirPack));
                }
                else if (!string.IsNullOrEmpty(f.filter))
                {
                    if (f.textureMerge)
                    {
                        rules.Add(new BuildAssetsTextureDirMerge(f.path, f.filter, f.subdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, f.abname, isSubDirPack: f.isSubDirPack));
                    }
                    else
                    {
                        rules.Add(new BuildAssetsWithFilename(f.path, f.filter, f.subdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly, f.abname, isSubDirPack: f.isSubDirPack));
                    }
                }
            }

            //�Դ������ԴĿ¼��������
            //BuildBundleUtils.SortRules(rules);
            AssetDatabase.Refresh();
            List<AssetBundleBuild> builds = BuildRule.GetBuilds(rules);


            var dic = new Dictionary<string, List<string>>();
            foreach (var build in builds)
            {
                if (!dic.ContainsKey(build.assetBundleName))
                {
                    dic[build.assetBundleName] = new List<string>();
                }

                dic[build.assetBundleName].AddRange(build.assetNames);
            }

            builds = new List<AssetBundleBuild>();
            foreach (var kv in dic)
            {
                var build = new AssetBundleBuild();
                build.assetBundleName = kv.Key;
                build.assetNames = kv.Value.ToArray();
                builds.Add(build);
            }

            BuildScript.BuildAssetBundles(builds, config.dlcFilter, resource_format);
            EditorUtility.ClearProgressBar();
            DeleteExtTxt(config.filters);
            AssetDatabase.Refresh();

            File.WriteAllText($"DLC/{Helper.GetPlatformName()}/version.txt", string.Format("{0}.{1}", PlayerSettings.bundleVersion, PlayerSettings.Android.bundleVersionCode));
            File.WriteAllText(Helper.BundleDlcvPath, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

            Debug.Log("BuildAssetBundles Build Ok : " + PlayerSettings.bundleVersion);
        }
    }
}