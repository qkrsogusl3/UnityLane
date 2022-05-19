﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityLane.Editor.ConfigSandbox
{
    public static class Env
    {
        private static readonly Dictionary<string, string> Variables = new();

        public static void Load()
        {
            Variables.Clear();
            ReadEnvironmentVariables(Variables);
            ReadEnvironmentFile(".env", Variables);

            if (!Application.isBatchMode)
            {
                ReadCommandLineArgs(Variables);
            }
        }
        

        private static void ReadCommandLineArgs(Dictionary<string, string> dictionary, bool isOverwrite = true)
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            for (var i = 0; i < commandLineArgs.Length; i++)
            {
                var arg = commandLineArgs[i];
                if (!arg.StartsWith("-"))
                    continue;

                var key = arg.Substring(1);
                var value = string.Empty;
                if (i + 1 < commandLineArgs.Length)
                {
                    var nextArg = commandLineArgs[i + 1];

                    if (!nextArg.StartsWith("-"))
                    {
                        value = nextArg;
                        i++;
                    }
                }

                Add(dictionary, key, value, isOverwrite);
            }
        }

        private static void ReadEnvironmentFile(string filePath, Dictionary<string, string> dictionary,
            bool isOverwrite = true)
        {
            var root = Directory.GetCurrentDirectory();
            var path = Path.Combine(root, filePath);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            using var reader = new StreamReader(path);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;

                var entry = line.Split('=');
                if (entry.Length != 2)
                    continue;

                Add(dictionary, entry[0], entry[1], isOverwrite);
            }
        }

        private static void ReadEnvironmentVariables(Dictionary<string, string> dictionary, bool isOverwrite = true)
        {
            var envs = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry entry in envs)
            {
                if (entry.Key is string key && entry.Value is string value)
                {
                    Add(dictionary, key, value, isOverwrite);
                }
            }
        }

        private static void Add(Dictionary<string, string> dictionary, string key, string value, bool isOverwrite)
        {
            if (isOverwrite || !dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
        }

        public static IReadOnlyDictionary<string, string> Read() => Variables;
        public static string GetValue(string key) => Variables[key];
        public static bool Contains(string key) => Variables.ContainsKey(key);

        public class FluentBuilder
        {
            private bool isOverwrite = true;

            public void WithoutOverwriteVariable()
            {
            }

            public void Load()
            {
            }

            public IReadOnlyDictionary<string, string> Read()
            {
                Load();
                return Env.Read();
            }
        }
    }
}