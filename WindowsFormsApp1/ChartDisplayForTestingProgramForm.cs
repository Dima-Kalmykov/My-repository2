﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MetroFramework;

namespace WindowsFormsApp1
{
    public partial class ChartDisplayForTestingProgramForm : MetroFramework.Forms.MetroForm
    {
        public ChartDisplayForTestingProgramForm(MainForm mf, int formNumber,
            List<ChartDisplayForTestingProgramForm> forms)
        {
            InitializeComponent();
            label5.Location = new Point(2000, 2000);
            textBox1.Text = string.Empty;
            FormCounter = formNumber;
            textBox1.KeyPress += FormNumberTextBoxKeyPress;
            this.forms = forms;
            this.forms.Add(this);
            ListOfFormsForTestingProgram.SetSwitcherTools(this.forms, formNumber);
            foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                form.label3.Text = ListOfFormsForTestingProgram.Pointer.ToString();
            }

            label4.Text = $"Number form: {formNumber}";
            var res = $"Array: ";
            res = ListOfFormsForTestingProgram.ChartFormsForTestingProgram
                .Aggregate(res, (current, form) => current + form.label4 + "    ");

            label5.Text = res;

            SetAllTools(mf);
        }

        public int FormCounter;
        private readonly List<ChartDisplayForTestingProgramForm> forms;
        private ToolsForDrawing toolsForDrawing;
        private readonly List<Stopwatch> timers = new List<Stopwatch>();
        private readonly List<Edge> points = new List<Edge>();
        private readonly Stopwatch currentTimeOfTestingTimer = new Stopwatch();
        private bool isFirstVertex;
        private Timer mainTimer = new Timer();
        private MainForm mainForm;

        private long requireTimeForTesting;
        private List<Vertex> vertices = new List<Vertex>();
        private List<Edge> edges = new List<Edge>();

        private float coefficient;
        private int totalCountOfPoints;

        /// <summary>
        /// Установка всех инструментов.
        /// </summary>
        /// <param name="mf"> Главная форма </param>
        public void SetAllTools(MainForm mf)
        {
            SetUpMainTools(mf);

            SetUpChart();

            foreach (var item in vertices)
            {
                item.HasPoint = false;
            }

            vertices[0].HasPoint = true;
            isFirstVertex = true;

            edges = toolsForDrawing.GetOtherGraphWithGivenAmountOfEdgesAndVertices(vertices, edges);

            SetUpTimers();
        }

        /// <summary>
        /// Метод для моделирования блуждания.
        /// </summary>
        /// <param name="adjacencyList"> список смежности </param>
        public void MainTick(List<Edge>[] adjacencyList)
        {
            timeLabel.Text = $@"Time in millisecond: {currentTimeOfTestingTimer.ElapsedMilliseconds}";
            // Если готова выпустить точку.
            var count = points.Count;
            coefficient = (float)(coefficientTrackBar.Value / 10.0);

            for (var i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].HasPoint)
                {
                    vertices[i].HasPoint = false;

                    if (isFirstVertex)
                    {
                        isFirstVertex = false;
                        totalCountOfPoints += adjacencyList[i].Count;
                    }
                    else
                    {
                        totalCountOfPoints += adjacencyList[i].Count - 1;
                    }

                    points.AddRange(adjacencyList[i]);

                    timers.AddRange(adjacencyList[i].ConvertAll(el => new Stopwatch()));
                }
            }

            for (var i = count; i < timers.Count; i++)
            {
                timers[i].Start();
            }

            toolsForDrawing.DrawFullGraph(edges, vertices);

            for (var i = 0; i < points.Count; i++)
            {
                if (timers[i].ElapsedMilliseconds * coefficient > points[i].Weight * 1000)
                {
                    vertices[points[i].Ver2].HasPoint = true;
                    points.RemoveAt(i);
                    timers.RemoveAt(i);
                    i--;
                    continue;
                }

                PointF point = GetPoint(vertices[points[i].Ver1],
                    vertices[points[i].Ver2], points[i].Weight, timers[i]);

                toolsForDrawing.DrawPoint(point.X, point.Y);

                field.Image = toolsForDrawing.GetBitmap();
            }

            field.Image = toolsForDrawing.GetBitmap();
        }

        /// <summary>
        /// Установка параметров таблицы.
        /// </summary>
        private void SetUpChart()
        {
            chart.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Title = "Time";
        }

        /// <summary>
        /// Установка главных инструментов.
        /// </summary>
        /// <param name="mf"> Главная форма </param>
        private void SetUpMainTools(MainForm mf)
        {
            mainForm = mf;
            vertices = mf.Vertices.GetRange(0, mf.Vertices.Count);
            edges = mf.Edges.GetRange(0, mf.Edges.Count);
            coefficient = mf.SpeedCoefficient;
            toolsForDrawing = mf.ToolsForDrawing;
            requireTimeForTesting = mf.RequireTimeForTesting;
        }

        /// <summary>
        /// Настройка всех таймеров.
        /// </summary>
        private void SetUpTimers()
        {
            var listArr = new List<Edge>[vertices.Count];

            for (var i = 0; i < vertices.Count; i++)
            {
                List<Edge> curEdges = edges.Where(el => el.Ver1 == i).ToList();
                listArr[i] = new List<Edge>();
                listArr[i].AddRange(curEdges);
            }

            RunTimers(listArr);
        }

        /// <summary>
        /// Запуск всех таймеров.
        /// </summary>
        /// <param name="adjacencyList"> Лист смежности </param>
        private void RunTimers(List<Edge>[] adjacencyList)
        {
            mainTimer = new Timer { Interval = 2 };
            mainTimer.Tick += (x, y) => MainTick(adjacencyList);
            mainTimer.Start();



            timerForPlotting.Start();
            currentTimeOfTestingTimer.Start();
            Show();
        }

        /// <summary>
        /// Вычислить текущие координаты точки.
        /// </summary>
        /// <param name="ver1"> Первая вершина </param>
        /// <param name="ver2"> Вторая вершина </param>
        /// <param name="allTime"> Суммарное время </param>
        /// <param name="timer"> Текущее время </param>
        /// <returns> Точку </returns>
        private PointF GetPoint(Vertex ver1, Vertex ver2, double allTime, Stopwatch timer)
        {
            var distanceX = Math.Abs(ver2.X - ver1.X);
            var distanceY = Math.Abs(ver2.Y - ver1.Y);

            float x = 0;
            float y = 0;

            // Loop. x^2 + y^2 = 400
            if (ver1 == ver2)
            {
                // Левая нижняя дуга.
                if (timer.ElapsedMilliseconds * coefficient / 1000.0 <= allTime / 4)
                {
                    x = (float)(100 / allTime * timer.ElapsedMilliseconds * coefficient / 1000.0);
                    y = (float)Math.Sqrt(400 - x * x);

                    if (x * x >= 400)
                    //return new PointF(ver1.X - 20, ver1.Y);
                    {
                        x = (float)(100 / allTime * (timer.ElapsedMilliseconds * coefficient / 1000.0 - allTime / 4));
                        y = -(float)Math.Sqrt(400 - x * x);

                        x += 20;
                        y -= 20;
                        return new PointF(y + ver1.X, ver1.Y - x);
                    }

                    x += 20;
                    y -= 20;
                    return new PointF(-x + ver1.X, ver1.Y + y);
                }

                if (timer.ElapsedMilliseconds * coefficient / 1000.0 <= allTime / 2)
                {
                    //Верхняя правая дуга.
                    x = (float)(100 / allTime * (timer.ElapsedMilliseconds * coefficient / 1000.0 - allTime / 4));
                    y = -(float)Math.Sqrt(400 - x * x);

                    if (x * x >= 400)
                    {
                        x = -(float)(100 / allTime * (timer.ElapsedMilliseconds * coefficient / 1000.0 - allTime / 2));
                        y = -(float)Math.Sqrt(400 - x * x);


                        x += 20;
                        y -= 20;
                        return new PointF(-x + ver1.X, ver1.Y + y);
                    }

                    x += 20;
                    y -= 20;
                    return new PointF(y + ver1.X, ver1.Y - x);
                }


                if (timer.ElapsedMilliseconds * coefficient / 1000.0 <= allTime * 3 / 4)
                {
                    // Правая верхняя дуга
                    x = -(float)(100 / allTime * (timer.ElapsedMilliseconds * coefficient / 1000.0 - allTime / 2));
                    y = -(float)Math.Sqrt(400 - x * x);

                    if (x * x >= 400)
                    {
                        x = -(float)(100 / allTime * (timer.ElapsedMilliseconds * coefficient / 1000.0 - allTime * 3 / 4));
                        y = -(float)Math.Sqrt(400 - x * x);

                        x += 20;
                        y += 20;
                        return new PointF(-y + ver1.X, ver1.Y - x);
                    }

                    x += 20;
                    y -= 20;
                    return new PointF(-x + ver1.X, ver1.Y + y);
                }



                if (timer.ElapsedMilliseconds * coefficient / 1000.0 <= allTime)
                {
                    x = -(float)(100 / allTime * (timer.ElapsedMilliseconds * coefficient / 1000.0 - allTime * 3 / 4));
                    y = -(float)Math.Sqrt(400 - x * x);

                    if (x * x >= 400)
                    {
                        x = (float)(100 / allTime * timer.ElapsedMilliseconds * coefficient / 1000.0);
                        y = (float)Math.Sqrt(400 - x * x);

                        return new PointF(ver1.X - 20, ver1.Y);
                    }

                    x += 20;
                    y += 20;
                    return new PointF(-y + ver1.X, ver1.Y - x);
                }
            }
            else
            {
                // Normal edge.
                if (ver1.X < ver2.X)
                {
                    if (ver1.Y > ver2.Y)
                    {
                        x = (float)(distanceX / allTime * timer.ElapsedMilliseconds * coefficient / 1000) + ver1.X;
                        y = -(float)(distanceY / allTime * timer.ElapsedMilliseconds * coefficient / 1000) + ver1.Y;
                    }
                    else
                    {
                        x = (float)(distanceX / allTime * timer.ElapsedMilliseconds * coefficient / 1000) + ver1.X;
                        y = (float)(distanceY / allTime * timer.ElapsedMilliseconds * coefficient / 1000) + ver1.Y;
                    }
                }
                else
                {
                    if (ver1.Y > ver2.Y)
                    {
                        x = -(float)(distanceX / allTime * timer.ElapsedMilliseconds * coefficient / 1000) + ver1.X;
                        y = -(float)(distanceY / allTime * timer.ElapsedMilliseconds * coefficient / 1000) + ver1.Y;
                    }
                    else
                    {
                        x = -(float)(distanceX / allTime * timer.ElapsedMilliseconds * coefficient / 1000) + ver1.X;
                        y = (float)(distanceY / allTime * timer.ElapsedMilliseconds * coefficient / 1000) + ver1.Y;
                    }
                }
            }

            return new PointF(x, y);
        }

        /// <summary>
        /// Построение графика.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerForPlottingTick(object sender, EventArgs e)
        {
            if (currentTimeOfTestingTimer.ElapsedMilliseconds > requireTimeForTesting)
            {
                timerForPlotting.Stop();
                mainTimer.Stop();
                Hide();
                var _ = new ChartDisplayForTestingProgramForm(mainForm, ++FormCounter, forms);
                return;
            }

            var xValue = (int)currentTimeOfTestingTimer.ElapsedMilliseconds / 1000;

            chart.Series["Amount of points"].Points.AddXY(xValue, totalCountOfPoints);
        }

        private void StopTestingButtonClick(object sender, EventArgs e)
        {
            foreach (var form in forms)
            {
                form.Height -= 60;
                form.timerForPlotting.Stop();
                form.mainTimer.Stop();
                form.timerForPlotting.Stop();
                form.mainTimer.Stop();

                form.label1.Visible = false;
                form.label2.Visible = false;
                form.coefficientTrackBar.Visible = false;
                form.stopTestingButton.Visible = false;
                form.textBox1.Text = (ListOfFormsForTestingProgram.Pointer + 1).ToString();
                form.leftButton.Visible = true;
                form.leftMiniButton.Visible = true;
                form.rightButton.Visible = true;
                form.rightMiniButton.Visible = true;
                form.textBox1.Visible = true;
                form.exitButton.Visible = true;
            }
        }

        /// <summary>
        /// Заугрузка формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChartForTestingProgramLoad(object sender, EventArgs e)
        {
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            leftButton.Visible = false;
            leftMiniButton.Visible = false;
            rightButton.Visible = false;
            rightMiniButton.Visible = false;
            textBox1.Visible = false;
            exitButton.Visible = false;

            ControlBox = false;
            chart.Legends[0].BackColor = Color.SlateGray;
            chart.Series["Amount of points"].Color = Color.Yellow;
            chart.ChartAreas["ChartArea1"].BackColor = Color.SlateGray;

            chart.BackColor = Color.SlateGray;
            Theme = MetroThemeStyle.Dark;
            Style = MetroColorStyle.Yellow;
            coefficientTrackBar.BackColor = Color.FromArgb(11, 17, 20);
            TopMost = true;
            Width = 1450;
            Height = 800;
            chart.Width = Consts.ChartTestingFormWidth - 170;
            chart.Height = Consts.ChartTestingFormHeight;

            field.Location = new Point(0, 5);
            field.Width = Consts.GraphPictureBoxWidth;
            field.Height = Consts.GraphPictureBoxHeight;

            chart.Location = new Point(field.Width, 5);

            timeLabel.Location = new Point(Consts.TimeLabelLocationX - 30, Consts.TimeLabelLocationY);

            label1.Location = new Point(Consts.LeftValueTrackBarLocationX - 35,
                Consts.LeftValueTrackBarLocationY - 4);
            label2.Location = new Point(Consts.RightValueTrackBarLocationX - 25,
                Consts.RightValueTrackBarLocationY - 4);

            coefficientTrackBar.Location = new Point(Consts.CoefficientTrackBarLocationX - 30,
                                                     Consts.CoefficientTrackBarLocationY);
            stopTestingButton.Location = new Point(Consts.StopTestingProgramButtonLocationX - 35,
                                                   Consts.StopTestingProgramButtonLocationY);
        }

        /// <summary>
        /// Первеключение страницы влево на 1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftMiniButtonClick(object sender, EventArgs e)
        {
            if (ListOfFormsForTestingProgram.Pointer > 0)
            {
                ListOfFormsForTestingProgram.Pointer--;
            }
            else
            {
                return;
            }

            foreach (var chartForTestingProgram in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                chartForTestingProgram.textBox1.Text = (ListOfFormsForTestingProgram.Pointer + 1).ToString();
            }

            foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                form.label3.Text = ListOfFormsForTestingProgram.Pointer.ToString();
            }

            HideAllForms();
            ListOfFormsForTestingProgram.ChartFormsForTestingProgram[ListOfFormsForTestingProgram.Pointer].Show();
            ListOfFormsForTestingProgram.ChartFormsForTestingProgram[ListOfFormsForTestingProgram.Pointer].Activate();
        }

        /// <summary>
        /// Скрыть все формы.
        /// </summary>
        private void HideAllForms()
        {
            foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                form.Hide();
            }
        }

        /// <summary>
        /// Переключение страницы на 1 вправо.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightMiniButtonClick(object sender, EventArgs e)
        {
            if (ListOfFormsForTestingProgram.Pointer < ListOfFormsForTestingProgram.ChartFormsForTestingProgram.Count - 1)
            {
                ListOfFormsForTestingProgram.Pointer++;
            }
            else
            {
                return;
            }

            foreach (var chartForTestingProgram in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                chartForTestingProgram.textBox1.Text = (ListOfFormsForTestingProgram.Pointer + 1).ToString();
            }

            foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                form.label3.Text = ListOfFormsForTestingProgram.Pointer.ToString();
            }

            HideAllForms();

            ListOfFormsForTestingProgram.ChartFormsForTestingProgram[ListOfFormsForTestingProgram.Pointer].Show();
            ListOfFormsForTestingProgram.ChartFormsForTestingProgram[ListOfFormsForTestingProgram.Pointer].Activate();
        }

        /// <summary>
        /// Переключиться на 1ую страницу.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftButtonClick(object sender, EventArgs e)
        {
            ListOfFormsForTestingProgram.Pointer = 0;

            foreach (var chartForTestingProgram in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                chartForTestingProgram.textBox1.Text = "1";
            }

            foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                form.label3.Text = ListOfFormsForTestingProgram.Pointer.ToString();
            }

            ListOfFormsForTestingProgram.ChartFormsForTestingProgram[0].Show();
            ListOfFormsForTestingProgram.ChartFormsForTestingProgram[0].Activate();
        }

        /// <summary>
        /// Перключиться на последнюю страницу.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightButtonClick(object sender, EventArgs e)
        {
            var lastForm = ListOfFormsForTestingProgram.ChartFormsForTestingProgram.LastOrDefault();

            if (lastForm is null)
            {
                return;
            }


            foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                form.label3.Text = ListOfFormsForTestingProgram.Pointer.ToString();
            }




            ListOfFormsForTestingProgram.Pointer = ListOfFormsForTestingProgram.ChartFormsForTestingProgram.Count - 1;
            foreach (var chartForTestingProgram in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                chartForTestingProgram.textBox1.Text = (ListOfFormsForTestingProgram.Pointer + 1).ToString();
            }

            foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
            {
                form.label3.Text = ListOfFormsForTestingProgram.Pointer.ToString();
            }

            HideAllForms();

            lastForm.Show();
            lastForm.Activate();
        }

        private void FormNumberTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            var curForm = ListOfFormsForTestingProgram.ChartFormsForTestingProgram[ListOfFormsForTestingProgram.Pointer];

            if (curForm.textBox1.Focused && e.KeyChar == (char)Keys.Enter)
            {
                try
                {
                    var point = int.Parse(textBox1.Text);
                    point--;

                    if (point > ListOfFormsForTestingProgram.ChartFormsForTestingProgram.Count ||
                        point < 0)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    ListOfFormsForTestingProgram.Pointer = point;
                    foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
                    {
                        form.textBox1.Text = (point + 1).ToString();
                    }
                    HideAllForms();
                    ListOfFormsForTestingProgram.ChartFormsForTestingProgram[point].Show();
                    ListOfFormsForTestingProgram.ChartFormsForTestingProgram[point].Activate();
                }
                catch (Exception)
                {
                    label5.Focus();
                    HideAllForms();
                    var ms = new MyMessageBox();
                    var res = ms.NotifyWrongNumberOfForm(1, ListOfFormsForTestingProgram.ChartFormsForTestingProgram.Count);

                    if (res == DialogResult.OK)
                    {

                        label5.Focus();

                        foreach (var form in ListOfFormsForTestingProgram.ChartFormsForTestingProgram)
                        {
                            form.textBox1.Text = "1";
                        }

                        curForm.Show();
                        curForm.Activate();
                    }
                }
            }
        }

        private void ExitButtonClick(object sender, EventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch (Exception)
            {
                Environment.Exit(1); 
            }
        }
    }
}
