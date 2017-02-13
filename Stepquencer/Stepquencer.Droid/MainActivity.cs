using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Media;

namespace Stepquencer.Droid
{
	[Activity (Label = "Stepquencer", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new Stepquencer.App ());

			/*
            AudioTrack timTrack = AudioTrackFromRawResource(Resource.Raw.tim);
            AudioTrack echTrack = AudioTrackFromRawResource(Resource.Raw.ech);

            int i = 0;
            while (true)
            {
                System.Threading.Thread.Sleep(500);

                //TODO: Does it matter whether you use Stop() vs. Pause()?
                timTrack.Pause();

                //Size of WAV header is 44 bytes
                timTrack.SetPlaybackHeadPosition(44);
                timTrack.SetPlaybackRate((int)(44100 * Math.Pow(2, (i%12) / 12.0)));

                if(i%2 == 1)
                {
                    //Resetting takes about 0.3-2.0ms (in debug mode)
                    //Playing takes about 0.5-1ms (in debug mode)

                    echTrack.Pause();
                    echTrack.SetPlaybackHeadPosition(44);
                    echTrack.Play();
                }

                timTrack.Play();

                i++;
            }
            */
            //audioTrack.Dispose();
        }

        private AudioTrack AudioTrackFromRawResource(int resourceID)
        {
            int fileLength;
            using (Android.Content.Res.AssetFileDescriptor fileInfo = Resources.OpenRawResourceFd(resourceID))
            {
                fileLength = (int)fileInfo.Length;
            }

            byte[] audioBuffer;
            using (System.IO.Stream stream = Resources.OpenRawResource(resourceID))
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(stream))
                {
                    audioBuffer = br.ReadBytes(fileLength);
                }
            }

            AudioTrack audioTrack = new AudioTrack(
                // Stream type
                Stream.Music,
                // Frequency
                44100,
                // Mono or stereo
                ChannelOut.Stereo,
                // Audio encoding
                Encoding.Pcm16bit,
                // Length of the audio clip.
                audioBuffer.Length,
                // Mode. Stream or static.
                AudioTrackMode.Static);

            audioTrack.Write(audioBuffer, 0, audioBuffer.Length, WriteMode.Blocking);
            return audioTrack;
        }
    }
}

