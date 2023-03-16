using System.Linq;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using NAudio.Midi;

namespace MidiToAHK;

internal static class Midi
{
    // Item1 <= milliseconds wait from the previous note
    // Item2 <= all notes that occur then
    public static List<(int, List<int>)> sMidi()
    {
        List<(int, List<int>)> notes = new();

        OpenFileDialog fileDialog = new()
        {
            Filter = "MIDI files (*.midi)|*.midi",
            Multiselect = false,
        };
        
        if (fileDialog.ShowDialog() is true) 
        {
            MidiFile midiFile = new(fileDialog.FileName, strictChecking: false);
            SortedDictionary<int, List<int>> rawNotes = new();
            List<(int, int)> rawTempos = new();
            // for each track
            for (int i = 0; i < midiFile.Tracks; i++)
            {
                // for each midi event
                int totalDeltaTime = 0;
                foreach (var midiEvent in midiFile.Events[i])
                {
                    totalDeltaTime += midiEvent.DeltaTime;
                    if (midiEvent is NoteOnEvent noteOnEvent)
                    {
                        if (rawNotes.ContainsKey(totalDeltaTime)) rawNotes[totalDeltaTime].Add(noteOnEvent.NoteNumber);
                        else rawNotes.Add(totalDeltaTime, new List<int> { noteOnEvent.NoteNumber });
                    }
                    else if (midiEvent is TempoEvent tempoEvent)
                    {
                        rawTempos.Add((totalDeltaTime, tempoEvent.MicrosecondsPerQuarterNote));
                    }
                }
            }

            // convert totalDeltaTime back to deltaTime (this time including all notes across ALL tracks)
            int previousTotalDeltaTime = 0;
            foreach (var item in rawNotes) 
            {
                notes.Add((item.Key - previousTotalDeltaTime, item.Value));
                previousTotalDeltaTime = item.Key;
            }

            // convert deltaTime to milliseocnds of wait
            int microsecondsPerQuarterNote = 500000;
            int newTotalDeltaTime = 0;
            int rawTemposIndex = 0;

            for (int i = 0; i < notes.Count; i++)
            {
                newTotalDeltaTime += notes[i].Item1;
                while (newTotalDeltaTime > rawTempos[rawTemposIndex].Item1)
                {
                    microsecondsPerQuarterNote = rawTempos[rawTemposIndex].Item2;
                    rawTemposIndex++;
                }
                notes[i] = (notes[i].Item1 / midiFile.DeltaTicksPerQuarterNote * microsecondsPerQuarterNote / 1000, notes[i].Item2);
            }
        }
        return notes;
    }
}