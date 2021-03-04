namespace MusicHub
{
    using System;
    using System.Linq;
    using System.Text;
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context = 
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            var result = ExportSongsAboveDuration(context, 4);
            Console.WriteLine(result);
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Producers
                        .FirstOrDefault(x => x.Id == producerId)
                        .Albums.Select(album => new
                        {
                           AlbumName = album.Name,
                           AlbumRelease =  album.ReleaseDate,
                           ProducerName = album.Producer.Name,
                           Songs = album.Songs.Select(song => new
                            {
                              SongName = song.Name,
                              SongPrice = song.Price,
                              SongWriter = song.Writer.Name
                            })
                           .OrderByDescending(x => x.SongName)
                           .ThenBy(x => x.SongWriter)
                           .ToList(),
                           AlbumPrice = album.Price
                        })
                        .OrderByDescending(x => x.AlbumPrice);
            var sb = new StringBuilder();
            foreach (var album in albums)
            {
                sb.AppendLine($"-AlbumName: {album.AlbumName}");
                sb.AppendLine($"-ReleaseDate: {album.AlbumRelease.ToString("MM/dd/yyyy")}");
                sb.AppendLine($"-ProducerName: {album.ProducerName}");
                sb.AppendLine("-Songs:");
                 int i = 1;
                foreach(var song in album.Songs)
                {
                    sb.AppendLine($"---#{i}");
                    sb.AppendLine($"---SongName: {song.SongName}");
                    sb.AppendLine($"---Price: {song.SongPrice:F2}");
                    sb.AppendLine($"---Writer: {song.SongWriter}");
                    i++;
                }
                sb.AppendLine($"-AlbumPrice: {album.AlbumPrice:F2}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songsNeeded = context.Songs
                                .Where(x => x.Duration.TotalSeconds > duration)
                                .Select(x => new
                                {
                                    SongName = x.Name,
                                    WriterName = x.Writer.Name,
                                    PerformerName = x.SongPerformers
                                                    .Select(x => x.Performer.FirstName + " " + x.Performer.LastName)
                                                    .First(),
                                    AlbumProducer = x.Album.Producer.Name,
                                    Duration = x.Duration
                                })
                                .OrderBy(X => X.SongName )
                                .ThenBy(x => x.WriterName)
                                .ThenBy(x => x.PerformerName)
                                .ToList();
            var sb = new StringBuilder();
            int i = 1;
            foreach (var song in songsNeeded)
            {
                sb.AppendLine($"-Song #{i}")
                  .AppendLine($"---SongName: {song.SongName}")
                  .AppendLine($"---Writer: {song.WriterName}")
                  .AppendLine($"---Performer: {song.PerformerName}")
                  .AppendLine($"---AlbumProducer: {song.AlbumProducer}")
                  .AppendLine($"---Duration: {song.Duration:c}");
                i++;
            }
           return sb.ToString().TrimEnd();
        }
                      //-Song #1 
                      //---SongName: Away 
                      //---Writer: Norina Renihan 
                      //---Performer: Lula Zuan 
                      //---AlbumProducer: Georgi Milkov 
                      //---Duration: 00:05:35 
    }
}
