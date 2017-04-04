using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace Stepquencer
{
    public partial class LoadPage : ContentPage
    {
        MainPage mainpage;
        Song song;

        public LoadPage(MainPage mainpage, Song song)
        {
            this.mainpage = mainpage;
            this.song = song;


            // Temporary, to be moved to an OnButtonClicked method or something along those lines

            Song loadedSong = LoadSongFromFile("TEST");
            mainpage.SetSong(loadedSong);

            Button test = new Button { Text = "Load TEST" };
            test.Clicked += returnToMainPage;
            Content = test;
        }

        async void returnToMainPage(Object sender, EventArgs e)
        {
            //Navigation.InsertPageBefore(mainpage, this);
            await Navigation.PopToRootAsync();
        }


        private static Song LoadSongFromFile(String songName)
        {
            String filePath = MoreOptionsPage.PathToSongFile(songName);

            Song loadedSong;

            using (StreamReader file = File.OpenText(filePath))
            {
                int totalBeats = int.Parse(file.ReadLine().Split(' ')[0]);
                loadedSong = new Song(totalBeats);
                for (int i = 0; i < totalBeats; i++)
                {
                    String header = file.ReadLine();
                    if (!header.Contains($"Beat {i}"))
                        throw new Exception("Invalid file or bug in file loader");
                    int numNotes = int.Parse(header.Split('|')[1]);

                    for (int n = 0; n < numNotes; n++)
                    {
                        String[] noteStringParts = file.ReadLine().Split(':');
                        String instrName = noteStringParts[0];
                        int semitoneShift = int.Parse(noteStringParts[1]);

                        Instrument.Note note = Instrument.loadedInstruments[instrName].AtPitch(semitoneShift);
                        loadedSong.AddNote(note, i);
                    }
                }
            }

            return loadedSong;
        }

        private static void DeleteSongFile(String songName)
        {
            File.Delete(MoreOptionsPage.PathToSongFile(songName));
        }
    }
}
