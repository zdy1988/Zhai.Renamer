using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using Zhai.Renamer.Core;
using Zhai.Renamer.Models;
using Zhai.Renamer.Properties;

namespace Zhai.Renamer
{
    internal static class RenamerSettings
    {
        internal static string ProfileDataPath { get; } = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Profiles");

        internal static bool Load()
        {
            try
            {
                if (Settings.Default.IsAddToWindowsContextMenu && !IsRegistryRightClickContextMenu())
                {
                    RegistryRightClickContextMenu(false);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return false;
            }

            return true;
        }

        internal static Dictionary<String, String[]> GetCounters()
        {
            Dictionary<String, String[]> counters;

            try
            {
                string json = Settings.Default.Counters;

                if (string.IsNullOrEmpty(json))
                { 
                    throw new ArgumentNullException(nameof(json));
                }

                counters = JsonConvert.DeserializeObject<Dictionary<String, String[]>>(json);
            }
            catch
            {
                // 读取配置文件错误则创建默认文件
                counters = new Dictionary<string, string[]>
                {
                    { "P", new string[] { "jpg", "jpeg", "png", "gif", "bmp", "ico", "tiff", "wmf" } },
                    { "V", new string[] { "asf", "avi", "rmvb", "divx", "dv", "flv", "gxf", "m1v", "m2v", "m2ts", "m4v", "mkv", "mov", "mp2", "mp4", "mpeg", "mpeg1", "mpeg2", "mpeg4", "mpg", "mts", "mxf", "ogg", "ogm", "ps", "ts", "vob", "wmv", "a52", "aac", "ac3", "dts", "flac", "m4a", "m4p", "mka", "mod", "mp1", "mp2", "mp3", "ogg" } }
                };

                Settings.Default.Counters = JsonConvert.SerializeObject(counters);

                Settings.Default.Save();
            }

            return counters;
        }

        internal static List<String[]> GetRegexFilters()
        {
            List<String[]> filters;

            try
            {
                string json = Settings.Default.RegexFilters;

                if (string.IsNullOrEmpty(json))
                {
                    throw new ArgumentNullException(nameof(json));
                }

                filters = JsonConvert.DeserializeObject<List<String[]>>(json);
            }
            catch
            {
                filters = JsonConvert.DeserializeObject<List<String[]>>(
                    @"[
	                    [ '包含空格符', '\\s' ],
	                    [ '包含整数', '^-{0,1}\\d+' ],
	                    [ '包含年份 (1900-2099)', '((19|20)\\d{2})' ],

	                    [ 'AVI (*.avi)', '\\.avi' ],
	                    [ 'JPEG (*.jpg;*.jpeg)', '\\.(jpg|jpeg)' ],
	                    [ 'MKV (*.mkv)', '\\.mkv' ],
	                    [ 'MP4 (*.mp4)', '\\.mp4' ],
	                    [ 'PNG (*.png)', '\\.png' ],
	                    [ 'SRT (*.srt)', '\\.srt' ]
                    ]");


                Settings.Default.RegexFilters = JsonConvert.SerializeObject(filters);

                Settings.Default.Save();
            }

            return filters;
        }

        internal static Dictionary<String, List<RenameModifier>> GetQuickModifiers()
        {
            Dictionary<String, List<RenameModifier>> modifiers;

            try
            {
                string json = Settings.Default.Modifiers;

                if (string.IsNullOrEmpty(json))
                {
                    throw new ArgumentNullException(nameof(json));
                }

                modifiers = JsonConvert.DeserializeObject<Dictionary<String, List<RenameModifier>>>(json);
            }
            catch
            {
                modifiers = new Dictionary<string, List<RenameModifier>>
                {
                    {
                        "No.编号",
                        new List<RenameModifier> {
                            new RenameModifier{ ModifierKind = ModifierKind.AppendBefore, FirstArgument=" ", SecondArgument=""},
                            new RenameModifier{ ModifierKind = ModifierKind.AddNumbering, FirstArgument="", SecondArgument=""},
                            new RenameModifier{ ModifierKind = ModifierKind.AppendBefore, FirstArgument="No.", SecondArgument=""},
                        }
                    },

                    {
                        "Vol.编号",
                        new List<RenameModifier> {
                            new RenameModifier{ ModifierKind = ModifierKind.AppendBefore, FirstArgument=" ", SecondArgument=""},
                            new RenameModifier{ ModifierKind = ModifierKind.AddNumbering, FirstArgument="", SecondArgument=""},
                            new RenameModifier{ ModifierKind = ModifierKind.AppendBefore, FirstArgument="Vol.", SecondArgument=""},
                        }
                    },

                    {
                        "文件名.7z",
                        new List<RenameModifier> {
                            new RenameModifier{ ModifierKind = ModifierKind.AppendAfter, FirstArgument=".7z", SecondArgument=""},
                        }
                    },

                    {
                        "文件名.rar",
                        new List<RenameModifier> {
                            new RenameModifier{ ModifierKind = ModifierKind.AppendAfter, FirstArgument=".rar", SecondArgument=""},
                        }
                    },
                };

                Settings.Default.Modifiers = JsonConvert.SerializeObject(modifiers);

                Settings.Default.Save();
            }

            return modifiers;
        }

        #region Registry

        internal static string RegistryKey = "Zhai.Renamer";

        internal static string RegistryMenu = "使用 Zhai.Renamer 进行高级重命名";

        internal static bool RegistryRightClickContextMenu(bool isShowMessage = true)
        {
            try
            {
                //注册到所有文件
                RegisterToRegistry(@"*\shell");

                //注册到所有目录
                RegisterToRegistry(@"directory\shell");

                if (isShowMessage)
                {
                    MessageBox.Show("添加右键快捷菜单成功！");
                }

                return true;
            }
            catch (System.Security.SecurityException)
            {
                if (isShowMessage)
                {
                    MessageBox.Show("权限不足！请使用管理员的身份重新打开程序！");
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return false;
            }
        }

        private static void RegisterToRegistry(string rootName)
        {
            //注册到所有文件
            RegistryKey shell = Registry.ClassesRoot.OpenSubKey(rootName, true);

            RegistryKey custom;

            if (shell.GetSubKeyNames().Any(key => key == RegistryKey))
            {
                custom = shell.OpenSubKey(RegistryKey);
            }
            else
            {
                custom = shell.CreateSubKey(RegistryKey);
            }

            // 设置名称
            custom.SetValue(string.Empty, RegistryMenu);
            // 设置Icon，可以是图片路径
            custom.SetValue("icon", Process.GetCurrentProcess().MainModule.FileName);
            // 设置选择范围：
            // Single：右击单个文件
            // Document：最多选15个文件
            // Player：看文档，相当于没限制
            custom.SetValue("MultiSelectModel", "Player");

            RegistryKey cmd;

            if (custom.GetValueNames().Any(name => name == "command"))
            {
                cmd = custom.OpenSubKey("command");
            }
            else
            {
                cmd = custom.CreateSubKey("command");
            }

            //Assembly.GetExecutingAssembly().Location 是本程序自身的路径
            //%1 是传入打开的文件路径
            cmd.SetValue(string.Empty, Process.GetCurrentProcess().MainModule.FileName + " %1");

            cmd.Close();
            custom.Close();
            shell.Close();
        }

        internal static bool UnRegistryRightClickContextMenu(bool isShowMessage = true)
        {
            try
            {
                RegistryKey shell = Registry.ClassesRoot.OpenSubKey(@"*\shell", true);
                shell.DeleteSubKeyTree(RegistryKey);
                shell.Close();

                RegistryKey shell2 = Registry.ClassesRoot.OpenSubKey(@"directory\shell", true);
                shell2.DeleteSubKeyTree(RegistryKey);
                shell2.Close();

                if (isShowMessage)
                {
                    MessageBox.Show("移除右键快捷菜单成功！");
                }

                return true;
            }
            catch (System.Security.SecurityException)
            {
                if (isShowMessage)
                {
                    MessageBox.Show("权限不足！请使用管理员的身份重新打开程序！");
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                return false;
            }
        }

        internal static bool IsRegistryRightClickContextMenu()
        {
            bool isRegistry = false;

            try
            {
                RegistryKey shell = Registry.ClassesRoot.OpenSubKey(@"*\shell", true);

                isRegistry = shell.GetSubKeyNames().Any(key => key == RegistryKey);

                shell.Close();

                RegistryKey shell2 = Registry.ClassesRoot.OpenSubKey(@"directory\shell", true);

                isRegistry = shell2.GetSubKeyNames().Any(key => key == RegistryKey);

                shell2.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return isRegistry;
        }

        #endregion
    }
}
