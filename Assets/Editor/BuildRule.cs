// 版权所有[成都尼毕鲁科技股份有限公司]
// 根据《保密信息使用许可证》获得许可;
// 除非符合许可，否则您不得使用此文件。
// 您可以在以下位置获取许可证副本，链接地址：
// https://wiki.tap4fun.com/pages/viewpage.action?pageId=29818250
// 除非适用法律要求或书面同意，否则保密信息按照使用许可证要求使用， 不附带任何明示或暗示的保证或条件。
// 有关管理权限的特定语言，请参阅许可证副本。


using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QFramework.Editor
{
    public abstract class BuildRule
    {
        protected static List<string> packedAssets = new List<string>();
        protected static Dictionary<string, List<string>> commonAssets = new Dictionary<string, List<string>>();
        protected static List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        static BuildRule()
        {
        }

        static List<string> GetFilesWithoutDirectories(string prefabPath, string searchPattern,
            SearchOption searchOption)
        {
            var files = new List<string>();
            var patterns = searchPattern.Split(';');
            var orls = Directory.GetFiles(prefabPath, "*.*", searchOption);
            foreach (var p in patterns)
            {
                if (!string.IsNullOrEmpty(p))
                {
                    if (p.Contains("*"))
                    {
                        if (p.IndexOf("*") > 0)
                        {
                            var pre = p.Substring(0, p.IndexOf("*")).ToLower();
                            var ext = p.Substring(p.LastIndexOf("*") + 1).ToLower();
                            foreach (var f in orls)
                            {
                                var fn = f.Substring(f.LastIndexOf("/") + 1).ToLower();
                                if (fn.StartsWith(pre) && fn.EndsWith(ext))
                                {
                                    files.Add(f);
                                }
                            }
                        }
                        else
                        {
                            var ext = p.Substring(p.LastIndexOf("*") + 1).ToLower();
                            foreach (var f in orls)
                            {
                                var fn = f.Substring(f.LastIndexOf("/") + 1).ToLower();
                                if (fn.EndsWith(ext))
                                {
                                    files.Add(f);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var f in orls)
                        {
                            var fn = f.ToLower();
                            var ext = p.ToLower();
                            if (fn.EndsWith(ext))
                            {
                                files.Add(f);
                            }
                        }
                    }
                }
            }

            List<string> items = new List<string>();
            foreach (var item in files)
            {
                var assetPath = item.Replace('\\', '/');
                if (!System.IO.Directory.Exists(assetPath))
                {
                    items.Add(assetPath);
                }
            }

            return items;
        }

        protected static List<string> GetFilesWithoutPacked(string searchPath, string searchPattern,
            SearchOption searchOption)
        {
            var files = GetFilesWithoutDirectories(searchPath, searchPattern, searchOption);
            var filesCount = files.Count;
            //移除所有已经存在或者需要忽略的路径
            var removeAll = files.RemoveAll((string obj) => { return packedAssets.Contains(obj.ToLower()) || IsContainIgnorePath(obj.ToLower()); });
            Debug.Log(string.Format("RemoveAll {0} size: {1}", removeAll, filesCount));
            return files;
        }


        public string searchPath;
        public string searchPattern;
        public SearchOption searchOption = SearchOption.AllDirectories;
        public string bundleName;
        /// <summary>
        /// 是否忽略Editor目录
        /// </summary>
        public bool isIgnoreEditor;
        /// <summary>
        /// 资源打包时忽略的资源路径
        /// </summary>
        protected static List<string> ignorePath = new List<string>() { "editor" };

        /// <summary>
        /// 是否子目录单独打包
        /// </summary>
        protected bool isSubDirAlonePack;

        public BuildRule(string path, string pattern, SearchOption option, string bname, 
            bool isIgnoreEditor = true, bool isSubDirAlonePack = false)
        {
            searchPath = path;
            searchPattern = pattern;
            searchOption = option;
            bundleName = bname;
            this.isIgnoreEditor = isIgnoreEditor;
            this.isSubDirAlonePack = isSubDirAlonePack;
        }

        /// <summary>
        /// 构建图集资源
        /// </summary>
        public virtual void BuildAtlas() { }

        /// <summary>
        /// 构建资源
        /// </summary>
        public virtual void BuildOther() { }

        /// <summary>
        /// 构建Texture资源合并
        /// </summary>
        public virtual void BuildTextureMerge() { }

        /// <summary>
        /// 构建依赖的引用资源
        /// </summary>
        public virtual void BuildCommon() { }

        /// <summary>
        /// 是否包含忽略的资源路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected static bool IsContainIgnorePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            for (int i = 0; i < ignorePath.Count; i++)
            {
                if (path.Contains(ignorePath[i]))
                    return true;
            }

            return false;
        }


        public static List<AssetBundleBuild> GetBuilds(List<BuildRule> rules)
        {
            rules.Sort((x, y) =>
            {
                return y.searchPath.Length - x.searchPath.Length;
            });
            
            packedAssets.Clear();
            builds.Clear();

            foreach (var item in rules)
            {
                item.BuildAtlas();
            }

            foreach (var item in rules)
            {
                item.BuildTextureMerge();
            }

            foreach (var item in rules)
            {
                item.BuildOther();
            }

            foreach (var item in rules)
            {
                item.BuildCommon();
            }

            EditorUtility.ClearProgressBar();

            return builds;
        }
    }

    /// <summary>
    /// 打包资源
    /// </summary>
    public class BuildAssetsWithFilename : BuildRule
    {
        public BuildAssetsWithFilename(string path, string pattern, SearchOption option, 
            string bname, bool isIgnoreEditor = true, bool isSubDirPack = false) 
            : base(path, pattern, option, bname, isIgnoreEditor, isSubDirPack)
        {
        }

        //public override void BuildOther()
        //{
        //    if (!Directory.Exists(searchPath))
        //    {
        //        Debug.LogWarning("Not exist " + searchPath);
        //        return;
        //    }

        //    var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);

        //    // Collect dependencies.
        //    //var commonAssets = new Dictionary<string, List<string>>();
        //    for (var i = 0; i < files.Count; i++)
        //    {
        //        var item = files[i];
        //        var dependencies = AssetDatabase.GetDependencies(item);
        //        if (EditorUtility.DisplayCancelableProgressBar($"Collecting... [{i}/{files.Count}]", item,
        //            i * 1f / files.Count))
        //        {
        //            break;
        //        }

        //        foreach (var assetPath in dependencies)
        //        {
        //            if (!commonAssets.ContainsKey(assetPath))
        //            {
        //                commonAssets[assetPath] = new List<string>();
        //            }

        //            if (!commonAssets[assetPath].Contains(item))
        //            {
        //                commonAssets[assetPath].Add(item);
        //            }
        //        }
        //    }

        //    // Generate AssetBundleBuild items.
        //    for (var i = 0; i < files.Count; i++)
        //    {
        //        var item = files[i];
        //        if (EditorUtility.DisplayCancelableProgressBar($"Packing... [{i}/{files.Count}]",
        //            item, i * 1f / files.Count))
        //        {
        //            break;
        //        }

        //        if (packedAssets.Contains(item.ToLower()))
        //        {
        //            continue;
        //        }

        //        var build = new AssetBundleBuild
        //        {
        //            assetBundleName = string.IsNullOrEmpty(bundleName) ? BuildScript.ConvertToBundleName(item) : bundleName,
        //            assetNames = new[]{ item }
        //        };
        //        builds.Add(build);
        //        packedAssets.Add(item.ToLower());
        //    }

        //    //// Pack the asset which is dependent by more than one asset in an AssetBundle along.
        //    //foreach (var item in commonAssets)
        //    //{
        //    //    var assetPath = item.Key;

        //    //    // Skip CS scripts.
        //    //    if (assetPath.EndsWith(".cs"))
        //    //    {
        //    //        continue;
        //    //    }

        //    //    // Skip the packed assets.
        //    //    if (packedAssets.Contains(assetPath.ToLower()))
        //    //    {
        //    //        continue;
        //    //    }

        //    //    // Pack the common assets.
        //    //    if (item.Value.Count > 1)
        //    //    {
        //    //        var build = new AssetBundleBuild
        //    //        {
        //    //            assetBundleName = BuildScript.ConvertToBundleName(assetPath, true),
        //    //            assetNames = new[] { assetPath }
        //    //        };
        //    //        builds.Add(build);
        //    //        packedAssets.Add(assetPath.ToLower());
        //    //    }
        //    //}
        //}

        /// <summary>
        /// 打包处理
        /// </summary>
        public override void BuildOther()
        {
            if (isSubDirAlonePack)
                SubDirAlonePack();
            else
                AllDirPack(searchPath, bundleName);
        }

        /// <summary>
        /// 所有目录统一打包
        /// </summary>
        private void AllDirPack(string path, string bundleName)
        {

            bool isFile = false;
            if (!Directory.Exists(path))
            {
                Debug.LogWarning("Not exist dir " + path);
                if(!File.Exists(path))
                {
                    Debug.LogWarning("Not exist file " + path);
                    return;
                }
                else
                {
                    isFile = true;
                }
            }

            List<string> files = null;
            if(isFile)
            {
                var filePath = path;
                Helper.FixTextAssetExt(ref filePath);

                //是否已经包含或者是在忽略列表中
                if (packedAssets.Contains(filePath.ToLower())
                    || IsContainIgnorePath(filePath.ToLower()))
                {
                    return;
                }

                files = new List<string>();
                files.Add(filePath);
            }
            else
            {
                files = GetFilesWithoutPacked(path, searchPattern, searchOption);
            }

            // Collect dependencies.
            //var commonAssets = new Dictionary<string, List<string>>();
            for (var i = 0; i < files.Count; i++)
            {
                var item = files[i];
                var dependencies = AssetDatabase.GetDependencies(item);
                if (EditorUtility.DisplayCancelableProgressBar($"Collecting... [{i}/{files.Count}]", item,
                    i * 1f / files.Count))
                {
                    break;
                }

                foreach (var assetPath in dependencies)
                {
                    if (!commonAssets.ContainsKey(assetPath))
                    {
                        commonAssets[assetPath] = new List<string>();
                    }

                    if (!commonAssets[assetPath].Contains(item))
                    {
                        commonAssets[assetPath].Add(item);
                    }
                }
            }

            // Generate AssetBundleBuild items.
            for (var i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (EditorUtility.DisplayCancelableProgressBar($"Packing... [{i}/{files.Count}]",
                    item, i * 1f / files.Count))
                {
                    break;
                }

                if (packedAssets.Contains(item.ToLower()))
                {
                    continue;
                }

                var build = new AssetBundleBuild
                {
                    assetBundleName = string.IsNullOrEmpty(bundleName) ? BuildScript.ConvertToBundleName(item) : bundleName,
                    assetNames = new[] { item }
                };
                builds.Add(build);
                packedAssets.Add(item.ToLower());
            }
        }

        /// <summary>
        /// 子目录单独打包
        /// </summary>
        private void SubDirAlonePack()
        {
            if (!Directory.Exists(searchPath))
            {
                Debug.LogWarning("Not exist " + searchPath);
                return;
            }

            if (!string.IsNullOrEmpty(bundleName) && bundleName.LastIndexOf('.') > 0)
            {
                //获取当前目录下的所有目录
                var dirInfo = new DirectoryInfo(searchPath);
                var arr = dirInfo.GetDirectories();
                for (int i = 0; i < arr.Length; i++)
                {
                    var path = arr[i].FullName.Replace('\\', '/');
                    path = path.Substring(path.IndexOf(searchPath));
                    AllDirPack(path, bundleName.Insert(bundleName.LastIndexOf('.'), $".{arr[i].Name.ToLower()}"));
                }
            }
            

            //顶层目录添加上，避免有遗漏东西
            AllDirPack(searchPath, bundleName);
        }

        /// <summary>
        /// 构建通用依赖的引用资源
        /// </summary>
        public override void BuildCommon()
        {
            //base.BuildCommon();

            // Pack the asset which is dependent by more than one asset in an AssetBundle along.
            foreach (var item in commonAssets)
            {
                var assetPath = item.Key;

                // Skip CS scripts.
                if (assetPath.EndsWith(".cs"))
                {
                    continue;
                }

                // Skip the packed assets.
                if (packedAssets.Contains(assetPath.ToLower()))
                {
                    continue;
                }

                // Pack the common assets.
                if (item.Value.Count > 1)
                {
                    var build = new AssetBundleBuild
                    {
                        assetBundleName = BuildScript.ConvertToBundleName(assetPath, true),
                        assetNames = new[] { assetPath }
                    };
                    builds.Add(build);
                    packedAssets.Add(assetPath.ToLower());
                }
            }
        }
    }

    

    /// <summary>
    /// 打包texture目录下的图片资源合并为一个包
    /// 图集名字默认为Texture目录位置
    /// </summary>
    public class BuildAssetsTextureDirMerge : BuildRule
    {

        #region 属性字段信息

        /// <summary>
        /// Textures目录过滤
        /// </summary>
        private const string TEXTURES_DIR_FILTER = "Textures";

        /// <summary>
        /// Texture目录过滤
        /// </summary>
        private const string TEXTURE_DIR_FILTER = "Texture";

        #endregion


        #region 构造函数

        public BuildAssetsTextureDirMerge(string path, string pattern, SearchOption option, 
            string bname, bool isIgnoreEditor = true, bool isSubDirPack = false)
            : base(path, pattern, option, bname, isIgnoreEditor, isSubDirPack)
        {

        }

        #endregion


        #region 资源构建

        ///// <summary>
        ///// 构建Texture目录下的资源合并
        ///// </summary>
        //public override void BuildTextureMerge()
        //{

        //    if (!Directory.Exists(searchPath))
        //    {
        //        Debug.LogWarning("Not exist " + searchPath);
        //        return;
        //    }

        //    var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);
        //    ////检测需要打包的texture目录资源
        //    //var commonAssets = new Dictionary<string, List<string>>();
        //    //for (int i = 0; i < files.Count; i++)
        //    //{
        //    //    //监测符合条件的资源目录
        //    //    var dir = CheckFilePathInfo(files[i]);
        //    //    if(!commonAssets.TryGetValue(dir, out var list))
        //    //    {
        //    //        list = new List<string>();
        //    //        list.Add(files[i]);
        //    //        commonAssets[dir] = list;
        //    //    }
        //    //    else
        //    //    {
        //    //        list.Add(files[i]);
        //    //    }
        //    //}


        //    for (int i = 0; i < files.Count; i++)
        //    {
        //        var item = files[i];
        //        //监测符合条件的资源目录
        //        if (EditorUtility.DisplayCancelableProgressBar($"Packing... [{i}/{files.Count}]",
        //            item, i * 1f / files.Count))
        //        {
        //            break;
        //        }

        //        if (packedAssets.Contains(item.ToLower()))
        //        {
        //            continue;
        //        }

        //        var dir = CheckFilePathInfo(item);
        //        //不符合目录规范，不做处理
        //        if (string.IsNullOrEmpty(dir))
        //            continue;

        //        //根据资源生成需要构建的bundle数据
        //        var build = new AssetBundleBuild
        //        {
        //            assetBundleName = string.IsNullOrEmpty(bundleName) ? BuildScript.ConvertToBundleName(dir) : bundleName,
        //            assetNames = new[] { item }
        //        };

        //        //添加到构建队列列表中
        //        builds.Add(build);
        //        //添加到已经被打包的资源列表中
        //        packedAssets.Add(item.ToLower());
        //    }
        //}

        /// <summary>
        /// 构建Texture目录下的资源合并
        /// </summary>
        public override void BuildTextureMerge()
        {
            if (isSubDirAlonePack)
                SubDirAlonePack();
            else
                AllDirPack(searchPath, bundleName);
        }


        /// <summary>
        /// 所有目录统一打包
        /// </summary>
        private void AllDirPack(string path, string bundleName)
        {
            if (!Directory.Exists(path))
            {
                Debug.LogWarning("Not exist " + path);
                return;
            }

            var files = GetFilesWithoutPacked(path, searchPattern, searchOption);
            ////检测需要打包的texture目录资源
            //var commonAssets = new Dictionary<string, List<string>>();
            //for (int i = 0; i < files.Count; i++)
            //{
            //    //监测符合条件的资源目录
            //    var dir = CheckFilePathInfo(files[i]);
            //    if(!commonAssets.TryGetValue(dir, out var list))
            //    {
            //        list = new List<string>();
            //        list.Add(files[i]);
            //        commonAssets[dir] = list;
            //    }
            //    else
            //    {
            //        list.Add(files[i]);
            //    }
            //}


            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                //监测符合条件的资源目录
                if (EditorUtility.DisplayCancelableProgressBar($"Packing... [{i}/{files.Count}]",
                    item, i * 1f / files.Count))
                {
                    break;
                }

                if (packedAssets.Contains(item.ToLower()))
                {
                    continue;
                }

                var dir = CheckFilePathInfo(item);
                //不符合目录规范，不做处理
                if (string.IsNullOrEmpty(dir))
                    continue;

                //根据资源生成需要构建的bundle数据
                var build = new AssetBundleBuild
                {
                    assetBundleName = string.IsNullOrEmpty(bundleName) ? BuildScript.ConvertToBundleName(dir) : bundleName,
                    assetNames = new[] { item }
                };

                //添加到构建队列列表中
                builds.Add(build);
                //添加到已经被打包的资源列表中
                packedAssets.Add(item.ToLower());
            }
        }

        /// <summary>
        /// 子目录单独打包
        /// </summary>
        private void SubDirAlonePack()
        {
            if (!Directory.Exists(searchPath))
            {
                Debug.LogWarning("Not exist " + searchPath);
                return;
            }

            if (!string.IsNullOrEmpty(bundleName) && bundleName.LastIndexOf('.') > 0)
            {
                //获取当前目录下的所有目录
                var dirInfo = new DirectoryInfo(searchPath);
                var arr = dirInfo.GetDirectories();
                for (int i = 0; i < arr.Length; i++)
                {
                    var path = arr[i].FullName.Replace('\\', '/');
                    path = path.Substring(path.IndexOf(searchPath));
                    AllDirPack(path, bundleName.Insert(bundleName.LastIndexOf('.'), $".{arr[i].Name.ToLower()}"));
                }
            }

            //顶层目录添加上，避免有遗漏东西
            AllDirPack(searchPath, bundleName);
        }


        /// <summary>
        /// 监测文件路径信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string CheckFilePathInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var indexId = path.IndexOf(TEXTURES_DIR_FILTER);
            if (indexId < 0)
            {
                indexId = path.IndexOf(TEXTURE_DIR_FILTER);
                if (indexId < 0)
                    return null;

                return path.Substring(0, indexId + TEXTURE_DIR_FILTER.Length);
            }

            return path.Substring(0, indexId + TEXTURES_DIR_FILTER.Length);
        }


        #endregion

    }

    /// <summary>
    /// 打包图集资源
    /// </summary>
    public class BuildAssetsSpriteAtlas : BuildRule
    {
        public BuildAssetsSpriteAtlas(string path, string pattern, SearchOption option, 
            string bname, bool isIgnoreEditor = true, bool isSubDirPack = false) 
            : base(path, pattern, option, bname, isIgnoreEditor, isSubDirPack)
        {
        }

        //public override void BuildAtlas()
        //{
        //    if (!Directory.Exists(searchPath))
        //    {
        //        Debug.LogWarning("Not exist " + searchPath);
        //        return;
        //    }

        //    var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);

        //    // Generate AssetBundleBuild items.
        //    for (var i = 0; i < files.Count; i++)
        //    {
        //        var item = files[i];

        //        List<string> sprites = new List<string>();
        //        sprites.Add(item);

        //        if (EditorUtility.DisplayCancelableProgressBar($"Packing... [{i}/{files.Count}]",
        //            item, i * 1f / files.Count))
        //        {
        //            break;
        //        }

        //        if (packedAssets.Contains(item.ToLower()))
        //        {
        //            continue;
        //        }

        //        var tmp = AssetDatabase.GetDependencies(item.ToLower()).Where(x=>!x.EndsWith(".spriteatlas"));
        //        foreach (var s in tmp)
        //        {
        //            if (packedAssets.Contains(s))
        //            {
        //                continue;
        //            }

        //            if(AssetDatabase.IsValidFolder(s))
        //                continue;

        //            sprites.Add(s);
        //            packedAssets.Add(s);
        //        }


        //        var build = new AssetBundleBuild
        //        {
        //            assetBundleName = string.IsNullOrEmpty(bundleName) ? BuildScript.ConvertToBundleName(item) : bundleName,
        //            assetNames = sprites.ToArray(),
        //        };
        //        builds.Add(build);
        //        packedAssets.Add(item.ToLower());
        //    }

        //    // Collect dependencies.
        //    for (var i = 0; i < files.Count; i++)
        //    {
        //        var item = files[i];
        //        var dependencies = AssetDatabase.GetDependencies(item);
        //        if (EditorUtility.DisplayCancelableProgressBar($"Collecting... [{i}/{files.Count}]", item,
        //            i * 1f / files.Count))
        //        {
        //            break;
        //        }

        //        foreach (var assetPath in dependencies)
        //        {
        //            if (!packedAssets.Contains(assetPath.ToLower()))
        //            {
        //                packedAssets.Add(assetPath.ToLower());
        //            }
        //        }
        //    }
        //}


        public override void BuildAtlas()
        {
            if (isSubDirAlonePack)
                SubDirAlonePack();
            else
                AllDirPack(searchPath, bundleName);

        }



        /// <summary>
        /// 所有目录统一打包
        /// </summary>
        private void AllDirPack(string path, string bundleName)
        {
            if (!Directory.Exists(path))
            {
                Debug.LogWarning("Not exist " + path);
                return;
            }

            var files = GetFilesWithoutPacked(path, searchPattern, searchOption);

            // Generate AssetBundleBuild items.
            for (var i = 0; i < files.Count; i++)
            {
                var item = files[i];

                List<string> sprites = new List<string>();
                sprites.Add(item);

                if (EditorUtility.DisplayCancelableProgressBar($"Packing... [{i}/{files.Count}]",
                    item, i * 1f / files.Count))
                {
                    break;
                }

                if (packedAssets.Contains(item.ToLower()))
                {
                    continue;
                }

                var tmp = AssetDatabase.GetDependencies(item.ToLower()).Where(x => !x.EndsWith(".spriteatlas"));
                foreach (var s in tmp)
                {
                    if (packedAssets.Contains(s))
                    {
                        continue;
                    }

                    if (AssetDatabase.IsValidFolder(s))
                        continue;

                    sprites.Add(s);
                    packedAssets.Add(s);
                }


                var build = new AssetBundleBuild
                {
                    assetBundleName = string.IsNullOrEmpty(bundleName) ? BuildScript.ConvertToBundleName(item) : bundleName,
                    assetNames = sprites.ToArray(),
                };
                builds.Add(build);
                packedAssets.Add(item.ToLower());
            }

            // Collect dependencies.
            for (var i = 0; i < files.Count; i++)
            {
                var item = files[i];
                var dependencies = AssetDatabase.GetDependencies(item);
                if (EditorUtility.DisplayCancelableProgressBar($"Collecting... [{i}/{files.Count}]", item,
                    i * 1f / files.Count))
                {
                    break;
                }

                foreach (var assetPath in dependencies)
                {
                    if (!packedAssets.Contains(assetPath.ToLower()))
                    {
                        packedAssets.Add(assetPath.ToLower());
                    }
                }
            }
        }

        /// <summary>
        /// 子目录单独打包
        /// </summary>
        private void SubDirAlonePack()
        {
            if (!Directory.Exists(searchPath))
            {
                Debug.LogWarning("Not exist " + searchPath);
                return;
            }

            if (!string.IsNullOrEmpty(bundleName) && bundleName.LastIndexOf('.') > 0)
            {
                //获取当前目录下的所有目录
                var dirInfo = new DirectoryInfo(searchPath);
                var arr = dirInfo.GetDirectories();
                for (int i = 0; i < arr.Length; i++)
                {
                    var path = arr[i].FullName.Replace('\\', '/');
                    path = path.Substring(path.IndexOf(searchPath));
                    AllDirPack(path, bundleName.Insert(bundleName.LastIndexOf('.'), $".{arr[i].Name.ToLower()}"));
                }
            }

            //顶层目录添加上，避免有遗漏东西
            AllDirPack(searchPath, bundleName);
        }
    }
}