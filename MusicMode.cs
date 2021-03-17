using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageMagick;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using static SpotifyAPI.Web.Scopes;

namespace LightGrid
{
    public static class MusicMode
    {
        public static bool Enabled = false;
        private static int loopDelay = 3000; // don't set below 1000ms.

        private static readonly SettingsManager<UserSettings> settingsManager = new SettingsManager<UserSettings>();
        private static UserSettings userSettings;

        private const string CredentialsFilename = "spotify_credentials.json";
        private static string CredentialsFilePath;
        private static string? spotifyClientId;
        private static string? spotifyClientSecret;

        private static EmbedIOAuthServer authServer;


        public static async Task Start()
        {
            userSettings = settingsManager.LoadSettings();

            spotifyClientId = userSettings.spotifyClientId;
            spotifyClientSecret = userSettings.spotifyClientSecret;

            // build credentials filepath
            CredentialsFilePath = Path.Combine(settingsManager.directoryPath, CredentialsFilename);

            // check if clientID and clientSecret values are set.
            if (spotifyClientId != null && spotifyClientSecret != null)
            {
                if (File.Exists(CredentialsFilePath))
                {
                    var authenticator = await LogIn();
                    await Run(authenticator);
                }
                else
                {
                    await StartAuthentication();
                }
            }
            else
            {
                // TODO: make a new window where we can input these values
                MessageBox.Show(
                    "You must set spotifyClientId and spotifyClientSecret " +
                    "values in your user settings file from the values on the Spotify Developer dashboard.");

                return;
            }
        }

        public static async Task Stop()
        {
            MusicMode.Enabled = false;
        }

        private static async Task<AuthorizationCodeAuthenticator> LogIn()
        {
            // do all that cool Spotify authentication shit
            var json = await File.ReadAllTextAsync(CredentialsFilePath);
            var token = JsonConvert.DeserializeObject<AuthorizationCodeTokenResponse>(json);

            var authenticator = new AuthorizationCodeAuthenticator(spotifyClientId!, spotifyClientSecret!, token);
            authenticator.TokenRefreshed += (sender, token) => File.WriteAllText(CredentialsFilePath, JsonConvert.SerializeObject(token));

            return authenticator;
        }

        private static async Task Run(AuthorizationCodeAuthenticator authenticator)
        {
            MusicMode.Enabled = true;

            var config = SpotifyClientConfig.CreateDefault()
                .WithAuthenticator(authenticator);

            // build spotify client
            var spotify = new SpotifyClient(config);

            string oldTrackId = ""; // used to track if we're playing a different track than last loop
            while (MusicMode.Enabled)
            {
                // get info on playback status
                var currentTrack = await spotify.Player.GetCurrentPlayback(new PlayerCurrentPlaybackRequest());

                // check if we're playing something, or if we pulled null data somehow (API failure?)
                if (currentTrack == null || currentTrack.Item == null || currentTrack.IsPlaying == false)
                {
                    await Task.Delay(loopDelay);
                    continue;
                }

                // grab the track details
                var currentTrackDetails = (FullTrack)currentTrack.Item;

                // check if it's different from what we saw last loop - if so, wait and try the loop again
                if (currentTrackDetails.Id == oldTrackId)
                {
                    await Task.Delay(loopDelay);
                    continue;
                }

                // download the album art to memory
                var albumArtUrl = currentTrackDetails.Album.Images[0].Url;
                WebRequest request = WebRequest.Create(albumArtUrl);
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();

                // convert album art to color - color should be average color of image after some adjustments
                Color albumColor = new Color();
                using (MagickImage image = new MagickImage(responseStream, MagickFormat.Jpg))
                {
                    // debug - output image before doing anything at all
                    var debugOutputPreProcessing = new FileInfo("debug-output-album-pre-processing.jpg");
                    image.Write(debugOutputPreProcessing);

                    // set white & black to transparent, to make sure they don't mess with the vibrancy of the image
                    image.ColorFuzz = (Percentage)15; // shades within 15% of pure white/black will count as pure
                    image.Opaque(new MagickColor(MagickColors.White), new MagickColor(MagickColors.Transparent));
                    image.Opaque(new MagickColor(MagickColors.Black), new MagickColor(MagickColors.Transparent));

                    // flatten image down to a few colors
                    image.Quantize(new QuantizeSettings { Colors = 5 });

                    // debug - output image after quantizing
                    var debugOutputQuantized = new FileInfo("debug-output-album-quantized.jpg");
                    image.Write(debugOutputQuantized);

                    // massively boost saturation so the lights actually have color to them
                    image.Modulate((Percentage)100, (Percentage)1000, (Percentage)100);

                    // resize to 1px, as the built-in converter will average out all the remaining colors into one
                    image.Resize(1, 1);

                    // get color rgb values and set to albumColor
                    var colorKey = image.Histogram().FirstOrDefault().Key;
                    albumColor = Color.FromArgb(255, colorKey.R, colorKey.G, colorKey.B);

                    // debug - output after all post processing. should return a 100x100px image of one solid color.
                    image.Resize(100, 100);
                    var debugOutputDone = new FileInfo("debug-output-done.jpg");
                    image.Write(debugOutputDone);

                    // get rid of any junk we don't need anymore
                    image.Dispose();
                    responseStream.Dispose();
                }

                // set the light color to the album art's average color
                YeelightHelper.SetRGBColor(albumColor.R, albumColor.G, albumColor.B, 250);

                oldTrackId = currentTrackDetails.Id;

                await Task.Delay(loopDelay);
            }
        }

        private static async Task StartAuthentication()
        {
            authServer = new EmbedIOAuthServer(new Uri("http://localhost:5050/callback"), 5050);

            await authServer.Start();
            authServer.AuthorizationCodeReceived += OnAuthorizationCodeReceived;

            var request = new LoginRequest(authServer.BaseUri, spotifyClientId!, LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { UserReadEmail, UserReadPrivate, PlaylistReadPrivate, PlaylistReadCollaborative, UserLibraryRead, UserLibraryModify, UserReadCurrentlyPlaying, UserReadPlaybackPosition, UserReadPlaybackState }
            };

            Uri uri = request.ToUri();
            try
            {
                BrowserUtil.Open(uri);
            }
            catch (Exception)
            {
                // TODO: exception handle auth server, since we don't have a console -- maybe a clickable link in a window?
                // Console.WriteLine("[AUTH] Unable to open URL, manually open: {0}", uri);
            }
        }

        private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            await authServer.Stop();
            AuthorizationCodeTokenResponse token = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(spotifyClientId!, spotifyClientSecret!, response.Code, authServer.BaseUri)
            );

            await File.WriteAllTextAsync(CredentialsFilePath, JsonConvert.SerializeObject(token));

            // get rid of the web server
            authServer.Dispose();

            await LogIn();
        }
    }
}
