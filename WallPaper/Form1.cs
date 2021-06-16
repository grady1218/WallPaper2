﻿using System;
using System.Media;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Media;
using System.Collections.Generic;

namespace WallPaper
{
    class Form1 : Form
    {

        public static Font MyFont = new Font("メイリオ", 30);

        Point ballPoint = new Point(100, 200);
        Point Vector = new Point( 8, 15 );

        Bitmap myWall;

        NotifyIcon NotifyIcon;

        bool isEnd = false;
        bool isStop = false;

        GetKey keyboardHook = new GetKey();

        List<DrawString> drawStrings = new List<DrawString>();

        public Form1()
        {
            DoubleBuffered = true;
            BackColor = System.Drawing.Color.FromArgb(0,0,0);
            Opacity = 0;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            ShowInTaskbar = false;

            StartPosition = FormStartPosition.CenterScreen;
            Location = Screen.AllScreens[0].Bounds.Location;

            keyboardHook.KeyDownEvent += (object sender, KeyEventArg e) =>
            {
                drawStrings.Add(new DrawString(new KeysConverter().ConvertToString(e.KeyCode), Screen.AllScreens[0].Bounds.Width));
            };

            keyboardHook.Hook();

            AddTask();

            Screen.GetBounds(this);
            //BackgroundImage = new Bitmap(new Bitmap(@"./annna.jpg"), new Size(Bounds.Width, Bounds.Height));

            MediaPlayer player = new MediaPlayer();
            Timer timer = new Timer()
            {
                Interval = 1000 / 75,
                Enabled = true
            };
            timer.Tick += Update;

            Paint += (object sender, PaintEventArgs e) =>
            {
                if (isEnd)
                {
                    Close();
                    Environment.Exit(0);
                    return;
                }
                e.Graphics.DrawImage(myWall,0,-100);
                for(int i = 0; i < drawStrings.Count; i++) drawStrings[i].Draw(e);
            };

            var a = new Bitmap("./kawaii.png");
            myWall = new Bitmap(a, new Size(1920, (int)(a.Height * (1920f / a.Width))));

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SetKawaii();
            keyboardHook.UnHook();
        }


        private IntPtr[] getWindow()
        {
            DLL.SendMessageTimeout(DLL.FindWindow("Progman", null), 0x052C, new IntPtr(0), IntPtr.Zero, 0x0, 1000, out var result);

            var hoge = new IntPtr[2];
            DLL.EnumWindows((h, l) =>
            {
                var shell = DLL.FindWindowEx( h, IntPtr.Zero, "SHELLDLL_DefView", null );
                if(shell != IntPtr.Zero)
                {
                    hoge[0] = DLL.FindWindowEx( IntPtr.Zero, h, "WorkerW", null );
                }
                var c = DLL.FindWindowEx( h, IntPtr.Zero, "Chrome_WidgetWin_1", null);
                if (c != IntPtr.Zero)
                {
                    hoge[1] = c;
                }
                return true;
            },IntPtr.Zero);
            return hoge;
        }

        private void Update(object sender, EventArgs e)
        {
            if (isStop) return;

            for (int i = 0; i < drawStrings.Count; i++)
            {
                drawStrings[i].Process(Screen.AllScreens[0].Bounds.Height);
                if (drawStrings[i].isEnded)
                {
                    drawStrings.RemoveAt(i);
                    i -= 1;
                }
            }

            var window = getWindow();
            var hdc = DLL.GetDCEx(window[0], IntPtr.Zero, 0x403);
            var formDC = DLL.GetDCEx(Handle, IntPtr.Zero, 0x403);
            DLL.BitBlt( hdc, Math.Abs(Screen.AllScreens[1].Bounds.X), Math.Abs(Screen.AllScreens[1].Bounds.Y), 3000, 1280, formDC, 0, 0, DLL.TernaryRasterOperations.SRCCOPY );
            DLL.ReleaseDC( window[0], hdc );
            DLL.ReleaseDC( Handle, formDC );
            Invalidate();
        }

        private void AddTask()
        {

            NotifyIcon = new NotifyIcon
            {
                Icon = new Icon("Wall.ico"),
                Visible = true,
                Text = "WallPaper"
            };

            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem stop = new ToolStripMenuItem()
            {
                Text = "&一時停止",
            };
            ToolStripMenuItem exit = new ToolStripMenuItem()
            {
                Text = "&終了",
            };
            stop.Click += (object sender, EventArgs e) =>
            {
                isStop = !isStop;
                if (isStop) SetKawaii();
                stop.Text = isStop ? "&再開" : "&一時停止";
            };
            exit.Click += (object sender, EventArgs e) => {
                if (isStop) Environment.Exit(0);
                isEnd = true;
            };
            
            contextMenuStrip.Items.Add(stop);
            contextMenuStrip.Items.Add(exit);
            NotifyIcon.ContextMenuStrip = contextMenuStrip;

        }

        private void SetKawaii()
        {
            var window = getWindow();
            var hdc = DLL.GetDCEx(window[0], IntPtr.Zero, 0x403);
            var hsrc = DLL.CreateCompatibleDC(hdc);
            var porg = DLL.SelectObject(hsrc,myWall.GetHbitmap());
            DLL.BitBlt(hdc, Math.Abs(Screen.AllScreens[1].Bounds.X), Math.Abs(Screen.AllScreens[1].Bounds.Y) - 100, 3000, 1280, hsrc, 0, 0, DLL.TernaryRasterOperations.SRCCOPY);
        }

    }
}
