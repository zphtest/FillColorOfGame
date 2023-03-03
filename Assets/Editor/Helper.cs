// 版权所有[成都尼毕鲁科技股份有限公司]
// 根据《保密信息使用许可证》获得许可;
// 除非符合许可，否则您不得使用此文件。
// 您可以在以下位置获取许可证副本，链接地址：
// https://wiki.tap4fun.com/pages/viewpage.action?pageId=29818250
// 除非适用法律要求或书面同意，否则保密信息按照使用许可证要求使用， 不附带任何明示或暗示的保证或条件。
// 有关管理权限的特定语言，请参阅许可证副本。

using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Text;

namespace QFramework.Editor
{
    public class Helper
	{

        /// <summary>
        /// 资源路径后缀 proto
        /// </summary>
        public const string STR_EXTENSION_PROTO = ".proto";
        /// <summary>
        /// 资源路径后缀 def
        /// </summary>
        public const string STR_EXTENSION_DEF = ".def";
        /// <summary>
        /// 资源路径后缀 tsv
        /// </summary>
        public const string STR_EXTENSION_TSV = ".tsv";


        /// <summary>
        /// Bundle文件夹名
        /// </summary>
        public const string BundleSaveName = "AssetBundles";

        /// <summary>
        /// Video文件夹名
        /// </summary>
        public const string VideoSaveName = "Video";

        /// <summary>
        /// 远程DLC bundle 资源名字
        /// </summary>
        public const string RemoteDLCBundleSaveName = "DLC";

        /// <summary>
        /// 动态加载的Bundle文件名
        /// </summary>
        public const string DynamicBundleSaveName = "DynamicLoadAssetBundles";

        /// <summary>
        /// ConfigCode动态加载的Bundle文件名
        /// </summary>
        public const string ConfigCodeDynamicBundleSaveName = "ConfigCodeDynamicLoadAssetBundles";

        /// <summary>
        /// 动态随机地图配置加载的bundle文件名
        /// </summary>
        public const string DynamicRandomMapBundleSaveName = "DynamicRandomMapLoadAssetBundles";

        public static void TraceTime (string name, System.Action action)
		{  
			var time = System.DateTime.Now.TimeOfDay.TotalSeconds;  
			if (action != null) {
				action ();
			}
			var elasped = System.DateTime.Now.TimeOfDay.TotalSeconds - time; 
		}

        /// <summary>
        /// 获取当前平台名称
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformName()
		{
			#if UNITY_EDITOR
			return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
			#else
			return GetPlatformForAssetBundles(Application.platform);
			#endif
		}

        /// <summary>
        /// 通过BuildTarget获取平台名称
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
		{
			switch (platform)
			{
			case RuntimePlatform.Android:
				return "android";
			case RuntimePlatform.IPhonePlayer:
				return "ios";
			case RuntimePlatform.WindowsPlayer:
				return "win32";
			default:
				return null;
			}
		}

#if UNITY_EDITOR
        /// <summary>
        /// 通过BuildTarget获取平台名称
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string GetPlatformForAssetBundles(BuildTarget target)
		{
			switch (target)
			{
			case BuildTarget.Android:
				return "android";
			case BuildTarget.iOS:
				return "ios";
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return "win32";
			default:
				return null;
			}
		}
		#endif 

#if UNITY_EDITOR
        /// <summary>
        /// AB 保存的路径
        /// </summary>
        public static string BundleSavePath { get { return "DLC/" + Helper.GetPlatformName() + "/" + Application.version + "/" + BundleSaveName; } }
        public static string BundleDlcvPath { get { return "DLC/" + Helper.GetPlatformName() + "/" + Application.version + "/ver.txt"; } }
        public static string BundleElogPath { get { return "DLC/" + Helper.GetPlatformName() + "/" + Application.version + "/elog.txt"; } }

#if UNITY_EDITOR

        /// <summary>
        /// 测试资源存储路径
        /// </summary>
        public static string TestBundleSavePathDynamic { get { return "DLC/" + Helper.GetPlatformName() + "/" + Application.version + "/" + BundleSaveName; } }

#endif

        /// <summary>
        /// 动态资源存储路径
        /// </summary>
        public static string BundleSavePathDynamic { get { return "Dynamic/" + Helper.GetPlatformName() + "/" + Application.version + "/" + DynamicBundleSaveName; } }
        /// <summary>
        /// 资源文件存储路径
        /// </summary>
        public static string BundleDlcvPathDynamic { get { return "Dynamic/" + Helper.GetPlatformName() + "/" + Application.version + "/ver.txt"; } }


        /// <summary>
        /// 配置和代码动态资源存储路径
        /// </summary>
        public static string BundleSavePathConfigCodeDynamic { get { return "ConfigCodeDynamic/" + Helper.GetPlatformName() + "/" + Application.version + "/" + ConfigCodeDynamicBundleSaveName; } }
        /// <summary>
        /// 配置和代码资源文件存储路径
        /// </summary>
        public static string BundleDlcvPathConfigCodeDynamic { get { return "ConfigCodeDynamic/" + Helper.GetPlatformName() + "/" + Application.version + "/ver.txt"; } }


        /// <summary>
        /// 随机地图配置动态资源存储路径
        /// </summary>
        public static string BundleSavePathDynamicRandomMap { get { return "DynamicMap/" + Helper.GetPlatformName() + "/" + Application.version + "/" + DynamicRandomMapBundleSaveName; } }
        /// <summary>
        /// 随机地图配置资源文件存储路径
        /// </summary>
        public static string BundleDlcvPathDynamicRandomMap { get { return "DynamicMap/" + Helper.GetPlatformName() + "/" + Application.version + "/ver.txt"; } }

#endif
        /// <summary>
        /// 获取 AB 源文件路径（网络的）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static StringBuilder temPath = new StringBuilder();

        /// <summary>
        /// 获取 AB 源文件路径（打包进安装包的）
        /// </summary>
        /// <param name="path">资源路径(Assets/xx/yy)</param>
        /// <returns>完整路径</returns>
        public static string GetInternalPath(string path)
        {
            temPath.Clear();
            temPath.Append(Application.streamingAssetsPath);
            temPath.Append("/");
            temPath.Append(BundleSaveName);
            temPath.Append("/");
            temPath.Append(path);
            return temPath.ToString();
        }

#if UNITY_EDITOR

        /// <summary>
        /// 将资源路径转换为Bundle名称
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="merge"></param>
        /// <returns></returns>
        public static string ConvertToBundleName(string assetPath, bool merge = false)
        {
            var bn = assetPath.Replace(Application.dataPath, "")
                              .Replace('\\', '.')
                              .Replace('/', '.')
                              .Replace(" ", "_")
                              .ToLower() + ".ab";

            if (merge)
            {
                var bns = bn.Split('.');

                // Promote bundle granularity.
                //if (bns.Length >= 3)
                //{
                //    bn = bns[0] + "." + bns[1] + "." + bns[2] + ".merge.ab";
                //}

                //直接使用路径中的前三位，会造成merge包体太大
                //所以使用当前目录路径的移除后两位
                
                if(bns.Length >= 3)
                {
                    if (bns.Length >= 5)
                    {
                        bn = $"{bns[0]}.{bns[1]}.{bns[2]}.{bns[3]}.{bns[4]}.merge.ab";
                    }
                    else
                    {

                        for (int i = 0; i < bns.Length; i++)
                        {
                            bn += $"{bns[i]}.";
                        }

                        bn += "merge.ab";

                        //if (bns.Length == 4)
                        //{
                        //    bn = $"{bns[0]}.{bns[1]}.{bns[2]}.{bns[3]}.merge.ab";
                        //}
                        //else
                        //{
                        //    bn = bns[0] + "." + bns[1] + "." + bns[2] + ".merge.ab";
                        //}
                    }
                }
            }
            return bn;
        }

#endif

        /// <summary>
        /// 将指定类型的文本文件路径加上.txt后缀
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

            //if (path.EndsWith(".proto")
            //    || path.EndsWith(".def")
            //    || path.EndsWith(".tsv")
            //    || path.EndsWith(".tmx")
            //    || path.EndsWith(".lua"))
            //{
            //    path += ".txt";
            //}
        }

#if UNITY_EDITOR

        /// <summary>
        /// 去掉指定指定类型的文本文件路径的.txt后缀
        /// </summary>
        /// <param name="path"></param>
        public static void TrimTextAssetExt(ref string path)
        {
            if (path.EndsWith(".proto.txt")
                || path.EndsWith(".def.txt")
                || path.EndsWith(".tsv.txt")
                || path.EndsWith(".tmx.txt")
                || path.EndsWith(".lua.txt"))
            {
                path = path.Substring(0, path.Length - 4);
            }
        }

        /// <summary>
        /// 将Sprite的路径转换为对应的Sprite名称
        /// </summary>
        /// <param name="spritePath">Sprite资源路径</param>
        /// <param name="spriteName">Sprite名称</param>
        /// <returns>是否是一个合法的Sprite路径</returns>
        public static bool ConvertToAtlasSpriteName(string spritePath,out string spriteName)
        {
            var bn = spritePath.ToLower();
            if (bn.EndsWith(".png", StringComparison.Ordinal)
                || bn.EndsWith(".tga", StringComparison.Ordinal)
                || bn.EndsWith(".jpg", StringComparison.Ordinal))
            {
                var sn = spritePath.Replace("\\", "");
                var idxs = sn.LastIndexOf("/");
                var idxe = sn.LastIndexOf(".");
                if (idxe - idxs > 1)
                {
                    spriteName = sn.Substring(idxs + 1, idxe - idxs - 1);
                    return true;
                }
            }
            spriteName = string.Empty;
            return false;
        }

#endif

        /// <summary>
        /// 从图集路径中提取出图集名称
        /// </summary>
        /// <param name="atlasPath">图集路径</param>
        /// <param name="atlasName">图集名称</param>
        /// <returns>是否是一个合法的图集路径</returns>
        public static bool ConvertToAtlasName(string atlasPath, out string atlasName)
        {
            //var bn = atlasPath.ToLower();
            if (atlasPath.EndsWith(".spriteatlas", StringComparison.Ordinal))
            {
                var sn = atlasPath.Replace("\\", "");
                var idxs = sn.LastIndexOf("/");
                var idxe = sn.LastIndexOf(".spriteatlas");
                if (idxe - idxs > 1)
                {
                    atlasName = sn.Substring(idxs + 1, idxe - idxs - 1);
                    return true;
                }
            }
            atlasName = string.Empty;
            return false;
        }



        /// <summary>
        /// Loading场景Bundle名
        /// </summary>
        public static string LogoLoadingPrefabName
        {
            get
            {
                return "LogoLoading.prefab.ab";
            }
        }


        #region 资源加载失败时的提示信息

        /// <summary>
        /// 加载资源失败Action
        /// </summary>
        private static Action<bool> loadResFailAction;

        /// <summary>
        /// 注册加载资源失败Action
        /// </summary>
        /// <param name="action"></param>
        public static void RegisterLoadResFailAction(Action<bool> action)
        {
            loadResFailAction += action;
        }

        /// <summary>
        /// 调用资源加载失败时Action
        /// </summary>
        public static void CallLoadResFailAction(bool isLoadDlc = false)
        {
            if (loadResFailAction != null)
                loadResFailAction(isLoadDlc);
        }

        #endregion


        #region  astc 格式支持

        /// <summary>
        /// 图片单独支持格式
        /// </summary>
        private static string TCF_TEX = "";

        /// <summary>
        /// astc图片格式
        /// </summary>
        private static string TCF_TEX_ASTC = "#tcf_astc";

        /// <summary>
        /// 初始化图片支持格式
        /// </summary>
        public static void InitTCF_TEX()
        {
#if UNITY_ANDROID

            if(SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_6x6)
                && SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_8x8))
            {
                TCF_TEX = TCF_TEX_ASTC;
            }

#endif
        }

        #endregion

    }
}

namespace UnityEngine
{
    public class ABTextAsset : TextAsset, ITFWCustomAsset
    {
        public byte[] ABBytes
        {
            get;
            set;
        }
        public string ABText
        {
            get;
            set;
        }
    }

    public interface ITFWCustomAsset
    {
    }
}

public static class TextAssetHelper
{
    public static byte[] GetBytes(this TextAsset asset)
    {
        if (asset is ABTextAsset)
        {
            var ab = asset as ABTextAsset;
            return ab.ABBytes;
        }
        return asset.bytes;
    }
    public static string GetText(this TextAsset asset)
    {
        if (asset is ABTextAsset)
        {
            var ab = asset as ABTextAsset;
            return ab.ABText;
        }
        return asset.text;
    }
}