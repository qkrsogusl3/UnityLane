using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YamlDotNet.Serialization;

namespace UnityLane.Editor.ConfigSandbox
{
    public class WorkflowActionRunner
    {
        private readonly Dictionary<string, Type> _actionTypes;
        private readonly Type[] _registrations;

        public WorkflowActionRunner()
        {
            _actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(_ => _.GetName().Name == "UnityLane.Editor")
                .SelectMany(_ => { return _.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IAction))); })
                .ToDictionary(_ =>
                {
                    var attribute = _.GetCustomAttribute<ActionAttribute>();
                    if (attribute != null)
                    {
                        return attribute.Name;
                    }

                    return _.Name.PascalToKebabCase();
                });

            _registrations = AppDomain.CurrentDomain.GetAssemblies()
                .Where(_ => _.GetName().Name == "UnityLane.Editor")
                .SelectMany(_ => { return _.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IRegistration))); })
                .ToArray();
        }

        public void Registration(DeserializerBuilder builder)
        {
            foreach (var type in _registrations)
            {
                var registration = Activator.CreateInstance(type) as IRegistration;
                registration?.Register(builder);
            }
        }

        public void Run(WorkflowContext context, string name, Dictionary<string, object> with)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (!_actionTypes.TryGetValue(name, out var type)) return;

            try
            {
                var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

                var withKeys = with.Keys.ToArray();
                var withValues = with.Values.ToArray();
                var withTypes = with.Select(_ => _.Value.GetType()).ToArray();

                var parameters = new List<object>();
                ConstructorInfo matchedConstructor = null;
                var withDictionaryType = typeof(Dictionary<string, object>);
                foreach (var constructorInfo in constructors)
                {
                    var parameterInfos = constructorInfo.GetParameters();
                    parameters.Clear();

                    if (parameterInfos.Length == 1 &&
                        parameterInfos[0].ParameterType == withDictionaryType)
                    {
                        parameters.Add(with);
                    }
                    else
                    {
                        for (int i = 0; i < parameterInfos.Length; i++)
                        {
                            var info = parameterInfos[i];

                            if (!info.HasDefaultValue
                                && withTypes[i] == info.ParameterType
                                && withKeys[i] == info.Name.PascalToKebabCase())
                            {
                                parameters.Add(withValues[i]);
                            }
                            else if (with.TryGetValue(info.Name, out var value))
                            {
                                parameters.Add(value);
                            }
                            else
                            {
                                parameters.Add(Type.Missing);
                            }
                        }
                    }

                    matchedConstructor = constructorInfo;
                }

                if (matchedConstructor?.Invoke(parameters.ToArray()) is IAction instance)
                {
                    if ((instance.Targets & context.CurrentTargets.TargetPlatform) > 0)
                    {
                        instance.Execute(context);
                    }
                    else
                    {
                        Debug.LogWarning($"[Action] {name} is not support {context.CurrentTargets}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[{name}] failed! find constructor");
                Debug.Log(e);
            }
        }
    }
}