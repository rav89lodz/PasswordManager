using PasswordManager.Entities;
using PasswordManager.Forms;
using System;
using System.Windows.Forms;

namespace PasswordManager
{    
    public partial class Form3 : Form
    {
        private readonly Form1 _form;

        public Form3(Form1 form)
        {
            _form = form;
            InitializeComponent();
            RunGrid();
        }

        private void RunGrid()
        {
            dataGridView1.Rows.Clear();
            TableRow[] tableRows = _form._dataBase.GetData();
            if (tableRows.Length < 1)
            {
                _form.Show();
                Hide();
            }
            else
            {
                foreach (TableRow row in tableRows)
                {
                    if (row == null)
                    {
                        continue;
                    }
                    dataGridView1.Rows.Add(row.Description, row.Login, "Kopiuj", row.Password, "Kopiuj", "Usuń", row.UUID);
                }
            }
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _form.Show();
            Hide();
        }
        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == 2)
            {
                Clipboard.SetText(dataGridView1[1, e.RowIndex].Value.ToString());
            }
            if (e.ColumnIndex == 4)
            {
                Clipboard.SetText(dataGridView1[3, e.RowIndex].Value.ToString());
            }
            if (e.ColumnIndex == 5)
            {
                DialogResult dialogResult = MessageBox.Show("Jesteś pewny, że chcesz usunąć?", "Usuń", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                var senderGrid = (DataGridView)sender;
                if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
                {
                    var tableRowElement = new TableRow(dataGridView1[6, e.RowIndex].Value.ToString(), // UUID
                                                        dataGridView1[1, e.RowIndex].Value.ToString(), // login
                                                        dataGridView1[3, e.RowIndex].Value.ToString(), // password
                                                        dataGridView1[0, e.RowIndex].Value.ToString()); // descryption
                    _form._dataBase.RemoveElement(tableRowElement);
                    RunGrid();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4(_form);
            form4.ShowDialog();
            if (!string.IsNullOrEmpty(_form.backupPassword))
            {
                string backupStatus = _form._dataBase.CreateBackup(_form.backupPassword);
                if (backupStatus != null)
                {
                    MessageBox.Show("Nie udało się utworzyć kopii zapasowej. \r\n" + backupStatus);
                    return;
                }
                MessageBox.Show("Kopia zapasowa została utworzona");
            }
        }
    }
}
