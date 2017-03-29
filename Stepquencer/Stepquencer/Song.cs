using System;
using System.Collections.Generic;
using System.Text;

namespace Stepquencer
{
    public class Song
    {
        HashSet<Instrument.Note>[] notesAtBeat;

        public int BeatCount
        {
            get
            {
                return notesAtBeat.Length;
            }
        }

        public Song(int beats)
        {
            notesAtBeat = new HashSet<Instrument.Note>[beats];
            for(int i = 0; i < beats; i++)
            {
                notesAtBeat[i] = new HashSet<Instrument.Note>();
            }
        }

        public void AddNote(Instrument.Note note, int beat)
        {
            lock (notesAtBeat)
            {
                notesAtBeat[beat].Add(note);
            }
        }

        public void RemoveNote(Instrument.Note note, int beat)
        {
            lock (notesAtBeat)
            {
                notesAtBeat[beat].Remove(note);
            }
        }

        public Instrument.Note[] NotesAtBeat(int beat)
        {
            Instrument.Note[] notes;
            lock (notesAtBeat)
            {
                notes = new Instrument.Note[notesAtBeat[beat].Count];
                notesAtBeat[beat].CopyTo(notes);
            }
            return notes;
        }
    }
}
