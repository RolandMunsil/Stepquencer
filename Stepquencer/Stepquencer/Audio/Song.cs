using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stepquencer
{
    /// <summary>
    /// Represents a song (i.e. a set of beats containing notes).
    /// Designed to be thread-safe, since it is accessed by both the UI thread and the audio thread
    /// </summary>
    public class Song
    {
        /// <summary>
        /// An array of hashsets. Each hashset contains all of the notes in a given beat
        /// </summary>
        HashSet<Instrument.Note>[] beats;

        /// <summary>
        ///The instruments associated with this song. The song may not necessarily have notes
        ///from all of the instruments in Instruments, but when the song is loaded these are
        ///the instruments that will be in the sidebar
        /// </summary>
        public Instrument[] Instruments { get; private set; }
        public int Tempo { get; set; }

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

        /// <summary>
        /// Returns true if song is empty
        /// </summary>
        /// <value><c>true</c> if is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get { return beats.All(b=>b.Count == 0); }
        }

        public Song(int numBeats, Instrument[] instruments, int tempo)
        {
            //Initialize the array of hashsets
            beats = new HashSet<Instrument.Note>[numBeats];
            for(int i = 0; i < numBeats; i++)
            {
                beats[i] = new HashSet<Instrument.Note>();
            }
            Instruments = instruments;
            Tempo = tempo;
        }

        /// <summary>
        /// Add a note to the song at a specific beat
        /// </summary>
        /// <param name="note">The note to add to the song</param>
        /// <param name="beat">The beat to add the note at</param>
        public void AddNote(Instrument.Note note, int beat)
        {
            //We use a lock so that the beat data is not modified while we are adding
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
            //We use a lock so that the beat data is not modified while we are removing
            lock (beats)
            {
                beats[beat].Remove(note);
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

        /// <summary>
        /// Replaces the instruments listed in oldInstruments with the respective instrument in newInstruments
        /// </summary>
        public void ReplaceInstruments(IList<Instrument> oldInstruments, IList<Instrument> newInstruments)
        {
            //Lock so that the beats do not change while they are being modified
            lock (beats)
            {
                foreach(HashSet<Instrument.Note> beat in beats)
                {
                    List<Instrument.Note> oldNotes = beat.ToList();
                    beat.Clear();

                    foreach(Instrument.Note oldNote in oldNotes)
                    {
                        //If this note's instrument is in oldInstrument, replace it with a note
                        //of the same pitch but from the new instrument
                        int index = oldInstruments.IndexOf(oldNote.instrument);
                        if(index < 0)
                        {
                            //Not supposed to be replaced, so just readd it.
                            beat.Add(oldNote);
                        }
                        else
                        {
                            //Replace with new note
                            Instrument.Note newNote = newInstruments[index].AtPitch(oldNote.semitoneShift);
                            beat.Add(newNote);
                        }
                    }
                }
                
                //Update Instruments
                for(int i = 0; i < oldInstruments.Count; i++)
                {
                    Instrument oldInstr = oldInstruments[i];
                    Instruments[Array.IndexOf(Instruments, oldInstr)] = newInstruments[i];
                }
            }
        }
    }
}
