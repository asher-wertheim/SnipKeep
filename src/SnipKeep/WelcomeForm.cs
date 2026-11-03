using System;
using System.Windows.Forms;

namespace SnipKeep
{
    /// <summary>First-run window: asks where screenshots should go.</summary>
    internal sealed class WelcomeForm : Form
    {
        private readonly SnipKeepApp _app;
        private readonly TextBox _folder;
        private bool _done;

        public WelcomeForm(SnipKeepApp app)
        {
            _app = app;
            Ui.SetupForm(this, "Welcome to SnipKeep", app.AppIcon);

            _folder = Ui.TextBox(360);
            _folder.ReadOnly = true;
            _folder.Text = app.Settings.Folder;

            var change = Ui.Button("Change...");
            change.Click += (s, e) =>
            {
                var picked = Ui.PickFolder(this, _folder.Text);
                if (picked != null) _folder.Text = picked;
            };

            var done = Ui.Button("Done");
            done.Click += (s, e) => Finish();
            AcceptButton = done;

            var doneRow = Ui.Row(done);
            doneRow.Anchor = AnchorStyles.Right;
            doneRow.Margin = new Padding(0, 16, 0, 0);

            var layout = Ui.Column();
            layout.Controls.Add(Ui.Label("Where should screenshots go?", bold: true));
            layout.Controls.Add(Ui.Label("SnipKeep saves every Win+Shift+S screenshot to this folder."));
            layout.Controls.Add(Ui.Row(_folder, change));
            layout.Controls.Add(doneRow);
            Controls.Add(layout);
        }

        private void Finish()
        {
            var error = _app.CompleteWelcome(_folder.Text);
            if (error != null)
            {
                MessageBox.Show(this, error, "SnipKeep", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _done = true;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Closing the window without Done keeps the folder shown, so SnipKeep is never left half set up.
            if (!_done && e.CloseReason == CloseReason.UserClosing)
            {
                var error = _app.CompleteWelcome(_folder.Text);
                if (error != null)
                {
                    MessageBox.Show(this, error, "SnipKeep", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    return;
                }
                _done = true;
            }
            base.OnFormClosing(e);
        }
    }
}
