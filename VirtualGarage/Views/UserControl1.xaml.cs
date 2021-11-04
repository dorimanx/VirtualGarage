using System.Windows.Controls;

 namespace VirtualGarage
{
    public partial class UserControlVirtualGarage : UserControl
    {
        private Plugin Plugin { get; }

        public UserControlVirtualGarage()
        {
            InitializeComponent();
            MainFilteredGrid.DataContext = Plugin.Instance.Config;
        }

        public UserControlVirtualGarage(Plugin plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }
    }
}
