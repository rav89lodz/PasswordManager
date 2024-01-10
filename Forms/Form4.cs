using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PasswordManager.Forms
{
    public partial class Form4 : Form
    {
        private readonly Form1 _form;

        [DllImport("kernel32.dll")]
        public static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref String lpBuffer, int nSize, ref IntPtr Arguments);

        public Form4(Form1 form)
        {
            _form = form;
            InitializeComponent();
        }

        public static string GetErrorMessage(int errorCode)
        {
            int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
            int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
            int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

            string lpMsgBuf = null;
            int dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;

            IntPtr lpSource = IntPtr.Zero;
            IntPtr lpArguments = IntPtr.Zero;
            int returnVal = FormatMessage(dwFlags, ref lpSource, errorCode, 0, ref lpMsgBuf, 255, ref lpArguments);

            if (returnVal == 0)
            {
                throw new Exception("Failed to format message for error code " + errorCode.ToString() + ". ");
            }
            return lpMsgBuf;
        }

        private void NextStep()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Uzupełnij pole z hasłem");
                return;
            }
            try
            {
                if(string.IsNullOrEmpty(_form.fileName))
                {
                    _form.backupPassword = textBox1.Text;
                    this.Hide();
                    return;
                }
                bool userTest = _form._user.CheckUserHaveAccessToBackup(_form.fileName, textBox1.Text);
                if (userTest == false)
                {
                    MessageBox.Show("Odmowa dostępu");
                    return;
                }
                string backupStatus = _form._dataBase.RestoreBackup(_form.fileName, textBox1.Text);
                if (backupStatus != null)
                {
                    MessageBox.Show("Nie udało się przywrócić danych z kopii zapasowej. \r\n" + backupStatus);
                    return;
                }
                MessageBox.Show("Kopia zapasowa została odtworzona, dane przywrócono");

                _form.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_form.fileName))
            {
                this.Hide();
                return;
            }
            _form.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NextStep();
        }        

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NextStep();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (textBox1.PasswordChar == '*')
            {
                textBox1.PasswordChar = '\0';
                pictureBox1.Image = Properties.Resources.okotak;
            }
            else
            {
                textBox1.PasswordChar = '*';
                pictureBox1.Image = Properties.Resources.okonie;
            }
        }
    }
}
