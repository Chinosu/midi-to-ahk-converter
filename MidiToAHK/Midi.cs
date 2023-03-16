using System.Linq;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using NAudio.Midi;

namespace MidiToAHK;

internal static class Midi
{
    public static List<(int, List<int>)> sMidi()
    {
        OpenFileDialog fileDialog = new()
        {
            Filter = "MIDI files (*.midi)|*.midi",
            Multiselect = false,
        };
        SortedDictionary<int, List<int>> rawNotes = new(); 
        if (fileDialog.ShowDialog() is true) 
        {
            MidiFile mf = new(fileDialog.FileName, strictChecking: false);

            // for each track
            for (int i = 0; i < mf.Tracks; i++) 
            {
                // for each midi event
                int totalDeltaTime = 0;
                foreach (var midiEvent in mf.Events[i])
                {
                    totalDeltaTime += midiEvent.DeltaTime;
                    if (midiEvent is NoteOnEvent noteOnEvent)
                    {
                        if (rawNotes.ContainsKey(totalDeltaTime)) rawNotes[totalDeltaTime].Add(noteOnEvent.NoteNumber);
                        else rawNotes.Add(totalDeltaTime, new List<int>{noteOnEvent.NoteNumber});
                    }
                }
            }
        }

        // convert totalDeltaTime back to deltaTime (this time including all notes across ALL tracks)
        List<(int, List<int>)> notes = new();
        int previousTotalDeltaTime = 0;
        foreach (var item in rawNotes) 
        {
            notes.Add((item.Key - previousTotalDeltaTime, item.Value));
            previousTotalDeltaTime = item.Key;
        }
        return notes;
    }
}