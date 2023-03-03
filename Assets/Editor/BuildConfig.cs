// 版权所有[成都尼毕鲁科技股份有限公司]
// 根据《保密信息使用许可证》获得许可;
// 除非符合许可，否则您不得使用此文件。
// 您可以在以下位置获取许可证副本，链接地址：
// https://wiki.tap4fun.com/pages/viewpage.action?pageId=29818250
// 除非适用法律要求或书面同意，否则保密信息按照使用许可证要求使用， 不附带任何明示或暗示的保证或条件。
// 有关管理权限的特定语言，请参阅许可证副本。


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QFramework.Editor
{

    /// <summary>
    /// dlc里面的资源类型
    /// </summary>
    public enum DlcAssetType
    {
        /// <summary>
        /// 角色模型
        /// </summary>
        ROLE_MODEL,
        /// <summary>
        /// 巨龙地图
        /// </summary>
        MAP_DRAGON_WAR,
        /// <summary>
        /// KvK地图
        /// </summary>
        MAP_KVK,
    }


    /// <summary>
    /// 构建默认包体内部资源过滤信息
    /// </summary>
    [System.Serializable]
    public class BuildAssetFilter
    {
        public string path = string.Empty;
        public string filter = "*.prefab";
        public string abname = "";
        public bool subdirs = false;        //包含子文件夹
        public bool atlas = false;          //合并到一个包中
        public bool textureMerge = false;   //目录下的texture都合并为一个包，包名默认为texture目录的路径
        public bool isSubDirPack = false;   //是否分子目录打包
    }

    /// <summary>
    /// 构建dlc资源过滤信息
    /// </summary>
    [System.Serializable]
    public class BuildDlcAssetFilter
    {
        /// <summary>
        /// 资源路径信息
        /// </summary>
        public string path;

        /// <summary>
        /// 此资源路径下的数据匹配
        /// </summary>
        public string filter;

        /// <summary>
        /// 资源类型归类
        /// </summary>
        public DlcAssetType assetType;
    }

    /// <summary>
    /// 构建资源数据信息
    /// </summary>
    [CreateAssetMenu(fileName = "BuildConfig", menuName = "BuildConfig", order = 1)]
    public class BuildConfig : ScriptableObject
    {
        /// <summary>
        /// 默认包体内的资源数据
        /// </summary>
        public List<BuildAssetFilter> filters = new List<BuildAssetFilter>();

        /// <summary>
        /// 单独dlc资源数据
        /// </summary>
        public List<BuildDlcAssetFilter> dlcFilter = new List<BuildDlcAssetFilter>();
    }
}