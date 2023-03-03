using Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Helper
{
    public sealed class NetHelper
    {
        public const int PackHeadSize = 4;

        public const byte CCC_Compress = 0x80;
        public const byte CCC_Crypto = 0x40;
        public const byte CCC_Crc = 0x20;
        static bool NetBigEndian = false;

        static void CheckReverse(byte[] bytes, int offset = 0, int len = -1)
        {
            if (BitConverter.IsLittleEndian == NetBigEndian)
            {
                len = len < 0 ? bytes.Length : len;
                Array.Reverse(bytes, offset, len);
            }
        }
        public static int ToInt32(byte[] bytes, int offset)
        {
            CheckReverse(bytes, offset, 4);
            var val = BitConverter.ToInt32(bytes, offset);
            CheckReverse(bytes, offset, 4);
            return val;
        }

        public static byte[] ToBytes(short i)
        {
            var bytes = BitConverter.GetBytes(i);
            CheckReverse(bytes, 0, 2);
            return bytes;
        }

        public static byte[] ToBytes(int i)
        {
            var bytes = BitConverter.GetBytes(i);
            CheckReverse(bytes, 0, 4);
            return bytes;
        }

        public static IPAddress ParseIpAddressV6(string address)
        {
            IPAddress addrOut = null;

            // 如果是IP，直接返回
            if (IPAddress.TryParse(address, out addrOut))
                return addrOut;

            // 如果是域名，查一下
            IPAddress[] addrList = Dns.GetHostAddresses(address);
            if (addrList.Length <= 0)
                return null; // 没找到就只能给空了

            // 默认使用第一ip
            addrOut = addrList[0];

#if false // 优先使用 v6
            for (int i = 0; i < addrList.Length; i++)
            {
                if (addrList[i].AddressFamily == AddressFamily.InterNetworkV6)
                    return addrList[i];
            }
#endif

            return addrOut;
        }

        static bool Reachable(IPAddress addr, int port, int timeout)
        {
            var ep = new IPEndPoint(addr, port);
            var suc = false;
            var wh = new System.Diagnostics.Stopwatch();
            wh.Start();
            using (var s = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    var ar = s.BeginConnect(ep, null, null);
                    suc = ar.AsyncWaitHandle.WaitOne(timeout, true);
                    if (suc)
                    {
                        s.EndConnect(ar);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
                finally
                {
                    s.Close();
                }
            }

            wh.Stop();
            UnityEngine.Debug.LogFormat("Reachable : {0}:{1} {2} in {3}ms timeout {4}ms", addr, port, suc,
                wh.ElapsedMilliseconds, timeout);
            return suc;
        }

        public static IPAddress ParseIpAddressV6(string address, int port)
        {
            IPAddress addrOut;
            if (IPAddress.TryParse(address, out addrOut))
            {
                return addrOut;
            }

            IPAddress[] addrList = Dns.GetHostAddresses(address);

            if (addrList.Length == 1)
            {
                addrOut = addrList[0];
            }

            if (addrOut == null)
            {
                for (int i = 0; i < addrList.Length; i++)
                {
                    if (Reachable(addrList[i], port, 500))
                    {
                        addrOut = addrList[i];
                        break;
                    }
                }
            }

            if (addrOut == null)
            {
                for (int i = 0; i < addrList.Length; i++)
                {
                    if (Reachable(addrList[i], port, 2000))
                    {
                        addrOut = addrList[i];
                        break;
                    }
                }
            }

            if (addrOut == null)
            {
                for (int i = 0; i < addrList.Length; i++)
                {
                    if (Reachable(addrList[i], port, 30000))
                    {
                        addrOut = addrList[i];
                        break;
                    }
                }
            }

            return addrOut;
        }

        public static bool PortInUse(int port)
        {
            try
            {
                var tl = new TcpListener(IPAddress.Loopback, port);
                tl.Start();
                tl.Stop();
            }
            catch
            {
                return true;
            }
            return false;
        }
    }
}
