using System;
using System.IO;

public class Chip8
{
    static public ushort op;
    static private byte[] ram = new byte[4096];
    static private byte[] V = new byte[16];
    static private ushort I;
    static public ushort PC;
    static public byte[] gfx = new byte[64 * 32];
    static public byte delay_timer;
    static public byte sound_timer;
    static private ushort[] stack = new ushort[16];
    static private byte sp;
    static public byte[] key = new byte[16];
    static public bool drawflag = false;

    static public string file;
    static public byte[] chip8_fontset =
    {
      0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
      0x20, 0x60, 0x20, 0x20, 0x70, // 1
      0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
      0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
      0x90, 0x90, 0xF0, 0x10, 0x10, // 4
      0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
      0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
      0xF0, 0x10, 0x20, 0x40, 0x40, // 7
      0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
      0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
      0xF0, 0x90, 0xF0, 0x90, 0x90, // A
      0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
      0xF0, 0x80, 0x80, 0x80, 0xF0, // C
      0xE0, 0x90, 0x90, 0x90, 0xE0, // D
      0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
      0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    };

    public Chip8()
    {

    }
    public void initialize()
    {
        op = 0;
        I = 0;
        PC = 0x200;
        delay_timer = 0;
        sound_timer = 0;
        for (int i = 0; i < ram.Length; i++)
        {
            ram[i] = 0;
        }
        for (int i = 0; i < V.Length; i++)
        {
            V[i] = 0;
        }
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                gfx[i + j * 64] = 0;
            }

        }
        for (int i = 0; i < stack.Length; i++)
        {
            stack[i] = 0;
        }
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = 0;
        }
        for (int i = 0; i < chip8_fontset.Length; i++)
        {
            ram[i] = chip8_fontset[i];
        }

        using (BinaryReader b = new BinaryReader(File.Open(file, FileMode.Open)))
        {
            int length = (int)b.BaseStream.Length;
            for (int pos = 0, i = 0; pos < length; pos += sizeof(byte))
            {
                ram[0x200 + i] = b.ReadByte();
                i++;
            }
        }
    }

    public void cycle()
    {
        op = (ushort)(ram[PC] << 8 | ram[PC + 1]); // fetch opcode

        ushort nnn = (ushort)(op & 0x0FFF);
        ushort kk = (ushort)(op & 0x00FF);
        ushort x = (ushort)((op & 0x0F00) / 0x100);
        ushort y = (ushort)((op & 0x00F0) / 0x10);


        switch (op & 0xF000)
        {
            case 0x0000:
                switch (op & 0x00FF)
                {
                    case 0x00E0://Clear the display.
                        for (int i = 0; i < 64; i++)
                        {
                            for (int j = 0; j < 32; j++)
                            {
                                gfx[i + j * 64] = 0;
                            }

                        }
                        PC += 2;
                        break;
                    case 0x00EE://The interpreter sets the program counter to the address at the top of the stack, then subtracts 1 from the stack pointer.
                        --sp;
                        PC = stack[sp];
                        PC += 2;
                        break;
                }
                break;
            case 0x1000://The interpreter sets the program counter to nnn.
                PC = nnn;
                break;
            case 0x2000://The interpreter increments the stack pointer, then puts the current PC on the top of the stack. The PC is then set to nnn.
                stack[sp] = PC;
                sp++;
                PC = nnn;
                break;
            case 0x3000://The interpreter compares register Vx to kk, and if they are equal, increments the program counter by 2.
                if (V[x] == kk)
                    PC += 2;
                PC += 2;
                break;
            case 0x4000://The interpreter compares register Vx to kk, and if they are not equal, increments the program counter by 2.
                if (V[x] != kk)
                    PC += 2;
                PC += 2;
                break;
            case 0x5000://The interpreter compares register Vx to register Vy, and if they are equal, increments the program counter by 2
                if (V[x] == V[y])
                    PC += 2;
                PC += 2;
                break;
            case 0x6000://The interpreter puts the value kk into register Vx.
                V[x] = (byte)kk;
                PC += 2;
                break;
            case 0x7000://Adds the value kk to the value of register Vx, then stores the result in Vx.
                V[x] += (byte)kk;
                PC += 2;
                break;
            case 0x8000:
                switch (op & 0x000F)
                {
                    case 0x0000://Stores the value of register Vy in register Vx.
                        V[x] = V[y];
                        break;
                    case 0x0001://Set Vx = Vx OR Vy.
                        V[x] = (byte)(V[x] | V[y]);
                        break;
                    case 0x0002://Set Vx = Vx AND Vy.
                        V[x] = (byte)(V[x] & V[y]);
                        break;
                    case 0x0003://Set Vx = Vx XOR Vy.
                        V[x] = (byte)(V[x] ^ V[y]);
                        break;
                    case 0x0004://Set Vx = Vx + Vy, set VF = carry.
                        if (V[x] + V[y] > 0xFF)
                        {
                            V[0xF] = 1;
                            V[x] = (byte)((V[x] + V[y]) & 0x0FF);
                        }
                        else
                        {
                            V[0xF] = 0;
                            V[x] = (byte)(V[x] + V[y]);
                        }
                        break;
                    case 0x0005://Set Vx = Vx - Vy, set VF = NOT borrow.
                        if (V[x] > V[y]) { V[x] -= V[y]; V[0xF] = 1; } //borrow might be wrong
                        else { V[x] = (byte)(V[x] + 0xFF - V[y]); V[0xF] = 0; }
                        break;
                    case 0x0006://If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.
                        V[0xF] = (byte)(V[x] & 1);
                        V[x] = (byte)(V[x] >> 1);
                        break;
                    case 0x0007:
                        if (V[y] >= V[x]) { V[x] = (byte)(V[y] - V[x]); V[0xF] = 1; }//borrow might be wrong
                        else { V[x] = (byte)(V[y] + 0xFF - V[x]); V[0xF] = 0; }
                        break;
                    case 0x000E://If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2.
                        V[0xF] = (byte)(V[x] & 1);
                        V[x] = (byte)(V[x] << 1);
                        break;

                }
                PC += 2;
                break;
            case 0x9000://The values of Vx and Vy are compared, and if they are not equal, the program counter is increased by 2
                if (V[x] != V[y])
                    PC += 2;
                PC += 2;
                break;
            case 0xA000://The value of register I is set to nnn.
                I = nnn;
                PC += 2;
                break;
            case 0xB000://The program counter is set to nnn plus the value of V0.
                PC = (byte)(V[0] + nnn);
                break;
            case 0xC000://The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk. The results are stored in Vx. 
                Random r = new Random();
                V[x] = (byte)(r.Next(0, 256) & kk);
                PC += 2;
                break;
            case 0xD000:

                int height = op & 0x000F;
                V[0xF] = 0;
                for (int i = 0; i < height; i++)
                {
                    for (int c = 0; c < 8; c++)
                    {
                        int xcoor, ycoor;
                        xcoor = V[x] + c;
                        for (int co = 0; xcoor >= 64; co++)
                        {
                            xcoor = V[x] + c - 64 * (co + 1);
                        }

                        ycoor = V[y] + i;
                        for (int co = 0; ycoor >= 32; co++)
                        {
                            ycoor = V[y] + i - 32 * (co + 1);
                        }

                        byte pixel = (byte)((ram[I + i] & (byte)Math.Pow(2, 7 - c)) >> (7 - c));
                        int TEST = (V[x] + c) + (V[y] + i) * 64;
                        if ((gfx[xcoor + ycoor * 64] != 0) & (gfx[xcoor + ycoor * 64] ^ pixel) == 0) { V[0xF] = 1; }
                        gfx[xcoor + ycoor * 64] ^= pixel;

                    }
                }

                drawflag = true;
                PC += 2;
                break;
            case 0xE000:
                switch (op & 0x00FF)
                {
                    case 0x009E:
                        if (key[V[x]] != 0)
                        {
                            PC += 2;
                        }
                        break;
                    case 0x00A1:
                        if (key[V[x]] == 0)
                        {
                            PC += 2;
                        }
                        break;
                }
                PC += 2;
                break;
            case 0xF000:
                switch (op & 0x00FF)
                {
                    case 0x0007://Set Vx = delay timer value.
                        V[x] = delay_timer;
                        break;
                    case 0x000A:
                        bool keyPress = false;

                        for (byte i = 0; i < 16; ++i)
                        {
                            if (key[i] != 0)
                            {
                                V[x] = i;
                                keyPress = true;
                            }
                        }

                        // If we didn't received a keypress, skip this cycle and try again.
                        if (!keyPress)
                            return;

                        PC += 2;
                        //TODO
                        break;
                    case 0x0015://Set delay timer = Vx
                        delay_timer = V[x];
                        break;
                    case 0x0018://Set sound timer = Vx.
                        sound_timer = V[x];
                        break;
                    case 0x001E://Set I = I + Vx.
                        I = (ushort)((I + V[x]));
                        break;
                    case 0x0029://Set I = location of sprite for digit Vx.
                        I = (ushort)((V[x]) * 0x5);
                        break;
                    case 0x0033://Store BCD representation of Vx in memory locations I, I+1, and I+2.
                        ram[I] = (byte)(V[(op & 0x0F00) >> 8] / 100);
                        ram[I + 1] = (byte)((V[(op & 0x0F00) >> 8] / 10) % 10);
                        ram[I + 2] = (byte)((V[(op & 0x0F00) >> 8] % 100) % 10);
                        break;
                    case 0x0055:
                        for (int i = 0; i <= x; i++)
                        {
                            ram[I + i] = V[i];
                            //         I += (ushort)(x + 1);
                        }
                        break;
                    case 0x0065://The interpreter reads values from memory starting at location I into registers V0 through Vx.
                        for (int i = 0; i <= x; i++)
                        {
                            V[i] = ram[I + i];
                        }
                        break;
                }
                PC += 2;
                break;

            default:
                Console.WriteLine("error");
                break;
        }

    }


}
