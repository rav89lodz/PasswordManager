using PasswordManager.Entities;
using PasswordManager.Forms;
using PasswordManager.Interfaces;
using System;
using System.IO;
using System.Windows.Forms;

namespace PasswordManager
{    
    public partial class Form1 : Form
    {
        public readonly IUserService _user;
        public readonly IPasswordService _password;
        public readonly IEncryptionService _encryption;
        public readonly IDataBaseService _dataBase;
        public string fileName;
        public string backupPassword;

        public Form1(IUserService user, IPasswordService password, IEncryptionService encryption, IDataBaseService dataBase)
        {
            _user = user;
            _password = password;
            _encryption = encryption;
            _dataBase = dataBase;
            InitializeComponent();
            comboBox1.Items.Add("Silne");
            comboBox1.Items.Add("Bardzo silne");
            comboBox1.Items.Add("Ekstremalnie silne");
            comboBox1.SelectedItem = "Bardzo silne";
            _dataBase.CreateFileOnStart();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            label2.Visible = true;
            label2.Text = _password.PassGen(comboBox1.SelectedIndex);
        }
       
        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label2.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text) && (string.IsNullOrWhiteSpace(textBox2.Text) || label2.Text == "123"))
            {
                MessageBox.Show("Pola z loginem i hasłem nie mogą być puste");
            }
            else if(label2.Text == "123" && string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Najpierw wygeneruj hasło lub wpisz swoje");
            }
            else
            {
                string passwd = label2.Text;
                if(!string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    passwd = textBox2.Text;
                }
                if (string.IsNullOrWhiteSpace(textBox3.Text))
                {
                    textBox3.Text = "-";
                }
                var tableRow = new TableRow(null, textBox1.Text, passwd, textBox3.Text);
                _dataBase.SaveToFile(tableRow);
                MessageBox.Show("Hasło zostało przypisane do loginu!");
            }
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (File.Exists(_dataBase.GetFileName()))
            {
                var form2 = new Form2(this);
                form2.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Odmowa dostępu lub plik z danymi nie istnieje.");
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(textBox2.Text))
            {
                button1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://rch-software.pl/index.php?action=lic");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if(textBox2.PasswordChar == '*')
            {
                textBox2.PasswordChar = '\0';
                pictureBox1.Image = Properties.Resources.okotak;
            }
            else
            {
                textBox2.PasswordChar = '*';
                pictureBox1.Image = Properties.Resources.okonie;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.ShowDialog(this);

            fileName = openFileDialog1.SafeFileName;
            if(string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var form4 = new Form4(this);
            form4.Show();
            this.Hide();
        }
    }
}
