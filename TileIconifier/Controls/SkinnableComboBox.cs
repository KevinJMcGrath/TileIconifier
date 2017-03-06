﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TileIconifier.Controls
{
    class SkinnableComboBox : ComboBox
    {
        private const TextFormatFlags DEFAULT_TEXT_FLAGS = TextFormatFlags.Default | TextFormatFlags.VerticalCenter;        

        private Font glyphFont = new Font("Marlett", 10);

        private bool CanCustomDraw
        {
            get
            {
                if (FlatStyle == FlatStyle.Flat && DropDownStyle == ComboBoxStyle.DropDownList)
                    return true;
                else
                    return false;
            }
        }

        private TextFormatFlags TextFlags
        {
            get
            {
                TextFormatFlags flags = DEFAULT_TEXT_FLAGS;
                if (RightToLeft == RightToLeft.Yes)
                    flags = flags | TextFormatFlags.RightToLeft | TextFormatFlags.Right;

                return flags;
            }
        }

        //There is no event for when this properties is changed,
        //so we need reimplement it to allow us to do stuff when it is changed.
        public new FlatStyle FlatStyle
        {
            get { return base.FlatStyle; }
            set
            {
                base.FlatStyle = value;
                ConfigureDrawingProperties(true);
            }
        }

        [DefaultValue(typeof(Color), "Control")]
        private Color flatButtonBackColor = SystemColors.Control;
        public Color FlatButtonBackColor
        {
            get
            {
                return flatButtonBackColor;
            }
            set
            {
                if (flatButtonBackColor != value)
                {
                    flatButtonBackColor = value;
                    if (CanCustomDraw)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [DefaultValue(typeof(Color), "ControlText")]
        private Color flatButtonForeColor = SystemColors.ControlText;
        public Color FlatButtonForeColor
        {
            get
            {
                return flatButtonForeColor;
            }
            set
            {
                if (flatButtonForeColor != value)
                {
                    flatButtonForeColor = value;
                    if (CanCustomDraw)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [DefaultValue(typeof(Color), "GrayText")]
        private Color flatButtonDisabledForeColor = SystemColors.GrayText;
        public Color FlatButtonDisabledForeColor
        {
            get
            {
                return flatButtonDisabledForeColor;
            }
            set
            {
                if (flatButtonDisabledForeColor != value)
                {
                    flatButtonDisabledForeColor = value;
                    if (CanCustomDraw)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [DefaultValue(typeof(Color), "ControlDark")]
        private Color flatButtonBorderColor = SystemColors.ControlDark;
        public Color FlatButtonBorderColor
        {
            get
            {
                return flatButtonBorderColor;
            }
            set
            {
                if (flatButtonBorderColor != value)
                {
                    flatButtonBorderColor = value;
                    if (CanCustomDraw)
                    {
                        Invalidate();
                    }
                }
            }
        }

        [DefaultValue(typeof(Color), "Highlight")]
        private Color flatButtonBorderFocusedColor = SystemColors.Highlight;
        public Color FlatButtonBorderFocusedColor
        {
            get
            {
                return flatButtonBorderFocusedColor;
            }
            set
            {
                if (flatButtonBorderFocusedColor != value)
                {
                    flatButtonBorderFocusedColor = value;
                    if (CanCustomDraw)
                    {
                        Invalidate();
                    }
                }
            }
        }

        private void ConfigureDrawingProperties(bool pNeedToRestoreDefaultProperties)
        {
            if (CanCustomDraw)
            {
                SetStyle(ControlStyles.UserPaint, true);
                DrawMode = DrawMode.OwnerDrawFixed;
            }
            else if (pNeedToRestoreDefaultProperties)
            {
                SetStyle(ControlStyles.UserPaint, false);
                DrawMode = DrawMode.Normal;
            }
        }

        protected override void OnDropDownStyleChanged(EventArgs e)
        {
            base.OnDropDownStyleChanged(e);

            ConfigureDrawingProperties(true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!CanCustomDraw)
                return;

            Rectangle bounds = ClientRectangle;
            int inGlyphAreaWidth = SystemInformation.HorizontalScrollBarThumbWidth;

            //Border
            Color borderColor;
            if (Focused)
                borderColor = FlatButtonBorderFocusedColor;
            else
                borderColor = FlatButtonBorderColor;

            //Compensation needed when drawing a rectangle with GDI+
            bounds.Width--;
            bounds.Height--;
           
            using (Pen p = new Pen(borderColor))
                e.Graphics.DrawRectangle(p, bounds);

            //Background
            //Removes the 1 pixel GDI+ compensation.
            bounds.Width++;
            bounds.Height++;
            //Skrinks the rectangle to fit within the borders we have just drawn.
            bounds.Inflate(-1, -1);

            using (SolidBrush b = new SolidBrush(FlatButtonBackColor))
                e.Graphics.FillRectangle(b, bounds);

            //Selected item text
            //We need to calculate the text bounds even when we don't draw text, 
            //because we use that rectangle for the glyphRect.
            //Same thing for the textColor.
            if (RightToLeft == RightToLeft.Yes)
            {
                bounds.X += inGlyphAreaWidth;
            }
            bounds.Width -= inGlyphAreaWidth;

            Color textColor;
            if (Enabled)
            {
                textColor = FlatButtonForeColor;
            }
            else
            {
                textColor = FlatButtonDisabledForeColor;
            }

            if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
            {
                string stText = GetItemText(Items[SelectedIndex]);
                
                TextRenderer.DrawText(e.Graphics, stText, Font, bounds, textColor, TextFlags);
            }

            //Glyph button
            Rectangle buttonRect = new Rectangle();
            buttonRect.Width = inGlyphAreaWidth;
            buttonRect.Height = bounds.Height;
            buttonRect.Y = bounds.Y;
            TextFormatFlags glyphFlags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            if (RightToLeft == RightToLeft.Yes)
            {
                buttonRect.X = bounds.X - inGlyphAreaWidth;
            }
            else
            {
                buttonRect.X = bounds.X + bounds.Width;
            }
            TextRenderer.DrawText(e.Graphics, "u", glyphFont, buttonRect, textColor, glyphFlags);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            //This method is only called when items are custom drawn and 
            //items always look the same regardless of the DropDownStyle anyway,
            //so there is no need to check if CanCustomDraw.

            e.DrawBackground();
            e.DrawFocusRectangle();

            int inIndex = e.Index;
            if (inIndex >= 0 & inIndex < Items.Count)
            {
                string stItemText;
                Color colTextColor;

                stItemText = GetItemText(Items[inIndex]);

                if (e.State.HasFlag(DrawItemState.Selected))
                {
                    colTextColor = SystemColors.HighlightText;
                }
                else
                {
                    colTextColor = ForeColor;
                }
                TextRenderer.DrawText(e.Graphics, stItemText, Font, e.Bounds, colTextColor, TextFlags);
            }
        }
    }
}
