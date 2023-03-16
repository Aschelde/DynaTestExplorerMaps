using CommunityToolkit.Mvvm.Messaging;
using DynaTestExplorerMaps.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DynaTestExplorerMaps.Interfaces
{
    internal interface IUserControlViewModel
    {
        ICommand GeoViewTappedCommand { get; set; }
    }
}
