using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Stepquencer
{
    public class InstrumentButton : Button
    {
        private Instrument instrument;

        public Instrument Instrument
        {
            get { return instrument; }
            set
            {
                this.instrument = value;
                this.Image = $"{instrument.name}.png";
                this.BackgroundColor = instrument.color;
            }
        }

        public bool Selected
        {
            set
            {
                this.BorderColor = value ? Color.White : Color.Black;
            }
        }

        public InstrumentButton(Instrument instrument) : base()
        {
            this.Instrument = instrument;

            this.BorderWidth = 3;
            this.Selected = false;
        }
    }
}
