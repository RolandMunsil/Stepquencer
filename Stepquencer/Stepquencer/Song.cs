using System;
using System.Collections.Generic;
using System.Text;

namespace Stepquencer
{
    public class Song
    {
        /// <summary>
        /// An array of hashsets. Each hashset contains all of the notes in a given beat
        /// </summary>
        HashSet<Instrument.Note>[] beats;

        /// <summary>
        /// The number of beats in this song
        /// </summary>
        public int BeatCount
        {
            get
            {
                return beats.Length;
            }
        }

        public Song(int numBeats)
        {
            beats = new HashSet<Instrument.Note>[numBeats];
            for(int i = 0; i < numBeats; i++)
            {
                beats[i] = new HashSet<Instrument.Note>();
            }
        }

        /// <summary>
        /// Add a note to the song at a specific beat
        /// </summary>
        /// <param name="note">The note to add to the song</param>
        /// <param name="beat">The beat to add the note at</param>
        public void AddNote(Instrument.Note note, int beat)
        {
            lock (beats)
            {
                beats[beat].Add(note);
            }
        }

        /// <summary>
        /// Remove a note from the song at a specific beat
        /// </summary>
        /// <param name="note">The note to remove from the song</param>
        /// <param name="beat">The beat to remove the note from</param>
        public void RemoveNote(Instrument.Note note, int beat)
        {
            lock (beats)
            {
                beats[beat].Remove(note);
            }
        }



        public void ClearAllBeats()
        {
            lock (beats)
            {
                foreach (HashSet<Instrument.Note> hashset in beats)
                {
                    hashset.Clear();
                }
            }
        }
        
        /// <summary>
        /// Returns an array containing the notes at the given beat
        /// </summary>
        /// <param name="beat">The beat to get the notes from</param>
        /// <returns>The notes at <code>beat</code></returns>
        public Instrument.Note[] NotesAtBeat(int beat)
        {
            Instrument.Note[] notes;

            //Lock so that the notes do not change while they are being copied
            lock (beats)
            {
                notes = new Instrument.Note[beats[beat].Count];
                beats[beat].CopyTo(notes);
            }
            return notes;
        }
    }
}
