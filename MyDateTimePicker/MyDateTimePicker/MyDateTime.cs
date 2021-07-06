using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MyDateTimePicker
{
    public class MyDateTime : Control
    {
        /// <summary>
        /// Button tipinde Dropbutton adinda MonthCalendar in acilmasini saglamak icin degisken taninmlandi.
        /// </summary>
        private Button DropButton;

        /// <summary>
        /// MaskedTextBox tipinde tarih yaziminin dogru kullaniminin saglanmasi icin maskedTextBox1 adinda degsiken tanimlandi
        /// </summary>
        private MaskedTextBox maskedTextBox1;

        /// <summary>
        /// MonthCalendar dan kalitim yaparak Moncal adinda sinif olusturuldu ve null olarak isaretlendi
        /// </summary>
        private MonCal monCal = null;

        /// <summary>
        /// EventArgs sinifindan kalitim yapilarak, secilen gunu yakalamak icin class olusturuluyor
        /// </summary>
        public class MonCalEventArgs : EventArgs
        {
            private DateTime SecilenGun;

            public MonCalEventArgs(DateTime sGun)
            {
                SecilenGun = sGun;
            }

            public string sGunYazdir { get { return SecilenGun.ToString("dd/MM/yyyy"); } }
        }

        /// <summary>
        /// MonthCalendar dan kalitim yapilarak olusturulan MonCal icin parametreler tanimlanarak yapici metotlar yardimiyla deger atamasi yapilabilmesi icin HighlightedDates adinda class olusturuldu.
        /// </summary>
        public class HighlightedDates
        {
            public DateTime Date;
            public Point Position = new Point(0, 0);
            public Color DateColor;
            public Color BoxColor;
            public Color BgColor;
            public bool Bold;

            public HighlightedDates(DateTime date)
            {
                this.Date = date;
                this.DateColor = this.BoxColor = this.BgColor = Color.Empty;
                this.Bold = true;
            }

            public HighlightedDates(DateTime date, Color dateColor, bool bold)
            {
                this.Date = date;
                this.BgColor = this.BoxColor = Color.Empty;
                this.DateColor = dateColor;
                this.Bold = bold;
            }

            public HighlightedDates(DateTime date, Color dateColor, Color boxColor,
                Color bgColor, bool bold)
            {
                this.Date = date;
                this.DateColor = dateColor;
                this.BoxColor = boxColor;
                this.BgColor = bgColor;
                this.Bold = bold;
            }
        }

        /// <summary>
        /// Yapici metot yardimiyla maskedTextBox a ve DropButton a default ozellikler tanimlaniyor.
        /// </summary>
        public MyDateTime()
        {  
            this.SuspendLayout();

            maskedTextBox1 = new MaskedTextBox();
            maskedTextBox1.Location = new Point(0, 0);
            maskedTextBox1.Mask = "00/00/0000";
            maskedTextBox1.Name = "maskedTextBox1";
            maskedTextBox1.Size = new Size(139, 20);
            maskedTextBox1.TabIndex = 0;
            maskedTextBox1.ValidatingType = typeof(DateTime);

            DropButton = new Button();
            DropButton.Size = new Size(16, 16);
            DropButton.FlatStyle = FlatStyle.Standard;
            DropButton.BackColor = Color.Transparent;
            DropButton.BackgroundImage = Image.FromFile(@"C:\Users\inalf\source\repos\MyDateTimePicker\MyDateTimePicker\icon\calendar-icon3.ico");
            DropButton.BackgroundImageLayout = ImageLayout.Tile;
            DropButton.Location = new Point(maskedTextBox1.Width - 20, 0);
            DropButton.Click += new EventHandler(DropButton_Click);
            DropButton.Cursor = Cursors.Arrow;
            maskedTextBox1.Controls.Add(DropButton);
            maskedTextBox1.Text = DateTime.Now.ToString("dd/MM/yyyy");

            this.Controls.Add(maskedTextBox1);

            this.ResumeLayout();
        }

        /// <summary>
        /// Takvim iconuna basildiginda MonthCalendar in acilmasi icin event olusturuldu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DropButton_Click(object sender, EventArgs e)
        {
            string a = maskedTextBox1.Text;
            string[] aa = a.Split('/');
            List<HighlightedDates> hlDates = new List<HighlightedDates>();
            hlDates.Add(new HighlightedDates(new DateTime(int.Parse(aa[2]), int.Parse(aa[1]), int.Parse(aa[0])), Color.Red, Color.Blue, Color.Pink, true));

            monCal = new MonCal(hlDates);
            monCal.monCalControlHandler += new MonCal.MonCalControlHandler(monCal_monCalControlHandler);
            monCal.Location = new Point(20, 20);
            this.Controls.Add(monCal);
        }

        /// <summary>
        /// Secilen gun MonCalEventArgs event i ile maskedTextBox1 a yazdiriliyor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void monCal_monCalControlHandler(object sender, MonCalEventArgs e)
        {
            maskedTextBox1.Text = e.sGunYazdir;
            this.Controls.Remove(monCal);
            monCal = null;
        }

        /// <summary>
        /// MonthCalendar dan kalitim yapilarak MonCal adinda internal class olusutuluyor.
        /// </summary>
        internal class MonCal : MonthCalendar
        {
            private Rectangle dayBox;
            private int dayTop = 0;
            private SelectionRange range;

            /// <summary>
            /// HighlightedDates classindan verilerin alinmasi icin Generic List tanimlaniyor
            /// </summary>
            private List<HighlightedDates> highlightedDates;
             
            /// <summary>
            /// Secilen gunun yakalanmasini saglayan MonCalEventArgs classindan parametre alan delegate tanimlaniyor
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public delegate void MonCalControlHandler(object sender, MonCalEventArgs e);
            public event MonCalControlHandler monCalControlHandler;

            /// <summary>
            /// Yapici metot ile HighlightedDates classindan olusturulan generic list ten alinan verilerle MonCal a deger atamasi yapiliyor.
            /// </summary>
            /// <param name="HighlightedDates"></param>
            public MonCal(List<HighlightedDates> HighlightedDates)
            {
                this.ShowTodayCircle = false;
                this.highlightedDates = HighlightedDates;
                range = GetDisplayRange(false);
                SetDayBoxSize();
                SetPosition(this.highlightedDates);

            }

            /// <summary>
            /// MonthCalendar dan kalitim yapilarak olusturulan MonCal in boyutu ayarlaniyor.
            /// </summary>
            private void SetDayBoxSize()
            {
                int bottom = this.Height;

                while (HitTest(1, dayTop).HitArea != HitArea.Nowhere && HitTest(1, dayTop).HitArea != HitArea.Date && HitTest(1, dayTop).HitArea != HitArea.PrevMonthDate)
                {
                    dayTop++;
                }

                while (HitTest(1, dayTop).HitArea != HitArea.Nowhere && HitTest(1, bottom).HitArea != HitArea.Date && HitTest(1, bottom).HitArea != HitArea.NextMonthDate)
                {
                    bottom--;
                }
            }

            /// <summary>
            /// Geriye deger dondermeyen, hlDates parametresinden deger alip takvimin satir ve sutunlarini belirleyen metot tanimlandi.
            /// </summary>
            /// <param name="hlDates"></param>
            private void SetPosition(List<HighlightedDates> hlDates)
            {
                int satir = 0, sutun = 0;

                hlDates.ForEach(delegate (HighlightedDates date)
                {
                    if (date.Date >= range.Start && date.Date <= range.End)
                    {
                        TimeSpan span = date.Date.Subtract(range.Start);
                        satir = span.Days / 7;
                        sutun = span.Days % 7;
                        date.Position = new Point(satir, sutun);
                    }
                });
            }

            /// <summary>
            /// Control sinifindan kalitim yapilarak alinan ve Control.Paint eventini baslatan OnPaint override edilerek kullanicinin gorsel bir aylik takvim erkanini kullanarak tarih secmesini saglayan bir pencere olusturuluyor.
            /// </summary>
            /// <param name="pe"></param>
            protected override void OnPaint(PaintEventArgs pe)
            {
                base.OnPaint(pe);

                Graphics g = pe.Graphics;
                Rectangle backgroundRect;

                highlightedDates.ForEach(delegate (HighlightedDates date)
                {
                    backgroundRect = new Rectangle( date.Position.Y * dayBox.Width + 1, date.Position.X * dayBox.Height + dayTop, dayBox.Width, dayBox.Height);

                    if (date.BgColor != Color.Empty)
                    {
                        using (Brush brush = new SolidBrush(date.BgColor))
                        {
                            g.FillRectangle(brush, backgroundRect);
                        }
                    }

                    if (date.Bold || date.DateColor != Color.Empty)
                    {
                        using (Font textFont = new Font(Font, (date.Bold ? FontStyle.Bold : FontStyle.Regular)))
                        {
                            TextRenderer.DrawText(g, date.Date.Day.ToString(), textFont, backgroundRect, date.DateColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }

                    if (date.BoxColor != Color.Empty)
                    {
                        using (Pen pen = new Pen(date.BoxColor))
                        {
                            Rectangle boxRect = new Rectangle( date.Position.Y * dayBox.Width + 1, date.Position.X * dayBox.Height + dayTop, dayBox.Width, dayBox.Height);
                            g.DrawRectangle(pen, boxRect);
                        }
                    }
                });

            }

            /// <summary>
            /// MonthCalendar sinifindan kalitim yapillarak alinan OnDateSelected override edilerek MonCalEventArgs dan alinan verilerle secilen gunun yakalanmasi saglaniyor
            /// </summary>
            /// <param name="drevent"></param>
            protected override void OnDateSelected(DateRangeEventArgs drevent)
            {
                base.OnDateSelected(drevent);
                MonCalEventArgs args = new MonCalEventArgs(drevent.Start);
                monCalControlHandler(this, args);
                this.Dispose();
            }
        }
    }
}