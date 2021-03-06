﻿using System;
using System.Threading;

namespace FantasyFootball.Terminal
{
    public class JinglePlayer
    {
        public static void Play()
        {
            const int BPM = 360;

            const int SIXTEENTH = (60 * 1000) / (4 * BPM);
            const int EIGHTH = 2 * SIXTEENTH;
            const int QUARTER = 2 * EIGHTH;
            const int HALF = 2 * QUARTER;
            const int WHOLE = 2 * HALF;

            Thread.Sleep(QUARTER);
            Console.Beep(523, EIGHTH);
            Console.Beep(587, EIGHTH);
            Console.Beep(659, QUARTER);
            Console.Beep(587, EIGHTH);
            Console.Beep(523, EIGHTH);

            Console.Beep(784, WHOLE);

            Thread.Sleep(QUARTER);
            Console.Beep(523, EIGHTH);
            Console.Beep(587, EIGHTH);
            Console.Beep(659, QUARTER);
            Console.Beep(587, EIGHTH);
            Console.Beep(523, EIGHTH);

            Console.Beep(988, WHOLE);

            Thread.Sleep(QUARTER);
            Console.Beep(523, EIGHTH);
            Console.Beep(587, EIGHTH);
            Console.Beep(659, QUARTER);
            Console.Beep(587, EIGHTH);
            Console.Beep(523, EIGHTH);

            Console.Beep(784, HALF);
            Console.Beep(698, HALF);//

            Console.Beep(659, HALF);//
            Console.Beep(831, HALF);//

            Console.Beep(740, WHOLE);//

            Thread.Sleep(QUARTER);
            Console.Beep(784, EIGHTH);
            Console.Beep(831, EIGHTH);
            Console.Beep(1245, QUARTER);
            Console.Beep(784, EIGHTH);
            Console.Beep(831, EIGHTH);

            Console.Beep(1245, QUARTER);
            Console.Beep(784, EIGHTH);
            Console.Beep(831, EIGHTH);
            Console.Beep(1245, QUARTER);
            Console.Beep(1568, QUARTER);

            Console.Beep(1175, 2 * WHOLE);
        }
    }
}
