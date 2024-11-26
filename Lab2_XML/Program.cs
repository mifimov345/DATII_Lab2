using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Xml.Xsl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;


namespace Lab2_XML
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CreateXmlFile();
            PerformXPathQueries();
            ValidateWithDtd();
            ValidateWithXsd();
            TransformToHtml();
            PerformJsonQueries();
            Console.ReadKey();
        }

        //Создаем 5 альбомов программным путем
        public static void CreateXmlFile()
        {
            var xml = new XmlDocument();

            var albums = xml.CreateElement("albums");

            for (int i = 1; i <= 5; i++)
            {
                var album = xml.CreateElement("album");

                // Название альбома
                var title = xml.CreateElement("title");
                title.InnerText = $"Album Title {i+6}";
                album.AppendChild(title);

                // Жанры
                var genres = xml.CreateElement("genres");
                foreach (var genre in new[] { "Rock", "Jazz" })
                {
                    var genreElement = xml.CreateElement("genre");
                    genreElement.InnerText = genre;
                    genres.AppendChild(genreElement);
                }
                album.AppendChild(genres);

                // Исполнители
                var artists = xml.CreateElement("artists");
                foreach (var artist in new[] { $"Artist {i}", $"Artist {i + 1}" })
                {
                    var artistElement = xml.CreateElement("artist");
                    artistElement.InnerText = artist;
                    artists.AppendChild(artistElement);
                }
                album.AppendChild(artists);

                // Дата выпуска
                var releaseDate = xml.CreateElement("releaseDate");
                releaseDate.InnerText = DateTime.Now.AddYears(-i).ToString("yyyy-MM-dd");
                album.AppendChild(releaseDate);

                // Возрастные ограничения
                var ageRestriction = xml.CreateElement("ageRestriction");
                ageRestriction.InnerText = "18+";
                album.AppendChild(ageRestriction);

                // Треки
                var tracks = xml.CreateElement("tracks");
                for (int j = 1; j <= 3; j++)
                {
                    var track = xml.CreateElement("track");

                    var trackTitle = xml.CreateElement("title");
                    trackTitle.InnerText = $"Track {j}";
                    track.AppendChild(trackTitle);

                    var duration = xml.CreateElement("duration");
                    duration.InnerText = TimeSpan.FromMinutes(3 + j).ToString(@"mm\:ss");
                    track.AppendChild(duration);

                    tracks.AppendChild(track);
                    Console.WriteLine($"{track} длится {duration.InnerText}");
                }
                album.AppendChild(tracks);

                albums.AppendChild(album);
            }

            xml.AppendChild(albums);
            xml.Save("Resources/music_albums.xml");
            Console.WriteLine("XML файл создан.");

        }

        public static void PerformXPathQueries()
        {
            var doc = new XPathDocument("Resources/music_albums.xml");
            var nav = doc.CreateNavigator();

            // a) Альбомы указанного жанра
            Console.WriteLine("Альбомы жанра 'Rock':");
            var albumsByGenre = nav.Select("/albums/album[genres/genre='Rock']");
            while (albumsByGenre.MoveNext())
            {
                Console.WriteLine($"- {albumsByGenre.Current.SelectSingleNode("title").Value}");
            }

            // b) Жанры исполнителя
            Console.WriteLine("\nЖанры, в которых работал Artist 1:");
            var genresByArtist = nav.Select("/albums/album[artists/artist='Artist 1']/genres/genre");
            while (genresByArtist.MoveNext())
            {
                Console.WriteLine($"- {genresByArtist.Current.Value}");
            }

            // c) Альбомы с треками > 5 минут
            Console.WriteLine("\nАльбомы с треками > 5 минут:");
            var longTrackAlbums = nav.Select("/albums/album[tracks/track[number(translate(duration, ':', '.')) > 5.00]]");
            while (longTrackAlbums.MoveNext())
            {
                Console.WriteLine($"- {longTrackAlbums.Current.SelectSingleNode("title").Value}");
            }

            // d) Список воспроизведения
            Console.WriteLine("\nСлучайный плейлист:");
            var tracks = nav.Select("/albums/album/tracks/track");
            var allTracks = new List<string>();
            while (tracks.MoveNext())
            {
                allTracks.Add(tracks.Current.SelectSingleNode("title").Value);
            }
            var playlist = allTracks.OrderBy(x => Guid.NewGuid()).Take(5);
            foreach (var track in playlist)
            {
                Console.WriteLine($"- {track}");
            }
        }

        public static void ValidateWithDtd()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.ValidationType = ValidationType.DTD;
            settings.ValidationEventHandler += (sender, e) =>
            {
                Console.WriteLine($"Validation Error: {e.Message}");
            };

            using (var reader = XmlReader.Create("Resources/music_albums.xml", settings))
            {
                while (reader.Read()) { }
            }
            Console.WriteLine("DTD валидация завершена.");
        }

        public static void ValidateWithXsd()
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(null, "Resources/music_albums.xsd");
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationEventHandler += (sender, e) =>
                {
                    Console.WriteLine($"Validation Error: {e.Message}");
                };

                using (var reader = XmlReader.Create("Resources/music_albums.xml", settings))
                {
                    while (reader.Read()) { }
                }

                Console.WriteLine("XSD валидация завершена успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

        }

        public static void TransformToHtml()
        {
            string xsltPath = "Resources/transform.xslt";
            string xmlPath = "Resources/music_albums.xml";
            string outputPath = "output.html";

            if (!File.Exists(xsltPath))
            {
                Console.WriteLine($"Файл '{xsltPath}' не найден.");
                return;
            }

            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($"Файл '{xmlPath}' не найден.");
                return;
            }

            try
            {
                var xslt = new XslCompiledTransform();
                xslt.Load(xsltPath); // Загрузка XSLT-файла
                xslt.Transform(xmlPath, outputPath); // Трансформация XML в HTML
                Console.WriteLine($"HTML файл успешно создан: {outputPath}");
            }
            catch (XmlException ex)
            {
                Console.WriteLine($"Ошибка XML: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка: {ex.Message}");
            }
        }

        public static void PerformJsonQueries()
        {
            var json = File.ReadAllText("Resources/music_albums.json");
            var albums = JToken.Parse(json)["albums"];

            // Пример запроса: найти все альбомы с треками > 5 минут
            var longTrackAlbums = albums.Where(album => album["tracks"]
                .Any(track => TimeSpan.Parse((string)track["duration"]) > TimeSpan.FromMinutes(5)));
            foreach (var album in longTrackAlbums)
            {
                Console.WriteLine(album["title"]);
            }
        }

    }
}
