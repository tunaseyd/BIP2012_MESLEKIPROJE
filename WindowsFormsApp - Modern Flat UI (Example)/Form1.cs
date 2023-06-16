using OfficeOpenXml;
using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp___Modern_Flat_UI__Example_
{
    public partial class Form1 : Form
    {
        //direkt dosya konumu ile ".db" bağlantısı sağlayacak patika
        string connectionString = @"Data Source=C:\Users\HUAWEİ\ProjectTest.db;Version=3;"; 
        private Button currentButton;
        private Random random;
        private int tempIndex;
        private Timer notificationTimer; //uyarı sisteminin çalışması sırasında kullanılan zamanlayıcı tanımı

        public Form1()
        {
            InitializeComponent();
            //.xlsl yani excel dosyalarını kullanmayı sağlayan uygulama pcdeki excel ile kullanıma geçebilmek için lisansa ihtiyaç duyuyor
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            random = new Random();
            this.Text = String.Empty;
            this.ControlBox = false;
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            pictureBox1.Visible = true;
            chart1.Visible = false;
            dataGridView1.Visible = false;

            InitializeNotificationTimer();
        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private Color SelectThemeColor()//themecolor.cs yardımıyla arayüze renk katmayı sağlıyor
        {
            int index = random.Next(ThemeColor.ColorList.Count);
            while (tempIndex == index)
            {
                index = random.Next(ThemeColor.ColorList.Count);
            }
            tempIndex = index;
            string color = ThemeColor.ColorList[index];
            return ColorTranslator.FromHtml(color);
        }

        private void InitializeNotificationTimer()//programlarda uyarı istenen uygulamaların ne sıklıkla uyarı vericeğini gösteren bir uygulama
        {
            notificationTimer = new Timer();
            notificationTimer.Interval = 3600000; // saat başı döngüye girmesini sağlar
            notificationTimer.Tick += NotificationTimer_Tick;
            notificationTimer.Start();
        }

        private void NotificationTimer_Tick(object sender, EventArgs e)
        {
            CheckNotificationConditions();//programların uyarı verme filtrelerine uygun olup olmamasına bakılıyor
        }

        private void CheckNotificationConditions()
        {
            DataTable dt = GetProductData();
            foreach (DataRow row in dt.Rows)
            {
                string name = row["name"].ToString();
                int time = Convert.ToInt32(row["time"]);
                //8 saaati geçen bir kullanımda uyarı vermeye başlayıp yine geçen her yeni saatte uyarılıyor
                if (time > 20 && (time - 20) % 4 == 0 && IsItemInRiskyTable(name, time.ToString()))
                {
                    SendNotification(name, time);
                }
            }
        }

        private void SendNotification(string name, int time)
        {
            //uyarının içeriği belirlenir
            MessageBox.Show($"Notification: 'time' value for '{name}' increased to {time}");
        }

        private void ActivateButton(object btnSender)
        {
            if (btnSender != null)
            {
                if (currentButton != (Button)btnSender)
                {
                    DisableButton();
                    Color color = SelectThemeColor();
                    currentButton = (Button)btnSender;
                    currentButton.BackColor = color;
                    currentButton.ForeColor = Color.White;
                    currentButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    PanelTitleBar.BackColor = color;
                    panellogo.BackColor = ThemeColor.ChangeColorBrightness(color, -0.3);
                }
            }
        }

        private void DisableButton()
        {
            foreach (Control previousBtn in panelMenu.Controls)
            {
                if (previousBtn.GetType() == typeof(Button))
                {
                    previousBtn.BackColor = Color.FromArgb(54, 72, 114);
                    previousBtn.ForeColor = Color.Gainsboro;
                    previousBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                }
            }
        }

        private DataTable GetProductData()
        {
            DataTable dt = new DataTable();
            //database sisteminin bağlantısını bir comut haline getirip nesneleşiyor
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM RTS"; //rts üzerinden her eşyayı almayı sağlıyor
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                adapter.Fill(dt);
            }

            return dt;
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            pictureBox1.Visible = false;
            chart1.Visible = false;
            dataGridView1.Visible = true;
            lblTitle.Text = "Actions";

            DataTable dt = GetProductData();
            dataGridView1.DataSource = dt;

            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            pictureBox1.Visible = false;
            chart1.Visible = true;
            dataGridView1.Visible = false;
            lblTitle.Text = "Current Log";

            // Ekrandaki halihazırda olan veriler temizlenir ve yeniden kullanıma hazır hale getirilir...
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add(new ChartArea());
            
            DataTable dt = GetDataFromDatabase();

            //Her data için karşılaştırmalı bir "chart" oluşturur.
            foreach (DataRow row in dt.Rows)
            {
                string name = row["name"].ToString();
                int time = Convert.ToInt32(row["time"]);

                Series series = new Series(name);
                series.Points.AddXY(name, time);
                chart1.Series.Add(series);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            pictureBox1.Visible = false;
            chart1.Visible = false;
            dataGridView1.Visible = true;

            lblTitle.Text = "Older Log";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
            openFileDialog.Title = "Select an Excel File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // file explorer açılıp 
                string filePath = openFileDialog.FileName;

                // Excel File dosya
                DataTable dt = ReadExcelFile(filePath);
                dataGridView1.DataSource = dt;

                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
        }

        private DataTable ReadExcelFile(string filePath)
        {
            
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                DataTable dt = new DataTable();

                foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns])
                {
                    dt.Columns.Add(firstRowCell.Text);
                }

                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    DataRow dataRow = dt.Rows.Add();
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        dataRow[col - 1] = worksheet.Cells[row, col].Value;
                    }
                }

                return dt;
            }
        }


        private void PanelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void Close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Maximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void Minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private DataTable GetDataFromDatabase()
        {
            DataTable dt = new DataTable();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT name, time FROM RTS";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                adapter.Fill(dt);
            }

            return dt;
        }

        private bool IsItemInRiskyTable(string name, string time)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM RiskyTable WHERE name = @name AND time = @time";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@time", time);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private void DeleteFromRiskyTable(string name, string time)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM RiskyTable WHERE name = @name AND time = @time";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@time", time);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void InsertIntoRiskyTable(string name, string time)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO RiskyTable (name, time) VALUES (@name, @time)";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@time", time);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void Marker(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex < 0)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                string name = selectedRow.Cells["name"].Value.ToString();
                string time = selectedRow.Cells["time"].Value.ToString();

                if (IsItemInRiskyTable(name, time))
                {
                    
                    DeleteFromRiskyTable(name, time);
                    selectedRow.DefaultCellStyle.BackColor = Color.White;
                    
                }
                else
                {
                    
                    InsertIntoRiskyTable(name, time);
                    selectedRow.DefaultCellStyle.BackColor = Color.Orange;
                    
                }
            }
        }
    }
}
