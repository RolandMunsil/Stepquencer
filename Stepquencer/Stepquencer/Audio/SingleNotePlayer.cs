using System;
using System.Collections.Generic;
using System.Text;

#if __ANDROID__
using Android.Media;
#endif

#if __IOS__
using AVFoundation;
using AudioToolbox;
using AudioUnit;
#endif

namespace Stepquencer
{
    static class SingleNotePlayer
    {
#if __IOS__
        //The ios object that manages audio playback
        static OutputAudioQueue audioQueue;
#endif
#if __ANDROID__
        //The android object that manages audio playback
        static AudioTrack playingTrack;
#endif

        static object syncObj = new object();

        /// <summary>
        /// Plays a single note. Separate from the rest of the song playing code
        /// </summary>
        public static void PlayNote(Instrument.Note note)
        {
            lock (syncObj)
            {
#if __ANDROID__
                if (playingTrack != null)
                {
                    //We use pause instead of stop because pause stops playing immediately
                    playingTrack.Pause();
                    playingTrack.Release();
                    playingTrack.Dispose();
                }
#endif
#if __IOS__
                if (audioQueue != null)
                {
                    //Pass true to stop immediately
                    audioQueue.Stop(true);
                    audioQueue.Dispose();
                }
#endif

#if __ANDROID__
                playingTrack = new AudioTrack(
                    // Stream type
                    Android.Media.Stream.Music,
                    // Frequency
                    SongPlayer.PLAYBACK_RATE,
                    // Mono or stereo
                    ChannelOut.Mono,
                    // Audio encoding
                    Android.Media.Encoding.Pcm16bit,
                    // Length of the audio clip in bytes
                    (note.data.Length * 2),
                    // Mode. Stream or static.
                    AudioTrackMode.Static);

                playingTrack.Write(note.data, 0, note.data.Length);
                playingTrack.Play();
#endif
#if __IOS__
                audioQueue = new OutputAudioQueue(AudioStreamBasicDescription.CreateLinearPCM(SongPlayer.PLAYBACK_RATE, 1, 16, false));
                unsafe
                {
                    AudioQueueBuffer* buffer;
                    audioQueue.AllocateBuffer(note.data.Length * 2, out buffer);

                    fixed (short* beatData = note.data)
                    {
                        buffer->CopyToAudioData((IntPtr)beatData, note.data.Length * 2);
                    }

                    audioQueue.EnqueueBuffer((IntPtr)buffer, note.data.Length * 2, null);
                }

                audioQueue.Start();
#endif
            }
        }

        public static void StopPlayingNote()
        {
            lock (syncObj)
            {
#if __ANDROID__
                if (playingTrack != null)
                {
                    //We use pause instead of stop because pause stops playing immediately
                    playingTrack.Pause();
                    //Don't dispose, as it will be dispoed next time PlayNote is called
                }
#endif
#if __IOS__
                if (audioQueue != null)
                {
                    //Pass true to stop immediately
                    audioQueue.Stop(true);
                    //Don't dispose, as it will be dispoed next time PlayNote is called
                }
#endif
            }
        }
    }
}
