﻿namespace UnityLane.Editor.ConfigSandbox.Actions
{
    public class SemanticVersioning : IAction
    {
        private readonly VersionType _type;

        public enum VersionType
        {
            Major,
            Minor,
            Patch
        }

        public TargetPlatform Targets => TargetPlatform.All;


        public SemanticVersioning(VersionType type)
        {
            _type = type;
        }

        public void Execute(WorkflowContext context)
        {
            
        }
    }
}