﻿using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityLane.Editor.ConfigSandbox
{
    [Serializable]
    public class Workflow
    {
        public Dictionary<string, string> env;
        public Dictionary<string, Job> jobs;
    }

    [Serializable]
    public class Step
    {
        // github actions style
        public string uses;
        public Dictionary<string, object> with;
    }

    [Serializable]
    public class AndroidKeystoreData
    {
        public string path;
        public string passwd;
        public string alias;
        public string aliasPasswd;
    }


    [Serializable]
    public class Job
    {
        public string platform;
        public Step[] steps;
    }
}