using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.IO;


namespace TietokoneTiedotApp
{
    public partial class MainWindow : Window
    {
        private TietokoneTiedot tiedot;

        public MainWindow()
        {
            InitializeComponent();
            tiedot = new TietokoneTiedot();
            PäivitäTiedot();
        }

        private void PaivitaTiedot_Click(object sender, RoutedEventArgs e)
        {
            tiedot = new TietokoneTiedot(); // lataa uudet tiedot
            PäivitäTiedot();
        }

        private void PäivitäTiedot()
        {
            NäytäTiedot();
        }




        #region TiedotPanel-kortit
        private void NäytäTiedot()
        {
            TiedotPanel.Children.Clear();

            TiedotPanel.Children.Add(LuoTietokortti("Päivämäärä", new List<string> { DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") }, "/calendar.png"));
            TiedotPanel.Children.Add(LuoTietokortti("Käyttöjärjestelmä", tiedot.HaeKayttojarjestelma(), "Icons/system.png"));
            TiedotPanel.Children.Add(LuoTietokortti("BIOS", tiedot.HaeBIOS(), "Icons/cpu.png"));
            TiedotPanel.Children.Add(LuoTietokortti("Emolevy", tiedot.HaeEmolevy(), "Icons/motherboard.png"));
            TiedotPanel.Children.Add(LuoTietokortti("CPU", tiedot.HaeCPU(), "Icons/cpu.png")); 
            TiedotPanel.Children.Add(LuoTietokortti("Muisti", tiedot.HaeMuisti(), "Icons/ram.png")); 
            TiedotPanel.Children.Add(LuoTietokortti("Näyttö", tiedot.HaeNaytot(), "Icons/monitor.png"));
            TiedotPanel.Children.Add(LuoTietokortti("Akku", tiedot.HaeAkku(), "Icons/battery.png"));
            TiedotPanel.Children.Add(LuoTietokortti("Levyasetukset", tiedot.HaeKovalevyTiedot(), "Icons/harddisk.png"));
            TiedotPanel.Children.Add(LuoTietokortti("Verkko", tiedot.HaeVerkko(), "Icons/web.png"));
            TiedotPanel.Children.Add(LuoTietokortti("Järjestelmä", tiedot.HaeTietokoneJarjestelma(), "Icons/system.png"));
        }

        private Border LuoTietokortti(string otsikko, List<string> rivit, string ikoniPolku)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                //Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 45)),
                Background = new SolidColorBrush(System.Windows.Media.Colors.White),
                Padding = new Thickness(12),
                Margin = new Thickness(8),
                //BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(1),
                Width = 320
            };

            var stack = new StackPanel();
            var header = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            System.Windows.Controls.Image icon = new System.Windows.Controls.Image
            {
                Width = 24,
                Height = 24,
                Margin = new Thickness(0, 0, 8, 0)
            };

            try
            {
                var bmp = new BitmapImage(new Uri(ikoniPolku, UriKind.RelativeOrAbsolute));
                icon.Source = bmp;
            }
            catch { }

            var title = new TextBlock
            {
                Text = otsikko,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Gold,
                VerticalAlignment = VerticalAlignment.Center
            };

            header.Children.Add(icon);
            header.Children.Add(title);
            stack.Children.Add(header);

            foreach (var rivi in rivit)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = rivi,
                    Foreground = System.Windows.Media.Brushes.Black,
                    FontSize = 13,
                    Margin = new Thickness(0, 2, 0, 2),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            border.Child = stack;
            return border;
        }
        #endregion

        private string pastebinUrl;

        private async void LähetäPastebiniin_Click(object sender, RoutedEventArgs e)
        {
            var kaikkiTiedot = tiedot.HaeKaikkiTiedot();
            PastebinLinkTextBlock.Text = "Lähetetään...";
            PastebinLinkTextBlock.Visibility = Visibility.Visible;

            try
            {
                pastebinUrl = await PastebinUploader.LähetäRaportti(kaikkiTiedot, "TietokoneRaportti");
                PastebinLinkTextBlock.Text = pastebinUrl;
            }
            catch (Exception ex)
            {
                PastebinLinkTextBlock.Text = $"Virhe: {ex.Message}";
            }
        }

        private void BtnTallennaRaportti_Click(object sender, RoutedEventArgs e)
        {
            var tiedot = new TietokoneTiedot();
            string polku = HtmlRaportti.TallennaHtmlTiedosto(tiedot);

            System.Windows.MessageBox.Show($"Raportti tallennettu:\n{polku}");
        }

        private void PastebinLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PastebinLinkTextBlock.Text) &&
                Uri.IsWellFormedUriString(PastebinLinkTextBlock.Text, UriKind.Absolute))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = PastebinLinkTextBlock.Text,
                    UseShellExecute = true
                });
            }
        }
    }
}
