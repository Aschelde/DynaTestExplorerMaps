﻿using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaTestExplorerMaps.Messages
{
    public class MeasurementTypeChangedMessage : ValueChangedMessage<string>
    {
        public MeasurementTypeChangedMessage(string value) : base(value)
        {
        }
    }
}