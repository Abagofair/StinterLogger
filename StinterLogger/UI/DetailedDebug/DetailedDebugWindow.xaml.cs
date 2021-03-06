﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StinterLogger.UI.DetailedDebug
{
    /// <summary>
    /// Interaction logic for DetailedDebugWindow.xaml
    /// </summary>
    public partial class DetailedDebugWindow : Window
    {
        public DetailedDebugWindow(List<string> detailedContent)
        {
            InitializeComponent();

            foreach (var str in detailedContent)
            {
                this.log.AppendText(str + "\n");
            }
        }
    }
}
