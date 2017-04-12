using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Stepquencer
{
    class InstrumentButton : Button
    {
        public Instrument instrument;

        public InstrumentButton(Instrument instrument) : base()
        {
            this.instrument = instrument;
            this.Image = $"{instrument.name}.png";
            this.BackgroundColor = instrument.color;

            this.BorderWidth = 3;
            this.Selected = false;
        }

        public bool Selected
        {
            set
            {
                this.BorderColor = value ? Color.White : Color.Black;
            }
        }
    }
}
