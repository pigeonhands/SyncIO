namespace SyncIO.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public class PluginManager<T>
    {
        public const string DefaultPluginFolderName = "Plugins";

        #region Variables

        private readonly Dictionary<Guid, Plugin<T>> _plugins;
        private readonly Dictionary<Type, object> _pluginHosts;
        private readonly Dictionary<Type, object> _pluginCallbacks;

        #endregion

        #region Properties

        public IReadOnlyDictionary<Guid, Plugin<T>> Plugins => _plugins;

        public IReadOnlyDictionary<Type, object> PluginHosts => _pluginHosts;

        public IReadOnlyDictionary<Type, object> PluginCallbacks => _pluginCallbacks;

        #endregion

        #region Events

        public event EventHandler<PluginLoadedEventArgs<T>> PluginLoaded;

        private void OnPluginLoaded(Plugin<T> plugin)
        {
            PluginLoaded?.Invoke(this, new PluginLoadedEventArgs<T>(plugin));
        }

        #endregion

        #region Constructor

        public PluginManager(Dictionary<Type, object> pluginHosts, Dictionary<Type, object> pluginCallbacks)
        {
            _pluginHosts = pluginHosts;
            _pluginCallbacks = pluginCallbacks;
            _plugins = new Dictionary<Guid, Plugin<T>>();

            if (!Directory.Exists(DefaultPluginFolderName))
            {
                Directory.CreateDirectory(DefaultPluginFolderName);
            }
        }

        #endregion

        #region Public Methods

        public void LoadPlugin(string filePath)
        {
            var plugin = Plugin<T>.Read(filePath, _pluginHosts, _pluginCallbacks);
            if (plugin == null)
            {
                Console.WriteLine("Failed to load server plugin.");
                return;
            }

            LoadPlugin(plugin);
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadPlugins(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"Folder {folder} does not exist...");
                return;
            }

            var files = Directory.GetFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (string pluginPath in files)
            {
                LoadPlugin(pluginPath);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void LoadPlugin(Plugin<T> p)
        {
            //string name = $"{DefaultPluginFolderName}\\{p.ServerPlugin.Name}.xyz";

            if (IsAlreadyLoaded(p))
                return;

            OnPluginLoaded(p);

            if (_plugins.ContainsKey(p.Id))
            {
                Console.WriteLine($"Plugin {p.Id} is already loaded.");
                return;
            }
            //p.PluginHosts.AddRange(_pluginHosts.Keys);
            //p.PluginCallbacks.AddRange(_pluginCallbacks.Keys);
            _plugins.Add(p.Id, p);
            /*
            var argsList = new List<object>();

            var type = p.GetType();
            var constructorInfo = type.GetConstructors()[0];
            var parameters = constructorInfo.GetParameters();
            foreach (var pi in parameters)
            {
                if (typeof(I) == pi.ParameterType)
                    argsList.Add(new AppHost(data.Guid));
                else if (typeof(IDatabaseHost) == pi.ParameterType)
                    argsList.Add(new DatabaseHost());
            }

            object objectValue = Utils.GetObjectValue(Activator.CreateInstance(type, argsList.ToArray()));
            foreach (Type t in type.GetInterfaces())
            {
                if (typeof(IApp) == t)
                    data.ClientHandlers.App = (IApp)objectValue;
            }
            */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsAlreadyLoaded(Plugin<T> p)
        {
            if (!_plugins.ContainsKey(p.Id))
                return false;

            var isCompared = _plugins[p.Id].Equals(p);
            var isVersionIdentical = _plugins[p.Id].Version >= p.Version;

            if (isCompared || isVersionIdentical)
                return true;

            //if (MessageBox.Show
            //    (
            //        $"{p.ServerPlugin.Name} contains the same GUID as {_plugins[p.Id].ServerPlugin.Name}. Remove plugin and continue?",
            //        "Plugin Already Exists",
            //        MessageBoxButtons.YesNo,
            //        MessageBoxIcon.Warning
            //    ) == DialogResult.No)
            //{
            //    //continue;
            //    return true;
            //}

            //_plugins.Remove(p.Id);

            return false;
        }

        #endregion
    }

    public class PluginLoadedEventArgs<T> : EventArgs
    {
        public Plugin<T> Plugin { get; }

        public PluginLoadedEventArgs(Plugin<T> plugin)
        {
            Plugin = plugin;
        }
    }
}