using Plugin.SimpleAudioPlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ET48_JuegoSimon
{
	public partial class MainPage : ContentPage
	{
        const int sequenceTime = 750;    // in msec 
        protected const int flashDuration = 250;
        const double offLuminosity = 0.4; // somewhat dimmer 
        const double onLuminosity = 0.75; // much brighter 
        BoxView[] boxViews;
        Color[] colors = { Color.Red, Color.Blue, Color.Yellow, Color.Green };
        List<int> sequence = new List<int>();
        int sequenceIndex;
        bool awaitingTaps;
        bool gameEnded;
        Random random = new Random();

        ISimpleAudioPlayer[] audios;
        //ISimpleAudioPlayer audio;
        Stream[] audioStreams;

        //ISimpleAudioPlayer audio = CrossSimpleAudioPlayer.Current;
        //Stream audioStream;

        public MainPage()
        {
            //audio = CrossSimpleAudioPlayer.Current;

            audios = new ISimpleAudioPlayer[4];
            audioStreams = new Stream[4];


            InitializeComponent();

            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += OnBoxViewTapped;
            boxview1.GestureRecognizers.Add(tap);

            boxViews = new BoxView[] { boxview0, boxview1, boxview2, boxview3 };
            InitializeBoxViewColors();

        }

        void InitializeBoxViewColors()
        {
            for (int index = 0; index < 4; index++)
            {
                boxViews[index].Color = colors[index].WithLuminosity(offLuminosity);

                audios[index] = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
                audios[index].Loop = false;
                audioStreams[index] = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("ET48_JuegoSimon.Assets." + "simonSound" + index + ".mp3");
                audios[index].Load(audioStreams[index]);


                //audioStream = assembly.GetManifestResourceStream("ET48_JuegoSimon.Assets." + "simonSound1" + ".mp3");
                //audio.Load(audioStream);
            }

        }

        protected void OnStartGameButtonClicked(object sender, EventArgs args)
        {
            gameEnded = false;
            startGameButton.IsVisible = false;
            InitializeBoxViewColors();
            sequence.Clear();
            StartSequence();
        }

        void StartSequence()
        {
            sequence.Add(random.Next(4));
            sequenceIndex = 0;
            Device.StartTimer(TimeSpan.FromMilliseconds(sequenceTime), OnTimerTick);
        }

        bool OnTimerTick()
        {
            if (gameEnded) return false;
            FlashBoxView(sequence[sequenceIndex]);
            sequenceIndex++;
            awaitingTaps = sequenceIndex == sequence.Count;
            sequenceIndex = awaitingTaps ? 0 : sequenceIndex;
            return !awaitingTaps;
        }

        void FlashBoxView(int index)
        {
            boxViews[index].Color = colors[index].WithLuminosity(onLuminosity);
            Device.StartTimer(TimeSpan.FromMilliseconds(flashDuration),
                () => {
                    if (gameEnded) return false;
                    boxViews[index].Color = colors[index].WithLuminosity(offLuminosity);

                    audios[index].Play(); // play audio

                    return false;
                });
        }

        void OnBoxViewTapped(object sender, EventArgs e)
        {
            if (gameEnded) return;
            if (!awaitingTaps)
            {
                EndGame();
                return;
            }
            BoxView tappedBoxView = (BoxView)sender;
            int index = Array.IndexOf(boxViews, tappedBoxView);
            if (index != sequence[sequenceIndex])
            {
                EndGame();
                return;
            }
            FlashBoxView(index);
            sequenceIndex++;
            awaitingTaps = sequenceIndex < sequence.Count;
            if (!awaitingTaps)
                StartSequence();
        }

        void EndGame()
        {
            gameEnded = true;
            for (int index = 0; index < 4; index++)
                boxViews[index].Color = Color.Gray;
            startGameButton.Text = "Try again?";
            startGameButton.IsVisible = true;
        }
    }
}
