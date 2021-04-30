using WindowsMediaController;
using Windows.Media.Control;
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaControll
{
    /// <summary>
    /// ViewModel for <see cref="MainWindow"/>
    /// </summary>
    class MainViewModel : BaseViewModel
    {
        #region Private properties

        // Keys may be replaced with Media events when implementing getting of currently played
        #region Constants for Key Events
        private const int KEYEVENTF_EXTENTEDKEY = 1; // code to hold key
        private const int KEYEVENTF_KEYUP = 0; // code to release key
        private const int VK_MEDIA_NEXT_TRACK = 0xB0;// code to jump to next track
        private const int VK_MEDIA_PLAY_PAUSE = 0xB3;// code to play or pause a song
        private const int VK_MEDIA_PREV_TRACK = 0xB1;// code to jump to previous track
        #endregion
        // Song / Video / Something else, that is currently playing
        private string currentlyPlaying;

        // Boolean to detect if there is currently something playing or not (change Play Pause button?) 
        private string interpretOfCurrentlyPlaying;

        // Interpret (if exists) of something, that is currently playing
        private bool isCurrentlyPlaying;

        // If there is an Icon of the current played song, show it. Else show fall back
        private byte[] thumbnail;

        #endregion

        #region Public properties

        /// <summary>
        /// Represents title of something, that is currently playing
        /// Setter / Getter for <see cref="currentlyPlayedSong"/>
        /// </summary>
        public string CurrentlyPlaying { get => currentlyPlaying; set => currentlyPlaying = value; }

        /// <summary>
        /// Represents interpret of something, that is currently playing
        /// Setter / Getter for <see cref="interpretOfCurrentlyPlayedSong"/>
        /// </summary>
        public string InterpretOfCurrentlyPlaying { get => interpretOfCurrentlyPlaying; set => interpretOfCurrentlyPlaying = value; }

        /// <summary>
        /// Shows if there is currently something played or paused
        /// Setter / Getter for <see cref="isCurrentlyPlaying"/>
        /// </summary>
        public bool IsCurrentlyPlaying { get => isCurrentlyPlaying; set => isCurrentlyPlaying = value; }

        /// <summary>
        /// path to the Icon of currently playing if exists
        /// Setter / Getter for <see cref="thumbnail"/>
        /// </summary>
        public byte[] Thumbnail { get => thumbnail; set => thumbnail = value; }

        #endregion

        #region Public Commands

        /// <summary>
        /// The command to skip to next
        /// </summary>
        public ICommand SkipNextCommand { get; set; }

        /// <summary>
        /// The command to Play Pause
        /// </summary>
        public ICommand PlayPauseCommand { get; set; }

        /// <summary>
        /// The command to skip to previous
        /// </summary>
        public ICommand SkipPreviousCommand { get; set; }

        public ICommand ReloadCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainViewModel()
        {

            #region Media Manager Events
            MediaManager.OnSongChanged += MediaManager_OnSongChanged;
            MediaManager.OnPlaybackStateChanged += MediaManager_OnPlaybackStateChanged;
            MediaManager.OnNewSource += MediaManager_OnNewSource;
            MediaManager.OnRemovedSource += MediaManager_OnRemovedSource;
            #endregion

            MediaManager.Start();

            #region Initialize Commands
            // To make the ICommands working, we have to attach a Relay Command with the corresponding Method to it
            this.SkipNextCommand = new RelayCommand(SkipNext);
            this.PlayPauseCommand = new RelayCommand(PlayPause);
            this.SkipPreviousCommand = new RelayCommand(SkipPrevious);
            this.ReloadCommand = new RelayCommand(Reload);
            #endregion

            currentlyPlaying = "Nothing is being played at the moment";

        }

        #endregion

        #region Button actions

        /// <summary>
        /// Emits keyboard button press to skip to next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkipNext()
        {
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        /// <summary>
        /// Emits keyboard button press to Play/Pause
        /// </summary>
        private void PlayPause()
        {
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        /// <summary>
        /// Emits keyboard button press to skip to previous
        /// </summary>
        private void SkipPrevious()
        {
            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        /// <summary>
        /// Reloads current MediaManager session
        /// </summary>
        private void Reload()
        {
            MediaManager.Reload();
        }

        #endregion

        #region Media Manager Actions

        /// <summary>
        /// Set the new Title, the new Interpret if exists and the Thumbnail if exists
        /// </summary>
        /// <param name="sender">Class that called method</param>
        /// <param name="args">Title, Artist, Thumbnail and more</param>
        private void MediaManager_OnSongChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
        {
            WriteLineColor($"{sender.ControlSession.SourceAppUserModelId} is now playing {args.Title} {(String.IsNullOrEmpty(args.Artist) ? "" : $"by {args.Artist}")}", ConsoleColor.Cyan);

            CurrentlyPlaying = String.IsNullOrEmpty(args.Title) ? "Nothing is being played at the moment" : args.Title;
            if (String.IsNullOrEmpty(args.Artist))
                InterpretOfCurrentlyPlaying = null;
            else
                InterpretOfCurrentlyPlaying = $"by {args.Artist}";

            //Set thumbnail if exists, else set default
            if (args.Thumbnail != null)
            {
                _ = SetThumbnailFromStreamAsync(args.Thumbnail);

            }
            else
            {
                // Create source.
                BitmapImage bi = new BitmapImage();
                // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                bi.BeginInit();
                bi.UriSource = new Uri(@"pack://application:,,,/src/DefaultMusic.jpg", UriKind.RelativeOrAbsolute);
                bi.EndInit();

                Thumbnail = getJPGFromImageControl(bi); ;
            }
        }

        /// <summary>
        /// Update the Playback-State according to passed arguments
        /// </summary>
        /// <param name="sender">Class that called method</param>
        /// <param name="args">Infos of playback state like is playing or shuffle</param>
        private void MediaManager_OnPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
        {
            WriteLineColor($"{sender.ControlSession.SourceAppUserModelId} is now {args.PlaybackStatus}", ConsoleColor.Magenta);
            IsCurrentlyPlaying = args.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing ? true : false;
        }

        /// <summary>
        /// Updates Title, Thumbnail and other on new session
        /// </summary>
        /// <param name="session">The new Session</param>
        private void MediaManager_OnNewSource(MediaManager.MediaSession session)
        {
            WriteLineColor("-- New Source: " + session.ControlSession.SourceAppUserModelId, ConsoleColor.Green);
            updateMediaInformationsAsync(session);

        }

        /// <summary>
        /// Updates Title, Thumbnail and other on new session
        /// </summary>
        /// <param name="session">The Session, that is being deleted</param>
        private void MediaManager_OnRemovedSource(MediaManager.MediaSession session)
        {
            WriteLineColor("-- Removed Source: " + session.ControlSession.SourceAppUserModelId, ConsoleColor.Red);
            updateMediaInformationsAsync(session);
        }

        /// <summary>
        /// Update Thumbnail, Title, Interpret and Playback status asynchronously
        /// </summary>
        /// <param name="session"></param>
        private async void updateMediaInformationsAsync(MediaManager.MediaSession session)
        {
            MediaManager_OnSongChanged(session, await session.ControlSession.TryGetMediaPropertiesAsync());
            MediaManager_OnPlaybackStateChanged(session, session.ControlSession.GetPlaybackInfo());
        }

        #endregion

        #region Private helper

        /// <summary>
        /// Virtually press key <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-keybd_event">keybd_event function (winuser.h)</see>
        /// </summary>
        /// <param name="virtualKey">A virtual-key code. The code must be a value in the range 1 to 254. For a complete list, see <see href="https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes">Virtual Key Codes</see>.</param>
        /// <param name="scanCode">A hardware scan code for the key.</param>
        /// <param name="flags">Controls various aspects of function operation:
        ///  - Value: KEYEVENTF_EXTENDEDKEY (0x0001); Meaning: If specified, the scan code was preceded by a prefix byte having the value 0xE0 (224).
        ///  - Value: KEYEVENTF_KEYUP (0x0002); Meaning: If specified, the key is being released. If not specified, the key is being depressed.
        ///  </param>
        /// <param name="extraInfo"> An additional value associated with the key stroke.</param>
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        /// <summary>
        /// converts an <see cref="BitmapImage"/> into an <see cref="byte[]"/>
        /// can be called like <code>getJPGFromImageControl(foo.Source as BitmapImage)</code>
        /// </summary>
        /// <param name="imageC">the Image </param>
        /// <returns></returns>
        private byte[] getJPGFromImageControl(BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }

        /// <summary>
        /// Set the thumbnail asynchronously by its StreamReference
        /// </summary>
        /// <param name="ThumbnailReference">Stream reference to the Thumbnail</param>
        /// <returns>Available, but not recommended to await</returns>
        public async Task SetThumbnailFromStreamAsync(IRandomAccessStreamReference ThumbnailReference)
        {
            if (ThumbnailReference != null)
            {
                using (IRandomAccessStreamWithContentType thumbnailStream = await ThumbnailReference.OpenReadAsync())
                {
                    byte[] bytes = null;
                    bytes = new Byte[thumbnailStream.Size];
                    await thumbnailStream.AsStream().ReadAsync(bytes, 0, bytes.Length);
                    Thumbnail = bytes;
                }
            }
        }

        /// <summary>
        /// Write to <see cref="Console"/> in specific color. Colors via <code>ConsoleColor.[Color]</code>
        /// </summary>
        /// <param name="toprint">Text to write</param>
        /// <param name="color">Color in which text should be printed</param>
        public static void WriteLineColor(object toprint, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + toprint);
        }

        #endregion
    }
}
