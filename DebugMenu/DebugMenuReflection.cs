using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.DebugMenu
{
    public partial class DebugMenu
    {
        private Godot.GodotThread CacheThread;
        public static bool CacheBuildDone = false;

        #region REFLECTION LOOKUP

        private void ProcessMembers(Node node, Type nodeType, bool add, bool buildCache = false)
        {
            IList<MemberInfo> members;
            if (buildCache)
            {
                members = DebugReflectionUtil.GetMemberInfos(nodeType);
            }
            else
            {
                members = IncludeList[nodeType].MembersWithAnnotation;
            }

            foreach (MemberInfo info in members)
            {
                ProcessOnScreenDebugAttributeForMember(info, node, nodeType, add, buildCache);
            }
        }

        private void ProcessMethods(Node node, Type nodeType, bool add, bool buildCache = false)
        {
            IList<MethodInfo> methods;
            if (buildCache)
            {
                methods = DebugReflectionUtil.GetMethodInfos(nodeType);
            }
            else
            {
                methods = IncludeList[nodeType].MethodsWithAnnotation;
            }
            foreach (MethodInfo info in methods)
            {
                ProcessOnScreenDebugAttributeForMethod(info, node, nodeType, add, buildCache);
                ProcessActionAttributeForMethod(info, node, nodeType, add, buildCache);
            }
        }

        private void ProcessClass(Type nodeType)
        {
            DebugIncludeClass attr = DebugReflectionUtil.FindClassAttributeInNode<DebugIncludeClass>(nodeType);
            if (attr != null)
            {
                if (!IncludeList.ContainsKey(nodeType))
                {
                    IncludeList.Add(nodeType, new NodeDebugInfo());
                }
            }
        }

        private void ProcessOnScreenDebugAttributeForMember(MemberInfo info, Node node, Type nodeType, bool add, bool buildCache = false)
        {
            OnScreenDebug attr = DebugReflectionUtil.GetCustomAttribute<OnScreenDebug>(info) as OnScreenDebug;
            if (attr != null)
            {
                if (buildCache)
                {
                    AddMemberInfoToNode(nodeType, info);
                    return;
                }
                if (add)
                {
                    // Add
                    OnScreenDebugManager.AddOnScreenDebugInfo(ReplaceName(attr.DebugCategory, node),
                        ReplaceName(attr.Name, node), () => GetValueOfMember(info, node), attr.Color);
                }
                else
                {
                    // Remove
                    OnScreenDebugManager.RemoveOnScreenDebugInfo(ReplaceName(attr.DebugCategory, node),
                        ReplaceName(attr.Name, node));
                }
            }
        }

        private string GetValueOfMember(MemberInfo info, Node node)
        {
            object value = info.GetValue(node);
            if (value == null)
            {
                return "null";
            }
            return value.ToString();
        }

        private void ProcessOnScreenDebugAttributeForMethod(MethodInfo info, Node node, Type nodeType, bool add, bool buildCache = false)
        {
            OnScreenDebug attr = DebugReflectionUtil.GetCustomAttribute<OnScreenDebug>(info) as OnScreenDebug;
            if (attr != null)
            {
                if (buildCache)
                {
                    AddMethodInfoToNode(nodeType, info);
                }
                if (add)
                {
                    // Add
                    OnScreenDebugManager.AddOnScreenDebugInfo(ReplaceName(attr.DebugCategory, node),
                        ReplaceName(attr.Name, node), () => GetValueOfMethod(info, node), attr.Color);
                }
                else
                {
                    // Remove
                    OnScreenDebugManager.RemoveOnScreenDebugInfo(ReplaceName(attr.DebugCategory, node),
                        ReplaceName(attr.Name, node));
                }
            }
        }

        private string GetValueOfMethod(MethodInfo info, Node node)
        {
            object value = info.Invoke(node, null);
            if (value == null)
            {
                return "null";
            }
            return value.ToString();
        }

        private void ProcessActionAttributeForMethod(MethodInfo info, Node node, Type nodeType, bool add, bool buildCache = false)
        {
            if (!add && !buildCache)
            {
                DebugButtonMenu.RemoveButtonsForNode(node);
                return;
            }

            // Simple menu entries
            object[] attrs = DebugReflectionUtil.GetCustomAttributes<DebugMenuEntry>(info, true);
            if (attrs.Length != 0)
            {
                if (buildCache)
                {
                    AddMethodInfoToNode(nodeType, info);
                    return;
                }
                List<DebugMenuEntry> dialogEntries = new List<DebugMenuEntry>();

                foreach (object objAttr in attrs)
                {
                    if (objAttr is DebugMenuEntrySimple)
                    {
                        DebugMenuEntrySimple attr = (DebugMenuEntrySimple)objAttr;
                        DebugButtonMenu.AddSimpleButton(ReplaceName(attr.ButtonText, node), attr,
                                 node, (btn, parameters) => info.Invoke(node, parameters));
                    }
                    else if (objAttr is DebugMenuEntryToggle)
                    {
                        DebugMenuEntryToggle attr = (DebugMenuEntryToggle)objAttr;
                        DebugButtonMenu.AddToggleButton(ReplaceName(attr.ButtonText, node), attr, node,
                        (btn, parameters) =>
                        {
                            bool newValue = (bool)info.Invoke(node, new object[] { !attr.CallGetValueMethod(node) });
                            btn.Modulate = newValue ? Colors.Green : Colors.Red;
                        });
                    }
                    else if (objAttr is DebugMenuEntryCallback)
                    {
                        DebugMenuEntryCallback attr = (DebugMenuEntryCallback)objAttr;
                        DebugMenuAction actRefresh = (btn, parameters) =>
                        {
                            try
                            {
                                btn.Text = (string)node.GetType().GetMethod(attr.TextCallback).Invoke(node, null);
                                btn.Modulate = (Color)node.GetType().GetMethod(attr.ColorCallback).Invoke(node, null);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Failed to refresh CallbackButton", ex);
                            }
                        };
                        DebugMenuAction actCall = (btn, parameters) =>
                        {
                            info.Invoke(node, parameters);
                            actRefresh.Invoke(btn, parameters);
                        };
                        DebugButtonMenu.AddCallbackButton(attr, node, actCall, actRefresh);
                    }
                    else if (objAttr is DebugMenuDialogField || objAttr is DebugMenuDialogValidator)
                    {
                        dialogEntries.Add((DebugMenuEntry)objAttr);
                    }
                }
                dialogEntries.ForEach(entry => DebugButtonMenu.AddDialogEntry(node, entry));
            }

            attrs = DebugReflectionUtil.GetCustomAttributes<DebugCategoryColumn>(info, true);
            if (attrs.Length != 0)
            {
                if (buildCache)
                {
                    AddMethodInfoToNode(nodeType, info);
                    return;
                }
                foreach (object objAttr in attrs)
                {
                    DebugCategoryColumn attr = (DebugCategoryColumn)objAttr;
                    DebugButtonMenu.SetCategoryColumn(attr.Category, attr.Column);
                }
            }

        }

        #endregion


        #region REFLECTION CACHE

        /// <summary>
        /// This pre-builds our reflection cache on load, thus we dont get stutters
        /// </summary>
        private long BuildReflectionCache()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (DebugReflectionUtil.IsInGodotOrSystemNamespace(type))
                {
                    continue;
                }
                BuildReflectionCacheForType(type);
            }
            watch.Stop();
            CallDeferred(nameof(DisposeReflectionCacheThread));
            return watch.ElapsedMilliseconds;
        }

        private void DisposeReflectionCacheThread()
        {
            // Disposes of the thread so it does not keep running
            object result = CacheThread.WaitToFinish();
            CacheBuildDone = true;
            Logger.Info($"*THREAD FINISHED* - Building reflection cache took {result.ToString()} ms");
        }

        protected void BuildReflectionCacheForType(Type type)
        {
            ProcessClass(type);
            if (IncludeList.ContainsKey(type))
            {
                ProcessMembers(null, type, true, true);
                ProcessMethods(null, type, true, true);
            }

        }

        #endregion
    }
}