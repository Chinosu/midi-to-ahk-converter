using Microsoft.Win32;
using System.Collections.Generic;
using NAudio.Midi;
using System;

namespace MidiToAHK;

internal static class Midi
{
    // Item1 <= milliseconds wait from the previous note
    // Item2 <= all notes that occur then
    internal static List<(int, List<int>)> Get()
    {
        List<(int, List<int>)> notes = new();

        OpenFileDialog fileDialog = new()
        {
            Filter = "MIDI files (*.mid)|*.mid",
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
                while (rawTemposIndex < rawTempos.Count && newTotalDeltaTime > rawTempos[rawTemposIndex].Item1)
                {
                    microsecondsPerQuarterNote = rawTempos[rawTemposIndex].Item2;
                    rawTemposIndex++;
                }
                notes[i] = (notes[i].Item1 * microsecondsPerQuarterNote / (midiFile.DeltaTicksPerQuarterNote * 1000), notes[i].Item2);
            }
        }
        return notes;
    }
    internal static string ConvertToAHKInstructions(List<(int, List<int>)> midiData)
    {
        string AHKOptimisations = "#NoEnv   \n#MaxHotkeysPerInterval 99000000   \n#HotkeyInterval 99000000  \n#KeyHistory 0 \nListLines Off \nSetBatchLines, -1  \nSetKeyDelay, -1 \nSetMouseDelay, -1  \nSetWinDelay, -1   \nSetControlDelay -1    \nSendMode Input    \n";
        string AHKInstructions  = AHKOptimisations;
        // string AHKInstructions = string.Empty;
        int currentTranspose = 0;
        int currentLowestNote() => 36 + currentTranspose;
        int currentHighestNote() => 96 + currentTranspose;
        const int abosluteLowestNote = 24;
        const int absoluteHighestNote = 108;

        foreach (var notes in midiData)
        {
            AHKInstructions += AHK.Wait(notes.Item1) + "\n";
            foreach (var note in notes.Item2)
            {
                if (note >= abosluteLowestNote && note <= absoluteHighestNote)
                {
                    if (note > currentHighestNote()) 
                    {
                        AHKInstructions += transpose(note - currentHighestNote());
                        AHKInstructions += $"Send {convertToKey(note)}" + "\n";
                    }
                    else if (note < currentLowestNote())
                    {
                        AHKInstructions += transpose(note - currentLowestNote());
                        AHKInstructions += $"Send {convertToKey(note)}" + "\n";
                    }
                    else 
                    {
                        AHKInstructions += $"Send {convertToKey(note)}" + "\n";
                    }
                }
            }
        }
        string convertToKey(int key) => (key - currentTranspose) switch 
        {
            36 => "1",
            37 => "!",
            38 => "2",
            39 => "@",
            40 => "3",
            41 => "4",
            42 => "$",
            43 => "5",
            44 => "`%",
            45 => "6",
            46 => "^",
            47 => "7",
            48 => "8",
            49 => "*",
            50 => "9",
            51 => "{Raw}(",
            52 => "0",
            53 => "q",
            54 => "Q",
            55 => "w",
            56 => "W",
            57 => "e",
            58 => "E",
            59 => "r",
            60 => "t",
            61 => "T",
            62 => "y",
            63 => "Y",
            64 => "u",
            65 => "i",
            66 => "I",
            67 => "o",
            68 => "O",
            69 => "p",
            70 => "P",
            71 => "a",
            72 => "s",
            73 => "S",
            74 => "d",
            75 => "D",
            76 => "f",
            77 => "g",
            78 => "G",
            79 => "h",
            80 => "H",
            81 => "j",
            82 => "J",
            83 => "k",
            84 => "l",
            85 => "L",
            86 => "z",
            87 => "Z",
            88 => "x",
            89 => "c",
            90 => "C",
            91 => "v",
            92 => "V",
            93 => "b",
            94 => "B",
            95 => "n",
            96 => "m",
            _ => throw new ArgumentException((key - currentTranspose).ToString()),
        };

        string transpose(int transposeBy) 
        {
            string AHKInstructions = string.Empty;
            if (transposeBy > 0 && currentTranspose + transposeBy <= 12)
            {
                currentTranspose += transposeBy;
                AHKInstructions += AHK.MouseMov("xAdd", "yAdd") + "\n";
                for (int i = 0; i < transposeBy; i++)
                {
                    AHKInstructions += AHK.Click + "\n"
                        + AHK.Wait() + "\n";
                }
                AHKInstructions += AHK.Wait(100) + "\n";
            }
            else if (transposeBy < 0 && currentTranspose + transposeBy >= -12)
            {
                currentTranspose += transposeBy;
                AHKInstructions += AHK.MouseMov("xLess", "yLess") + "\n";
                for (int i = 0; i > transposeBy; i--)
                {
                    AHKInstructions += AHK.Click + "\n"
                        + AHK.Wait() + "\n";
                }
                AHKInstructions += AHK.Wait(100) + "\n";
            }
            return AHKInstructions;
        }
        return AHKInstructions;
    }

}