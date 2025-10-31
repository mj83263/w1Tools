namespace WinFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            this.dgv.Columns.Add(new DataGridViewTextBoxColumn() { AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells });
        }
    }
}
