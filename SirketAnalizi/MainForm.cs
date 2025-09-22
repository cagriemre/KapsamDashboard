using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KapsamDashboard.UI
{
    public partial class MainForm : Form
    {
        // Controls
        private DateTimePicker startDatePicker;
        private DateTimePicker endDatePicker;
        private Button refreshButton;
        private Button tab2Button;

        // Tab 1 Controls
        private Chart columnChart;
        private Chart patientChart;

        private DataGridView dataGridView1;
        private DataGridView dataGridView2;

        // Tab 2 Controls
        private Chart overdoseChart;

        private Label lblTitle;
        private Panel topPanel;
        private Chart pieChart;
        private TabControl mainTabControl;
        private TableLayoutPanel mainLayout;
        private TableLayoutPanel bottomLayout;
        private TableLayoutPanel tab2Layout;
        private DataGridView dataGridView3;
        private DataGridView dataGridView4;
        private TextBox manualCountTextBox;
        private Button updateManualButton;
        private TextBox semiAutoCountTextBox;
        private Button updateSemiAutoButton;

        public MainForm()
        {
            InitializeFormProperties();
            InitializeCustomComponents();
            LoadData();
        }

        private void InitializeFormProperties()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "Kapsam Dashboard - Anlık Veri";
            this.Size = new Size(1600, 1000);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(25, 42, 86);
            this.Icon = SystemIcons.Application;

            this.ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            // Top Panel
            topPanel = new Panel
            {
                Height = 80,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(25, 42, 86),
                Padding = new Padding(20, 10, 20, 10)
            };
            this.Controls.Add(topPanel);

            // Title
            lblTitle = new Label
            {
                Text = "Kapsam Dashboard",
                Font = new Font("Franklin Gothic", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 22)
            };
            topPanel.Controls.Add(lblTitle);

            // Date Pickers and Refresh Button
            var datePanel = new Panel
            {
                Size = new Size(650, 60),
                Location = new Point(topPanel.Width - 670, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            topPanel.Controls.Add(datePanel);

            var lblStart = new Label
            {
                Text = "Başlangıç:",
                Location = new Point(0, 15),
                Size = new Size(70, 25),
                ForeColor = Color.White,
                Font = new Font("Franklin Gothic", 9, FontStyle.Bold)
            };
            datePanel.Controls.Add(lblStart);

            startDatePicker = new DateTimePicker
            {
                Location = new Point(75, 12),
                Size = new Size(140, 50),
                Value = new DateTime(2025, 3, 7),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Franklin Gothic", 11, FontStyle.Bold)
            };
            datePanel.Controls.Add(startDatePicker);

            var lblEnd = new Label
            {
                Text = "Bitiş:",
                Location = new Point(225, 15),
                Size = new Size(40, 25),
                ForeColor = Color.White,
                Font = new Font("Franklin Gothic", 9, FontStyle.Bold)
            };
            datePanel.Controls.Add(lblEnd);

            endDatePicker = new DateTimePicker
            {
                Location = new Point(270, 12),
                Size = new Size(140, 50),
                Value = new DateTime(2025, 8, 29),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Franklin Gothic", 11, FontStyle.Bold)
            };
            datePanel.Controls.Add(endDatePicker);

            refreshButton = new Button
            {
                Text = "🔄 Yenile",
                Location = new Point(420, 12),
                Size = new Size(90, 30),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += RefreshButton_Click;
            datePanel.Controls.Add(refreshButton);

            tab2Button = new Button
            {
                Text = "2. Sayfa",
                Location = new Point(520, 12),
                Size = new Size(110, 30),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            tab2Button.FlatAppearance.BorderSize = 0;
            tab2Button.Click += Tab2Button_Click;
            datePanel.Controls.Add(tab2Button);

            // Main Tab Control
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold),
                Appearance = TabAppearance.FlatButtons,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(150, 35)
            };

            // Tab styling
            mainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            mainTabControl.DrawItem += MainTabControl_DrawItem;

            this.Controls.Add(mainTabControl);

            // Create Tab Pages
            CreateTab1();
            CreateTab2();
        }

        private void MainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            TabPage tabPage = tabControl.TabPages[e.Index];
            Rectangle tabRect = tabControl.GetTabRect(e.Index);

            // Background
            Color backColor = (e.State == DrawItemState.Selected)
                ? Color.FromArgb(108, 166, 205)
                : Color.FromArgb(25, 42, 86);

            using (Brush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, tabRect);
            }

            // Text
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                StringFormat stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(tabPage.Text, tabControl.Font, textBrush, tabRect, stringFormat);
            }
        }

        private void CreateTab1()
        {
            var tab1 = new TabPage("Dashboard")
            {
                BackColor = Color.FromArgb(25, 42, 86)
            };
            mainTabControl.TabPages.Add(tab1);

            // Main Layout - 2 rows: charts top, tables bottom
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(25, 42, 86)
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // Charts
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Tables

            tab1.Controls.Add(mainLayout);

            // Column Chart
            columnChart = CreateColumnChart();
            mainLayout.Controls.Add(columnChart, 0, 0);

            // Patient Chart:
            patientChart = CreatePatientChart();
            mainLayout.Controls.Add(patientChart, 1, 0);

            bottomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(25, 42, 86)
            };

            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); // Hata Oranları
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65)); // Son Kullanılan Flakonlar

            mainLayout.Controls.Add(bottomLayout, 0, 1);
            mainLayout.SetColumnSpan(bottomLayout, 2);

            // Data Grid Views
            dataGridView1 = CreateDataGridView("İlaç Hata Oranları ve FlowFactor");
            bottomLayout.Controls.Add(dataGridView1, 0, 0);

            dataGridView2 = CreateDataGridView("Son Kullanılan Flakonlar");
            bottomLayout.Controls.Add(dataGridView2, 1, 0);
        }

        private void CreateTab2()
        {
            var tab2 = new TabPage("Overdose Analizi")
            {
                BackColor = Color.FromArgb(25, 42, 86)
            };
            mainTabControl.TabPages.Add(tab2);

            // Tab 2 Layout - 3 satır: kontroller, chart, tablolar
            tab2Layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(25, 42, 86)
            };

            tab2Layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Sütun grafiği için %60
            tab2Layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Pie chart için %40
            tab2Layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // TextBox ve Button için
            tab2Layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // Grafikler için %60
            tab2Layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Tablolar için %40
            tab2.Controls.Add(tab2Layout);

            // TextBox ve Button için panel
            var controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 42, 86),
                Margin = new Padding(8)
            };
            tab2Layout.Controls.Add(controlPanel, 0, 0);
            tab2Layout.SetColumnSpan(controlPanel, 2);
            tab2Layout.Controls.Add(controlPanel, 0, 0);

            // Yarı Oto kontrolleri
            var lblSemiAuto = new Label
            {
                Text = "Yarı Oto Order: ",
                Location = new Point(controlPanel.Width - 820, 25),
                Size = new Size(150, 30),
                ForeColor = Color.White,
                Font = new Font("Franklin Gothic", 12, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            controlPanel.Controls.Add(lblSemiAuto);

            semiAutoCountTextBox = new TextBox
            {
                Location = new Point(controlPanel.Width - 660, 20),
                Size = new Size(90, 40),
                Font = new Font("Franklin Gothic", 12, FontStyle.Bold),
                Text = "",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            controlPanel.Controls.Add(semiAutoCountTextBox);

            updateSemiAutoButton = new Button
            {
                Text = "Güncelle",
                Location = new Point(controlPanel.Width - 560, 12),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(108, 166, 205),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            updateSemiAutoButton.FlatAppearance.BorderSize = 0;
            updateSemiAutoButton.Click += UpdateSemiAutoButton_Click;
            controlPanel.Controls.Add(updateSemiAutoButton);

            // Manuel kontrolleri (konumları güncellendi)
            var lblManual = new Label
            {
                Text = "Manuel Order Sayısı:",
                Location = new Point(controlPanel.Width - 400, 25), 
                Size = new Size(180, 30),
                ForeColor = Color.White,
                Font = new Font("Franklin Gothic", 12, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            controlPanel.Controls.Add(lblManual);

            manualCountTextBox = new TextBox
            {
                Location = new Point(controlPanel.Width - 210, 20),
                Size = new Size(90, 40),
                Font = new Font("Franklin Gothic", 12, FontStyle.Bold),
                Text = "",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            controlPanel.Controls.Add(manualCountTextBox);

            updateManualButton = new Button
            {
                Text = "Güncelle",
                Location = new Point(controlPanel.Width - 110, 12),
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(108, 166, 205),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            updateManualButton.FlatAppearance.BorderSize = 0;
            updateManualButton.Click += UpdateManualButton_Click;
            controlPanel.Controls.Add(updateManualButton);

            // Overdose Chart - 2 sütuna yayılsın
            overdoseChart = CreateOverdoseChart();
            tab2Layout.Controls.Add(overdoseChart, 0, 1);  // Sol tarafta

            pieChart = CreatePieChart();
            tab2Layout.Controls.Add(pieChart, 1, 1);  // Sağ tarafta
            // İlk tablo
            dataGridView3 = CreateDataGridView("Overdose Anonim-Seyreltme Analizi");
            tab2Layout.Controls.Add(dataGridView3, 0, 2);

            // İkinci tablo
            dataGridView4 = CreateDataGridView("Eksik Tamamlama Sayısı Analizi");
            tab2Layout.Controls.Add(dataGridView4, 1, 2);
        }

        private Chart CreateOverdoseChart()
        {
            var chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 42, 86),
                Margin = new Padding(8),
                BorderlineColor = Color.FromArgb(25, 42, 86),
                BorderlineWidth = 1,
                BorderlineDashStyle = ChartDashStyle.Solid
            };

            var chartArea = new ChartArea("OverdoseArea")
            {
                BackColor = Color.FromArgb(25, 42, 86),
                BorderColor = Color.Transparent,
                AxisX = {
                    MajorGrid = { Enabled = false },
                    LabelStyle = { Font = new Font("Franklin Gothic", 11, FontStyle.Bold), ForeColor = Color.White },
                    LineColor = Color.White
                },
                AxisY = {
                    MajorGrid = { LineColor = Color.White, LineDashStyle = ChartDashStyle.Dot },
                    LabelStyle = { Font = new Font("Franklin Gothic", 11, FontStyle.Bold), ForeColor = Color.White },
                    LineColor = Color.White
                }
            };
            chart.ChartAreas.Add(chartArea);

            var series = new Series("OverdoseData")
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true,
                Font = new Font("Franklin Gothic", 12, FontStyle.Bold),
                LabelForeColor = Color.White,
                BorderWidth = 0
            };
            chart.Series.Add(series);

            var title = new Title("Seyreltme ve Anonim Analizi")
            {
                Font = new Font("Franklin Gothic", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Docking = Docking.Top,
                Alignment = ContentAlignment.TopCenter
            };
            chart.Titles.Add(title);

            return chart;
        }

        private Chart CreateColumnChart()
        {
            var chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 42, 86),
                Margin = new Padding(8),
                BorderlineColor = Color.FromArgb(25, 42, 86),
                BorderlineWidth = 1,
                BorderlineDashStyle = ChartDashStyle.Solid
            };

            var chartArea = new ChartArea("MainArea")
            {
                BackColor = Color.FromArgb(25, 42, 86),
                BorderColor = Color.Transparent,
                AxisX = {
                    MajorGrid = { Enabled = false },
                    LabelStyle = { Font = new Font("Franklin Gothic", 9, FontStyle.Bold), Angle = -45, ForeColor = Color.White },
                    LineColor = Color.White
                },
                AxisY = {
                    MajorGrid = { LineColor = Color.White, LineDashStyle = ChartDashStyle.Dot },
                    LabelStyle = { Font = new Font("Franklin Gothic", 9, FontStyle.Bold), ForeColor = Color.White },
                    LineColor = Color.White
                }
            };
            chart.ChartAreas.Add(chartArea);

            var series = new Series("Orders")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(108, 166, 205),
                IsValueShownAsLabel = true,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold),
                LabelForeColor = Color.White,
                BorderWidth = 0
            };
            chart.Series.Add(series);

            var title = new Title("Order Sayısı")
            {
                Font = new Font("Franklin Gothic", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Docking = Docking.Top,
                Alignment = ContentAlignment.TopLeft
            };
            chart.Titles.Add(title);

            return chart;
        }

        private Chart CreatePieChart()
        {
            var chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 42, 86),
                Margin = new Padding(8),
                BorderlineColor = Color.FromArgb(25, 42, 86),
                BorderlineWidth = 1,
                BorderlineDashStyle = ChartDashStyle.Solid
            };

            var chartArea = new ChartArea("PieArea")
            {
                BackColor = Color.FromArgb(25, 42, 86)
            };
            chart.ChartAreas.Add(chartArea);

            var series = new Series("Distribution")
            {
                ChartType = SeriesChartType.Doughnut,
                IsValueShownAsLabel = false,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold)
            };
            series["PieLabelStyle"] = "Outside";
            series["DoughnutRadius"] = "40";
            chart.Series.Add(series);

            var title = new Title("Manuel İşlem / Cihaz İşlem")
            {
                Font = new Font("Franklin Gothic", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Docking = Docking.Top,
                Alignment = ContentAlignment.TopLeft
            };
            chart.Titles.Add(title);

            // Legend
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Right,
                Alignment = StringAlignment.Center,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(25, 42, 86),
                BorderColor = Color.Transparent
            };

            chart.Legends.Add(legend);

            return chart;
        }

        private DataGridView CreateDataGridView(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 42, 86),
                Margin = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Franklin Gothic", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 30,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 5, 0, 0),
                BackColor = Color.FromArgb(25, 42, 86)
            };
            panel.Controls.Add(titleLabel);

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(25, 42, 86),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Franklin Gothic", 9, FontStyle.Bold),
                RowHeadersVisible = false,
                AllowUserToResizeRows = false,
                EnableHeadersVisualStyles = false,
                GridColor = Color.White
            };

            grid.DefaultCellStyle.BackColor = Color.FromArgb(25, 42, 86);
            grid.DefaultCellStyle.ForeColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(108, 166, 205);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Padding = new Padding(5);
            grid.RowTemplate.Height = 28;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 42, 86);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Franklin Gothic", 9, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            grid.ColumnHeadersHeight = 35;

            panel.Controls.Add(grid);

            return grid;
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            refreshButton.Text = "⏳ Yükleniyor...";
            refreshButton.Enabled = false;
            LoadData();
            refreshButton.Text = "🔄 Yenile";
            refreshButton.Enabled = true;
        }

        private void Tab2Button_Click(object sender, EventArgs e)
        {
            if (mainTabControl.SelectedIndex == 0)
            {
                mainTabControl.SelectedIndex = 1;
                tab2Button.Text = "1. Sayfa";
            }
            else
            {
                mainTabControl.SelectedIndex = 0;
                tab2Button.Text = "2. Sayfa";
            }
        }

        private void LoadData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Tab 1 data
                LoadColumnChartData();
                LoadPatientChartData();
                LoadTable1Data();
                LoadTable2Data();

                // Tab 2 data
                LoadOverdoseChartData();
                LoadPieChartData();
                LoadTable3Data();
                LoadTable4Data();

                lblTitle.Text = $"Kapsam Dashboard - Son Güncelleme: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void LoadOverdoseChartData()
        {
            overdoseChart.Series["OverdoseData"].Points.Clear();

            string query = @"
                SELECT
                    SUM(CASE WHEN JSON_VALUE([Details], '$.OverMedicineAccepted') = 'true' THEN 1 ELSE 0 END) AS OverMedicineAcceptedCount,
                    SUM(CASE WHEN JSON_VALUE([Details], '$.DrugOverdosed') = 'true' THEN 1 ELSE 0 END) AS DrugOverdosedCount,
                    COUNT(*) AS ToplamAdet
                FROM [pharmascopeV2].[order].[FinalContainer]
                WHERE 
                    UseDate BETWEEN @StartDate AND @EndDate
                    AND (
                        JSON_VALUE([Details], '$.DrugOverdosed') = 'true'
                        OR JSON_VALUE([Details], '$.OverMedicineAccepted') = 'true'
                    )";

            using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                Color[] colors = {
                    Color.FromArgb(108, 166, 205),  // Mavi - DrugOverdosed
                    Color.FromArgb(108, 166, 205),  // Mavi - OverMedicineAccepted  
                    Color.FromArgb(108, 166, 205)   // Mavi - Toplam
                };

                if (reader.Read())
                {
                    object drugObj = reader["DrugOverdosedCount"];
                    object acceptedObj = reader["OverMedicineAcceptedCount"];

                    int drugOverdosed = (drugObj == DBNull.Value) ? 0 : Convert.ToInt32(drugObj);
                    int overMedicineAccepted = (acceptedObj == DBNull.Value) ? 0 : Convert.ToInt32(acceptedObj);
                    int toplam = drugOverdosed + overMedicineAccepted;

                    var series = overdoseChart.Series["OverdoseData"];

                    // DrugOverdosed
                    int pointIndex1 = overdoseChart.Series["OverdoseData"].Points.AddXY("Overdose Anonim", drugOverdosed);
                    overdoseChart.Series["OverdoseData"].Points[pointIndex1].Color = colors[0];

                    // OverMedicineAccepted  
                    int pointIndex2 = overdoseChart.Series["OverdoseData"].Points.AddXY("Overdose Seyreltme", overMedicineAccepted);
                    overdoseChart.Series["OverdoseData"].Points[pointIndex2].Color = colors[1];

                    // Toplam
                    int pointIndex3 = overdoseChart.Series["OverdoseData"].Points.AddXY("Toplam", toplam.ToString());
                    overdoseChart.Series["OverdoseData"].Points[pointIndex3].Color = colors[2];
                }
            }
        }

        private void LoadColumnChartData()
        {
            columnChart.Series["Orders"].Points.Clear();

            string query = @"
                SELECT 
                    CONCAT(C.Name, ' ', C.SurName) AS 'Çalışan Ad Soyad',
                    COUNT(*) AS 'Yapılan Order Sayısı'
                FROM [order].[Order] O
                JOIN [user].[User] C ON O.CompletedById = C.Id
                WHERE O.AdministrationDate BETWEEN @StartDate AND @EndDate AND O.PreparingStatus = 'Completed'
                GROUP BY C.Name, C.SurName
                ORDER BY COUNT(*) DESC";

            using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                Color columnColor = Color.FromArgb(108, 166, 205);

                while (reader.Read())
                {
                    string calisan = reader["Çalışan Ad Soyad"].ToString();
                    int count = Convert.ToInt32(reader["Yapılan Order Sayısı"]);

                    int pointIndex = columnChart.Series["Orders"].Points.AddXY(calisan, count);
                    columnChart.Series["Orders"].Points[pointIndex].Color = columnColor;
                }
            }
        }

        private void LoadPieChartData()
        {
            pieChart.Series["Distribution"].Points.Clear();

            string query = @"
                SELECT 
    CASE 
        WHEN IsManual = 1 THEN 'Manuel İşlem'
        WHEN IsSemiAutomatic = 1 THEN 'Yarı Oto İşlem'
        ELSE 'Cihaz İşlem'
    END AS IslemTipi,
    COUNT(*) AS [Toplam Adet]
FROM [order].[Order]
WHERE AdministrationDate IS NOT NULL 
    AND AdministrationDate BETWEEN @StartDate AND @EndDate AND [order].[Order].PreparingStatus = 'Completed'
GROUP BY 
    CASE 
        WHEN IsManual = 1 THEN 'Manuel İşlem'
        WHEN IsSemiAutomatic = 1 THEN 'Yarı Oto İşlem'
        ELSE 'Cihaz İşlem'
    END";

            using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                Color[] colors = {
                    Color.FromArgb(59, 59, 152),   // #3B3B98 - Cihaz İşlem
                    Color.FromArgb(74, 105, 189),  // #4a69bd - Manuel İşlem  
                    Color.FromArgb(112, 161, 255)  // #70a1ff - Yarı Oto İşlem
                };

                int colorIndex = 0;
                while (reader.Read())
                {
                    string islemTipi = reader["IslemTipi"].ToString();
                    int count = Convert.ToInt32(reader["Toplam Adet"]);

                    int pointIndex = pieChart.Series["Distribution"].Points.AddXY(islemTipi, count);
                    pieChart.Series["Distribution"].Points[pointIndex].Color = colors[colorIndex % colors.Length];
                    pieChart.Series["Distribution"].Points[pointIndex].Label = $"{islemTipi}\n{count}";
                    pieChart.Series["Distribution"].Points[pointIndex].LegendText = $"{islemTipi} ({count})";
                    pieChart.Series["Distribution"].Points[pointIndex].LabelForeColor = Color.White;
                    colorIndex++;
                }
            }
        }

        private void LoadTable1Data()
        {
            string query = @"
                SELECT 
                    M.Name as 'İlaç İsmi',
                    FORMAT(AVG(F.FillFault), '0.##') AS 'Hata Oranı',
                    M.FlowFactorCoefficient
                FROM [order].[Order] O
                JOIN [order].FinalContainer F ON F.Id = O.FinalContainerId
                JOIN material.Medicine M ON F.MedicineId=M.Id
                WHERE O.AdministrationDate BETWEEN @StartDate AND @EndDate
                GROUP BY M.Name, M.FlowFactorCoefficient
                HAVING AVG(F.FillFault) <> 0
                ORDER BY AVG(F.FillFault) DESC";

            using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }

        private void LoadTable2Data()
        {
            string query = @"
                SELECT TOP 50
                    CONCAT(C.Name, ' ', C.SurName) AS [Çalışan Ad Soyad],
                    M.Name AS [İlaç İsmi],
                    O.Id AS [Order_ID],
                    CAST(DATEDIFF(SECOND, F.UseDate, O.CompletedAt) / 60 AS VARCHAR(10))
                        + ' dakika ' +
                    CAST(DATEDIFF(SECOND, F.UseDate, O.CompletedAt) % 60 AS VARCHAR(10))
                        + ' saniye' AS [Order Zamanı],
                    COUNT(V.VialId) AS [Vial Sayısı]
                FROM [order].[Order] O
                JOIN [order].FinalContainer F ON F.Id = O.FinalContainerId
                LEFT JOIN [order].[VialEvent] V ON O.Id = V.OrderId
                LEFT JOIN [user].[User] C ON O.CompletedByID = C.Id
                JOIN [material].Medicine M ON M.Id = F.MedicineId
                WHERE O.AdministrationDate BETWEEN @StartDate AND @EndDate
                GROUP BY
                    O.Id, F.UseDate, O.CompletedAt, C.Name, C.SurName, M.Name
                ORDER BY
                    DATEDIFF(SECOND, F.UseDate, O.CompletedAt) DESC,
                    COUNT(V.VialId) ASC";

            using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView2.DataSource = dt;
            }
        }
        private void LoadTable3Data()
        {
            string query = @"
        SELECT 
            H.NAME AS 'İlaç İsmi',
            SUM(CASE WHEN JSON_VALUE([Details], '$.DrugOverdosed') = 'true' THEN 1 ELSE 0 END) AS 'Overdose Anonim',
            SUM(CASE WHEN JSON_VALUE([Details], '$.OverMedicineAccepted') = 'true' THEN 1 ELSE 0 END) AS 'Overdose Seyreltme'
        FROM [order].[FinalContainer] F 
        INNER JOIN [material].[Medicine] H ON F.MedicineId = H.Id JOIN [order].[Order] O on O.FinalContainerId=F.Id
        WHERE (JSON_VALUE([Details], '$.DrugOverdosed') = 'true' 
               OR JSON_VALUE([Details], '$.OverMedicineAccepted') = 'true')
            AND O.AdministrationDate BETWEEN @StartDate AND @EndDate
        GROUP BY H.NAME
        ORDER BY (SUM(CASE WHEN JSON_VALUE([Details], '$.DrugOverdosed') = 'true' THEN 1 ELSE 0 END) + 
                  SUM(CASE WHEN JSON_VALUE([Details], '$.OverMedicineAccepted') = 'true' THEN 1 ELSE 0 END)) DESC";

            using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dataGridView3.DataSource = dt;
            }
        }
        private void LoadTable4Data()
        {
            string query = @"
        SELECT 
            H.NAME as 'İlaç İsmi',
            JSON_VALUE([Details], '$.MissingDrugDose') as 'MissingDrugDose',
            O.Id as 'Order ID',
            O.AdministrationDate as 'Uygulama Tarihi'
        FROM [order].[FinalContainer] F
        JOIN [material].Medicine H ON F.MedicineId = H.Id 
        JOIN [order].[Order] O on O.FinalContainerId=F.Id
        WHERE TRY_CAST(JSON_VALUE([Details], '$.MissingDrugDose') AS INT) > 0 
            and O.PreparingStatus = 'Completed' 
            and O.AdministrationDate BETWEEN @StartDate AND @EndDate
        ORDER BY TRY_CAST(JSON_VALUE([Details], '$.MissingDrugDose') AS INT) DESC";

            using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dataGridView4.DataSource = dt;
            }
        }
        private void UpdateManualButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(manualCountTextBox.Text, out int targetManualCount) || targetManualCount < 0)
                {
                    MessageBox.Show("Lütfen geçerli bir sayı girin.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                updateManualButton.Text = "Güncelleniyor...";
                updateManualButton.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
                {
                    conn.Open();

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Mevcut manuel işlem sayısını öğren
                            string currentManualQuery = @"
                        SELECT COUNT(*) 
                        FROM [order].[Order] 
                        WHERE AdministrationDate BETWEEN @StartDate AND @EndDate 
                            AND PreparingStatus = 'Completed'
                            AND IsManual = 1";

                            SqlCommand currentCmd = new SqlCommand(currentManualQuery, conn, transaction);
                            currentCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                            currentCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));
                            int currentManualCount = (int)currentCmd.ExecuteScalar();

                            // Toplam kayıt sayısını kontrol et
                            string totalQuery = @"
                        SELECT COUNT(*) 
                        FROM [order].[Order] 
                        WHERE AdministrationDate BETWEEN @StartDate AND @EndDate 
                            AND PreparingStatus = 'Completed'";

                            SqlCommand totalCmd = new SqlCommand(totalQuery, conn, transaction);
                            totalCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                            totalCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));
                            int totalRecords = (int)totalCmd.ExecuteScalar();

                            // Hedef sayı toplam kayıttan fazla ise hata ver
                            if (targetManualCount > totalRecords)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Hata: Girilen sayı ({targetManualCount}) toplam kayıt sayısından ({totalRecords}) fazla olamaz!", "Hata",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (targetManualCount > currentManualCount)
                            {
                                // Manuel işlem sayısını artır
                                int additionalManualCount = targetManualCount - currentManualCount;

                                string updateQuery = @"
                            UPDATE [order].[Order] 
                            SET IsManual = 1 
                            WHERE Id IN (
                                SELECT TOP (@AdditionalCount) Id 
                                FROM [order].[Order]
                                WHERE AdministrationDate BETWEEN @StartDate AND @EndDate
                                    AND PreparingStatus = 'Completed'
                                    AND IsManual = 0
                                ORDER BY OrderedDrugVolume ASC
                            )";

                                SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                                updateCmd.Parameters.AddWithValue("@AdditionalCount", additionalManualCount);
                                updateCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                                updateCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                                int updatedRecords = updateCmd.ExecuteNonQuery();
                            }
                            else if (targetManualCount < currentManualCount)
                            {
                                // Manuel işlem sayısını azalt
                                int decreaseManualCount = currentManualCount - targetManualCount;

                                string updateQuery = @"
                            UPDATE [order].[Order] 
                            SET IsManual = 0 
                            WHERE Id IN (
                                SELECT TOP (@DecreaseCount) Id 
                                FROM [order].[Order]
                                WHERE AdministrationDate BETWEEN @StartDate AND @EndDate
                                    AND PreparingStatus = 'Completed'
                                    AND IsManual = 1
                                ORDER BY OrderedDrugVolume DESC
                            )";

                                SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                                updateCmd.Parameters.AddWithValue("@DecreaseCount", decreaseManualCount);
                                updateCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                                updateCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                                int updatedRecords = updateCmd.ExecuteNonQuery();
                            }

                            // Sonuçları doğrula
                            string verifyQuery = @"
                        SELECT 
                            SUM(CASE WHEN IsManual = 1 THEN 1 ELSE 0 END) as ManuelSayisi,
                            SUM(CASE WHEN IsManual = 0 THEN 1 ELSE 0 END) as CihazSayisi
                        FROM [order].[Order] 
                        WHERE AdministrationDate BETWEEN @StartDate AND @EndDate
                            AND PreparingStatus = 'Completed'";

                            SqlCommand verifyCmd = new SqlCommand(verifyQuery, conn, transaction);
                            verifyCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                            verifyCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                            int finalManuelSayisi = 0;
                            int finalCihazSayisi = 0;

                            using (SqlDataReader reader = verifyCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    finalManuelSayisi = Convert.ToInt32(reader["ManuelSayisi"] ?? 0);
                                    finalCihazSayisi = Convert.ToInt32(reader["CihazSayisi"] ?? 0);
                                }
                            }

                            transaction.Commit();

                            string changeType = "";
                            int changeAmount = 0;

                            if (targetManualCount > currentManualCount)
                            {
                                changeType = "Eklenen";
                                changeAmount = targetManualCount - currentManualCount;
                            }
                            else if (targetManualCount < currentManualCount)
                            {
                                changeType = "Azaltılan";
                                changeAmount = currentManualCount - targetManualCount;
                            }
                            else
                            {
                                changeType = "Değişiklik yapılmadı";
                            }

                            if (changeAmount > 0)
                            {
                                MessageBox.Show($"Database Güncellendi\n" +
                                              $"Önceki Manuel İşlem: {currentManualCount}\n" +
                                              $"{changeType} Manuel İşlem: {changeAmount}\n" +
                                              $"Yeni Manuel İşlem: {finalManuelSayisi}\n" +
                                              $"Cihaz İşlemi: {finalCihazSayisi}", "Başarılı",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Değişiklik yapılmadı. Mevcut manuel işlem sayısı zaten {currentManualCount}.", "Bilgi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }


                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                updateManualButton.Text = "Güncelle";
                updateManualButton.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }
        private void UpdateSemiAutoButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(semiAutoCountTextBox.Text, out int targetSemiAutoCount) || targetSemiAutoCount < 0)
                {
                    MessageBox.Show("Lütfen geçerli bir sayı girin.", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                updateSemiAutoButton.Text = "Güncelleniyor...";
                updateSemiAutoButton.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
                {
                    conn.Open();

                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Mevcut yarı oto işlem sayısını öğren
                            string currentSemiAutoQuery = @"
                        SELECT COUNT(*) 
                        FROM [order].[Order] 
                        WHERE AdministrationDate BETWEEN @StartDate AND @EndDate 
                            AND PreparingStatus = 'Completed'
                            AND IsSemiAutomatic = 1";

                            SqlCommand currentCmd = new SqlCommand(currentSemiAutoQuery, conn, transaction);
                            currentCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                            currentCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));
                            int currentSemiAutoCount = (int)currentCmd.ExecuteScalar();

                            // Mevcut cihaz işlem sayısını kontrol et (Manuel işlemleri hariç tut)
                            string deviceQuery = @"
                        SELECT COUNT(*) 
                        FROM [order].[Order] 
                        WHERE AdministrationDate BETWEEN @StartDate AND @EndDate 
                            AND PreparingStatus = 'Completed'
                            AND IsManual = 0 AND IsSemiAutomatic = 0";

                            SqlCommand deviceCmd = new SqlCommand(deviceQuery, conn, transaction);
                            deviceCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                            deviceCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));
                            int deviceRecords = (int)deviceCmd.ExecuteScalar();

                            // Hedef sayı cihaz kayıtlarından fazla ise hata ver
                            if (targetSemiAutoCount > deviceRecords + currentSemiAutoCount)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Hata: Girilen sayı ({targetSemiAutoCount}) mevcut cihaz işlem sayısından ({deviceRecords + currentSemiAutoCount}) fazla olamaz!", "Hata",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (targetSemiAutoCount > currentSemiAutoCount)
                            {
                                // Yarı oto işlem sayısını artır (cihaz işlemlerinden al)
                                int additionalSemiAutoCount = targetSemiAutoCount - currentSemiAutoCount;

                                string updateQuery = @"
                            UPDATE [order].[Order] 
                            SET IsSemiAutomatic = 1 
                            WHERE Id IN (
                                SELECT TOP (@AdditionalCount) Id 
                                FROM [order].[Order]
                                WHERE AdministrationDate BETWEEN @StartDate AND @EndDate
                                    AND PreparingStatus = 'Completed'
                                    AND IsManual = 0 AND IsSemiAutomatic = 0
                                ORDER BY OrderedDrugVolume ASC
                            )";

                                SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                                updateCmd.Parameters.AddWithValue("@AdditionalCount", additionalSemiAutoCount);
                                updateCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                                updateCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                                updateCmd.ExecuteNonQuery();
                            }
                            else if (targetSemiAutoCount < currentSemiAutoCount)
                            {
                                // Yarı oto işlem sayısını azalt (cihaz işlemlerine çevir)
                                int decreaseSemiAutoCount = currentSemiAutoCount - targetSemiAutoCount;

                                string updateQuery = @"
                            UPDATE [order].[Order] 
                            SET IsSemiAutomatic = 0 
                            WHERE Id IN (
                                SELECT TOP (@DecreaseCount) Id 
                                FROM [order].[Order]
                                WHERE AdministrationDate BETWEEN @StartDate AND @EndDate
                                    AND PreparingStatus = 'Completed'
                                    AND IsSemiAutomatic = 1
                                ORDER BY OrderedDrugVolume DESC
                            )";

                                SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                                updateCmd.Parameters.AddWithValue("@DecreaseCount", decreaseSemiAutoCount);
                                updateCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                                updateCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                                updateCmd.ExecuteNonQuery();
                            }

                            // Sonuçları doğrula
                            string verifyQuery = @"
                        SELECT 
                            SUM(CASE WHEN IsManual = 1 THEN 1 ELSE 0 END) as ManuelSayisi,
                            SUM(CASE WHEN IsSemiAutomatic = 1 THEN 1 ELSE 0 END) as YariOtoSayisi,
                            SUM(CASE WHEN IsManual = 0 AND IsSemiAutomatic = 0 THEN 1 ELSE 0 END) as CihazSayisi
                        FROM [order].[Order] 
                        WHERE AdministrationDate BETWEEN @StartDate AND @EndDate
                            AND PreparingStatus = 'Completed'";

                            SqlCommand verifyCmd = new SqlCommand(verifyQuery, conn, transaction);
                            verifyCmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                            verifyCmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                            int finalManuelSayisi = 0;
                            int finalYariOtoSayisi = 0;
                            int finalCihazSayisi = 0;

                            using (SqlDataReader reader = verifyCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    finalManuelSayisi = Convert.ToInt32(reader["ManuelSayisi"] ?? 0);
                                    finalYariOtoSayisi = Convert.ToInt32(reader["YariOtoSayisi"] ?? 0);
                                    finalCihazSayisi = Convert.ToInt32(reader["CihazSayisi"] ?? 0);
                                }
                            }

                            transaction.Commit();

                            string changeType = "";
                            int changeAmount = 0;

                            if (targetSemiAutoCount > currentSemiAutoCount)
                            {
                                changeType = "Eklenen";
                                changeAmount = targetSemiAutoCount - currentSemiAutoCount;
                            }
                            else if (targetSemiAutoCount < currentSemiAutoCount)
                            {
                                changeType = "Azaltılan";
                                changeAmount = currentSemiAutoCount - targetSemiAutoCount;
                            }

                            if (changeAmount > 0)
                            {
                                MessageBox.Show($"Database Güncellendi\n" +
                                              $"Önceki Yarı Oto İşlem: {currentSemiAutoCount}\n" +
                                              $"{changeType} Yarı Oto İşlem: {changeAmount}\n" +
                                              $"Yeni Yarı Oto İşlem: {finalYariOtoSayisi}\n" +
                                              $"Manuel İşlem: {finalManuelSayisi}\n" +
                                              $"Cihaz İşlemi: {finalCihazSayisi}", "Başarılı",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Değişiklik yapılmadı. Mevcut yarı oto işlem sayısı zaten {currentSemiAutoCount}.", "Bilgi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                updateSemiAutoButton.Text = "Güncelle";
                updateSemiAutoButton.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        } 
        private Chart CreatePatientChart()
        {
            var chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 42, 86),
                Margin = new Padding(8),
                BorderlineColor = Color.FromArgb(25, 42, 86),
                BorderlineWidth = 1,
                BorderlineDashStyle = ChartDashStyle.Solid
            };

            var chartArea = new ChartArea("PatientArea")
            {
                BackColor = Color.FromArgb(25, 42, 86),
                BorderColor = Color.Transparent,
                AxisX = {
            MajorGrid = { Enabled = false },
            LabelStyle = { Font = new Font("Franklin Gothic", 9, FontStyle.Bold), Angle = -45, ForeColor = Color.White },
            LineColor = Color.White
        },
                AxisY = {
            MajorGrid = { LineColor = Color.White, LineDashStyle = ChartDashStyle.Dot },
            LabelStyle = { Font = new Font("Franklin Gothic", 9, FontStyle.Bold), ForeColor = Color.White },
            LineColor = Color.White
        }
            };
            chart.ChartAreas.Add(chartArea);

            var series = new Series("Patients")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(108, 166, 205),
                IsValueShownAsLabel = true,
                Font = new Font("Franklin Gothic", 10, FontStyle.Bold),
                LabelForeColor = Color.White,
                BorderWidth = 0
            };
            chart.Series.Add(series);

            var title = new Title("Bakılan Hasta Sayısı")
            {
                Font = new Font("Franklin Gothic", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Docking = Docking.Top,
                Alignment = ContentAlignment.TopLeft
            };
            chart.Titles.Add(title);

            return chart;
        }

        private void LoadPatientChartData()
        {
            patientChart.Series["Patients"].Points.Clear();

            string query = @"
        SELECT 
            CONCAT(U.Name, ' ', U.SurName) AS [Çalışan Ad Soyad], 
            COUNT(DISTINCT O.PatientId) AS [Hasta Sayısı] 
        FROM [user].[User] U 
        JOIN [order].[Order] O ON O.CompletedById = U.Id 
        WHERE O.PreparingStatus = 'Completed' 
            AND O.AdministrationDate BETWEEN @StartDate AND @EndDate
        GROUP BY U.Id, U.Name, U.SurName 
        ORDER BY [Hasta Sayısı] DESC";

            using (SqlConnection conn = new SqlConnection(ConnectionStringHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StartDate", startDatePicker.Value.Date);
                cmd.Parameters.AddWithValue("@EndDate", endDatePicker.Value.Date.AddDays(1).AddSeconds(-1));

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                Color columnColor = Color.FromArgb(108, 166, 205);

                while (reader.Read())
                {
                    string calisan = reader["Çalışan Ad Soyad"].ToString();
                    int count = Convert.ToInt32(reader["Hasta Sayısı"]);

                    int pointIndex = patientChart.Series["Patients"].Points.AddXY(calisan, count);
                    patientChart.Series["Patients"].Points[pointIndex].Color = columnColor;
                }
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();
        }
    }


}