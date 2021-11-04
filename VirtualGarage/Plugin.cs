using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using NLog;
using Sandbox.Game.Entities.Blocks;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;

namespace VirtualGarage
{
    public class Plugin : TorchPluginBase, IWpfPlugin
    {
        public static readonly Logger Log = LogManager.GetLogger("VirtualGarage");
        public static readonly Random RandomPos = new Random();
        private TorchSessionManager _sessionManager;
        private Persistent<Config> _configPersistent;

        public UserControlVirtualGarage Control;
        public static Plugin Instance { get; private set; }
        public Config Config => _configPersistent?.Data;
        public void Save() => _configPersistent.Save();

        public static MethodInfo m_myProgrammableBlockKillProgramm;

        public override void Init(ITorchBase torch)
        {
            SetupConfig();
            base.Init(torch);
            Instance = this;

            _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (_sessionManager == null)
                return;

            _sessionManager.SessionStateChanged += SessionManager_SessionStateChanged;
            m_myProgrammableBlockKillProgramm = typeof(MyProgrammableBlock).GetMethod("OnProgramTermination", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void SessionManager_SessionStateChanged(ITorchSession session, TorchSessionState newState)
        {
            if (newState == TorchSessionState.Unloading)
                return;

            if (newState != TorchSessionState.Loaded)
                return;

            VirtualGarageOldGridProcessor.OldGridProcessor.OnLoaded();
        }

        public void LoadConfig()
        {
            if (_configPersistent?.Data != null)
                _configPersistent = Persistent<Config>.Load(Path.Combine(StoragePath, "VirtualGarage.cfg"));
        }

        private void SetupConfig()
        {
            try
            {
                _configPersistent = Persistent<Config>.Load(Path.Combine(StoragePath, "VirtualGarage.cfg"));
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
            }
            if (_configPersistent?.Data != null)
                return;

            Log.Info("Create Default Config, because none was found!");
            _configPersistent = new Persistent<Config>(Path.Combine(StoragePath, "VirtualGarage.cfg"), new Config());
            _configPersistent.Save(null);
        }

        public UserControl GetControl()
        {
            if (Control == null)
                Control = new UserControlVirtualGarage(this);

            return Control;
        }
        public override void Dispose()
        {
            if (_sessionManager != null)
            {
                _sessionManager.SessionStateChanged -= SessionManager_SessionStateChanged;
            }
            _sessionManager = null;
        }
    }
}