using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoHotkey.Interop;

namespace MidiToAHK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AHK.Run("F12::ExitApp");
            AHK.Run("F11::Pause");
        }
        void AHKCalibration()
        {
            AHK.Run($"F1::{AHK.GetPos("xSound","ySound")}");
            AHK.Run($"F2::{AHK.GetPos("xLess","yLess")}");
            AHK.Run($"F3::{AHK.GetPos("xAdd","yAdd")}");
            AHK.Run($"1::\n{AHK.MouseMov("xSound", "ySound")}\n{AHK.Click}");
            AHK.Run($"2::\n{AHK.MouseMov("xLess", "yLess")}\n{AHK.Click}");
            AHK.Run($"3::\n{AHK.MouseMov("xAdd", "yAdd")}\n{AHK.Click}");
        }

        private void Calibrate_Click(object sender, RoutedEventArgs e)
        {
            AHK.Run($"F1::{AHK.GetPos("xSound","ySound")}");
            AHK.Run($"F2::{AHK.GetPos("xLess","yLess")}");
            AHK.Run($"F3::{AHK.GetPos("xAdd","yAdd")}");
            AHK.Run($"MsgBox Press F1 when mouse is hovering over SOUND button. Press F2 when mouse is hovering over - (TRANSPOSE) button. Press F3 when mouse is hovering over + (TRANSPOSE) button.");
            WindowState = WindowState.Minimized;
        }
        private void LoadMidi_Click(object sender, RoutedEventArgs e) => AHKInstructions = Midi.ConvertToAHKInstructions(Midi.Get());
        string AHKInstructions = string.Empty;
        private void ExecuteMidi_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            AHK.Run(AHK.Wait(1000));

            AHK.Run(AHKInstructions);
        }
    }
}
