namespace SyncIO.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using SyncIO.ClientPlugin;
    using SyncIO.ServerPlugin;

    public class Plugin<T>
    {
        private const string IPacket = "IPacket";

        public List<Type> PluginCallbacks { get; private set; }

        public List<Type> PluginHosts { get; private set; }

        public List<Type> PacketTypes { get; set; }

        public Guid Id { get; }

        public Version Version { get; }

        public T PluginType { get; set; }

        public Plugin(Guid id, Version version)
        {
            Id = id;
            Version = version;
        }

        public static Plugin<T> Read(string filePath, Dictionary<Type, object> pluginHosts, Dictionary<Type, object> pluginCallbacks)
        {
            if (!File.Exists(filePath))
                return null;

            var data = File.ReadAllBytes(filePath);
            if (data == null)
                return null;

            return Read(data, pluginHosts, pluginCallbacks);
        }

        public static Plugin<T> Read(byte[] data, Dictionary<Type, object> pluginHosts, Dictionary<Type, object> pluginCallbacks)
        {
            //var serverPlugin = Assembly.Load(data);
            var type = IsValidPlugin
            (
                data,
                typeof(T) == typeof(ISyncIOServerPlugin) ? ServerPluginTypes.Callbacks : ClientPluginTypes.Callbacks,
                typeof(T) == typeof(ISyncIOServerPlugin) ? ServerPluginTypes.Hosts : ClientPluginTypes.Hosts
            );

            if (type == null)
            {
                Console.WriteLine("LoadPlugin: {0}", new Exception("Server plugin assembly does not meet type requirements."));
                return null;
            }

            var guid = type.Plugin.GUID;
            var version = type.Plugin.Assembly.GetName().Version;

            var list = new List<object>();
            var constructorInfo = type.Plugin.GetConstructors()[0];
            var parameters = constructorInfo.GetParameters();
            foreach (var pi in parameters)
            {
                foreach (var host in pluginHosts)
                {
                    if (pi.ParameterType == host.Key)
                    {
                        list.Add(host.Value);
                    }
                }
            }

            var plugin = (T)Activator.CreateInstance(type.Plugin, list.ToArray());
            /*
            foreach (var t in type.GetInterfaces())
            {
                //if (pluginCallbacks.ContainsKey(t))
                //{
                //    pluginCallbacks[t] = Convert.ChangeType(plugin, t);
                //}

                //if (typeof(IApp) == t)
                //    data.ClientHandlers.App = (IApp)objectValue;
                //else if (typeof(IUI) == t)
                //    data.ClientHandlers.UI = (IUI)objectValue;
                //else if (typeof(INetwork) == t)
                //    data.ClientHandlers.Network = (INetwork)objectValue;
            }

#pragma warning disable RECS0017 // Possible compare of value type with 'null'
            if (plugin == null)
#pragma warning restore RECS0017 // Possible compare of value type with 'null'
                throw new Exception("Failed to load ISyncIOServerPlugin plugin.");
            */
            return new Plugin<T>(guid, version)
            {
                PluginType = plugin,
                PacketTypes = type.PacketTypes,
                PluginHosts = pluginHosts.Keys.ToList(),
                PluginCallbacks = pluginCallbacks.Keys.ToList()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="dataTypes"></param>
        /// <param name="pluginHosts"></param>
        /// <returns></returns>
        private static ValidPlugin IsValidPlugin(byte[] plugin, Type[] dataTypes, Type[] pluginHosts)
        {
            var assembly = Assembly.Load(plugin);
            var types = assembly.GetTypes();
            var packets = new List<Type>();
            foreach (var type in types)
            {
                var packetInteface = type.GetInterfaces().FirstOrDefault(x => string.Compare(x.Name, IPacket, true) == 0);
                if (packetInteface != null)
                {
                    packets.Add(type);
                }
            }

            foreach (var type in types)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (Array.IndexOf(dataTypes, interfaceType) != -1)
                    {
                        return new ValidPlugin
                        {
                            Plugin = type,
                            PacketTypes = packets
                        };
                    }
                }

                var constructors = type.GetConstructors();
                if (constructors.Length == 1)
                {
                    var constructor = constructors[0];
                    foreach (var info in constructor.GetParameters())
                    {
                        if (Array.IndexOf(pluginHosts, info.ParameterType) != -1)
                        {
                            return new ValidPlugin
                            {
                                Plugin = type,
                                PacketTypes = packets
                            };
                        }
                    }
                }
            }

            return null;
        }
    }

    public class ValidPlugin
    {
        public Type Plugin { get; set; }

        public List<Type> PacketTypes { get; set; }
    }
}