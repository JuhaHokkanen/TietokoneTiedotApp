using System;
using System.Collections.Generic;
using System.Management;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;


namespace TietokoneTiedotApp
{
    public class TietokoneTiedot
    {
        public List<string> HaeKaikkiTiedot()
        {
            var lista = new List<string>();
            lista.Add($"Päivämäärä: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            lista.AddRange(HaeKayttojarjestelma());
            lista.AddRange(HaeBIOS());
            lista.AddRange(HaeEmolevy());
            lista.AddRange(HaeCPU());
            lista.AddRange(HaeMuisti());
            lista.AddRange(HaeNaytot());
            lista.AddRange(HaeAkku());
            lista.AddRange(HaeKovalevyTiedot());
            lista.AddRange(HaeVerkko());
            lista.AddRange(HaeTietokoneJarjestelma());
            return lista;
        }


        public List<string> HaeTietokoneJarjestelma()
        {
            var lista = new List<string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject cs in searcher.Get())
                    {
                        string nimi = cs["Name"]?.ToString() ?? "Tuntematon";
                        string valmistaja = cs["Manufacturer"]?.ToString() ?? "Tuntematon";
                        string malli = cs["Model"]?.ToString() ?? "Tuntematon";
                        string kayttaja = cs["UserName"]?.ToString() ?? "Tuntematon";
                        string domain = cs["Domain"]?.ToString() ?? "Tuntematon";

                        lista.Add($"Tietokoneen nimi: {nimi}");
                        lista.Add($"Valmistaja: {valmistaja}");
                        lista.Add($"Malli: {malli}");
                        lista.Add($"Kirjautunut käyttäjä: {kayttaja}");
                        lista.Add($"Domain / työryhmä: {domain}");
                    }
                }
                if (lista.Count == 0)
                    lista.Add("Tietokonejärjestelmä: Ei löytynyt");
            }
            catch (Exception ex)
            {
                lista.Add($"Tietokonejärjestelmä: Tietoja ei saatavilla ({ex.Message})");
            }

            return lista;
        }

        public List<string> HaeBIOS()
        {
            var lista = new List<string>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
            {
                foreach (var bios in searcher.Get())
                {
                    string ver = bios["SMBIOSBIOSVersion"]?.ToString() ?? "";
                    string man = bios["Manufacturer"]?.ToString() ?? "";
                    string rel = bios["ReleaseDate"]?.ToString() ?? "";
                    lista.Add($"BIOS: {man} {ver}, Päiväys: {rel}");
                }
            }
            return lista;
        }

        public List<string> HaeVerkko()
        {
            var lista = new List<string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE"))
                {
                    int index = 1;
                    foreach (ManagementObject nic in searcher.Get())
                    {
                        string nimi = nic["Description"]?.ToString() ?? "Tuntematon";
                        string mac = nic["MACAddress"]?.ToString() ?? "Tuntematon";

                        string[] ipt = nic["IPAddress"] as string[];
                        string[] maskit = nic["IPSubnet"] as string[];
                        string[] gatewayt = nic["DefaultIPGateway"] as string[];
                        string dhcpEnabled = (nic["DHCPEnabled"] != null && (bool)nic["DHCPEnabled"]) ? "Kyllä" : "Ei";

                        string ip = (ipt != null && ipt.Length > 0) ? string.Join(", ", ipt) : "-";
                        string maski = (maskit != null && maskit.Length > 0) ? string.Join(", ", maskit) : "-";
                        string gateway = (gatewayt != null && gatewayt.Length > 0) ? string.Join(", ", gatewayt) : "-";

                        lista.Add(
                            $"Verkkosovitin #{index}\n" +
                            $"Nimi: {nimi}\n" +
                            $"MAC-osoite: {mac}\n" +
                            $"IP-osoite(t): {ip}\n" +
                            $"Aliverkon peite: {maski}\n" +
                            $"Yhdyskäytävä(t): {gateway}\n" +
                            $"DHCP käytössä: {dhcpEnabled}\n"
                        );

                        index++;
                    }

                    if (lista.Count == 0)
                        lista.Add("Verkko: IP-käytössä olevia sovittimia ei löytynyt");
                }
            }
            catch (Exception ex)
            {
                lista.Add($"Verkko: Tietoja ei saatavilla ({ex.Message})");
            }

            return lista;
        }




        public List<string> HaeEmolevy()
        {
            var lista = new List<string>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (var mb in searcher.Get())
                {
                    string man = mb["Manufacturer"]?.ToString() ?? "";
                    string prod = mb["Product"]?.ToString() ?? "";
                    lista.Add($"Emolevy: {man} {prod}");
                }
            }
            return lista;
        }

        public List<string> HaeNaytot()
        {
            var lista = new List<string>();

            // Haetaan GPU-tiedot
            var adapterit = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController")
                .Get()
                .Cast<ManagementObject>()
                .ToList();

            int index = 1;
            foreach (var screen in Screen.AllScreens)
            {
                var gpu = adapterit.FirstOrDefault();
                string gpuNimi = gpu?["Name"]?.ToString() ?? "Tuntematon";

                string rivi =
                    $"Näyttö #{index}\n" +
                    $"Nimi: {screen.DeviceName}\n" +
                    $"Resoluutio: {screen.Bounds.Width} x {screen.Bounds.Height}\n" +
                    $"Päänäyttö: {(screen.Primary ? "Kyllä" : "Ei")}\n" +
                    $"Näytönohjain: {gpuNimi}\n";


                lista.Add(rivi);
                index++;
            }

            return lista;
        }

        public List<string> HaeAkku()
        {
            var lista = new List<string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    var found = false;

                    foreach (var akku in searcher.Get())
                    {
                        found = true;
                        try
                        {
                            string name = akku["Name"]?.ToString() ?? "Akku";
                            string status = (akku["BatteryStatus"] != null) ? AkkuStatus((ushort)akku["BatteryStatus"]) : "Tuntematon";
                            string charge = akku["EstimatedChargeRemaining"]?.ToString() ?? "-";

                            lista.Add($"Akku: {name}, Tila: {status}, Varaus: {charge}%");
                        }
                        catch
                        {
                            lista.Add($"Akku: Tietoja ei saatavilla");
                        }
                    }

                    if (!found)
                    {
                        lista.Add("Akku: Ei löytynyt");
                    }
                }
            }
            catch
            {
                lista.Add("Akku: Haku epäonnistui");
            }

            return lista;
        }

        // Muuntaa BatteryStatus-koodin luettavaksi tekstiksi
        private string AkkuStatus(ushort code)
        {
            return code switch
            {
                1 => "Ei lataa",
                2 => "Lataa",
                3 => "Tyhjä",
                4 => "Täynnä",
                5 => "Alhainen",
                6 => "Kriittinen",
                7 => "Varautuminen",
                8 => "Ylikuormitettu",
                9 => "Laturissa, ei käytössä",
                10 => "Tuntematon",
                _ => "Tuntematon"
            };
        }


        public List<LevyInfo> HaeLevyKokoJaVapaa()
        {
            var lista = new List<LevyInfo>();
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3"))
            {
                foreach (var disk in searcher.Get())
                {
                    try
                    {
                        ulong koko = (ulong)disk["Size"];
                        ulong vapaa = (ulong)disk["FreeSpace"];
                        ulong kaytetty = koko - vapaa;

                        lista.Add(new LevyInfo
                        {
                            Nimi = disk["DeviceID"].ToString(),
                            KaytettyGB = Math.Round(kaytetty / 1024.0 / 1024.0 / 1024.0, 2),
                            VapaaGB = Math.Round(vapaa / 1024.0 / 1024.0 / 1024.0, 2)
                        });
                    }
                    catch
                    {
                        // jotkin virtuaaliasemat voivat aiheuttaa poikkeuksia
                    }
                }
            }
            return lista;
        }

        public List<string> HaeMuisti()
        {
            var lista = new List<string>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    int index = 1;
                    foreach (ManagementObject ram in searcher.Get())
                    {


                        string tyyppi = MemoryTypeToString(ram);
                        string nopeus = ram["Speed"]?.ToString() ?? "Tuntematon";

                        string kapasiteettiGB = "Tuntematon";
                        if (ram["Capacity"] != null)
                        {
                            try
                            {
                                ulong bytes = Convert.ToUInt64(ram["Capacity"]);
                                kapasiteettiGB = Math.Round(bytes / 1024.0 / 1024.0 / 1024.0, 2) + " GB";
                            }
                            catch { kapasiteettiGB = "Tuntematon"; }
                        }

                        lista.Add($"Muisti #{index}: Tyyppi: {tyyppi}, Kapasiteetti: {kapasiteettiGB}, Nopeus: {nopeus} MHz");
                        index++;
                    }

                    if (lista.Count == 0)
                        lista.Add("Muisti: Ei löytynyt");
                }
            }
            catch (Exception ex)
            {
                lista.Add($"Muisti: Tietoja ei saatavilla ({ex.Message})");
            }

            return lista;
        }

        private string MemoryTypeToString(ManagementObject ram)
        {
            if (ram["SMBIOSMemoryType"] != null)
            {
                ushort smbiosType = Convert.ToUInt16(ram["SMBIOSMemoryType"]);
                return smbiosType switch
                {
                    20 => "DDR",
                    21 => "DDR2",
                    24 => "DDR3",
                    26 => "DDR4",
                    30 => "LPDDR4", // Yleinen uusissa läppäreissä
                    34 => "DDR5",   // <-- TÄMÄ LISÄTTIIN
                    35 => "LPDDR5", // Uusimmat ohuet läppärit
                    0 => "Tuntematon", // Nolla tarkoittaa usein, että BIOS ei kerro tyyppiä
                    _ => "Muu / Tuntematon"
                };
            }

            if (ram["MemoryType"] != null)
            {
                ushort type = Convert.ToUInt16(ram["MemoryType"]);
                return type switch
                {
                    20 => "DDR",
                    21 => "DDR2",
                    24 => "DDR3",
                    26 => "DDR4",
                    _ => "Tuntematon"
                };
            }

            return "Tuntematon";
        }


        public List<string> HaeKayttojarjestelma()
        {
            var lista = new List<string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        string nimi = os["Caption"]?.ToString() ?? "Tuntematon";
                        string versio = os["Version"]?.ToString() ?? "Tuntematon";
                        string arkkitehtuuri = os["OSArchitecture"]?.ToString() ?? "Tuntematon";

                        lista.Add($"Käyttöjärjestelmä: {nimi}");
                        lista.Add($"Versio: {versio}");
                        lista.Add($"Arkkitehtuuri: {arkkitehtuuri}");
                    }
                }
                if (lista.Count == 0)
                    lista.Add("Käyttöjärjestelmä: Ei löytynyt");
            }
            catch (Exception ex)
            {
                lista.Add($"Käyttöjärjestelmä: Tietoja ei saatavilla ({ex.Message})");
            }

            return lista;
        }




        public List<string> HaeCPU()
        {
            var lista = new List<string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var cpu in searcher.Get())
                    {
                        string nimi = cpu["Name"]?.ToString() ?? "Tuntematon";
                        string coret = cpu["NumberOfCores"]?.ToString() ?? "0";
                        string threadit = cpu["NumberOfLogicalProcessors"]?.ToString() ?? "0";
                        lista.Add($"CPU: {nimi} \nYtimet: {coret}, Säikeet: {threadit}");
                    }
                }
            }
            catch (Exception ex)
            {
                lista.Add($"CPU: Tietoja ei saatavilla ({ex.Message})");
            }

            return lista;
        }
        public List<string> HaeKovalevyTiedot()
        {
            var tulokset = new List<string>();
            try
            {
                var asemat = new Dictionary<string, (ulong koko, ulong vapaa)>();

                using (var logicalSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3"))
                {
                    foreach (var d in logicalSearcher.Get())
                    {
                        string nimi = d["DeviceID"]?.ToString() ?? "";
                        if (string.IsNullOrEmpty(nimi)) continue;
                        ulong koko = Convert.ToUInt64(d["Size"]);
                        ulong vapaa = Convert.ToUInt64(d["FreeSpace"]);
                        asemat[nimi] = (koko, vapaa);
                    }
                }

                using (var diskSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject disk in diskSearcher.Get())
                    {
                        string model = disk["Model"]?.ToString() ?? "Tuntematon";
                        string serial = disk["SerialNumber"]?.ToString()?.Trim() ?? "Ei saatavilla";
                        string media = disk["MediaType"]?.ToString() ?? "Tuntematon";
                        string deviceId = disk["DeviceID"]?.ToString() ?? "";

                        tulokset.Add($"Levy: {model} ({media})");
                        tulokset.Add($"  Sarjanumero: {serial}");

                        // Liittyvät logiset asemat
                        using (var partitionSearcher = new ManagementObjectSearcher(
                            $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceId}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition"))
                        {
                            foreach (ManagementObject partition in partitionSearcher.Get())
                            {
                                using (var logicalSearcher2 = new ManagementObjectSearcher(
                                    $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass=Win32_LogicalDiskToPartition"))
                                {
                                    foreach (ManagementObject logical in logicalSearcher2.Get())
                                    {
                                        string asemakirjain = logical["DeviceID"]?.ToString() ?? "";
                                        if (asemakirjain != "" && asemat.ContainsKey(asemakirjain))
                                        {
                                            var (koko, vapaa) = asemat[asemakirjain];
                                            double kokoGB = Math.Round(koko / 1024.0 / 1024.0 / 1024.0, 2);
                                            double vapaaGB = Math.Round(vapaa / 1024.0 / 1024.0 / 1024.0, 2);
                                            double kaytettyGB = Math.Round(kokoGB - vapaaGB, 2);
                                            tulokset.Add($"  Asema: {asemakirjain} ({kaytettyGB:0.00} / {kokoGB:0.00} GB käytössä)");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tulokset.Add($"Virhe levyjen tietojen haussa: {ex.Message}");
            }

            return tulokset;
        }
    }
}
    


    

    public class LevyInfo
    {
        public string Nimi { get; set; }
        public double KaytettyGB { get; set; }
        public double VapaaGB { get; set; }
        public double KokoGB => KaytettyGB + VapaaGB;
    }


