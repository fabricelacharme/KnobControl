#region License

/* Copyright (c) 2017 Fabrice Lacharme
 * This code was originally written by Jigar Desai 
 * http://www.c-sharpcorner.com/article/knob-control-using-windows-forms-and-gdi/
 * Note that another implementation exists in vb.net by Blong
 * https://www.codeproject.com/Articles/2563/VB-NET-Knob-Control-using-Windows-Forms-and-GDI?msg=1884770#xx1884770xx
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

#endregion

#region Contact

/*
 * Fabrice Lacharme
 * Email: fabrice.lacharme@gmail.com
 */

#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KnobControl
{

    /* Original code from Jigar Desai on C-SharpCorner.com
    * see https://www.c-sharpcorner.com/article/knob-control-using-windows-forms-and-gdi/
    * KnobControl is a knob control written in C#  
    * 
    * CodeProject: https://www.codeproject.com/Tips/1187460/Csharp-Knob-Control-using-Windows-Forms
    * Github: https://github.com/fabricelacharme/KnobControl
    * 
    * 22/08/18 - version 1.0.O.1
    * Fixed: erroneous display in case of minimum value <> 0 (negative or positive)
    * Modified: DrawColorSlider, OnMouseMove
    * 
    * Added: Font selection
    * 
    * 
    * 25/08/18 - version 1.0.0.2
    * Fixed: mouse click event: pointer button is not displayed correctly when the minimum is set to a non zero value.
    * Modified: getValueFromPosition
    * 
    * 
    * 04/01/2019 - version 1.0.0.3
    * Font & Size selection for graduations:
    * New property ScaleFontAutoSize:
    * - false = no AutoSize => Allow font selection 
    * - true = AutoSize by program
    */


    // A delegate type for hooking up ValueChanged notifications. 
    public delegate void ValueChangedEventHandler(object Sender);

    /// <summary>
    /// Summary description for KnobControl.
    /// </summary>
    public class KnobControl : System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;       

        /// <summary>
        /// Styles of pointer button
        /// </summary>
        public enum KnobPointerStyles
        {
            circle,
            line,
        }


        #region private properties

        private KnobPointerStyles _knobPointerStyle = KnobPointerStyles.circle;

        private int _minimum = 0;
        private int _maximum = 25;
        private int _LargeChange = 5;
        private int _SmallChange = 1;

        private int _scaleDivisions;
        private int _scaleSubDivisions;
        

        private Font _scaleFont;
        private bool _scaleFontAutoSize = true;

        
        private bool _drawDivInside;

        private bool _showSmallScale = false;
        private bool _showLargeScale = true;

        private float _startAngle = 135;
        private float _endAngle = 405;
        private float deltaAngle;
        private int _mouseWheelBarPartitions = 10;


        private float drawRatio = 1;
        private float gradLength = 4;

        // Color of the pointer
        private Color _PointerColor = Color.SlateBlue;
        private Color _knobBackColor = Color.LightGray;
        private Color _scaleColor = Color.Black;

        private int _Value = 0;
        private bool isFocused = false;
        private bool isKnobRotating = false;
        private Rectangle rKnob;
        private Point pKnob;
        private Pen DottedPen;

        Brush brushKnob;
        Brush brushKnobPointer;

        private Font knobFont;

        //-------------------------------------------------------
        // declare Off screen image and Offscreen graphics       
        //-------------------------------------------------------
        private Image OffScreenImage;
        private Graphics gOffScreen;

        #endregion


        #region event
        //-------------------------------------------------------
        // An event that clients can use to be notified whenever 
        // the Value is Changed.                                 
        //-------------------------------------------------------
        public event ValueChangedEventHandler ValueChanged;

        //-------------------------------------------------------
        // Invoke the ValueChanged event; called  when value     
        // is changed                                            
        //-------------------------------------------------------
        protected virtual void OnValueChanged(object sender)
        {
            ValueChanged?.Invoke(sender);
        }

        #endregion


        #region (* public Properties *)

        /// <summary>
        /// Font of graduations
        /// </summary>
        [Description("Font of graduations")]
        [Category("KnobControl")]
        public Font ScaleFont
        {
            get { return _scaleFont; }
            set
            {
                _scaleFont = value;
                // Redraw
                SetDimensions();
                Invalidate();
            }
        }

        /// <summary>
        /// Autosize or not for font of graduations
        /// </summary>
        [Description("Autosize Font of graduations")]
        [Category("KnobControl")]
        [DefaultValue(true)]
        public bool ScaleFontAutoSize
        {
            get { return _scaleFontAutoSize; }
            set
            {
                _scaleFontAutoSize = value;
                // Redraw
                SetDimensions();
                Invalidate();
            }
        }

        /// <summary>
        /// Start angle to display graduations
        /// </summary>
        /// <value>The start angle to display graduations.</value>
        [Description("Set the start angle to display graduations (min 90)")]
        [Category("KnobControl")]
        [DefaultValue(135)]
        public float StartAngle
        {
            get { return _startAngle; }
            set
            {
                if (value >= 90 && value < _endAngle)
                {
                    _startAngle = value;
                    deltaAngle = _endAngle - StartAngle;
                    // Redraw
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// End angle to display graduations
        /// </summary>
        /// <value>The end angle to display graduations.</value>
        [Description("Set the end angle to display graduations (max 450)")]
        [Category("KnobControl")]
        [DefaultValue(405)]
        public float EndAngle
        {
            get { return _endAngle; }
            set
            {
                if (value <= 450 && value > _startAngle)
                {
                    _endAngle = value;
                    deltaAngle = _endAngle - _startAngle;
                    // Redraw
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Style of pointer: circle or line
        /// </summary>
        [Description("Set the style of the knob pointer: a circle or a line")]
        [Category("KnobControl")]
        public KnobPointerStyles KnobPointerStyle
        {
            get { return _knobPointerStyle; }
            set
            {
                _knobPointerStyle = value;
                // Redraw
                Invalidate();
            }
        }


        /// <summary>
        /// Gets or sets the mouse wheel bar partitions.
        /// </summary>
        /// <value>The mouse wheel bar partitions.</value>
        /// <exception cref="T:System.ArgumentOutOfRangeException">exception thrown when value isn't greather than zero</exception>
        [Description("Set to how many parts is bar divided when using mouse wheel")]
        [Category("KnobControl")]
        [DefaultValue(10)]
        public int MouseWheelBarPartitions
        {
            get { return _mouseWheelBarPartitions; }
            set
            {
                if (value > 0)
                    _mouseWheelBarPartitions = value;
                else throw new ArgumentOutOfRangeException("MouseWheelBarPartitions has to be greather than zero");
            }
        }

        /// <summary>
        /// Draw string graduations inside or outside knob circle
        /// </summary>
        /// 
        [Description("Draw graduation strings inside or outside the knob circle")]
        [Category("KnobControl")]
        [DefaultValue(false)]
        public bool DrawDivInside
        {
            get { return _drawDivInside; }
            set
            {
                _drawDivInside = value;
                // Redraw
                SetDimensions();
                Invalidate();
            }
        }

        /// <summary>
        /// Color of graduations
        /// </summary>
        [Description("Color of graduations")]
        [Category("KnobControl")]
        public Color ScaleColor
        {
            get { return _scaleColor; }
            set
            {
                _scaleColor = value;
                // Redraw
                Invalidate();
            }
        }

        /// <summary>
        /// Color of graduations
        /// </summary>
        [Description("Color of knob")]
        [Category("KnobControl")]
        public Color KnobBackColor
        {
            get { return _knobBackColor; }
            set
            {
                _knobBackColor = value;
                
                SetDimensions();
                
                // Redraw
                Invalidate();
            }
        }

        /// <summary>
        /// How many divisions of maximum?
        /// </summary>
        [Description("Set the number of intervals between minimum and maximum")]
        [Category("KnobControl")]
        public int ScaleDivisions
        {
            get { return _scaleDivisions; }
            set
            {
                if (value > 1)
                {
                    _scaleDivisions = value;
                    // Redraw
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// How many subdivisions for each division
        /// </summary>
        [Description("Set the number of subdivisions between main divisions of graduation.")]
        [Category("KnobControl")]
        public int ScaleSubDivisions
        {
            get { return _scaleSubDivisions; }
            set
            {
                if (value > 0 && _scaleDivisions > 0 && (_maximum - _minimum) / (value * _scaleDivisions) > 0)
                {
                    _scaleSubDivisions = value;
                    // Redraw
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Shows Small Scale marking.
        /// </summary>
        [Description("Show or hide subdivisions of graduations")]
        [Category("KnobControl")]
        public bool ShowSmallScale
        {
            get { return _showSmallScale; }
            set
            {
                if (value == true)
                {
                    if (_scaleDivisions > 0 && _scaleSubDivisions > 0 && (_maximum - _minimum) / (_scaleSubDivisions * _scaleDivisions) > 0)
                    {
                        _showSmallScale = value;
                        // Redraw 
                        Invalidate();
                    }
                }
                else
                {
                    _showSmallScale = value;
                    // Redraw 
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Shows Large Scale marking
        /// </summary>
        [Description("Show or hide graduations")]
        [Category("KnobControl")]
        public bool ShowLargeScale
        {
            get { return _showLargeScale; }
            set
            {
                _showLargeScale = value;
                // need to redraw
                SetDimensions();
                // Redraw 
                Invalidate();
            }
        }

        /// <summary>
        /// Minimum Value for knob Control
        /// </summary>
        [Description("set the minimum value for the knob control")]
        [Category("KnobControl")]
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                // Redraw 
                Invalidate();
            }
        }
        
        /// <summary>
        /// Maximum value for knob control
        /// </summary>
        [Description("set the maximum value for the knob control")]
        [Category("KnobControl")]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (value > _minimum)
                {
                    _maximum = value;


                    if (_scaleSubDivisions > 0 && _scaleDivisions > 0 && (_maximum - _minimum) / (_scaleSubDivisions * _scaleDivisions) <= 0)
                    {
                        _showSmallScale = false;
                    }

                    SetDimensions();
                    // Redraw
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// value set for large change
        /// </summary>
        [Description("set the value for the large changes")]
        [Category("KnobControl")]
        public int LargeChange
        {
            get { return _LargeChange; }
            set
            {
                _LargeChange = value;
                // Redraw
                Invalidate();
            }
        }
        
        /// <summary>
        /// value set for small change.
        /// </summary>
        [Description("set the minimum value for the small changes")]
        [Category("KnobControl")]
        public int SmallChange
        {
            get { return _SmallChange; }
            set
            {
                _SmallChange = value;
                // Redraw
                Invalidate();
            }
        }

        /// <summary>
        /// Current Value of knob control
        /// </summary>
        [Description("set the current value of the knob control")]
        [Category("KnobControl")]
        public int Value
        {
            get { return _Value; }
            set
            {
                if (value >= _minimum && value <= _maximum)
                {
                    _Value = value;
                    // Redraw
                    Invalidate();
                    // call delegate  
                    OnValueChanged(this);
                }
            }
        }

        /// <summary>
        /// Color of the button
        /// </summary>
        [Description("set the color of the pointer")]
        [Category("KnobControl")]
        public Color PointerColor
        {
            get { return _PointerColor; }
            set
            {
                _PointerColor = value;
                
                SetDimensions();

                // Redraw
                Invalidate();
            }
        }

        #endregion properties


        public KnobControl()
        {

            // This call is required by the Windows.Forms Form Designer.
            DottedPen = new Pen(Utility.GetDarkColor(this.BackColor, 40))
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
                DashCap = System.Drawing.Drawing2D.DashCap.Flat
            };

            InitializeComponent();

            knobFont = new Font(this.Font.FontFamily, this.Font.Size);
            _scaleFont = new Font(this.Font.FontFamily, this.Font.Size);


            // Properties initialisation

            // "start angle" and "end angle" possible values:

            // 90 = bottom (minimum value for "start angle")
            // 180 = left
            // 270 = top
            // 360 = right
            // 450 = bottom again (maximum value for "end angle")

            // So the couple (90, 450) will give an entire circle and the couple (180, 360) will give half a circle.

            _startAngle = 135;
            _endAngle = 405;
            deltaAngle = _endAngle - _startAngle;

            _minimum = 0;
            _maximum = 100;
            _scaleDivisions = 11;
            _scaleSubDivisions = 4;
            _mouseWheelBarPartitions = 10;

            _scaleColor = Color.Black;
            _knobBackColor = Color.White;

            SetDimensions();
        }


        #region override

        /// <summary>
        /// Paint event: draw all
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {

            Graphics g = e.Graphics;
            // Set background color of Image...            
            gOffScreen.Clear(this.BackColor);
            // Fill knob Background to give knob effect....
            gOffScreen.FillEllipse(brushKnob, rKnob);
            // Set antialias effect on                     
            gOffScreen.SmoothingMode = SmoothingMode.AntiAlias;
            // Draw border of knob                         
            gOffScreen.DrawEllipse(new Pen(this.BackColor), rKnob);

            //if control is focused 
            if (this.isFocused)
            {
                gOffScreen.DrawEllipse(DottedPen, rKnob);
            }

            // DrawPointer
            DrawPointer(gOffScreen);

            //---------------------------------------------
            // draw small and large scale                  
            //---------------------------------------------
            DrawDivisions(gOffScreen, rKnob);

            // Draw image on screen                    
            g.DrawImage(OffScreenImage, 0, 0);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Empty To avoid Flickring due do background Drawing.
        }

        /// <summary>
        /// Mouse down event: select control
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (Utility.IsPointinRectangle(new Point(e.X, e.Y), rKnob))
            {
                if (isFocused)
                {
                    // was already selected
                    // Start Rotation of knob only if it was selected before        
                    isKnobRotating = true;
                }
                else
                {
                    // Was not selected before => select it
                    Focus();
                    isFocused = true;
                    isKnobRotating = false; // disallow rotation, must click again
                    // draw dotted border to show that it is selected
                    Invalidate();
                }
            }

        }


        //----------------------------------------------------------
        // we need to override IsInputKey method to allow user to   
        // use up, down, right and bottom keys other wise using this
        // keys will change focus from current object to another    
        // object on the form                                       
        //----------------------------------------------------------
        protected override bool IsInputKey(Keys key)
        {
            switch (key)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Right:
                case Keys.Left:
                    return true;
            }
            return base.IsInputKey(key);
        }

        /// <summary>
        /// Mouse up event: display new value
        /// </summary>
        /// <param name="e"></param>
		protected override void OnMouseUp(MouseEventArgs e)
        {
            if (Utility.IsPointinRectangle(new Point(e.X, e.Y), rKnob))
            {
                if (isFocused == true && isKnobRotating == true)
                {
                    // Change value is allowed only only after 2nd click                   
                    this.Value = this.GetValueFromPosition(new Point(e.X, e.Y));
                }
                else
                {
                    // 1st click = only focus
                    isFocused = true;
                    isKnobRotating = true;
                }

            }
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Mouse move event: drag the pointer to the mouse position
        /// </summary>
        /// <param name="e"></param>
		protected override void OnMouseMove(MouseEventArgs e)
        {
            //--------------------------------------
            //  Following Handles Knob Rotating     
            //--------------------------------------
            if (e.Button == MouseButtons.Left && this.isKnobRotating == true)
            {
                this.Cursor = Cursors.Hand;
                Point p = new Point(e.X, e.Y);
                int posVal = this.GetValueFromPosition(p);
                Value = posVal;
            }

        }

        /// <summary>
        /// Mousewheel: change value
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (isFocused && isKnobRotating && Utility.IsPointinRectangle(new Point(e.X, e.Y), rKnob))
            {
                // the Delta value is always 120, as explained in MSDN
                int v = (e.Delta / 120) * (_maximum - _minimum) / _mouseWheelBarPartitions;
                SetProperValue(Value + v);

                // Avoid to send MouseWheel event to the parent container
                ((HandledMouseEventArgs)e).Handled = true;
            }
        }



        /// <summary>
        /// Leave event: disallow knob rotation
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLeave(EventArgs e)
        {
            // unselect the control (remove dotted border)
            isFocused = false;
            isKnobRotating = false;
            Invalidate();

            base.OnLeave(new EventArgs());
        }

        /// <summary>
        /// Key down event: change value
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (isFocused)
            {
                //--------------------------------------------------------
                // Handles knob rotation with up,down,left and right keys 
                //--------------------------------------------------------
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Right)
                {
                    if (_Value < _maximum) Value = _Value + 1;
                    this.Refresh();
                }
                else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Left)
                {
                    if (_Value > _minimum) Value = _Value - 1;
                    this.Refresh();
                }
            }
        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        #endregion


        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // KnobControl
            // 
            this.ImeMode = System.Windows.Forms.ImeMode.On;
            this.Name = "KnobControl";
            this.Resize += new System.EventHandler(this.KnobControl_Resize);

        }
        #endregion


        #region Draw

        /// <summary>
        /// Draw the pointer of the knob (a small button inside the main button)
        /// </summary>
        /// <param name="Gr"></param>
        private void DrawPointer(Graphics Gr)
        {
            try
            {
                float radius = (float)(rKnob.Width / 2);

                // Draw a line
                if (_knobPointerStyle == KnobPointerStyles.line)
                {
                    int l = (int)radius / 2;
                    int w = l / 4;
                    Point[] pt = GetKnobLine(Gr, l);

                    Gr.DrawLine(new Pen(_PointerColor, w), pt[0], pt[1]);

                }
                else
                {
                    // Draw a circle
                    int w = 0;
                    int h = 0;
                    int l = 0;

                    string strvalmax = _maximum.ToString();
                    string strvalmin = _minimum.ToString();
                    string strval = strvalmax.Length > strvalmin.Length ? strvalmax : strvalmin;
                    double val = Convert.ToDouble(strval);
                    String str = String.Format("{0,0:D}", (int)val);

                    float fSize;
                    SizeF strsize;
                    if (_scaleFontAutoSize)
                    {
                        // Use font family = _scaleFont, but size = automatic
                        fSize = (float)(6F * drawRatio);
                        if (fSize < 6)
                            fSize = 6;
                        strsize = Gr.MeasureString(str, new Font(_scaleFont.FontFamily, fSize));
                    }
                    else
                    {
                        // Use font family = _scaleFont, but size = fixed
                        fSize = _scaleFont.Size;
                        strsize = Gr.MeasureString(str, _scaleFont);
                    }                    

                    int strw = (int)strsize.Width;
                    int strh = (int)strsize.Height;
                    w = Math.Max(strw, strh);
                    // radius of small circle
                    l = (int)radius - w / 2;

                    h = w;

                    Point Arrow = this.GetKnobPosition(l - 2); // Remove 2 pixels to offset the small circle inside the knob

                    // Draw pointer arrow that shows knob position             
                    Rectangle rPointer = new Rectangle(Arrow.X - w / 2, Arrow.Y - w / 2, w, h);


                    //Utility.DrawInsetCircle(ref Gr, rPointer, new Pen(_PointerColor));
                    Utility.DrawInsetCircle(ref Gr, rPointer, new Pen(Utility.GetLightColor(_PointerColor, 55)));
                    
                    Gr.FillEllipse(brushKnobPointer, rPointer);

                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        /// <summary>
        /// Draw graduations
        /// </summary>
        /// <param name="Gr"></param>
        /// <param name="rc">Knob rectangle</param>
        /// <returns></returns>
        private bool DrawDivisions(Graphics Gr, RectangleF rc)
        {
            if (this == null)
                return false;

            float cx = pKnob.X;
            float cy = pKnob.Y;

            float w = rc.Width;
            float h = rc.Height;

            float tx;
            float ty;

            float incr = Utility.GetRadian((_endAngle - _startAngle) / ((_scaleDivisions - 1) * (_scaleSubDivisions + 1)));
            float currentAngle = Utility.GetRadian(_startAngle);

            float radius = (float)(rc.Width / 2);
            float rulerValue = (float)_minimum;

            Font font;

            Pen penL = new Pen(_scaleColor, (2 * drawRatio));
            Pen penS = new Pen(_scaleColor, (1 * drawRatio));

            SolidBrush br = new SolidBrush(_scaleColor);

            PointF ptStart = new PointF(0, 0);
            PointF ptEnd = new PointF(0, 0);
            int n = 0;

            if (_showLargeScale)
            {
                // Size of maxi string
                string strvalmax = _maximum.ToString();
                string strvalmin = _minimum.ToString();
                string strval = strvalmax.Length > strvalmin.Length ? strvalmax : strvalmin;
                double val = Convert.ToDouble(strval);
                //double val = _maximum;
                String str = String.Format("{0,0:D}", (int)val);
                float fSize;
                SizeF strsize;

                if (_scaleFontAutoSize)
                {
                    fSize = (float)(6F * drawRatio);
                    if (fSize < 6)
                        fSize = 6;
                }
                else
                {
                    fSize = _scaleFont.Size;
                }

                font = new Font(_scaleFont.FontFamily, fSize);
                strsize = Gr.MeasureString(str, font);

                int strw = (int)strsize.Width;
                int strh = (int)strsize.Height;
                int wmax = Math.Max(strw, strh);

                float l = 0;
                gradLength = 2 * drawRatio;

                for (; n < _scaleDivisions; n++)
                {                   
                    // draw divisions
                    ptStart.X = (float)(cx + (radius) * Math.Cos(currentAngle));
                    ptStart.Y = (float)(cy + (radius) * Math.Sin(currentAngle));

                    ptEnd.X = (float)(cx + (radius + gradLength) * Math.Cos(currentAngle));
                    ptEnd.Y = (float)(cy + (radius + gradLength) * Math.Sin(currentAngle));

                    Gr.DrawLine(penL, ptStart, ptEnd);


                    //Draw graduation values                                                                                
                    val = Math.Round(rulerValue);
                    str = String.Format("{0,0:D}", (int)val);

                    // If autosize
                    if (_scaleFontAutoSize)
                        strsize = Gr.MeasureString(str, new Font(_scaleFont.FontFamily, fSize));
                    else
                        strsize = Gr.MeasureString(str, new Font(_scaleFont.FontFamily, _scaleFont.Size));



                    if (_drawDivInside)
                    {
                        // graduations values inside the knob                        
                        l = (int)radius - (wmax / 2) - 2;

                        tx = (float)(cx + l * Math.Cos(currentAngle));
                        ty = (float)(cy + l * Math.Sin(currentAngle));

                    }
                    else
                    {
                        // graduation values outside the knob 
                        //l = (Width / 2) - (wmax / 2) ;
                        l = radius + gradLength + wmax/2;

                        tx = (float)(cx + l * Math.Cos(currentAngle));
                        ty = (float)(cy + l * Math.Sin(currentAngle));
                    }

                    Gr.DrawString(str,
                                    font,
                                    br,
                                    tx - (float)(strsize.Width * 0.5),
                                    ty - (float)(strsize.Height * 0.5));



                    rulerValue += (float)((_maximum - _minimum) / (_scaleDivisions - 1));

                    if (n == _scaleDivisions - 1)
                    {
                        break;
                    }


                    // Subdivisions
                    #region SubDivisions

                    if (_scaleDivisions <= 0)
                        currentAngle += incr;
                    else
                    {

                        for (int j = 0; j <= _scaleSubDivisions; j++)
                        {
                            currentAngle += incr;

                            // if user want to display small graduations
                            if (_showSmallScale)
                            {
                                ptStart.X = (float)(cx + radius * Math.Cos(currentAngle));
                                ptStart.Y = (float)(cy + radius * Math.Sin(currentAngle));
                                ptEnd.X = (float)(cx + (radius + gradLength / 2) * Math.Cos(currentAngle));
                                ptEnd.Y = (float)(cy + (radius + gradLength / 2) * Math.Sin(currentAngle));

                                Gr.DrawLine(penS, ptStart, ptEnd);
                            }
                        }
                    }
                    #endregion                    
                }
                font.Dispose();
            }

            return true;
        }

        /// <summary>
        /// Set position of button inside its rectangle to insure that divisions will fit.
        /// </summary>
        private void SetDimensions()
        {
            Font font;

            // Rectangle
            float x, y, w, h;
            x = 0;
            y = 0;
            w = h = Width;

            // Calculate ratio
            drawRatio = w / 150;
            if (drawRatio == 0.0)
                drawRatio = 1;
            

            if (_showLargeScale)
            {
                Graphics Gr = this.CreateGraphics();
                string strvalmax = _maximum.ToString();
                string strvalmin = _minimum.ToString();
                string strval = strvalmax.Length > strvalmin.Length ? strvalmax : strvalmin;
                double val = Convert.ToDouble(strval);

                //double val = _maximum;
                String str = String.Format("{0,0:D}", (int)val);

                float fSize = _scaleFont.Size;

                if (_scaleFontAutoSize)
                {
                    fSize = (float)(6F * drawRatio);
                    if (fSize < 6)
                        fSize = 6;
                    font = new Font(_scaleFont.FontFamily, fSize);
                }
                else
                {
                    fSize = _scaleFont.Size;
                    font = new Font(_scaleFont.FontFamily, _scaleFont.Size);
                }

                SizeF strsize = Gr.MeasureString(str, font);

                // Graduations outside
                gradLength = 4 * drawRatio;

                if (_drawDivInside)
                {
                    // Graduations inside : remove only 2*8 pixels
                    //x = y = 8;
                    x = y = gradLength;
                    w = Width - 2 * x;
                }
                else
                {
                    // remove 2 * size of text and length of graduation
                    //gradLength = 4 * drawRatio;
                    int strw = (int)strsize.Width;
                    int strh = (int)strsize.Height;

                    int max = Math.Max(strw, strh);
                    x = max;
                    y = max;
                    w = (int)(Width - 2 * max - gradLength);                     
                }

                if (w <= 0)
                    w = 1;
                h = w;

                // Rectangle of the rounded knob
                this.rKnob = new Rectangle((int)x, (int)y, (int)w, (int)h);

                Gr.Dispose();
            }
            else
            {
                this.rKnob = new Rectangle(0, 0, Width, Height);
            }


            // Center of knob
            this.pKnob = new Point(rKnob.X + rKnob.Width / 2, rKnob.Y + rKnob.Height / 2);

            // create offscreen image                                 
            this.OffScreenImage = new Bitmap(this.Width, this.Height);
            // create offscreen graphics                              
            this.gOffScreen = Graphics.FromImage(OffScreenImage);


            // Depends on retangle dimensions
            // create LinearGradientBrush for creating knob            
            brushKnob = new LinearGradientBrush(
                rKnob, Utility.GetLightColor(_knobBackColor, 55), Utility.GetDarkColor(_knobBackColor, 55), LinearGradientMode.ForwardDiagonal);

            // create LinearGradientBrush for knobPointer                
            brushKnobPointer = new LinearGradientBrush(
                rKnob, Utility.GetLightColor(_PointerColor, 55), Utility.GetDarkColor(_PointerColor, 55), LinearGradientMode.ForwardDiagonal);
            
        }

        #endregion


        #region resize

        /// <summary>
        /// Resize event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KnobControl_Resize(object sender, System.EventArgs e)
        {
            // Control remains square
            Height = Width;
           
            SetDimensions();            
            Invalidate();
        }

        #endregion


        #region private functions

        /// <summary>
        /// Sets the trackbar value so that it wont exceed allowed range.
        /// </summary>
        /// <param name="val">The value.</param>
        private void SetProperValue(int val)
        {
            if (val < _minimum) Value = _minimum;
            else if (val > _maximum) Value = _maximum;
            else Value = val;
        }

        /// <summary>
        /// gets knob position that is to be drawn on control minus a small amount in order that the knob position stay inside the circle.
        /// </summary>
        /// <returns>Point that describes current knob position</returns>
        private Point GetKnobPosition(int l)
        {
            float cx = pKnob.X;
            float cy = pKnob.Y;

            // FAB: 21/08/18            
            float degree = deltaAngle * (this.Value - _minimum) / (_maximum - _minimum);

            degree = Utility.GetRadian(degree + _startAngle);

            Point Pos = new Point(0, 0)
            {
                X = (int)(cx + l * Math.Cos(degree)),
                Y = (int)(cy + l * Math.Sin(degree))
            };

            return Pos;
        }

        /// <summary>
        /// return 2 points of a line starting from the center of the knob to the periphery
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        private Point[] GetKnobLine(Graphics Gr, int l)
        {
            Point[] pret = new Point[2];

            float cx = pKnob.X;
            float cy = pKnob.Y;


            float radius = (float)(rKnob.Width / 2);

            // FAB: 21/08/18            
            float degree = deltaAngle * (this.Value - _minimum) / (_maximum - _minimum);

            degree = Utility.GetRadian(degree + _startAngle);


            double val = _maximum;
            String str = String.Format("{0,0:D}", (int)val);
            float fSize;
            SizeF strsize;

            if (!_scaleFontAutoSize)
            {
                fSize = _scaleFont.Size;
                strsize = Gr.MeasureString(str, _scaleFont);
            }
            else
            {
                fSize = (float)(6F * drawRatio);
                if (fSize < 6)
                    fSize = 6;

                knobFont = new Font(_scaleFont.FontFamily, fSize);
                strsize = Gr.MeasureString(str, knobFont);
            }

            int strw = (int)strsize.Width;
            int strh = (int)strsize.Height;
            int w = Math.Max(strw, strh);


            Point Pos = new Point(0, 0);

            if (_drawDivInside)
            {
                // Center (from)
                Pos.X = (int)(cx + (radius / 10) * Math.Cos(degree));
                Pos.Y = (int)(cy + (radius / 10) * Math.Sin(degree));
                pret[0] = new Point(Pos.X, Pos.Y);

                // External (to)
                Pos.X = (int)(cx + (radius - w) * Math.Cos(degree));
                Pos.Y = (int)(cy + (radius - w) * Math.Sin(degree));
                pret[1] = new Point(Pos.X, Pos.Y);
            }
            else
            {
                // Internal (from)
                Pos.X = (int)(cx + (radius - drawRatio * 10 - l) * Math.Cos(degree));
                Pos.Y = (int)(cy + (radius - drawRatio * 10 - l) * Math.Sin(degree));


                pret[0] = new Point(Pos.X, Pos.Y);

                // External (to)
                Pos.X = (int)(cx + (radius - 4) * Math.Cos(degree));
                Pos.Y = (int)(cy + (radius - 4) * Math.Sin(degree));

                pret[1] = new Point(Pos.X, Pos.Y);
            }
            return pret;
        }

        /// <summary>
        /// converts geometrical position into value..
        /// </summary>
        /// <param name="p">Point that is to be converted</param>
        /// <returns>Value derived from position</returns>
        private int GetValueFromPosition(Point p)
        {
            float degree = 0;
            int v = 0;

            if (p.X <= pKnob.X)
            {
                degree = (float)(pKnob.Y - p.Y) / (float)(pKnob.X - p.X);
                degree = (float)Math.Atan(degree);

                degree = (degree) * (float)(180 / Math.PI) + (180 - _startAngle);

            }
            else if (p.X > pKnob.X)
            {
                degree = (float)(p.Y - pKnob.Y) / (float)(p.X - pKnob.X);
                degree = (float)Math.Atan(degree);

                degree = (degree) * (float)(180 / Math.PI) + 360 - _startAngle;
            }

            // round to the nearest value (when you click just before or after a graduation!)
            // FAB: 25/08/18            
            v = _minimum + (int)Math.Round(degree * (_maximum - _minimum) / deltaAngle);

            if (v > _maximum) v = _maximum;
            if (v < _minimum) v = _minimum;
            return v;
        }

        #endregion


    }
}
