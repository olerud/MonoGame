// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

extern alias MicrosoftXnaFramework;
using MsXna_MediaPlayer = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaPlayer;

using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using System.Threading;

namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
        internal static MediaElement _mediaElement;
        private static Uri source;
        private static TimeSpan elapsedTime;

        // track state of player before game is deactivated
        private static MediaState deactivatedState;
        private static bool wasDeactivated;

        private static void PlatformInitialize()
        {
            PhoneApplicationService.Current.Activated += (sender, e) =>
                {
                    if (_mediaElement != null)
                    {
                        if (_mediaElement.Source == null && source != null)
                        {
                            _mediaElement.AutoPlay = false;
                            Threading.BlockOnUIThread(() => _mediaElement.Source = source);
                        }

                        // Ensure only one subscription
                        _mediaElement.MediaOpened -= MediaElement_MediaOpened;
                        _mediaElement.MediaOpened += MediaElement_MediaOpened;
                    }
                };

            PhoneApplicationService.Current.Deactivated += (sender, e) => 
                {
                    if (_mediaElement != null)
                    {
                        source = _mediaElement.Source;
                        elapsedTime = _mediaElement.Position;

                        wasDeactivated = true;
                        deactivatedState = _state;
                    }
                };
        }

        private static void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (elapsedTime != TimeSpan.Zero)
                Threading.BlockOnUIThread(() =>
                {
                    _mediaElement.Position = elapsedTime;
                    elapsedTime = TimeSpan.Zero;
                });

            if (wasDeactivated)
            {
                if (deactivatedState == MediaState.Playing)
                    _mediaElement.Play();
 
                //reset the deactivated flag
                wasDeactivated = false;
 
                //set auto-play back to default
                _mediaElement.AutoPlay = true;
            }
        }

        #region Properties

        private static void PlatformSetIsMuted()
        {
            Threading.BlockOnUIThread(() =>
            {
                _mediaElement.IsMuted = _isMuted;
            });
        }

        private static TimeSpan PlatformGetPlayPosition()
        {
            TimeSpan pos = TimeSpan.Zero;
            if(_mediaElement.Dispatcher.CheckAccess()) {
                pos = _mediaElement.Position;
            }
            else {
                Threading.BlockOnUIThread(() =>
                {
                    pos = _mediaElement.Position;
                });
            }
            return (pos);
        }

        private static bool PlatformGetGameHasControl()
        {
            return State == MediaState.Playing || MsXna_MediaPlayer.GameHasControl;
        }

        private static void PlatformSetVolume()
        {
                Threading.BlockOnUIThread(() =>
                {
                    _mediaElement.Volume = _volume;
                });
        }
		
		#endregion

        private static void PlatformPause()
        {
            Threading.BlockOnUIThread(() =>
            {
                _mediaElement.Pause();
            });
        }

        private static void PlatformPlaySong(Song song)
        {
            Threading.BlockOnUIThread(() =>
            {
                _mediaElement.Source = new Uri(song.FilePath, UriKind.Relative);
                _mediaElement.Play();

                // Ensure only one subscribe
                _mediaElement.MediaEnded -= OnSongFinishedPlaying;
                _mediaElement.MediaEnded += OnSongFinishedPlaying;
            });
        }

        private static void PlatformResume()
        {
            Threading.BlockOnUIThread(() =>
            {
                _mediaElement.Play();
            });
        }

        private static void PlatformStop()
        {
            Threading.BlockOnUIThread(() =>
            {
                _mediaElement.Stop();
            });
        }
    }
}

