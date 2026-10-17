using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MeaMod.Utilities.UI.WinForms
{
    /// <summary>A label that draws "Service Status: " followed by a status word in its own colour.</summary>
    public class ServiceStatusLabel : Control
    {
        private string _Status = "Status";
        private Color _StatusColour = Color.Green;

        /// <summary>Gets or sets the status text drawn after the caption.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Description("Service Status")]
        [Browsable(true)]
        public string Status
        {
            get
            {
                return _Status;
            }

            set
            {
                _Status = value;
                Invalidate();
            }
        }

        /// <summary>Gets or sets the colour the status text is drawn in.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Description("Status Colour")]
        [Browsable(true)]
        public Color StatusColour
        {
            get
            {
                return _StatusColour;
            }

            set
            {
                _StatusColour = value;
                Invalidate();
            }
        }

        /// <summary>Draws the caption and the status text, vertically centred.</summary>
        /// <param name="e">Paint event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var textMeasure = TextRenderer.MeasureText(e.Graphics, "Service Status: ", Font);
            var statusMeasure = TextRenderer.MeasureText(e.Graphics, _Status, Font);
            TextRenderer.DrawText(e.Graphics, "Service Status: ", Font, new Point(2, (int)Math.Round(ClientSize.Height / 2d - textMeasure.Height / 2d)), ForeColor);
            TextRenderer.DrawText(e.Graphics, _Status, Font, new Point(textMeasure.Width, (int)Math.Round(ClientSize.Height / 2d - statusMeasure.Height / 2d)), _StatusColour);
        }
    }
}
