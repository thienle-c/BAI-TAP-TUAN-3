using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace list
{
    public partial class Form1 : Form
    {
        // Một textbox tóm tắt dùng khi view != Details
        private TextBox detailSummary;

        public Form1()
        {
            InitializeComponent();

            // Tạo menu bằng code (File / View / Format ListView)
            MenuStrip menu = new MenuStrip();

            // FILE
            ToolStripMenuItem mFile = new ToolStripMenuItem("File");
            ToolStripMenuItem miOpen = new ToolStripMenuItem("Open...");
            ToolStripMenuItem miSave = new ToolStripMenuItem("Save...");
            ToolStripMenuItem miExit = new ToolStripMenuItem("Exit");
            miOpen.Click += MiOpen_Click;
            miSave.Click += MiSave_Click;
            miExit.Click += (s, e) => this.Close();
            mFile.DropDownItems.AddRange(new ToolStripItem[] { miOpen, miSave, new ToolStripSeparator(), miExit });

            // VIEW
            ToolStripMenuItem mView = new ToolStripMenuItem("View");
            ToolStripMenuItem miToggleDetail = new ToolStripMenuItem("Toggle Detail Panel");
            ToolStripMenuItem miGridLines = new ToolStripMenuItem("Grid lines") { CheckOnClick = true, Checked = true };
            ToolStripMenuItem miFullRow = new ToolStripMenuItem("Full row select") { CheckOnClick = true, Checked = true };
            miToggleDetail.Click += (s, e) => ToggleDetailPanel();
            miGridLines.CheckedChanged += (s, e) => lvStudents.GridLines = miGridLines.Checked;
            miFullRow.CheckedChanged += (s, e) => lvStudents.FullRowSelect = miFullRow.Checked;
            mView.DropDownItems.AddRange(new ToolStripItem[] { miToggleDetail, miGridLines, miFullRow });

            // FORMAT LISTVIEW
            ToolStripMenuItem mFormat = new ToolStripMenuItem("Format ListView");
            ToolStripMenuItem miDetails = new ToolStripMenuItem("Details");
            ToolStripMenuItem miLarge = new ToolStripMenuItem("LargeIcon");
            ToolStripMenuItem miSmall = new ToolStripMenuItem("SmallIcon");
            ToolStripMenuItem miList = new ToolStripMenuItem("List");
            ToolStripMenuItem miTile = new ToolStripMenuItem("Tile");

            // Map clicks to change list view mode and update Detail display
            miDetails.Click += (s, e) => SetListViewFormat(View.Details);
            miLarge.Click += (s, e) => SetListViewFormat(View.LargeIcon);
            miSmall.Click += (s, e) => SetListViewFormat(View.SmallIcon);
            miList.Click += (s, e) => SetListViewFormat(View.List);
            miTile.Click += (s, e) => SetListViewFormat(View.Tile);
            mFormat.DropDownItems.AddRange(new ToolStripItem[] { miDetails, miLarge, miSmall, miList, miTile });

            // Add top-level menus
            menu.Items.AddRange(new ToolStripItem[] { mFile, mView, mFormat });
            menu.Dock = DockStyle.Top;
            this.MainMenuStrip = menu;
            this.Controls.Add(menu);

            // ------- Khởi tạo ListView (chạy 1 lần) -------
            lvStudents.View = View.Details;
            lvStudents.FullRowSelect = true;
            lvStudents.GridLines = true;
            lvStudents.MultiSelect = false;
            lvStudents.SelectedIndexChanged += lvStudents_SelectedIndexChanged;
            lvStudents.Click += lvStudents_Click;
                                        
            // Add columns nếu chưa có (tránh lặp)
            if (lvStudents.Columns.Count == 0)
            {
                lvStudents.Columns.Add("Last Name", 140);
                lvStudents.Columns.Add("First Name", 180);
                lvStudents.Columns.Add("Phone", 100);
            }

            // Tạo control detailSummary (hiển thị tóm tắt khi view != Details)
            detailSummary = new TextBox();
            detailSummary.Multiline = true;
            detailSummary.ReadOnly = true;
            detailSummary.ScrollBars = ScrollBars.Vertical;
            // Đặt vị trí và kích thước giống groupBox1 (nếu dùng Designer, groupBox1 tồn tại)
            // Nếu groupBox1 tồn tại, đặt detailSummary chồng lên vị trí đó; nếu không, đặt mặc định
            if (this.Controls.Contains(groupBox1))
            {
                detailSummary.Location = new System.Drawing.Point(groupBox1.Left, groupBox1.Top);
                detailSummary.Size = new System.Drawing.Size(groupBox1.Width, groupBox1.Height);
            }
            else
            {
                detailSummary.Location = new System.Drawing.Point(470, 36);
                detailSummary.Size = new System.Drawing.Size(320, 200);
            }
            detailSummary.Visible = false; // mặc định ẩn (since default view is Details)
            this.Controls.Add(detailSummary);

            // Nếu groupBox1 không tồn tại trong Designer, tạo groupBox1 và textboxes? (Giả thiết ngài đã có groupBox1 và txtLastName/txtFirstName/txtPhone)
            // Thêm vài item mẫu nếu muốn (chỉ thêm khi list trống)
          

            // Gán nút Add (nếu tên nút của ngài là button1)
            // Nếu Designer đã gán handler button1_Click thì không cần dòng dưới, nhưng gán an toàn:
            try
            {
                this.btnAdd.Click -= button1_Click; // tránh gán đôi nếu đã có
            }
            catch { }
            this.btnAdd.Click += button1_Click;

            // Ensure initial Detail display corresponds to current view
            UpdateTbDetailFromView();
        }

        // Set ListView view and update Detail display
        private void SetListViewFormat(View view)
        {
            lvStudents.View = view;

            // Hiển thị tên View vào tbDetail
            tbDetail.Text = view.ToString();
        }
        private void UpdateTbDetailFromView()
        {
            if (lvStudents.SelectedItems.Count == 0)
            {
                tbDetail.Text = "detail";
                return;
            }

            var item = lvStudents.SelectedItems[0];

            switch (lvStudents.View)
            {
                case View.Details:
                    tbDetail.Text = $"Họ: {GetSubItemText(item, 0)}\r\nTên: {GetSubItemText(item, 1)}\r\nPhone: {GetSubItemText(item, 2)}";
                    break;
                case View.LargeIcon:
                case View.SmallIcon:
                    tbDetail.Text = $"Họ-Tên: {GetSubItemText(item, 0)} {GetSubItemText(item, 1)}\r\nPhone: {GetSubItemText(item, 2)}";
                    break;
                case View.List:
                case View.Tile:
                    tbDetail.Text = $"{GetSubItemText(item, 0)} {GetSubItemText(item, 1)} - {GetSubItemText(item, 2)}";
                    break;
            }
        }



        // Toggle detail panel visibility (View menu)
        private void ToggleDetailPanel()
        {
            if (this.Controls.Contains(groupBox1))
            {
                groupBox1.Visible = !groupBox1.Visible;
            }
            detailSummary.Visible = !detailSummary.Visible && lvStudents.View != View.Details ? true : detailSummary.Visible;
        }

        // Update which detail UI to show based on current ListView.View



        // Khi selection thay đổi, cập nhật cả hai hiển thị tương ứng
        private void lvStudents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvStudents.SelectedItems.Count == 0) return;

            var item = lvStudents.SelectedItems[0];

            // Đổ dữ liệu lên TextBox
            txtLastName.Text = GetSubItemText(item, 0);
            txtFirstName.Text = GetSubItemText(item, 1);
            txtPhone.Text = GetSubItemText(item, 2);

            // Luôn hiển thị View hiện tại
            tbDetail.Text = lvStudents.View.ToString();
        }




        // Khi click (giữ để tương thích)
        private void lvStudents_Click(object sender, EventArgs e)
        {
            lvStudents_SelectedIndexChanged(sender, e);
        }


        // Cập nhật detailSummary (tóm tắt) từ mục đang chọn
        private void UpdateDetailSummaryFromSelection()
        {
            if (lvStudents.SelectedItems.Count == 0)
            {
                detailSummary.Text = "detail";
                return;
            }

            var item = lvStudents.SelectedItems[0];
            // Tùy view, ta có thể thay đổi định dạng tóm tắt:
            // - LargeIcon / SmallIcon: show Last Name + First name lớn
            // - List/Tile: show 1 dòng tóm tắt
            // - Details: không dùng detailSummary
            string summary = "";
            switch (lvStudents.View)
            {
                case View.LargeIcon:
                case View.SmallIcon:
                    // Hiển thị dạng "Last Name - First Name" trên nhiều dòng
                    summary = $"Họ: {GetSubItemText(item, 0)}{Environment.NewLine}Tên: {GetSubItemText(item, 1)}{Environment.NewLine}Phone: {GetSubItemText(item, 2)}";
                    break;
                case View.List:
                case View.Tile:
                    // 1 dòng tóm tắt
                    summary = $"{GetSubItemText(item, 0)} {GetSubItemText(item, 1)} - {GetSubItemText(item, 2)}";
                    break;
                default:
                    summary = $"{GetSubItemText(item, 0)} {GetSubItemText(item, 1)} - {GetSubItemText(item, 2)}";
                    break;
            }
            detailSummary.Text = summary;
        }

        private string GetSubItemText(ListViewItem item, int index)
        {
            if (item == null) return "";
            if (item.SubItems.Count > index) return item.SubItems[index].Text;
            return "";
        }

        // Nút Add: thêm item, không thao tác cột
        private void button1_Click(object sender, EventArgs e)
        {
            // Sử dụng tên textbox là txtLastName, txtFirstName, txtPhone theo Designer ngài đã tạo
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Ngài vui lòng nhập đầy đủ thông tin!", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ListViewItem item = new ListViewItem(txtLastName.Text.Trim());
            item.SubItems.Add(txtFirstName.Text.Trim());
            item.SubItems.Add(txtPhone.Text.Trim());

            lvStudents.Items.Add(item);

            txtLastName.Clear();
            txtFirstName.Clear();
            txtPhone.Clear();
            txtLastName.Focus();

            // Cập nhật summary nếu đang ở chế độ non-Details
            UpdateDetailSummaryFromSelection();
        }

        // FILE -> OPEN: load CSV (Last,First,Phone)
        private void MiOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    lvStudents.Items.Clear();
                    using (var sr = new StreamReader(ofd.FileName, Encoding.UTF8))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 3)
                            {
                                lvStudents.Items.Add(new ListViewItem(new string[] { parts[0].Trim(), parts[1].Trim(), parts[2].Trim() }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể mở file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // FILE -> SAVE: lưu CSV (Last,First,Phone)
        private void MiSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                sfd.FileName = "listview_export.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        foreach (ListViewItem it in lvStudents.Items)
                        {
                            string last = it.SubItems.Count > 0 ? it.SubItems[0].Text.Replace(",", "") : "";
                            string first = it.SubItems.Count > 1 ? it.SubItems[1].Text.Replace(",", "") : "";
                            string phone = it.SubItems.Count > 2 ? it.SubItems[2].Text.Replace(",", "") : "";
                            sw.WriteLine($"{last},{first},{phone}");
                        }
                    }
                    MessageBox.Show("Lưu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể lưu file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Các handler rỗng tồn tại nếu Designer đã tham chiếu — giữ để tránh lỗi
        private void txt_LastName(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }
        private void detailsToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void largeIconToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void smallIconToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void listToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void tileToolStripMenuItem_Click(object sender, EventArgs e) { }

        private void tbDetail_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
