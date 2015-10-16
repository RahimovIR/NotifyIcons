using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NotifyIcons
{
    //public enum NOTIFYITEM_PREFERENCE
    //{
    //    // In Windows UI: "Only show notifications."
    //    PREFERENCE_SHOW_WHEN_ACTIVE = 0,
    //    // In Windows UI: "Hide icon and notifications."
    //    PREFERENCE_SHOW_NEVER = 1,
    //    // In Windows UI: "Show icon and notifications."
    //    PREFERENCE_SHOW_ALWAYS = 2
    //};

    public struct Header
    {
        public uint cbSize;
        public uint unknown1;
        public uint unknown2;
        public uint count;
        public uint unknown3;
    }
    // The known values for NOTIFYITEM's dwPreference member.
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct NotifyItem
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 528)]
        public byte[] exe_path;

        public int preference;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1108)]
        public byte[] dontcare;

        public unsafe string ExePath
        {
            get
            {
                byte[] exeCopy = new byte[exe_path.Length];

                for (int i = 0; i < exe_path.Length; i++)
                {
                    byte byteToRot = exe_path[i];
                    byte rot = new byte();
                    if (byteToRot > 64 && byteToRot < 91)
                    {
                        rot = (byte)((byteToRot - 64 + 13) % 26 + 64);
                    }
                    else if (byteToRot > 96 && byteToRot < 123)
                    {
                        rot = (byte)((byteToRot - 96 + 13) % 26 + 96);
                    }
                    else
                    {
                        rot = byteToRot;
                    }
                    exeCopy[i] = rot;
                }

                fixed (byte* b = exeCopy)
                {
                    return Marshal.PtrToStringUni((IntPtr)b);
                }
            }
        }

    };


    class TrayHelper
    {
        static int headerSize = 20;
        static int blockSize = 1640;
        static public string trayNotifyPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\TrayNotify";


        static NotifyItem Byte2NotifyItem(byte[] bytes)
        {
            int size = Marshal.SizeOf(typeof(NotifyItem));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            NotifyItem result = (NotifyItem)Marshal.PtrToStructure(ptr, typeof(NotifyItem));

            return result;
        }

        static Header Byte2Header(byte[] bytes)
        {
            int size = Marshal.SizeOf(typeof(Header));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            Header result = (Header)Marshal.PtrToStructure(ptr, typeof(Header));

            return result;
        }

        public static int getOffSet(int id)
        {
            return headerSize + id * blockSize + 528;
        }

        public static NotifyItem[] getNotifyItems(byte[] bytes)
        {
            Header header = Byte2Header(bytes.Take(headerSize).ToArray());

            NotifyItem[] items = new NotifyItem[header.count];
            for (int i = 0; i < header.count; i++)
            {
                byte[] blockByte = bytes.Skip(headerSize + i * blockSize).Take(blockSize).ToArray();
                items[i] = Byte2NotifyItem(blockByte);
            }

            return items;
        }
    }
}
