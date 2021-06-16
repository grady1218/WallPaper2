using System;
using System.Media;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO;

namespace WallPaper
{
    class Form1 : Form
    {

        public static Font MyFont = new Font("メイリオ", 30);

        Bitmap startImage;
        Bitmap currentImage;

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

            startImage = GetDefaultImage();

            keyboardHook.KeyDownEvent += (object sender, KeyEventArg e) =>
            {
                drawStrings.Add(new DrawString(new KeysConverter().ConvertToString(e.KeyCode), Screen.AllScreens[0].Bounds.Width));
            };

            keyboardHook.Hook();

            AddTask();

            Screen.GetBounds(this);

            Timer timer = new Timer
            {
                Interval = 1000 / 75,
                Enabled = true
            };
            timer.Tick += Update;

            Paint += (object sender, PaintEventArgs e) =>
            {
                if (isEnd)
                {
                    SetDefaultImageWallPaper();
                    Close();
                    Environment.Exit(0);
                    return;
                }
                e.Graphics.DrawImage( currentImage, 0, 0 );
                for(int i = 0; i < drawStrings.Count; i++) drawStrings[i].Draw(e);
            };

            Bitmap image;
            if (File.Exists("./background.png")) image = new Bitmap("./background.png");
            else image = startImage;
            

            currentImage = new Bitmap(image, new Size(1920, (int)(image.Height * (1920f / image.Width) ) ) );

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SetDefaultImageWallPaper();
            keyboardHook.UnHook();
        }


        private IntPtr getWindow()
        {
            DLL.SendMessageTimeout(DLL.FindWindow("Progman", null), 0x052C, new IntPtr(0), IntPtr.Zero, 0x0, 1000, out var result);

            var hoge = new IntPtr();
            DLL.EnumWindows((h, l) =>
            {
                var shell = DLL.FindWindowEx( h, IntPtr.Zero, "SHELLDLL_DefView", null );
                if(shell != IntPtr.Zero)
                {
                    hoge = DLL.FindWindowEx( IntPtr.Zero, h, "WorkerW", null );
                    return false;
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

            DrawWallPaper();
            Invalidate();
        }

        private void DrawWallPaper()
        {
            var window = getWindow();
            var hdc = DLL.GetDCEx(window, IntPtr.Zero, 0x403);
            var formDC = DLL.GetDCEx(Handle, IntPtr.Zero, 0x403);
            DLL.BitBlt(hdc, Math.Abs(Screen.AllScreens[0].Bounds.X), Math.Abs(Screen.AllScreens[0].Bounds.Y), Screen.AllScreens[0].Bounds.Width, Screen.AllScreens[0].Bounds.Height, formDC, 0, 0, DLL.TernaryRasterOperations.SRCCOPY);
            DLL.ReleaseDC(window, hdc);
            DLL.ReleaseDC(Handle, formDC);
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
                if (isStop) SetDefaultImageWallPaper();
                stop.Text = isStop ? "&再開" : "&一時停止";
            };
            exit.Click += (object sender, EventArgs e) => {
                SetDefaultImageWallPaper();
                if (isStop) Environment.Exit(0);
                isEnd = true;
            };
            
            contextMenuStrip.Items.Add(stop);
            contextMenuStrip.Items.Add(exit);
            NotifyIcon.ContextMenuStrip = contextMenuStrip;

        }

        private Bitmap GetDefaultImage()
        {
            var window = getWindow();
            var whdc = DLL.GetDCEx(window, IntPtr.Zero, 0x403);

            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage( bitmap );
            IntPtr ihdc = g.GetHdc();

            DLL.BitBlt( ihdc, 0, 0, bitmap.Width, bitmap.Height, whdc, 0, 0, DLL.TernaryRasterOperations.SRCCOPY );

            g.ReleaseHdc( ihdc );
            g.Dispose();
            DLL.ReleaseDC( window, whdc );

            return bitmap;

        }

        private void SetDefaultImageWallPaper()
        {
            var window = getWindow();
            var hdc = DLL.GetDCEx(window, IntPtr.Zero, 0x403);
            var hsrc = DLL.CreateCompatibleDC(hdc);
            var porg = DLL.SelectObject(hsrc, startImage.GetHbitmap());
            DLL.BitBlt(hdc, 0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, hsrc, 0, 0, DLL.TernaryRasterOperations.SRCCOPY);

            DLL.ReleaseDC( window, hdc );
        }

    }
}
