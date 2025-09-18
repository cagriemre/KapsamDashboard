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
        private Chart pieChart;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;

        // Tab 2 Controls
        private Chart overdoseChart;

        private Label lblTitle;
        private Panel topPanel;
        private TabControl mainTabControl;
        private TableLayoutPanel mainLayout;
        private TableLayoutPanel bottomLayout;
        private TableLayoutPanel tab2Layout;
        private DataGridView dataGridView3;
        private DataGridView dataGridView4;

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

            // Pie Chart
            pieChart = CreatePieChart();
            mainLayout.Controls.Add(pieChart, 1, 0);

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

            // Tab 2 Layout - Chart üstte 50, tablolar altta %25 + %25
            tab2Layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(15),
                BackColor = Color.FromArgb(25, 42, 86)
            };

            tab2Layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tab2Layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tab2Layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Chart
            tab2Layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // Tablolar
            tab2.Controls.Add(tab2Layout);

            // Overdose Chart - 2 sütuna yayılsın
            overdoseChart = CreateOverdoseChart();
            tab2Layout.Controls.Add(overdoseChart, 0, 0);
            tab2Layout.SetColumnSpan(overdoseChart, 2);

            // İlk tablo
            dataGridView3 = CreateDataGridView("Overdose Anonim-Seyreltme Analizi");
            tab2Layout.Controls.Add(dataGridView3, 0, 1);

            // İkinci tablo
            dataGridView4 = CreateDataGridView("Eksik Tamamlama Sayısı Analizi");
            tab2Layout.Controls.Add(dataGridView4, 1, 1);
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
                LoadPieChartData();
                LoadTable1Data();
                LoadTable2Data();

                // Tab 2 data
                LoadOverdoseChartData();
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
                WHERE O.AdministrationDate BETWEEN @StartDate AND @EndDate
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
                        ELSE 'Cihaz İşlem'
                    END AS IslemTipi,
                    COUNT(*) AS [Toplam Adet]
                FROM [order].[Order]
                WHERE AdministrationDate IS NOT NULL 
                    AND AdministrationDate BETWEEN @StartDate AND @EndDate
                GROUP BY 
                    CASE 
                        WHEN IsManual = 1 THEN 'Manuel İşlem'
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
                    Color.FromArgb(133, 193, 233),
                    Color.FromArgb(85, 142, 213)
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
                SELECT TOP 100
                    CONCAT(C.Name, ' ', C.SurName) AS [Çalışan Ad Soyad],
                    M.Name AS [İlaç İsmi],
                    O.Id AS [Order_ID],
                    CAST(DATEDIFF(SECOND, F.UseDate, O.AdministrationDate) / 60 AS VARCHAR(10))
                        + ' dakika ' +
                    CAST(DATEDIFF(SECOND, F.UseDate, O.AdministrationDate) % 60 AS VARCHAR(10))
                        + ' saniye' AS [Order Zamanı],
                    COUNT(V.VialId) AS [Vial Sayısı]
                FROM [order].[Order] O
                JOIN [order].FinalContainer F ON F.Id = O.FinalContainerId
                LEFT JOIN [order].[VialEvent] V ON O.Id = V.OrderId
                LEFT JOIN [user].[User] C ON O.CompletedByID = C.Id
                JOIN [material].Medicine M ON M.Id = F.MedicineId
                WHERE O.AdministrationDate BETWEEN @StartDate AND @EndDate
                GROUP BY
                    O.Id, F.UseDate, O.AdministrationDate, C.Name, C.SurName, M.Name
                ORDER BY
                    DATEDIFF(SECOND, F.UseDate, O.AdministrationDate) DESC,
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();
        }
    }
}
