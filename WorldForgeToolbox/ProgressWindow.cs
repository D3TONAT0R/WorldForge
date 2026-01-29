using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RegionViewer
{
	public partial class ProgressWindow : Form
	{
		public static ProgressWindow Show(CancellationTokenSource token)
		{
			var window = new ProgressWindow();
			window.progressBar.Maximum = 1;
			window.cancellationTokenSource = token;
			window.Show();
			_ = window.Handle;
			return window;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MaxProgressValue
		{
			get => progressBar.Maximum;
			set => Invoke(() => {
				progressBar.Maximum = value;
				Invalidate(true);
			});
		}

		private CancellationTokenSource? cancellationTokenSource;

		private ProgressWindow()
		{
			InitializeComponent();
		}

		public void UpdateProgress(int value, string? text = null)
		{
			Invoke(() =>
			{
				progressBar.Value = value;
				if (text != null) progressLabel.Text = text;
				Invalidate(true);
			});
		}

		public void UpdateText(string text)
		{
			Invoke(() =>
			{
				progressLabel.Text = text;
			});
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
			cancellationTokenSource?.Cancel();
		}
	}
}
