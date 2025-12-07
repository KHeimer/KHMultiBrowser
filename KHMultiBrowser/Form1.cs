/*
Pseudocode (ausführlich, Schritt-für-Schritt):

1. InitForm:
   - Erstelle ein TableLayoutPanel 3x3 und fülle es mit BrowserWithAddressBox-Instanzen.
   - Referenz auf das TableLayoutPanel speichern.
   - Controls dem Form hinzufügen.
   - Asynchron den gespeicherten Zustand (Adressen + Zoom) laden, ohne den UI-Thread zu blockieren:
     - Starte LoadBrowserAddressesAsync als Fire-and-forget (z.B. _ = LoadBrowserAddressesAsync()).

2. LoadBrowserAddressesAsync:
   - Wenn die Datei nicht existiert oder leer ist, beenden.
   - Versuche, das neue Format zu deserialisieren: Dictionary<string, BrowserState>.
     - Für jede BrowserWithAddressBox im TableLayoutPanel:
       - Wenn Eintrag vorhanden:
         - Wenn Address gesetzt ist, Address zuweisen (dies löst intern Navigation aus).
         - Versuche asynchron SetZoomAsync(factor) aufzurufen und darauf zu warten.
           - Fehler beim Setzen des Zooms ignorieren.
     - Rückkehr (Fertig).
   - Falls neues Format nicht passt, Fallback auf altes Format Dictionary<string, string> und Adressen setzen.
   - Alle Fehler fangen und ignorieren, damit Start nicht abstürzt.

3. SaveBrowserAddresses bleibt unverändert: Beim Schließen die Adressen und Zooms sammeln.
   - Für Zoom weiterhin GetZoom() verwenden (sofern verfügbar).

Hinweis: Die asynchrone Methode benutzt await ctrl.SetZoomAsync(saved.Zoom) für jedes Pane.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using MyApp.Controls;

namespace KHMultiBrowser
{
    public partial class Form1 : Form
    {
        private TableLayoutPanel tableLayoutPanelBrowsers;
        private readonly string stateFilePath;

        // Hilfsklasse für gespeicherten Zustand pro Pane
        private class BrowserState
        {
            public string Address { get; set; } = string.Empty;
            public double Zoom { get; set; }
        }

        public Form1()
        {
            InitializeComponent();

            // Speicherpfad in %AppData%\KHMultiBrowser\browsers.json
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "KHMultiBrowser");
            stateFilePath = Path.Combine(dir, "browsers.json");

            InitForm();

            // Beim Schließen speichern
            this.FormClosing += Form1_FormClosing;
        }

        private void InitForm()
        {
            // Moderneres Form- und Layout-Styling
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.FromArgb(245, 245, 247);
            this.Padding = new Padding(8);
            this.DoubleBuffered = true;

            // TableLayoutPanel anlegen
            var table = new TableLayoutPanel
            {
                Name = "tableLayoutPanelBrowsers",
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(12),
                Margin = new Padding(0),
                BackColor = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // Referenz speichern, damit andere Methoden darauf zugreifen können
            this.tableLayoutPanelBrowsers = table;

            // Spalten- und Zeilenstile auf Prozentwerte setzen (gleichmäßig)
            for (int i = 0; i < 3; i++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3f));
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 3f));
            }

            // 3x3 BrowserWithAddressBox hinzufügen
            int index = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    var browser = new BrowserWithAddressBox
                    {
                        Name = $"browserWithAddressBox_{row}_{col}",
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0)
                    };

                    // Optional: Default-Startadresse
                    browser.Address = $"https://example.com/?pane={index + 1}";

                    // Card-like panel für moderneres Aussehen
                    var card = new CardPanel
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Padding(6),
                        Margin = new Padding(6),
                        BackColor = Color.White,
                        CornerRadius = 8,
                        ShadowDepth = 6
                    };

                    browser.Dock = DockStyle.Fill;
                    card.Controls.Add(browser);

                    table.Controls.Add(card, col, row);
                    index++;
                }
            }

            // TableLayoutPanel dem Form hinzufügen (unter Beibehaltung vorhandener Controls)
            this.Controls.Add(table);
            table.BringToFront();

            // Nach dem Anlegen versuchen, gespeicherte Adressen und Zooms asynchron zu laden
            _ = LoadBrowserAddressesAsync(); // Fire-and-forget, Fehler intern behandelt
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SaveBrowserAddresses();
        }

        private async Task LoadBrowserAddressesAsync()
        {
            try
            {
                if (!File.Exists(stateFilePath))
                    return;

                var json = File.ReadAllText(stateFilePath);
                if (string.IsNullOrWhiteSpace(json))
                    return;

                // Versuche neues Format (Address + Zoom)
                Dictionary<string, BrowserState>? dictComplex = null;
                try
                {
                    dictComplex = JsonSerializer.Deserialize<Dictionary<string, BrowserState>>(json);
                }
                catch
                {
                    dictComplex = null;
                }

                if (dictComplex != null)
                {
                    foreach (var ctrl in EnumerateBrowsers())
                    {
                        if (dictComplex.TryGetValue(ctrl.Name, out var saved) && saved != null)
                        {
                            if (!string.IsNullOrWhiteSpace(saved.Address))
                            {
                                // Adresse zuweisen; BrowserWithAddressBox sollte intern beim Set den Navigationsaufruf behandeln
                                ctrl.Address = saved.Address;
                            }

                            try
                            {
                                // Verwende direkt die asynchrone Public-Methode SetZoomAsync
                                await ctrl.SetZoomAsync(saved.Zoom).ConfigureAwait(true);
                            }
                            catch
                            {
                                // Fehler beim Setzen des Zooms ignorieren
                            }
                        }
                    }

                    return;
                }

                // Fallback: altes Format (nur Address dictionary)
                try
                {
                    var dictOld = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                    foreach (var ctrl in EnumerateBrowsers())
                    {
                        if (dictOld.TryGetValue(ctrl.Name, out var savedAddress) && !string.IsNullOrWhiteSpace(savedAddress))
                        {
                            ctrl.Address = savedAddress;
                        }
                    }
                }
                catch
                {
                    // Fehler beim Fallback ignorieren
                }
            }
            catch
            {
                // Fehler beim Laden ignorieren (kein Absturz beim Start)
            }
        }

        private void SaveBrowserAddresses()
        {
            try
            {
                var dict = new Dictionary<string, BrowserState>();

                foreach (var ctrl in EnumerateBrowsers())
                {
                    double zoom = 0;
                    try
                    {
                        // Benutzeranforderung: Benutze die Methode GetZoom zum Auslesen des Zoomfaktors
                        zoom = ctrl.GetZoom();
                    }
                    catch
                    {
                        // Falls GetZoom nicht verfügbar oder fehlschlägt, Zoom = 0 belassen
                    }

                    // Name als Schlüssel, Address und Zoom als Wert
                    dict[ctrl.Name] = new BrowserState
                    {
                        Address = ctrl.Address ?? string.Empty,
                        Zoom = zoom
                    };
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(dict, options);

                var dir = Path.GetDirectoryName(stateFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(stateFilePath, json);
            }
            catch
            {
                // Fehler beim Speichern ignorieren (z.B. keine Schreibrechte)
            }
        }

        // Helper: Enumerate BrowserWithAddressBox controls that may be direct children or nested inside CardPanel
        private IEnumerable<MyApp.Controls.BrowserWithAddressBox> EnumerateBrowsers()
        {
            if (tableLayoutPanelBrowsers == null)
                yield break;

            foreach (Control c in tableLayoutPanelBrowsers.Controls)
            {
                if (c is MyApp.Controls.BrowserWithAddressBox bw)
                {
                    yield return bw;
                }
                else
                {
                    // look for nested BrowserWithAddressBox
                    foreach (var nested in c.Controls.OfType<MyApp.Controls.BrowserWithAddressBox>())
                        yield return nested;
                }
            }
        }
    }
}