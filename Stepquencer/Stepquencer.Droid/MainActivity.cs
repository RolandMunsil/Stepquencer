using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Media;
using System.Collections.Generic;
using System.Timers;
using System.Threading;

namespace Stepquencer.Droid
{
    [Activity (Label = "Stepquencer", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        struct Note
        {
            public AudioTrack track;
            public AutoResetEvent stopPlayingEvent;

            public Note(AudioTrack track, AutoResetEvent stopPlayingEvent)
            {
                this.track = track;
                this.stopPlayingEvent = stopPlayingEvent;
            }
        }

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            global::Xamarin.Forms.Forms.Init (this, bundle);
            LoadApplication (new Stepquencer.App ());

            /*
            Note hihat = new Note(AudioTrackFromRawResource(Resource.Raw.hihat), new AutoResetEvent(false));
            Note snare = new Note(AudioTrackFromRawResource(Resource.Raw.snare), new AutoResetEvent(false));
            Note bassdrum = new Note(AudioTrackFromRawResource(Resource.Raw.bassdrum), new AutoResetEvent(false));

            //Make song
            Note[][] song = new Note[16][];
            for(int t = 0; t < 16; t++)
            {
                List<Note> notesThisTimestep = new List<Note>();
                notesThisTimestep.Add(hihat);
                if (t % 2 == 0)
                    notesThisTimestep.Add(bassdrum);
                if (t % 4 == 2)
                    notesThisTimestep.Add(snare);

                song[t] = notesThisTimestep.ToArray();
            }

            //Start up threads
            Barrier playBarrier = new Barrier(0);

            MakeNoteThread(hihat.track, hihat.stopPlayingEvent, playBarrier);
            MakeNoteThread(snare.track, snare.stopPlayingEvent, playBarrier);
            MakeNoteThread(bassdrum.track, bassdrum.stopPlayingEvent, playBarrier);

            int curBeat = 0;
            System.Timers.Timer timer = new System.Timers.Timer(250);
            timer.Elapsed += delegate (Object source, System.Timers.ElapsedEventArgs e)
            {
                //Dont let multiple notes try to play at once.
                if (!Monitor.TryEnter(timer)) return;

                if (song[curBeat].Length > 0)
                {
                    if (playBarrier.ParticipantCount > 0)
                    {
                        playBarrier.RemoveParticipants(playBarrier.ParticipantCount);
                    }
                    playBarrier.AddParticipants(song[curBeat].Length);

                    for(int i = 0; i < song[curBeat].Length; i++)
                    {
                        song[curBeat][i].stopPlayingEvent.Set();
                    }
                }

                curBeat = (curBeat + 1) % 16;

                Monitor.Exit(timer);
            };
            timer.Start();
            */
        }
        /*
        private void MakeNoteThread(AudioTrack note, AutoResetEvent stopPlayingEvent, Barrier playBarrier)
        {
            new System.Threading.Thread(delegate ()
            {
                while (true)
                {
                    stopPlayingEvent.WaitOne();

                    note.Pause();
                    //TODO: WAV header size seems to be somewhat variable. Come up with a better solution than this.
                    note.SetPlaybackHeadPosition(44);

                    playBarrier.SignalAndWait();
                    note.Play();
                }
            }).Start();
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
                ChannelOut.Mono,
                // Audio encoding
                Encoding.Pcm16bit,
                // Length of the audio clip.
                audioBuffer.Length,
                // Mode. Stream or static.
                AudioTrackMode.Static);

            audioTrack.Write(audioBuffer, 0, audioBuffer.Length, WriteMode.Blocking);
            return audioTrack;
        }
        */
    }
}

