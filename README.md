# Knob Control
Yet another Knob Control in C#

![GitHub Logo](/gifs/knobcontrol.jpg)

This code was originally written by Jigar Desai a long time ago as a proof of concept. (The original article for this control can be found on C-SharpCorner.com).

I needed a knob control in C#, with full functionalities for my personal projects and decided to improve it by making its output type a class library and adding as many properties as possible to be a full and flexible control.

The control accepts now graduations with text and sub graduations.
The text of the graduations can be drawn inside or outside the knob.

The "start angle" and the "end angle" of the graduations are adjustable:
```
// "start angle" and "end angle" possible values:
// 90 = bottom (minimum value for "start angle")
// 180 = left
// 270 = top
// 360 = right
// 450 = bottom again (maximum value for "end angle")
// So the couple (90, 450) will give an entire circle 
// and the couple (180, 360) will give half a circle.
```

The MouseWheel event is now managed.

Tip: You might have noticed that a UserControl doesn't show the MouseWheel event in the Properties window. 
Hint of trouble there. The WM_MOUSEWHEEL message bubbles. 
If the control that has the focus doesn't handle it, then Windows passes it on to its Parent repeatedly, until it finds a parent window that wants to handle it.
HandledMouseEventArgs lets you stop the bubbling.

```
protected override void OnMouseWheel(MouseEventArgs e)
{           
    base.OnMouseWheel(e);
    if ( isFocused && isKnobRotating && 
    Utility.isPointinRectangle(new Point(e.X, e.Y), rKnob))
    {                
        // the Delta value is always 120, as explained in MSDN
        int v = (e.Delta / 120) * (_maximum - _minimum) / _mouseWheelBarPartitions;
        SetProperValue(Value + v);

        // Avoid to send MouseWheel event to the parent container
        ((HandledMouseEventArgs)e).Handled = true;
    }
}
```

#Properties

Parameter | signification
------------ | -------------
**Values**                | 
Value                     | Gets or sets the value of the Knob 
_minimum (int, 0)         | Gets or sets the lower limit of the range of values 
_maximum (int, 100)       | Gets or sets the upper limit of the range of values 
_LargeChange (int)        | Gets or sets a value to be added to or subtracted to the Value property when the change is made with the mouse. 
_SmallChange (int)        | Gets or sets a value to be added to or subtracted to the Value property when the change is made with the keyboard. 
_mouseWheelBarPartitions  | Sets how many parts are bar divided when using mouse wheel 
**Appearance**            | 
_showLargeScale (bool)    | Displays or hides the main tick marks 
_showSmallScale (bool)    | Displays or hides intermediate tick marks 
KnobPointerStyles (enum)  | Sets the style of the knob pointer: a circle or a line 
_startAngle               | Sets the start angle to display graduations (default 135, min 90) 
_endAngle                 | Sets the end angle to display graduations (default 405, max 450) 
**Ticks**                 | 
_scaleDivisions (int)     | Sets the number of intervals between minimum and maximum 
_scaleSubDivisions (int)  | Sets the number of subdivisions between main tick marks. 
_drawDivInside (bool)     | Draws graduation strings inside or outside the knob circle 
**Colors**                | 
_scaleColor (Color)       | Color of the tick marks 
_knobBackColor (Color)    | backcolor of the knob (default LightGray) 
_PointerColor (Color)     | backcolor of the pointer (default SlateBlue) 
**Font**                  | 
_scaleFont (Font)         | Sets the Font (see _scaleFontAutoSize for the size) 
_scaleFontAutoSize (bool) | if true, the font size is calculated automatically, otherwise it is the selected size 



The single event managed by this control is the ValueChanged event:
```
private void knobControl1_ValueChanged(object Sender)
{
     label1.Text = knobControl1.Value.ToString();
}
```
This project was developed with Visual Studio version 2017.

The code is not difficult to understand and to modify to suit your needs. 
The main difficulty is that you need some Math skills to understand how all is displayed (not so much in fact: sinus and cosinus level required :-)).

# Author
Fabrice Lacharme
