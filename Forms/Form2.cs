using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PasswordManager
{
    public partial class Form2 : Form
    {
        private readonly Form1 _form;

        [DllImport("kernel32.dll")]
        public static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref String lpBuffer, int nSize, ref IntPtr Arguments);

        public Form2(Form1 form)
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
            if (FieldsAreNotEmpty() == false)
            {
                MessageBox.Show("Uzupełnij pola z loginem i hasłem");
                return;
            }
            try
            {
                int message = _form._user.CheckUserIsSystemUser(textBox1.Text, textBox2.Text);
                if (message > 0)
                {
                    string errmsg = GetErrorMessage(message);
                    MessageBox.Show("Odmowa dostępu \r\n" + errmsg);
                    return;
                }
                bool userTest = _form._user.CheckUserHaveAccess(textBox1.Text, null);
                if (userTest == false)
                {
                    MessageBox.Show("Odmowa dostępu");
                    return;
                }
                var form3 = new Form3(_form);
                form3.Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            NextStep();
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _form.Show();
            this.Hide();
        }

        private bool FieldsAreNotEmpty()
        {
            if(string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                return false;
            }
            return true;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NextStep();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (textBox2.PasswordChar == '*')
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
    }
}