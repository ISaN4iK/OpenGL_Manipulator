using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static RGR.MainForm;
using static System.Math;


namespace RGR
{
    public partial class RenderControl : OpenGL
    {
        private double size;
        public bool ortho;
        private double AspectRatio { get => (double)Width / Height; }
        private double xMin { get => (AspectRatio > 1) ? -size * AspectRatio : -size; }
        private double xMax { get => (AspectRatio > 1) ? +size * AspectRatio : +size; }
        private double yMin { get => (AspectRatio < 1) ? -size / AspectRatio : -size; }
        private double yMax { get => (AspectRatio < 1) ? +size / AspectRatio : +size; }
        private double zMin { get => -size * 2.0; }
        private double zMax { get => +size * 2.0; }

        public double aX { get { return ax; } }
        public double aY { get { return ay; } }
        public double aW { get { return aw; } }
        public double M { get { return m; } }

        public bool gridX { get; set; }
        public bool gridY { get; set; }
        public bool gridZ { get; set; }


        // Axes and mouse control 
        private double ax;
        private double ay;
        private double aw;
        private double m;

        private bool fDown = false;
        private double x0, y0;

        public double angTheta;
        public double angFi;
        public double angPsi;
        public double a;
        public double b;
        public double S;

        private double maxFi;
        private double minFi;
        private double maxPsi;
        private double minPsi;

        private IntPtr quadric;
        public updateDelegate updateDeleg { get; set; }

        uint[] textures = new uint[1];

        public RenderControl()
        {
            InitializeComponent();
            MouseWheel += OnMouseWheel;
            size = 1.5;
            ortho = true;

            ax = +20;
            ay = -30;
            //ax = 0;
            //ay = 0;
            aw = 0;
            m = 1;

            a = 0.4;
            b = 0.7;

            angFi = 30.0;
            angPsi = 90.0;

            maxFi = 165.0;
            minFi = 5.0;

            maxPsi = 180 - angTheta;
            minPsi = -angTheta;

            quadric = gluNewQuadric();
        }

        private void LoadTexture(string filename)
        {
            Bitmap bitmap = new Bitmap(filename);
            Rectangle bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            BitmapData bitmapData = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            glGenTextures(1, textures);
            glBindTexture(GL_TEXTURE_2D, textures[0]);
            glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGB, bitmap.Width, bitmap.Height, 0, /*GL_BGR*/ GL_RGB, GL_UNSIGNED_BYTE, bitmapData.Scan0);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_LINEAR);
            bitmap.UnlockBits(bitmapData);
            bitmap.Dispose();
        }

        private void RenderControl_Render(object sender, EventArgs e)
        {
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            glLoadIdentity();
            glViewport(0, 0, Width, Height);
            if (ortho)
                glOrtho(xMin, xMax, yMin, yMax, zMin, zMax);
            else
            {
                gluPerspective(88.0, (double)Width / Height, 0.5, 10);
                glTranslated(0, 0, -size * 1.3);
            }
            glEnable(GL_DEPTH_TEST);

            gluQuadricDrawStyle(quadric, GLU_FILL);
            //gluQuadricDrawStyle(quadric, GLU_LINE);
            gluQuadricOrientation(quadric, GLU_OUTSIDE);

            glRotated(ax, 1, 0, 0);
            glRotated(ay, 0, 1, 0);
            glScaled(m, m, m);

            float[] lightAmbient = new float[] { 0.25f, 0.25f, 0.25f, 1.0f };
            float[] lightDiffuse = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] lightSpecular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] lightPos = new float[] { 0.0f, 0.0f, (float)zMax * 0.8f, 1.0f };

            glLightfv(GL_LIGHT0, GL_AMBIENT, lightAmbient);
            glLightfv(GL_LIGHT0, GL_DIFFUSE, lightDiffuse);
            glLightfv(GL_LIGHT0, GL_SPECULAR, lightSpecular);
            glLightfv(GL_LIGHT0, GL_POSITION, lightPos);

            glColor(Color.Black);
            Axes(size * 0.9);

            glEnable(GL_NORMALIZE);
            glEnable(GL_LIGHTING);
            glEnable(GL_LIGHT0);
            glLightModelf(GL_FRONT_AND_BACK, GL_LIGHT_MODEL_LOCAL_VIEWER);
            glColorMaterial(GL_FRONT_AND_BACK, GL_AMBIENT_AND_DIFFUSE);
            S = Sqrt(a * a + b * b - 2.0 * a * b * Cos((angFi) * PI / 180.0));
            angTheta = Acos((S * S + b * b - a * a) / (2 * S * b)) / PI * 180.0;
            maxPsi = 180 - angTheta - 5.0;
            minPsi = -angTheta + 5.0;
            angPsi = (angPsi < minPsi) ? minPsi : (angPsi > maxPsi) ? maxPsi : angPsi;

            double maxS = Sqrt(a * a + b * b - 2.0 * a * b * Cos((maxFi) * PI / 180.0)) + a / 2;
            double oBx = S * Sin(angTheta * PI / 180.0);
            double oBy = a * Sin((90 - angFi) * PI / 180.0);

            glRotated(aw, 0, -1, 0);
            glPushMatrix();
            glRotatef((float)angFi, 0, 0, -10);
            drawCylinder(size * 0.01, a, 100, 100, 1f, 0f, 0f);
            //Segment(a, Color.Red, b * 0.1, 5);
            glPopMatrix();

            glPushMatrix();
            glTranslated(0, b, 0);
            glRotated(180 - angTheta, 0, 0, -1);
            drawCylinder(size * 0.01, maxS, 100, 100, 1f, (float)(109.0 / 255.0), 0f);
            //Segment(maxS, Color.Orange, b * 0.1, 5);
            glPopMatrix();

            /* TEXTURE */
            LoadTexture("tex1.jpg");
            glEnable(GL_TEXTURE_2D);
            glTexEnvf(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_MODULATE);
            gluQuadricTexture(quadric, 1);

            glPushMatrix();
            glTranslated(oBx, oBy, 0);
            glRotated(angPsi, 0, 0, -1);
            drawCylinder(size * 0.01, b, 100, 100, 1f, 0f, 1f);
            //Segment(b, Color.Green, b * 0.1, 5);
            glPopMatrix();

            glDisable(GL_TEXTURE_2D);
            glDisable(GL_LIGHTING);
            glDisable(GL_DEPTH_TEST);

            updateDeleg.Invoke();
        }

        private void drawCylinder(
            /*double x0, double y0, double z0,*/
            double R, double h, int slices, int stacks, float r, float g, float b)
        {
            float[] materialAmbient = new float[] { r, g, b, 1.0f };
            float[] materialDiffuse = new float[] { r, g, b, 1.0f };
            float[] materialSpecular = new float[] { r, g, b, 1.0f };
            float materialShininess = 50.0f;

            glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, materialAmbient);
            glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, materialDiffuse);
            glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, materialSpecular);
            glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, materialShininess);

            //glPushMatrix();
            //glTranslated(x0, y0, z0);
            glRotated(90, -1, 0, 0);
            gluCylinder(quadric, R, R, h, slices, stacks);
            //glPopMatrix();
        }

        private void Segment(double size, Color color, double width, int widthLine)
        {
            glColor(color);
            glLineWidth(widthLine);
            glBegin(GL_LINE_STRIP);
            glVertex3d(0.0, 0.0, width);
            glVertex3d(0.0, size, width);
            glVertex3d(0.0, size, -width);
            glVertex3d(0.0, 0.0, -width);
            glVertex3d(0.0, 0.0, width);
            glEnd();
            glLineWidth(1);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                fDown = true;
                x0 = e.Location.X;
                y0 = e.Location.Y;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (fDown && (e.Button == MouseButtons.Left))
                fDown = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (fDown)
            {
                ay -= (x0 - e.Location.X) / 2.0;
                ax -= (y0 - e.Location.Y) / 2.0;
                x0 = e.Location.X;
                y0 = e.Location.Y;
                Invalidate();
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            m += e.Delta / 1000.0;
            Invalidate();
        }

        private void Axes(double size)
        {
            double a = size / 15.0;
            glColor(Color.Black);
            glBegin(GL_LINES);
            glVertex3d(-a, 0, 0); glVertex3d(+size, 0, 0);
            glVertex3d(0, -a, 0); glVertex3d(0, +size, 0);
            glVertex3d(0, 0, -a); glVertex3d(0, 0, +size);
            glEnd();
            DrawText("+X", size + a, 0, 0);
            DrawText("+Y", 0, size + a, 0);
            DrawText("+Z", 0, 0, size + a);

            Grid.drawGrid(gridX, gridY, gridZ, size, -size * 0.1, 5);
        }

        private void RenderControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            const double stepAng = 1;
            if (e.KeyCode == Keys.Up) angFi = (angFi - stepAng) < minFi ? (minFi) : (angFi - stepAng);
            if (e.KeyCode == Keys.Down) angFi = (angFi + stepAng) > maxFi ? (maxFi) : (angFi + stepAng);
            if (e.KeyCode == Keys.Left) aw = (aw + stepAng > 180) ? (aw - 360 + stepAng) : (aw + stepAng);
            if (e.KeyCode == Keys.Right) aw = (aw - stepAng < -180) ? (aw + 360 - stepAng) : (aw - stepAng);
            if (e.KeyCode == Keys.PageUp) angPsi = (angPsi - stepAng) < minPsi ? (minPsi) : (angPsi - stepAng);
            if (e.KeyCode == Keys.PageDown) angPsi = (angPsi + stepAng) > maxPsi ? (maxPsi) : (angPsi + stepAng);

            Invalidate();
        }
    }
}

