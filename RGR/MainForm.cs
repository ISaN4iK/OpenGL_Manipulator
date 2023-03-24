using System.Windows.Forms;


namespace RGR
{
    public partial class MainForm : Form
    {
        public delegate void updateDelegate();
        private updateDelegate update;
        public MainForm()
        {
            InitializeComponent();
            update = new updateDelegate(OnUpdateInfo);
            rc.updateDeleg = update;
        }

        private void OnUpdateInfo()
        {
            S.Text = $"S = {rc.S.ToString("0.00")}";
            ag.Text = $"φ = {rc.angFi.ToString("0.0")}°";
            at.Text = $"θ = {rc.angTheta.ToString("0.0")}°";
            az.Text = $"ψ = {rc.angPsi.ToString("0.0")}°";
            aw.Text = $"ω = {rc.aW.ToString("0.0")}°";
            aX.Text = $"α = {rc.aX.ToString("0.0")}°";
            aY.Text = $"β = {rc.aY.ToString("0.0")}°";
            M.Text = $"M 1 : {rc.M.ToString("0.0")}";
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            radioButtonOrtho.Checked = true;
            OnUpdateInfo();
        }

        private void gridCheckedChanged(object sender, System.EventArgs e)
        {
            rc.gridX = gridX.Checked;
            rc.gridY = gridY.Checked;
            rc.gridZ = gridZ.Checked;
            rc.Invalidate();
        }

        private void radioButtonOrtho_CheckedChanged(object sender, System.EventArgs e)
        {
            rc.ortho = radioButtonOrtho.Checked;
            rc.Invalidate();
        }
    }
}
