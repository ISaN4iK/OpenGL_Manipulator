using static RGR.OpenGL;

namespace RGR
{
    internal class Grid
    {
        public static void drawGrid(bool x, bool y, bool z, double s, double shift, int count = 10)
        {
            double h = s / count;
            glColor3d(0.75, 0.75, 0.75);
            glEnable(GL_LINE_STIPPLE);
            glLineStipple(1, 0xCCCC);
            glBegin(GL_LINES);
            for (int i = 0; (i <= count) && z; i++)
            {
                glVertex3d(0, i * h, shift); glVertex3d(s, i * h, shift);
                glVertex3d(i * h, 0, shift); glVertex3d(i * h, s, shift);
            }
            for (int i = 0; (i <= count) && y; i++)
            {
                glVertex3d(0, shift, i * h); glVertex3d(s, shift, i * h);
                glVertex3d(i * h, shift, 0); glVertex3d(i * h, shift, s);
            }
            for (int i = 0; (i <= count) && x; i++)
            {
                glVertex3d(shift, 0, i * h); glVertex3d(shift, s, i * h);
                glVertex3d(shift, i * h, 0); glVertex3d(shift, i * h, s);
            }
            glEnd();
            glDisable(GL_LINE_STIPPLE);
        }
    }
}