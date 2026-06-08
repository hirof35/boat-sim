using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TrigBoatApp
{
    public partial class Form1 : Form
    {
        // ⭐ 改良：WinForms用のタイマーに変更（スレッド問題を回避）
        private System.Windows.Forms.Timer _timer;
        private float _theta = 0;
        private const int TargetFps = 30;

        public Form1()
        {
            // 画面の初期設定
            this.Text = "Trig Boat Simulation";
            this.ClientSize = new Size(600, 600);
            this.BackColor = Color.White;

            // 描画のちらつきを抑えるダブルバッファリングを有効化
            this.DoubleBuffered = true;

            // ⭐ 改良：WinForms用タイマーの初期化とイベント登録
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000 / TargetFps; // 約33ms
            _timer.Tick += OnTimerTick;         // 独自のイベントハンドラに紐付け
            _timer.Start();

            // 描画イベントの登録
            this.Paint += OnPaint;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _theta += 1.0f; // 角度を進める
            this.Invalidate(); // これで安全にOnPaintが呼ばれるようになります
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // ボートのY座標（一番高い波の位置を見つけるため、初期値を画面最下部に設定）
            float highestBoatY = this.ClientSize.Height;

            // 3つの波のパラメータ（角度倍率、振幅、色）
            var waves = new[]
            {
                new { ThetaMult = 1.0f,  Amplitude = 40f, Color = Color.FromArgb(96, 0, 0, 255) },
                new { ThetaMult = 2.5f,  Amplitude = 30f, Color = Color.FromArgb(96, 0, 0, 225) },
                new { ThetaMult = 3.0f,  Amplitude = 20f, Color = Color.FromArgb(96, 30, 0, 200) }
            };

            // 波の描画とボート位置の計算
            foreach (var w in waves)
            {
                float currentBoatY = DrawWave(g, _theta * w.ThetaMult, w.Amplitude, w.Color);
                // 一番上＝Y座標の値が「最小」のものを選ぶ
                highestBoatY = Math.Min(highestBoatY, currentBoatY);
            }

            // ボート（緑の長方形）の描画
            using (Brush boatBrush = new SolidBrush(Color.FromArgb(0, 128, 0)))
            {
                // ボートのサイズ: 幅50, 高さ20、中央がX=300になるように配置
                g.FillRectangle(boatBrush, 275, highestBoatY - 20, 50, 20);
            }
        }

        /// <summary>
        /// 波を描画し、中央（X=300）のY座標を返します
        /// </summary>
        private float DrawWave(Graphics g, float currentTheta, float amplitude, Color color)
        {
            List<PointF> points = new List<PointF>();
            float boatY = 0;
            int clientWidth = this.ClientSize.Width;
            int clientHeight = this.ClientSize.Height;

            // 左下の開始点
            points.Add(new PointF(0, clientHeight));

            // 波の曲線を計算
            for (int x = 0; x <= clientWidth + 20; x += 20)
            {
                // 度数法からラジアンへの変換
                double radians = (x + currentTheta) * Math.PI / 180.0;
                float y = (float)(Math.Sin(radians) * amplitude) + (clientHeight / 2);

                points.Add(new PointF(x, y));

                // 画面中央（X=300）の時の波のY座標を記録
                if (x == 300)
                {
                    boatY = y;
                }
            }

            // 右下の終了点
            points.Add(new PointF(clientWidth, clientHeight));

            // 多角形（波）の塗りつぶし描画
            using (Brush waveBrush = new SolidBrush(color))
            {
                g.FillPolygon(waveBrush, points.ToArray());
            }

            return boatY;
        }
    }
}
