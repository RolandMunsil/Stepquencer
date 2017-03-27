using System;
using System.Collections.Generic;
using System.Linq;
using AudioToolbox;
using Foundation;
using UIKit;

namespace Stepquencer.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, "AppDelegate");
		}

        static void IOSAppendStreamingAudio(short[] data, OutputAudioQueue audioQueue)
        {
            unsafe
            {
                fixed (short* p = data)
                {
                    audioQueue.EnqueueBuffer((IntPtr)p, data.Length * 2, null);
                }
            }
        }
	}
}
