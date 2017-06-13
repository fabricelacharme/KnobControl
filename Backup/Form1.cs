using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace knobControl
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private KnobControl.KnobControl Rknob;
		private KnobControl.KnobControl Bknob;
		private System.Windows.Forms.Label label1;
		private KnobControl.KnobControl GKnob;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Rknob = new KnobControl.KnobControl();
			this.Bknob = new KnobControl.KnobControl();
			this.GKnob = new KnobControl.KnobControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// Rknob
			// 
			this.Rknob.ForeColor = System.Drawing.Color.Red;
			this.Rknob.ImeMode = System.Windows.Forms.ImeMode.On;
			this.Rknob.LargeChange = 51;
			this.Rknob.Location = new System.Drawing.Point(8, 176);
			this.Rknob.Maximum = 255;
			this.Rknob.Minimum = 0;
			this.Rknob.Name = "Rknob";
			this.Rknob.ShowLargeScale = true;
			this.Rknob.ShowSmallScale = false;
			this.Rknob.Size = new System.Drawing.Size(96, 96);
			this.Rknob.SmallChange = 1;
			this.Rknob.TabIndex = 0;
			this.Rknob.Value = 0;
			this.Rknob.ValueChanged += new KnobControl.ValueChangedEventHandler(this.Rknob_ValueChanged);
			// 
			// Bknob
			// 
			this.Bknob.ForeColor = System.Drawing.Color.Blue;
			this.Bknob.ImeMode = System.Windows.Forms.ImeMode.On;
			this.Bknob.LargeChange = 50;
			this.Bknob.Location = new System.Drawing.Point(216, 176);
			this.Bknob.Maximum = 250;
			this.Bknob.Minimum = 0;
			this.Bknob.Name = "Bknob";
			this.Bknob.ShowLargeScale = true;
			this.Bknob.ShowSmallScale = true;
			this.Bknob.Size = new System.Drawing.Size(104, 96);
			this.Bknob.SmallChange = 10;
			this.Bknob.TabIndex = 2;
			this.Bknob.Value = 0;
			this.Bknob.ValueChanged += new KnobControl.ValueChangedEventHandler(this.Bknob_ValueChanged);
			// 
			// GKnob
			// 
			this.GKnob.ForeColor = System.Drawing.Color.Green;
			this.GKnob.ImeMode = System.Windows.Forms.ImeMode.On;
			this.GKnob.LargeChange = 51;
			this.GKnob.Location = new System.Drawing.Point(112, 176);
			this.GKnob.Maximum = 255;
			this.GKnob.Minimum = 0;
			this.GKnob.Name = "GKnob";
			this.GKnob.ShowLargeScale = true;
			this.GKnob.ShowSmallScale = false;
			this.GKnob.Size = new System.Drawing.Size(96, 96);
			this.GKnob.SmallChange = 1;
			this.GKnob.TabIndex = 1;
			this.GKnob.Value = 0;
			this.GKnob.ValueChanged += new KnobControl.ValueChangedEventHandler(this.Gknob_ValueChanged);
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(16, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(304, 128);
			this.label1.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 280);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Red";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(136, 280);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 23);
			this.label3.TabIndex = 5;
			this.label3.Text = "Green";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(248, 280);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 23);
			this.label4.TabIndex = 6;
			this.label4.Text = "Blue";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(336, 317);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label4,
																		  this.label3,
																		  this.label2,
																		  this.label1,
																		  this.GKnob,
																		  this.Bknob,
																		  this.Rknob});
			this.Name = "Form1";
			this.Text = "C-SharpCorner";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Rknob_ValueChanged(object Sender)
		{
			ChangeLabelColor();
		}
		private void ChangeLabelColor()
		{
			this.label1.BackColor = Color.FromArgb(this.Rknob.Value,this.GKnob.Value,this.Bknob.Value);
		}

		private void Gknob_ValueChanged(object Sender)
		{
			ChangeLabelColor();
		}

		private void Bknob_ValueChanged(object Sender)
		{
			ChangeLabelColor();
		}
	}
}
