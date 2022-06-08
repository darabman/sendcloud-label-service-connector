using KeyEncryptorLib;

namespace KeyEncryptorUtil
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            textBoxOutputKey.Text = KeyEncryptor.Crypt(textBoxInputKey.Text);
        }

        private void textBoxInputKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                buttonEncrypt_Click(this, EventArgs.Empty);
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxOutputKey.Text);
            MessageBox.Show("Encrypted key copied to clipboard");
        }
    }
}