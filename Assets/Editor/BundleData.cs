using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QFramework.Editor
{
    [Serializable]
    public class BundleData
    {
        /// <summary>
        /// Bundle位置数据枚举
        /// </summary>
        public enum BundleLocationEnum
        {
            /// <summary>
            /// 空，正常没有这种情况，一般为没有相应的Bundle文件
            /// </summary>
            NONE,
            /// <summary>
            /// 包内
            /// </summary>
            IN_PACK,
            /// <summary>
            /// 远程cdn
            /// </summary>
            REMOTE_CDN,
            /// <summary>
            /// 远程动态资源位置
            /// </summary>
            REMOTE_DYNAMIC,
            /// <summary>
            /// 远程配置和代码资源位置
            /// </summary>
            REMOTE_CONFIG_CODE_DYNAMIC,
            /// <summary>
            /// 远程随机地图动态配置
            /// </summary>
            REMOTE_RANDOM_MAP_DYNAMIC,
        }

        /// <summary>
        /// bundle 的hash
        /// </summary>
        private string bundleHash;
        //public bool bundleInPack;
        /// <summary>
        /// bundle名字
        /// </summary>
        public string bundleName;
        /// <summary>
        /// bundle资源依赖数据
        /// </summary>
        public string[] bundleDependencies;
        /// <summary>
        /// bundle大小
        /// </summary>
        public long bundleSize;

        /// <summary>
        /// Bundle位置类型枚举
        /// </summary>
        public BundleLocationEnum bundleLocation;

        /// <summary>
        /// bundle的hash值
        /// </summary>
        public Hash128 bundleHash128
        {
            get
            {
                return Hash128.Parse(bundleHash);
            }
            set
            {
                bundleHash = value.ToString();
            }
        }
    }

    /// <summary>
    /// bundle名字
    /// </summary>
    [Serializable]
    public class BundleName
    {
        /// <summary>
        /// 是否为atlas图集资源
        /// </summary>
        public bool atlasSprite;
        /// <summary>
        /// bundle名字
        /// </summary>
        public string bundleName;
        /// <summary>
        /// atlas名字
        /// </summary>
        public string atlasName;
        /// <summary>
        /// sprite名字
        /// </summary>
        public string spriteName;
    }

    /// <summary>
    /// bundle数据内容
    /// </summary>
    [Serializable]
    public class BundleDatas
    {
        public Dictionary<string, BundleData> Datas = new Dictionary<string, BundleData>();
        public Dictionary<string, BundleName> Names = new Dictionary<string, BundleName>();

        /// <summary>
        /// dlc资源数据列表<资源路径，资源所属类型>
        /// </summary>
        public Dictionary<string, int> dlcDic = new Dictionary<string, int>();


        //public Dictionary<string, string> AtlasNames = new Dictionary<string, string>();

        //public static byte[] Serialize(BundleDatas o)
        //{
        //    var s = new MemoryStream();
        //    var formatter = new BinaryFormatter();
        //    formatter.Serialize(s, o);
        //    return s.ToArray();
        //}

        //public static BundleDatas Deserialize(byte[] arr)
        //{
        //    var s = new MemoryStream(arr);
        //    var formatter = new BinaryFormatter();
        //    var o = (BundleDatas)formatter.Deserialize(s);
        //    return o;
        //}





        /// <summary>
        /// 文件内容序列化
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static byte[] Serialize(BundleDatas o)
        {
            using (MemoryStream s = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(s);
                o.WriteDatas(bw);
                o.WriteNames(bw);
                o.WriteDlcDic(bw);
                return s.ToArray();
            }
        }

        /// <summary>
        /// 文件内容反序列话
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static BundleDatas Deserialize(byte[] arr)
        {
            try
            {
                using (MemoryStream s = new MemoryStream(arr))
                {
                    BinaryReader br = new BinaryReader(s);
                    BundleDatas o = new BundleDatas();
                    o.ReadDatas(br);
                    o.ReadNames(br);
                    o.ReadDlcDic(br);
                    return o;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Deserialize bundleDatas failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 数据内容写入
        /// </summary>
        /// <param name="bw"></param>
        private void WriteDatas(BinaryWriter bw)
        {
            bw.Write(Datas.Count);
            foreach (var data in Datas)
            {
                bw.Write(data.Key);
                BundleData bd = data.Value;
                bw.Write(bd.bundleHash128.ToString());
                //bw.Write(bd.bundleInPack);
                bw.Write(bd.bundleName);
                bw.Write(bd.bundleDependencies.Length);
                foreach (var dp in bd.bundleDependencies)
                {
                    bw.Write(dp);
                }

                bw.Write(bd.bundleSize);
            }
        }

        /// <summary>
        /// 写入资源名字
        /// </summary>
        /// <param name="bw"></param>
        private void WriteNames(BinaryWriter bw)
        {
            bw.Write(Names.Count);
            foreach (var name in Names)
            {
                bw.Write(name.Key);
                BundleName n = name.Value;
                bw.Write(n.atlasSprite);
                bw.Write(n.bundleName);
                bw.Write(n.atlasName ?? string.Empty);
                bw.Write(n.spriteName ?? string.Empty);
            }
        }

        /// <summary>
        /// 写入dlc资源名字
        /// </summary>
        /// <param name="bw"></param>
        private void WriteDlcDic(BinaryWriter bw)
        {
            bw.Write(dlcDic.Count);
            foreach (var dlc in dlcDic)
            {
                bw.Write(dlc.Key);
                bw.Write((int)dlc.Value);
            }
        }

        /// <summary>
        /// 读取数据内容
        /// </summary>
        /// <param name="br"></param>
        private void ReadDatas(BinaryReader br)
        {
            Datas.Clear();
            int dataCount = br.ReadInt32();
            for (int i = 0; i < dataCount; ++i)
            {
                string key = br.ReadString();
                BundleData data = new BundleData();
                data.bundleHash128 = Hash128.Parse(br.ReadString());
                //data.bundleInPack = br.ReadBoolean();
                data.bundleName = string.Intern(br.ReadString());
                int depCnt = br.ReadInt32();
                data.bundleDependencies = new string[depCnt];
                for (int j = 0; j < depCnt; ++j)
                {
                    data.bundleDependencies[j] = string.Intern(br.ReadString());
                }

                data.bundleSize = br.ReadInt64();
                Datas.Add(key, data);
            }
        }

        /// <summary>
        /// 读取名字内容信息
        /// </summary>
        /// <param name="br"></param>
        private void ReadNames(BinaryReader br)
        {
            Names.Clear();
            int nameCount = br.ReadInt32();
            for (int i = 0; i < nameCount; ++i)
            {
                string key = br.ReadString();
                BundleName name = new BundleName();
                name.atlasSprite = br.ReadBoolean();
                name.bundleName = string.Intern(br.ReadString());
                name.atlasName = string.Intern(br.ReadString());
                name.spriteName = string.Intern(br.ReadString());
                Names.Add(key, name);
            }
        }

        /// <summary>
        /// 读取名字内容信息
        /// </summary>
        /// <param name="br"></param>
        private void ReadDlcDic(BinaryReader br)
        {
            dlcDic.Clear();
            int nameCount = br.ReadInt32();
            for (int i = 0; i < nameCount; ++i)
            {
                dlcDic[br.ReadString()] = br.ReadInt32();
            }
        }
    }
}
