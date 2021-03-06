﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NiseSeriko
{
    /// <summary>
    /// descript.txt を表すクラス
    /// </summary>
    public class DescriptText
    {
        public virtual string Path { get; set; }
        public virtual Dictionary<string, string> Values { get; protected set; } = new Dictionary<string, string>();
        public virtual DateTime LastWriteTime { get; protected set; }

        /// <summary>
        /// 指定したパスの descript.txt を読み込む
        /// </summary>
        public static DescriptText Load(string path)
        {
            var descript = new DescriptText() { Path = path };
            descript.Load();
            return descript;
        }

        /// <summary>
        /// descript.txt を読み込む
        /// </summary>
        public virtual void Load()
        {
            var charsetPattern = new Regex(@"charset\s*,\s*(.+?)\s*\z", RegexOptions.IgnoreCase);
            var entryPattern = new Regex(@"(.+?)\s*,\s*(.+?)\s*\z");

            // 既存の値はクリア
            Values.Clear();

            // ファイル更新日時セット
            LastWriteTime = File.GetLastWriteTime(Path);

            // まずはエンコーディングの判定を行うために、対象ファイルの内容を1行ずつ読み込む
            var sjis = Encoding.GetEncoding(932);
            var encoding = sjis;
            var preLines = File.ReadLines(Path, encoding: sjis);
            foreach (var line in preLines)
            {
                // charset行が見つかり、かつ有効なエンコーディング名である場合は、エンコーディングを設定してループ終了
                var matched = charsetPattern.Match(line);
                if (matched.Success)
                {
                    var charset = matched.Groups[1].Value;
                    try
                    {
                        encoding = Encoding.GetEncoding(charset);
                        break;
                    }
                    catch (ArgumentException)
                    {
                        // 読み込めない場合は設定しない (sjisのまま)
                    }
                }
            }

            // エンコーディングが確定したら、読み込みメイン処理
            var lines = File.ReadLines(Path, encoding: encoding);
            foreach (var line in lines)
            {
                var matched = entryPattern.Match(line);
                if (matched.Success)
                {
                    var key = matched.Groups[1].Value;
                    var value = matched.Groups[2].Value;
                    Values[key] = value;
                    //Debug.WriteLine(string.Format("  '{0}' : '{1}'", key, value));
                }
            }
        }

        /// <summary>
        /// 指定キーの値を取得。キーが存在しない場合はnullを返す
        /// </summary>
        public virtual string Get(string key)
        {
            if (Values.ContainsKey(key))
            {
                return Values[key];
            }
            else
            {
                return null;
            }
        }
    }
}
