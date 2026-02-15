using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace WorldForgeToolbox
{
	[DesignerCategory("Code")]
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
	public class NumericUpDownToolStripControl : ToolStripControlHost
	{
		public NumericUpDownToolStripControl() : base(new NumericUpDown())
		{
			Numeric.ValueChanged += OnValueChanged;
		}

		public NumericUpDown Numeric => Control as NumericUpDown;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Values")]
		public decimal Value
		{
			get => Numeric.Value;
			set
			{
				Numeric.Value = value;
				Parent?.FindForm().Invalidate(true);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Values")]
		public decimal Minimum
		{
			get => Numeric.Minimum;
			set => Numeric.Minimum = value;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Values")]
		public decimal Maximum
		{
			get => Numeric.Maximum;
			set => Numeric.Maximum = value;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Category("Values")]
		public decimal Increment
		{
			get => Numeric.Increment;
			set => Numeric.Increment = value;
		}

		public event EventHandler? ValueChanged
		{
			add => Numeric.ValueChanged += value;
			remove => Numeric.ValueChanged -= value;
		}

		private void OnValueChanged(object? sender, EventArgs e)
		{
			Parent?.FindForm().Invalidate(true);
		}
	}
}
