using System;
using System.Drawing;
using System.Windows.Forms;

namespace CHIP8
{
    public partial class MainForm : Form
    {
        int emuRate = 20;
        Chip8 chip8 = new Chip8();
        public MainForm()
        {
            InitializeComponent();

        }

        public void draw()
        {
            Chip8.drawflag = false;
            SolidBrush brush = new SolidBrush(Color.White);
            Graphics formGraphics = panel1.CreateGraphics();
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (Chip8.gfx[i + j * 64] != 0)
                    {
                        brush.Color = Color.White;
                        formGraphics.FillRectangle(brush, new Rectangle(10 * i, 10 * j, 10, 10));
                    }
                    else
                    {
                        brush.Color = Color.Black;
                        formGraphics.FillRectangle(brush, new Rectangle(10 * i, 10 * j, 10, 10));
                    }

                }
            }
            brush.Dispose();
            formGraphics.Dispose();
        }


        private void timer_game_Tick(object sender, EventArgs e)
        {
            if (Chip8.delay_timer > 0)
            {
                Chip8.delay_timer--;
            }
            if (Chip8.sound_timer > 0)
            {
                Chip8.sound_timer--;
            }
        }

        private void timer_cycle_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < emuRate; i++)
                chip8.cycle();
            if (Chip8.drawflag == true)
                draw();
            toolStripStatusLabel1.Text = "Program Counter " + Chip8.PC.ToString();
            toolStripStatusLabel2.Text = "OPCode " + Chip8.op.ToString("X2");
            toolStripLabel6.Text = emuRate.ToString() + "X";
        }

        private void Key_Down(object sender, KeyEventArgs e)
        {
            timer_input.Stop();
            switch (e.KeyCode)
            {
                case Keys.X:
                    Chip8.key[0] = 1;
                    break;
                case Keys.D1:
                    Chip8.key[1] = 1;
                    break;
                case Keys.D2:
                    Chip8.key[2] = 1;
                    break;
                case Keys.D3:
                    Chip8.key[3] = 1;
                    break;
                case Keys.Q:
                    Chip8.key[4] = 1;
                    break;
                case Keys.W:
                    Chip8.key[5] = 1;
                    break;
                case Keys.E:
                    Chip8.key[6] = 1;
                    break;
                case Keys.A:
                    Chip8.key[7] = 1;
                    break;
                case Keys.S:
                    Chip8.key[8] = 1;
                    break;
                case Keys.D:
                    Chip8.key[9] = 1;
                    break;
                case Keys.Z:
                    Chip8.key[10] = 1;
                    break;
                case Keys.C:
                    Chip8.key[11] = 1;
                    break;
                case Keys.D4:
                    Chip8.key[12] = 1;
                    break;
                case Keys.R:
                    Chip8.key[13] = 1;
                    break;
                case Keys.F:
                    Chip8.key[14] = 1;
                    break;
                case Keys.V:
                    Chip8.key[15] = 1;
                    break;
            }
            timer_input.Start();
        }

        private void timer_input_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < Chip8.key.Length; i++)
            {
                Chip8.key[i] = 0;
            }
            timer_input.Stop();
        }


        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Chip8.file = openFileDialog1.FileName;
            }
            chip8.initialize();
            timer_game.Start();
            timer_cycle.Start();
        }

        private void toolStripLabel2_Click(object sender, EventArgs e)
        {
            chip8.initialize();
            timer_game.Start();
            timer_cycle.Start();
        }

        private void toolStripLabel4_Click(object sender, EventArgs e)
        {
            emuRate++;
        }

        private void toolStripLabel5_Click(object sender, EventArgs e)
        {
            if (emuRate > 0)
            {
                emuRate--;
            }
        }
    }
}
