using System.Drawing;
using System.Windows.Forms;

namespace SnipKeep
{
    /// <summary>Small helpers so the windows can be built in code without designer files.</summary>
    internal static class Ui
    {
        public static void SetupForm(Form form, string title, Icon icon)
        {
            form.Text = title;
            form.Icon = icon;
            form.Font = SystemFonts.MessageBoxFont;
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.AutoSize = true;
            form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = true;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.ShowInTaskbar = true;
            form.Padding = new Padding(16);
        }

        public static TableLayoutPanel Column()
        {
            return new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };
        }

        public static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 4),
            };
            row.Controls.AddRange(controls);
            return row;
        }

        public static Label Label(string text, bool bold = false, int topMargin = 0)
        {
            var label = new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, topMargin, 0, 4),
            };
            if (bold) label.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
            return label;
        }

        public static Button Button(string text)
        {
            return new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(88, 0),
                Margin = new Padding(6, 0, 0, 0),
                UseVisualStyleBackColor = true,
            };
        }

        public static TextBox TextBox(int width)
        {
            return new TextBox { Width = width, Margin = new Padding(0, 1, 0, 0) };
        }

        /// <summary>Shows a folder picker. Returns the chosen folder, or null if the user cancelled.</summary>
        public static string PickFolder(IWin32Window owner, string current)
        {
            using (var dialog = new FolderBrowserDialog
            {
                Description = "Choose where SnipKeep saves screenshots.",
                ShowNewFolderButton = true,
                SelectedPath = current ?? "",
            })
            {
                return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }
    }
}
