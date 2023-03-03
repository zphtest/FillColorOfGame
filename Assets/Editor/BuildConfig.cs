// ��Ȩ����[�ɶ����³�Ƽ��ɷ����޹�˾]
// ���ݡ�������Ϣʹ�����֤��������;
// ���Ƿ�����ɣ�����������ʹ�ô��ļ���
// ������������λ�û�ȡ���֤���������ӵ�ַ��
// https://wiki.tap4fun.com/pages/viewpage.action?pageId=29818250
// �������÷���Ҫ�������ͬ�⣬��������Ϣ����ʹ�����֤Ҫ��ʹ�ã� �������κ���ʾ��ʾ�ı�֤��������
// �йع���Ȩ�޵��ض����ԣ���������֤������


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QFramework.Editor
{

    /// <summary>
    /// dlc�������Դ����
    /// </summary>
    public enum DlcAssetType
    {
        /// <summary>
        /// ��ɫģ��
        /// </summary>
        ROLE_MODEL,
        /// <summary>
        /// ������ͼ
        /// </summary>
        MAP_DRAGON_WAR,
        /// <summary>
        /// KvK��ͼ
        /// </summary>
        MAP_KVK,
    }


    /// <summary>
    /// ����Ĭ�ϰ����ڲ���Դ������Ϣ
    /// </summary>
    [System.Serializable]
    public class BuildAssetFilter
    {
        public string path = string.Empty;
        public string filter = "*.prefab";
        public string abname = "";
        public bool subdirs = false;        //�������ļ���
        public bool atlas = false;          //�ϲ���һ������
        public bool textureMerge = false;   //Ŀ¼�µ�texture���ϲ�Ϊһ����������Ĭ��ΪtextureĿ¼��·��
        public bool isSubDirPack = false;   //�Ƿ����Ŀ¼���
    }

    /// <summary>
    /// ����dlc��Դ������Ϣ
    /// </summary>
    [System.Serializable]
    public class BuildDlcAssetFilter
    {
        /// <summary>
        /// ��Դ·����Ϣ
        /// </summary>
        public string path;

        /// <summary>
        /// ����Դ·���µ�����ƥ��
        /// </summary>
        public string filter;

        /// <summary>
        /// ��Դ���͹���
        /// </summary>
        public DlcAssetType assetType;
    }

    /// <summary>
    /// ������Դ������Ϣ
    /// </summary>
    [CreateAssetMenu(fileName = "BuildConfig", menuName = "BuildConfig", order = 1)]
    public class BuildConfig : ScriptableObject
    {
        /// <summary>
        /// Ĭ�ϰ����ڵ���Դ����
        /// </summary>
        public List<BuildAssetFilter> filters = new List<BuildAssetFilter>();

        /// <summary>
        /// ����dlc��Դ����
        /// </summary>
        public List<BuildDlcAssetFilter> dlcFilter = new List<BuildDlcAssetFilter>();
    }
}