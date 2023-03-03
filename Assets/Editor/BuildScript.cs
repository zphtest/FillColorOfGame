// 版权所有[成都尼毕鲁科技股份有限公司]
// 根据《保密信息使用许可证》获得许可;
// 除非符合许可，否则您不得使用此文件。
// 您可以在以下位置获取许可证副本，链接地址：
// https://wiki.tap4fun.com/pages/viewpage.action?pageId=29818250
// 除非适用法律要求或书面同意，否则保密信息按照使用许可证要求使用， 不附带任何明示或暗示的保证或条件。
// 有关管理权限的特定语言，请参阅许可证副本。


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QFramework.Editor
{ 
	public class BuildScript : Helper
	{
		[InitializeOnLoadMethod]
		public static void Clear ()
		{
			EditorUtility.ClearProgressBar (); 
		}

		public static void BuildAssetBundles (List<AssetBundleBuild> builds, 
            List<BuildDlcAssetFilter> dlcFilter, string resource_format = null)
		{  
			// Choose the output path according to the build target.
			string outputPath = Application.streamingAssetsPath +"/../../" + BundleSavePath;

#if !UNITY_EDITOR && UNITY_ANDROID
            var options = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree;
#else
            var options = BuildAssetBundleOptions.ChunkBasedCompression;
#endif
            if (dlcFilter != null && dlcFilter.Count > 0)
            {
                //从dlc目录里面复制资源到assetbundle目录
                var dlcPath = $"DLC/{Helper.GetPlatformName()}/{Application.version}/DLC{resource_format}/";
                if (Directory.Exists(dlcPath))
                {
                    var arr = Directory.GetFiles(dlcPath, "*.ab", SearchOption.TopDirectoryOnly);
                    if (arr != null)
                    {
                        Debug.LogError("从dlc里面移动资源到assetbundle");

                        for (int i = 0; i < arr.Length; i++)
                        {
                            if (File.Exists(arr[i]))
                            {
                                var name = Path.GetFileName(arr[i]);
                                //Debug.LogError(arr[i] + "  name " + Path.GetFileName(arr[i]) + " ttt " + $"{outputPath}/{name}");
                                File.Copy(arr[i], $"{outputPath}/{name}", true);
                            }
                        }
                    }
                }
            }

            //打包资源路径排序
            builds.Sort((a, b) => string.CompareOrdinal(a.assetBundleName, b.assetBundleName));
            if (builds == null || builds.Count == 0)
            {
                //@TODO: use append hash... (Make sure pipeline works correctly with it.)
                BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);
            }
            else
            {
                //builds.Sort((a,b)=> string.Compare(a.assetBundleName,b.assetBundleName));
                BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), options, EditorUserBuildSettings.activeBuildTarget);
            }

            //计算assetbundleSize数据
            CacAssetBundleSize(outputPath, dlcFilter, resource_format);
        }

        /// <summary>
        /// 生成assetbundle_size
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="buildType"></param>
        static void CacAssetBundleSize(string outputPath,
            List<BuildDlcAssetFilter> dlcFilter, string resource_format = null)
        {
            var tempPath = "";
            tempPath = BundleSaveName;
            var BundlePath = outputPath + "/";
            //dlc路径
            var dlcPath = $"DLC/{Helper.GetPlatformName()}/{Application.version}/DLC{resource_format}/";
            Debug.LogError("  bundlePath " + BundlePath);
            Debug.LogError("  dlcPath " + dlcPath);
            //项目工程目录
            var projectDir = Application.dataPath.Replace("Assets", "");
            var srcAMF = BundlePath + tempPath;
            if (File.Exists(srcAMF))
            {
                var ab = AssetBundle.LoadFromFile(srcAMF);
                var abm = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                ab.Unload(false);
	            
	            // Create Map from asset to AssetBundle.
                var data = new BundleDatas();
                var bundles = abm.GetAllAssetBundles();
                foreach (var bundle in bundles)
                {
                    var bd = new BundleData
                    {
                        //bundleInPack = false,
                        bundleLocation = BundleData.BundleLocationEnum.REMOTE_CDN,
		                bundleName = bundle,
		                bundleHash128 = abm.GetAssetBundleHash(bundle),
		                bundleDependencies = abm.GetDirectDependencies(bundle),
		                bundleSize = new FileInfo(BundlePath + bundle).Length
	                };
	                data.Datas.Add(bundle, bd);

                    var bdm = AssetBundle.LoadFromFile(BundlePath + bundle);
                    var bdabns = bdm.GetAllAssetNames();
                    var bdabps = bdm.GetAllScenePaths();
                    bdm.Unload(true);
                    foreach(var asset in bdabns)
                    {
                        if (!data.Names.ContainsKey(asset.ToLower()))
                        {
                            data.Names.Add(asset.ToLower(), new BundleName() { bundleName = bundle });
                        }
                        if(asset.EndsWith(".spriteatlas", StringComparison.Ordinal))
                        {
                            //单个Sprite图片
                            var deps = AssetDatabase.GetDependencies(asset, false);
                            foreach (var dep in deps)
                            {
                                string spriteName = string.Empty;
                                if (Helper.ConvertToAtlasSpriteName(dep, out spriteName))
                                {
                                    data.Names[dep.ToLower()] = new BundleName() { atlasSprite = true, bundleName = bundle, atlasName = asset, spriteName = spriteName };
                                }
                            }

                            ////图集相关记录
                            //string atlasName = null;
                            //if (Helper.ConvertToAtlasName(asset.ToLower(), out atlasName))
                            //{
                            //    data.AtlasNames.Add(atlasName, asset.ToLower());
                            //}
                        }
                    }
                    foreach (var asset in bdabps)
                    {
                        data.Names.Add(asset.ToLower(), new BundleName() { bundleName = bundle });
                    }

                    //检测dlc资源
                    var assetType = DlcAssetType.ROLE_MODEL;
                    if(IsDlcAssetBundle(dlcFilter, bundle, ref assetType))
                    {
                        var srcPath = $"{projectDir}/{BundlePath}{bundle}";
                        var targetPath = $"{projectDir}/{dlcPath}{bundle}";
                        //Debug.LogError("  sss " + srcPath);
                        //如果文件不存在，那么就跳过
                        if (!File.Exists(srcPath))
                            continue;

                        //创建目标目录
                        if (!Directory.Exists($"{projectDir}/{dlcPath}"))
                            Directory.CreateDirectory($"{projectDir}/{dlcPath}");

                        //如果目标数据存在，那么就移除
                        if(File.Exists(targetPath))
                            File.Delete(targetPath);

                        //复制资源数据
                        File.Move(srcPath, targetPath);
                        //移除原始资源
                        //File.Delete(srcPath);
                        //添加dlc资源列表
                        data.dlcDic[bundle] = (int)assetType;
                    }
                }

                var bytes = BundleDatas.Serialize(data);
				var nbytes = Crypto.ZLib.Zip(bytes);
				//bytes = Crypto.UnZip(nbytes);
                //var obj = ScriptableBundleData.Deserialize(bytes);
                File.WriteAllBytes(srcAMF + "_size", nbytes);
            }

            if (!string.IsNullOrEmpty(resource_format))
            {
                var abPath = outputPath + "/" + tempPath + resource_format;
                if (File.Exists(abPath))
                {
                    var newAbPath = outputPath + "/" + tempPath;
                    var manifestAbPath = abPath + ".manifest";
                    var newManifestAbPath = newAbPath + ".manifest";
                    UnityEngine.Debug.LogError(" resource_format " + resource_format + "  abPath " + abPath + " newPath " + newAbPath);
                    File.Copy(abPath, newAbPath, true);
                    File.Copy(manifestAbPath, newManifestAbPath, true);
                    //移除原始资源
                    File.Delete(abPath);
                    File.Delete(manifestAbPath);

                }
            }
        }

        /// <summary>
        /// 是否为dlc 资源
        /// </summary>
        /// <param name="dlcFilter"></param>
        /// <param name="bundlePath"></param>
        /// <returns></returns>
        private static bool IsDlcAssetBundle(List<BuildDlcAssetFilter> dlcFilter, string bundlePath, ref DlcAssetType assetType)
        {
            if (string.IsNullOrEmpty(bundlePath)
                || dlcFilter == null
                || dlcFilter.Count == 0)
                return false;

            foreach (var item in dlcFilter)
            {
                if (item == null
                    || string.IsNullOrEmpty(item.path))
                    continue;

                if(string.IsNullOrEmpty(item.filter))
                {
                    if (bundlePath.Contains(item.path))
                    {
                        assetType = item.assetType;
                        return true;
                    }
                }
                else
                {
                    var arr = item.filter.Split(';');
                    var path = "";
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (string.IsNullOrEmpty(arr[i]))
                            continue;

                        path = $"{item.path}{arr[i]}";
                        if (bundlePath.Contains(path))
                        {
                            assetType = item.assetType;
                            return true;
                        }
                    }
                }


               
            }


            return false;
        }



		public static void BuildPlayerWithoutAssetBundles ()
		{
			var outputPath = EditorUtility.SaveFolderPanel ("Choose Location of the Built Game", "", "");
			if (outputPath.Length == 0)
				return;

			string[] levels = GetLevelsFromBuildSettings ();
			if (levels.Length == 0) {
				Debug.Log ("Nothing to build.");
				return;
			}

			string targetName = GetBuildTargetName (EditorUserBuildSettings.activeBuildTarget);
			if (targetName == null)
				return; 

			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions ();
			buildPlayerOptions.scenes = levels;
			buildPlayerOptions.locationPathName = outputPath + targetName;
			buildPlayerOptions.assetBundleManifestPath = GetAssetBundleManifestFilePath ();
			buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
			buildPlayerOptions.options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer (buildPlayerOptions);
		}

		public static void BuildStandalonePlayer ()
		{
			var outputPath = EditorUtility.SaveFolderPanel ("Choose Location of the Built Game", "", "");
			if (outputPath.Length == 0)
				return;

			string[] levels = GetLevelsFromBuildSettings ();
			if (levels.Length == 0) {
				Debug.Log ("Nothing to build.");
				return;
			}

			string targetName = GetBuildTargetName (EditorUserBuildSettings.activeBuildTarget);
			if (targetName == null)
				return; 
			
            CopyAssetBundlesTo (Path.Combine (Application.streamingAssetsPath, Helper.BundleSaveName));
			AssetDatabase.Refresh ();

			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions ();
			buildPlayerOptions.scenes = levels;
			buildPlayerOptions.locationPathName = outputPath + targetName;
			buildPlayerOptions.assetBundleManifestPath = GetAssetBundleManifestFilePath ();
			buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
			buildPlayerOptions.options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
			BuildPipeline.BuildPlayer (buildPlayerOptions);
		}

		public static string GetBuildTargetName (BuildTarget target)
		{
			string name = PlayerSettings.productName + "_" + PlayerSettings.bundleVersion;
			switch (target) {
			case BuildTarget.Android:
				return "/" + name + PlayerSettings.Android.bundleVersionCode + ".apk";
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return "/" + name + PlayerSettings.Android.bundleVersionCode + ".exe";
			case BuildTarget.StandaloneOSX:
				return "/" + name + ".app";
			//                case BuildTarget.WebPlayer:
			//                case BuildTarget.WebPlayerStreamed:
			case BuildTarget.WebGL:
			case BuildTarget.iOS:
				return "";
			// Add more build targets for your own.
			default:
				Debug.Log ("Target not implemented.");
				return null;
			}
		}

		static public void CopyAssetBundlesTo (string outputPath)
		{
			// Clear streaming assets folder.
			//            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
			if (!Directory.Exists (outputPath)) {
				Directory.CreateDirectory (outputPath);  
			}

			// Setup the source folder for assetbundles.
            var source = Helper.BundleSavePath;
			if (!System.IO.Directory.Exists (source))
				Debug.Log ("No assetBundle output folder, try to build the assetBundles first.");

			// Setup the destination folder for assetbundles.
            if (System.IO.Directory.Exists (outputPath))
                FileUtil.DeleteFileOrDirectory (outputPath);

            FileUtil.CopyFileOrDirectory (source, outputPath);

            AssetDatabase.Refresh();
		}

        static public void CopyAssetBundlesSizeTo(string outputPath)
        {
            // Clear streaming assets folder.
            //            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            

            // Setup the source folder for assetbundles.
            var source = $"{Helper.BundleSavePath}/AssetBundles_size";
            if (!File.Exists(source))
                Debug.Log("No assetBundle_size output folder, try to build the assetBundles first.");

            // Setup the destination folder for assetbundles.
            if (System.IO.Directory.Exists(outputPath))
                FileUtil.DeleteFileOrDirectory(outputPath);

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            outputPath = $"{outputPath}/AssetBundles_size";
            FileUtil.CopyFileOrDirectory(source, outputPath);

            AssetDatabase.Refresh();
        }


        static string[] GetLevelsFromBuildSettings ()
		{
			List<string> levels = new List<string> ();
			for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i) {
				if (EditorBuildSettings.scenes [i].enabled)
					levels.Add (EditorBuildSettings.scenes [i].path);
			}

			return levels.ToArray ();
		}

		static string GetAssetBundleManifestFilePath ()
		{
            return Path.Combine (Helper.BundleSavePath, Helper.BundleSaveName) + ".manifest";
		}


        #region 刷新dlc资源

        /// <summary>
        /// 刷新存储的dlc资源
        /// </summary>
        /// <param name="isHaveDlc"></param>
        [MenuItem("Tools/Bundle/UpdateDlc")]
        public static void UpdateDlcAssetBundles(bool isHaveDlc)
        {
            //有dlc资源，那么不做处理
            //不需要dlc资源配置的时候，那么出包的时候移除此资源
            if (isHaveDlc)
                return;

            var path = Path.Combine(Application.streamingAssetsPath, Helper.BundleSaveName, Helper.BundleSaveName + "_size");
            if(File.Exists(path))
            {
                var bytes = File.ReadAllBytes(path);
                //解压缩文件
                var nb = Crypto.ZLib.UnZip(bytes);
                //反序列话数据，获取真是的bundle数据
                var data = BundleDatas.Deserialize(nb);
                if(data == null)
                {
                    Debug.LogError("update dlc assetbundles data is null");
                    return;
                }

                //数据清理，重新写入
                data.dlcDic.Clear();

                bytes = BundleDatas.Serialize(data);
                nb = Crypto.ZLib.Zip(bytes);
                File.WriteAllBytes(path, nb);
            }
            else
            {
                Debug.LogError("update dlc assetbundles can not find path" + path);
            }
        }



        #endregion

    }
}