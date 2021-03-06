﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DtxCS;
using DtxCS.DataTypes;
using GameArchives.STFS;
using LibForge.Lipsync;
using LibForge.Midi;
using LibForge.Milo;
using LibForge.RBSong;
using LibForge.SongData;
using LibOrbisPkg.PFS;

namespace LibForge.Util
{
  public static class PkgCreator
  {
    static string gp4 = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<psproject fmt=""gp4"" version=""1000"">
  <volume>
    <volume_type>pkg_ps4_ac_data</volume_type>
    <volume_id></volume_id>
    <volume_ts>2018-01-01 00:00:00</volume_ts>
    <package content_id=""UP8802-CUSA02084_00-RBCUSTOMXXXX5000"" passcode=""00000000000000000000000000000000""/>
  </volume>
  <files img_no=""0"">
    <file targ_path=""sce_sys/param.sfo"" orig_path=""param.sfo""/>
FILES  </files>
  <rootdir>
    <dir targ_name=""sce_sys""/>
    <dir targ_name=""songs"">
SHORTNAMES
    </dir>
  </rootdir>
</psproject>";

    public static byte[] MakeParamSfo(string pkgId, string description, bool eu)
    {
      if (pkgId.Length != 36) throw new Exception("Content ID is not formatted correctly. It should be 36 characters");
      var param = new LibOrbisPkg.SFO.ParamSfo();
      param.Values.Add(new LibOrbisPkg.SFO.IntegerValue("ATTRIBUTE", 0));
      param.Values.Add(new LibOrbisPkg.SFO.Utf8Value("CATEGORY", "ac", 4));
      param.Values.Add(new LibOrbisPkg.SFO.Utf8Value("CONTENT_ID", pkgId, 48));
      param.Values.Add(new LibOrbisPkg.SFO.Utf8Value("FORMAT", "obs", 4));
      param.Values.Add(new LibOrbisPkg.SFO.Utf8Value("TITLE", description, 128));
      param.Values.Add(new LibOrbisPkg.SFO.Utf8Value("TITLE_ID", eu ? "CUSA02901" : "CUSA02084", 12));
      param.Values.Add(new LibOrbisPkg.SFO.Utf8Value("VERSION", "01.00", 8));
      var descBytes = Encoding.UTF8.GetBytes(description);
      return param.Serialize();
    }

    public static byte[] MakeGp4(string pkgId, IList<string> shortnames, List<string> files)
    {
      var fileSb = new StringBuilder();
      foreach(var f in files)
      {
        fileSb.AppendLine($"    <file targ_path=\"{f}\" orig_path=\"{f.Replace('/', '\\')}\"/>");
      }
      var shortname_dirs = new StringBuilder();
      foreach(var shortname in shortnames)
      {
        shortname_dirs.AppendLine($"      <dir targ_name=\"{shortname}\"/>");
      }
      var project = gp4.Replace("UP8802-CUSA02084_00-RBCUSTOMXXXX5000", pkgId)
                       .Replace("SHORTNAMES", shortname_dirs.ToString())
                       .Replace("FILES", fileSb.ToString())
                       .Replace("2018-01-01 00:00:00", DateTime.UtcNow.ToString("s").Replace('T', ' '));
      return Encoding.UTF8.GetBytes(project);
    }

    public static DataArray MakeMoggDta(DataArray array)
    {
      var moggDta = new DataArray();
      var trackArray = new DataArray();
      trackArray.AddNode(DataSymbol.Symbol("tracks"));
      var trackSubArray = trackArray.AddNode(new DataArray());
      foreach (var child in array.Array("song").Array("tracks").Array(1).Children)
      {
        if(child is DataArray a && a.Children[1] is DataArray b && b.Count > 0)
          trackSubArray.AddNode(child);
      }
      var totalTracks = array.Array("song").Array("pans").Array(1).Children.Count;
      // Get the last track number. This is based on the assumption that the tracks are in order
      // We have to filter out empty track arrays because GHtoRB(?) does stuff like (keys ()) instead
      // of leaving out the keys array entirely
      var lastTrack = ((trackSubArray.Children
        .Where(x => x is DataArray dx ? dx.Array(1).Children.Count > 0 : false)
        .Last() as DataArray)
        .Array(1).Children.Last() as DataAtom).Int;
      var crowdChannel = array.Array("song").Array("crowd_channels")?.Int(1);
      if (crowdChannel != null)
      {
        if (crowdChannel == lastTrack + 2)
          trackSubArray.AddNode(DTX.FromDtaString($"fake ({lastTrack + 1})"));
        else if (crowdChannel == lastTrack + 3)
          trackSubArray.AddNode(DTX.FromDtaString($"fake ({lastTrack + 1} {lastTrack + 2})"));
        trackSubArray.AddNode(DTX.FromDtaString($"crowd ({crowdChannel} {crowdChannel + 1})"));
      }
      else
      {
        if (totalTracks == lastTrack + 2)
          trackSubArray.AddNode(DTX.FromDtaString($"fake ({lastTrack + 1})"));
        else if (totalTracks == lastTrack + 3)
          trackSubArray.AddNode(DTX.FromDtaString($"fake ({lastTrack + 1} {lastTrack + 2})"));
      }
      moggDta.AddNode(trackArray);
      moggDta.AddNode(array.Array("song").Array("pans"));
      moggDta.AddNode(array.Array("song").Array("vols"));
      return moggDta;
    }
    
    // TODO: RBSONG
    public static RBSong.RBSong MakeRBSong(DataArray array)
    {
      var drumBank = array.Array("drum_bank")?.Any(1)
        .Replace("sfx", "fusion/patches")
        .Replace("_bank.milo", ".fusion") 
        ?? "fusion/patches/kit01.fusion";
      var editorComponent = new Component
      {
        ClassName = "Editor",
        Name = "Editor",
        Unknown1 = 3,
        Unknown2 = 2,
        Props = new[]
        {
          new Property("capabilities", new FlagValue(50))
        }
      };
      var vec3 = StructType.FromData(DTX.FromDtaString("define vec3 (props (x float) (y float) (z float))"));
      var xfm_type = StructType.FromData(DTX.FromDtaString(
        @"define xfm
            (props
              (basis_x vec3)
              (basis_y vec3)
              (basis_z vec3)
              (translate vec3))"));
      var entityHeaderComponent = new Component
      {
        ClassName = "EntityHeader",
        Name = "EntityHeader",
        Unknown1 = 3,
        Unknown2 = 1,
        Props = new[]
        {
          new Property("copy_on_instance", new BoolValue(true)),
          new Property("drives_parent", new BoolValue(false)),
          new Property("static", new BoolValue(false)),
          new Property("instance_polling_mode", new IntValue(0)),
          new Property("num_instances", new IntValue(0)),
          new Property("num_meshes", new IntValue(0)),
          new Property("num_particles", new IntValue(0)),
          new Property("num_propanims", new IntValue(0)),
          new Property("num_lights", new IntValue(0)),
          new Property("num_verts", new IntValue(0)),
          new Property("num_faces", new IntValue(0)),
          new Property("changelist", new IntValue(0)),
          new Property("icon_cam_initialized", new BoolValue(false)),
          new Property("icon_cam_near", new FloatValue(0.1f)),
          new Property("icon_cam_far", new FloatValue(1000f)),
          new Property("icon_cam_xfm", StructValue.FromData(xfm_type, DTX.FromDtaString(
            @"(basis_x ((x 1.0) (y 0.0) (z 0.0)))
              (basis_y ((x 0.0) (y 1.0) (z 0.0)))
              (basis_z ((x 0.0) (y 0.0) (z 1.0)))
              (translate ((x 0.0) (y 0.0) (z 0.0)))"))),
          new Property("icon_data", 
            new ArrayValue(new ArrayType{ElementType = PrimitiveType.Byte, InternalType = RBSong.DataType.Uint8 | RBSong.DataType.Array }, new Value[] { }))
        }
      };
      return new RBSong.RBSong
      {
        Version = 0xE,
        Object1 = new ObjectContainer
        {
          Unknown1 = 20,
          Unknown2 = 1,
          Unknown3 = 20,
          Unknown4 = 0,
          Unknown5 = 1,
          Entities = new[] {
            new Entity
            {
              Index0 = 0,
              Index1 = 0,
              Name = "root",
              Coms = new Component[] {
                editorComponent,
                entityHeaderComponent,
                new Component
                {
                  ClassName = "RBSongMetadata",
                  Name = "RBSongMetadata",
                  Unknown1 = 3,
                  Unknown2 = 4,
                  Props = new[]
                  {
                    new Property("tempo", new SymbolValue("medium")),
                    new Property("vocal_tonic_note", new LongValue(array.Array("vocal_tonic_note")?.Int(1) ?? 0)),
                    new Property("vocal_track_scroll_duration_ms", new LongValue(array.Array("song_scroll_speed")?.Int(1) ?? 2300)),
                    new Property("global_tuning_offset", new FloatValue(array.Array("tuning_offset_cents")?.Number(1) ?? 0)),
                    new Property("band_fail_sound_event", new SymbolValue("", true)),
                    new Property("vocal_percussion_patch", new ResourcePathValue("fusion/patches/vox_perc_tambourine.fusion", true)),
                    new Property("drum_kit_patch", new ResourcePathValue(drumBank)),
                    new Property("improv_solo_patch", new SymbolValue("gtrsolo_amer_03")),
                    new Property("dynamic_drum_fill_override", new IntValue(10)),
                    new Property("improv_solo_volume_db", new FloatValue(-9))
                  }
                },
                new Component
                {
                  ClassName = "RBVenueAuthoring",
                  Name = "RBVenueAuthoring",
                  Unknown1 = 3,
                  Unknown2 = 0,
                  Props = new[]
                  {
                    new Property("part2_instrument", new IntValue(2)),
                    new Property("part3_instrument", new IntValue(0)),
                    new Property("part4_instrument", new IntValue(1))
                  }
                }
              }
            }
          }
        },
        KV = new KeyValue
        {
          Str1 = "PropAnimResource",
          Str2 = "venue_authoring_data"
        },
        Object2 = new ObjectContainer
        {
          Unknown1 = 20,
          Unknown2 = 1,
          Unknown3 = 20,
          Unknown4 = 0,
          Unknown5 = 1,
          Entities = new[] {
            new Entity
            {
              Index0 = 0,
              Index1 = 0,
              Name = "root",
              Coms = new[]
              {
                editorComponent,
                entityHeaderComponent,
                new Component
                {
                  ClassName = "PropAnim",
                  Name = "PropAnim",
                  Unknown1 = 3,
                  Unknown2 = 0,
                  Props = StructValue.FromData(
                            StructType.FromData(DTX.FromDtaString(
                              @"(props 
                                  (frame_range_start float)
                                  (frame_range_end float)
                                  (time_units int)
                                  (is_looping bool))")),
                            DTX.FromDtaString(
                              @"(frame_range_start 3.402823E+38)
                                (frame_range_end -3.402823E+38)
                                (time_units 0)
                                (is_looping 0)")).Props

                }
              }
            }
          }
        }
      };
    }

    public static DLCSong ConvertDLCSong(DataArray songDta, GameArchives.IDirectory songRoot, Action<string> warner)
    {
      var path = songDta.Array("song").Array("name").String(1);
      var hopoThreshold = songDta.Array("song").Array("hopo_threshold")?.Int(1) ?? 170;
      var shortname = path.Split('/').Last();
      var midPath = shortname + ".mid";
      var artPath = $"gen/{shortname}_keep.png_xbox";
      var miloPath = $"gen/{shortname}.milo_xbox";
      var songId = songDta.Array("song_id").Node(1);
      var name = songDta.Array("name").String(1);
      var artist = songDta.Array("artist").String(1);
      var mid = MidiCS.MidiFileReader.FromBytes(songRoot.GetFileAtPath(midPath).GetBytes());

      // TODO: Catch possible conversion exceptions? i.e. Unsupported milo version
      var milo = MiloFile.ReadFromStream(songRoot.GetFileAtPath(miloPath).GetStream());
      var songData = SongDataConverter.ToSongData(songDta);

      Texture.Texture artwork = null;
      if (songData.AlbumArt)
      {
        try
        {
          artwork = Texture.TextureConverter.MiloPngToTexture(songRoot.GetFileAtPath(artPath).GetStream());
        }
        catch(Exception e)
        {
          warner?.Invoke("Failed to convert texture: "+e.Message);
          songData.AlbumArt = false;
        }
      }
      return new DLCSong
      {
        SongData = songData,
        Lipsync = LipsyncConverter.FromMilo(milo),
        Mogg = songRoot.GetFile(shortname + ".mogg"),
        MoggDta = MakeMoggDta(songDta),
        MoggSong = DTX.FromDtaString($"(mogg_path \"{songData.Shortname}.mogg\")\r\n(midi_path \"{songData.Shortname}.rbmid\")\r\n"),
        RBMidi = RBMidConverter.ToRBMid(mid, hopoThreshold, warner),
        Artwork = artwork,
        RBSong = MakeRBSong(songDta)
      };
    }

    /// <summary>
    /// Converts an RB3 DLC songs folder into RB4 DLC songs
    /// </summary>
    /// <param name="dlcRoot"></param>
    /// <returns></returns>
    public static List<DLCSong> ConvertDLCPackage(GameArchives.IDirectory dlcRoot, Action<string> warner = null)
    {
      var dlcSongs = new List<DLCSong>();
      var dta = DTX.FromPlainTextBytes(dlcRoot.GetFile("songs.dta").GetBytes());
      DataArray arr;
      for(int i = 0; i < dta.Count; i++)
      {
        arr = dta.Array(i);
        dlcSongs.Add(ConvertDLCSong(arr, dlcRoot.GetDirectory(arr.Array("song").Array("name").String(1).Split('/').Last()), warner));
      }
      return dlcSongs;
    }

    /// <summary>
    /// Writes the DLCSong to disk within the given directory.
    /// For example given a song called "custom" and a directory called J:\customs,
    /// you'll end up with J:\customs\custom\custom.mogg, J:\customs\custom\custom.rbsong, etc
    /// </summary>
    /// <param name="song">The song to write</param>
    /// <param name="dir">The parent directory of the song directory</param>
    public static void WriteDLCSong(DLCSong song, string dir)
    {
      var shortname = song.SongData.Shortname;
      var songPath = Path.Combine(dir, "songs", shortname);
      Directory.CreateDirectory(songPath);
      using (var lipsyncFile = File.OpenWrite(Path.Combine(songPath, $"{shortname}.lipsync_ps4")))
      {
        new LipsyncWriter(lipsyncFile).WriteStream(song.Lipsync);
      }
      using (var mogg = File.OpenWrite(Path.Combine(songPath, $"{shortname}.mogg")))
      using (var conMogg = song.Mogg.GetStream())
      {
        conMogg.CopyTo(mogg);
      }
      File.WriteAllText(Path.Combine(songPath, $"{shortname}.mogg.dta"), song.MoggDta.ToFileString());
      File.WriteAllText(Path.Combine(songPath, shortname + ".moggsong"), song.MoggSong.ToFileString());
      using (var rbmid = File.OpenWrite(Path.Combine(songPath, $"{shortname}.rbmid_ps4")))
        RBMidWriter.WriteStream(song.RBMidi, rbmid);
      using (var rbsongFile = File.OpenWrite(Path.Combine(songPath, $"{shortname}.rbsong")))
        new RBSongWriter(rbsongFile).WriteStream(song.RBSong);
      using (var songdtaFile = File.OpenWrite(Path.Combine(songPath, $"{shortname}.songdta_ps4")))
        SongDataWriter.WriteStream(song.SongData, songdtaFile);
      if (song.SongData.AlbumArt)
      {
        using (var artFile = File.OpenWrite(Path.Combine(songPath, $"{shortname}.png_ps4")))
          Texture.TextureWriter.WriteStream(song.Artwork, artFile);
      }
    }

    private static FSFile WriterToFile(string name, Action<Stream> writer)
    {
      using (var ms = new MemoryStream())
      {
        writer(ms);
        var bytes = ms.ToArray();
        return new FSFile(s => s.Write(bytes, 0, bytes.Length), name, bytes.Length);
      }
    }

    public static void DLCSongToFsFiles(DLCSong song, FSDir songsDir)
    {
      var shortname = song.SongData.Shortname;
      var songDir = new FSDir() { name = shortname, Parent = songsDir };
      songsDir.Dirs.Add(songDir);
      songDir.Files.Add(WriterToFile(
        $"{shortname}.lipsync_ps4",
        s => new LipsyncWriter(s).WriteStream(song.Lipsync)));
      songDir.Files.Add(new FSFile(
        s => { using (var mogg = song.Mogg.GetStream()) mogg.CopyTo(s); },
        $"{shortname}.mogg",
        song.Mogg.Size));
      var moggFileString = Encoding.UTF8.GetBytes(song.MoggDta.ToFileString());
      songDir.Files.Add(new FSFile(
        s => s.Write(moggFileString, 0, moggFileString.Length),
        $"{shortname}.mogg.dta",
        moggFileString.Length));
      var moggSongFileString = Encoding.UTF8.GetBytes(song.MoggSong.ToFileString());
      songDir.Files.Add(new FSFile(
        s => s.Write(moggSongFileString, 0, moggSongFileString.Length),
        $"{shortname}.moggsong",
        moggSongFileString.Length));
      songDir.Files.Add(WriterToFile(
        $"{shortname}.rbmid_ps4",
        s => RBMidWriter.WriteStream(song.RBMidi, s)));
      songDir.Files.Add(WriterToFile(
        $"{shortname}.rbsong",
        s => new RBSongWriter(s).WriteStream(song.RBSong)));
      songDir.Files.Add(WriterToFile(
        $"{shortname}.songdta_ps4",
        s => SongDataWriter.WriteStream(song.SongData, s)));
      if (song.SongData.AlbumArt)
      {
        songDir.Files.Add(WriterToFile(
          $"{shortname}.png_ps4",
          s => Texture.TextureWriter.WriteStream(song.Artwork, s)));
      }
      foreach (var f in songDir.Files) f.Parent = songDir;
    }

    /// <summary>
    /// Writes all the songs and creates a Publishing Tools .gp4 project in the given directory
    /// </summary>
    /// <param name="songs">Songs to include</param>
    /// <param name="pkgId">36-character package ID</param>
    /// <param name="pkgDesc">User-visible name of the package</param>
    /// <param name="buildDir">Directory in which to put the project and files</param>
    /// <param name="eu">Set to true if SCEE</param>
    public static void DLCSongsToGP4(IList<DLCSong> songs, string pkgId, string pkgDesc, string buildDir, bool eu)
    {
      var shortnames = new List<string>(songs.Count);
      var files = new List<string>(songs.Count * 8);
      foreach (var song in songs)
      {
        var shortname = song.SongData.Shortname;
        files.AddRange(new[] {
          $"songs/{shortname}/{shortname}.lipsync_ps4",
          $"songs/{shortname}/{shortname}.mogg",
          $"songs/{shortname}/{shortname}.mogg.dta",
          $"songs/{shortname}/{shortname}.moggsong",
          $"songs/{shortname}/{shortname}.rbmid_ps4",
          $"songs/{shortname}/{shortname}.rbsong",
          $"songs/{shortname}/{shortname}.songdta_ps4",
        });
        if (song.Artwork != null)
            files.Add($"songs/{shortname}/{shortname}.png_ps4");
        shortnames.Add(shortname);
      }
      
      var paramSfo = MakeParamSfo(pkgId, pkgDesc, eu);

      // Write all the files
      foreach(var song in songs)
      {
        WriteDLCSong(song, buildDir);
      }
      File.WriteAllBytes(Path.Combine(buildDir, "param.sfo"), paramSfo);
      File.WriteAllBytes(Path.Combine(buildDir, "project.gp4"), MakeGp4(pkgId, shortnames, files));
    }

    /// <summary>
    /// Generates a 16-char ID for a PKG
    /// </summary>
    /// <param name="song">Song to use for generation</param>
    /// <returns>16 char id</returns>
    public static string GenId(DLCSong song)
    {
      var shortname = new Regex("[^a-zA-Z0-9]").Replace(song.SongData.Shortname, "");
      var pkgName = shortname.ToUpper().Substring(0, Math.Min(shortname.Length, 12)).PadRight(12, 'X');
      string pkgNum = (song.SongData.SongId % 10000).ToString().PadLeft(4, '0');
      return pkgName + pkgNum;
    }

    /// <summary>
    /// Does the whole process of converting a CON to a GP4 project
    /// </summary>
    /// <param name="conPath">Path to CON file</param>
    /// <param name="buildDir">Output directory for project and files</param>
    /// <param name="eu">If true then an SCEE project is made (otherwise, SCEA)</param>
    public static void ConToGp4(string conPath, string buildDir, bool eu = false, string id = null, string desc = null)
    {
      // Phase 1: Reading from CON
      var con = STFSPackage.OpenFile(GameArchives.Util.LocalFile(conPath));
      if(con.Type != STFSType.CON)
      {
        Console.WriteLine("Error: given file was not a CON file");
        return;
      }
      var songs = ConvertDLCPackage(con.RootDirectory.GetDirectory("songs"));
      if(songs.Count > 1)
      {
        if ((id?.Length ?? 0) < 16)
        {
          throw new Exception("You must provide a 16 char ID if you are building a custom package with multiple songs");
        }
      }
      var identifier = id ?? GenId(songs[0]);
      var pkgId = eu ? $"EP8802-CUSA02901_00-{identifier}" : $"UP8802-CUSA02084_00-{identifier}";
      var pkgDesc = $"Custom: \"{songs[0].SongData.Name} - {songs[0].SongData.Artist}\"";
      DLCSongsToGP4(songs, pkgId, desc ?? pkgDesc, buildDir, eu);
    }

    public static void ConsToGp4(string conPath, string buildDir, bool eu, string id, string desc)
    {
      var songs = new List<DLCSong>();
      foreach (var conFilename in Directory.EnumerateFiles(conPath))
      {
        var file = GameArchives.Util.LocalFile(conFilename);
        var stfs = STFSPackage.IsSTFS(file);
        STFSPackage conFile;
        if (stfs != GameArchives.PackageTestResult.YES
          || null == (conFile = STFSPackage.OpenFile(file))
          || conFile.Type != STFSType.CON)
        {
          Console.WriteLine($"Skipping \"{conFilename}\": not a CON file");
          continue;
        }
        songs.AddRange(ConvertDLCPackage(conFile.RootDirectory.GetDirectory("songs")));
      }

      if (songs.Count > 1)
      {
        if ((id?.Length ?? 0) < 16)
        {
          throw new Exception("You must provide a 16 char ID if you are building a custom package with multiple songs");
        }
      }
      var pkgId = eu ? $"EP8802-CUSA02901_00-{id}" : $"UP8802-CUSA02084_00-{id}";
      DLCSongsToGP4(songs, pkgId, desc ?? "", buildDir, eu);
    }

    public static void BuildPkg(List<DLCSong> songs, string contentId, string desc, bool eu, string output, Action<string> logger)
    {
      var shortnames = new List<string>(songs.Count);
      var root = new FSDir();
      var sys = new FSDir() { name = "sce_sys", Parent = root };
      root.Dirs.Add(sys);
      var songsDir = new FSDir() { name = "songs", Parent = root };
      root.Dirs.Add(songsDir);
      foreach (var song in songs)
      {
        DLCSongToFsFiles(song, songsDir);
      }
      var paramSfo = MakeParamSfo(contentId, desc, eu);
      sys.Files.Add(new FSFile(s => s.Write(paramSfo, 0, paramSfo.Length), "param.sfo", paramSfo.Length) { Parent = sys });
      new LibOrbisPkg.PKG.PkgBuilder(new LibOrbisPkg.PKG.PkgProperties
      {
        ContentId = contentId,
        CreationDate = DateTime.UtcNow,
        TimeStamp = DateTime.UtcNow,
        UseCreationTime = true,
        EntitlementKey = "00000000000000000000000000000000",
        Passcode = "00000000000000000000000000000000",
        RootDir = root,
        VolumeType = LibOrbisPkg.GP4.VolumeType.pkg_ps4_ac_data,
      }).Write(output, logger);
      logger("Done!");
    }

    public static void BuildPkg(string proj, string outPath)
    {
      using (var projFile = File.OpenRead(proj))
      {
        var project = LibOrbisPkg.GP4.Gp4Project.ReadFrom(projFile);
        using (var outFile = File.Open(
          Path.Combine(
            outPath,
            $"{project.volume.Package.ContentId}.pkg"),
            FileMode.Create))
        {
          new LibOrbisPkg.PKG.PkgBuilder(
            LibOrbisPkg.PKG.PkgProperties.FromGp4(
            project, Path.GetDirectoryName(proj))).Write(outFile);
        }
      }
    }
  }

  public static class DataArrayExtension
  {
    /// <summary>
    /// Renders a DataArray that represents a DTA file.
    /// Basically, DataArray to string without parens.
    /// </summary>
    public static string ToFileString(this DataArray d)
    {
      var ret = d.ToString();
      return ret.Substring(1, ret.Length - 2);
    }
  }
}
