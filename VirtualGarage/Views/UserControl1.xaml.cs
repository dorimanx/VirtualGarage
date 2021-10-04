using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Torch.API;
using Torch.Views;

namespace VirtualGarage
{
    public partial class UserControlVirtualGarage : UserControl
    {
        private Plugin Plugin { get; }

        public UserControlVirtualGarage()
        {
            InitializeComponent();
        }

        public UserControlVirtualGarage(Plugin plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }
    }
}
