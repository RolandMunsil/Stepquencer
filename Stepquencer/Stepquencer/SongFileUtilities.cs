using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Stepquencer
{
    static class SongFileUtilities
    {
        public static String PathToSongDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "stepsongs/");
            }
        }

        public static String PathToSongFile(String songName)
        {
            String documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            String savePath = Path.Combine(documentsPath, "stepsongs/");
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            return Path.Combine(savePath, $"{songName}.txt");
        }
    }
}
