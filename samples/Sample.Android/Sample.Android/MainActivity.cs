using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using ZXing;
using ZXing.Mobile;
using System.Net;
using System;
using System.Collections.Specialized;
using System.Text;
using Android.Telephony;
using Newtonsoft.Json;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Media;

namespace Sample.Android
{
    [Activity(Label = "Waldquiz", MainLauncher = true, Theme = "@android:style/Theme.Holo.Light", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
    public class Activity1 : Activity
    {
        Button buttonScanDefaultView;
        Button buttonErstelleUser;
        Button buttonFragebeantworten;

        List<Frage> fragen;
        User user;
        int count;
        int score;
        string info;

        MobileBarcodeScanner scanner;

        WebClient getUserClient;
        Uri userURL;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Initialize the scanner first so we can track the current context
            MobileBarcodeScanner.Initialize(Application);

            //Create a new instance of our Scanner
            scanner = new MobileBarcodeScanner();
            fragen = new List<Frage>();
            count = score = 0;
            info = "";

            // wichtig
            // warum geht das nicht ??
            user = new User();

            // Getting IMEI of Smartphone
            var telephonyManager = (TelephonyManager)GetSystemService(TelephonyService);
            user.IMEI = telephonyManager.DeviceId.ToString();

            // Searching IMEI in DB 
            getUserClient = new WebClient();
            userURL = new Uri("http://www.wurzelpark.at/App/PHP/GetUser.php");
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("IMEI", user.IMEI);
            getUserClient.UploadValuesAsync(userURL, parameters);
            getUserClient.UploadValuesCompleted += GetUserClient_UploadValuesCompleted;
        }

        private void GetUserClient_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        { 
            RunOnUiThread(() =>
            {
                string json = System.Text.Encoding.UTF8.GetString(e.Result);
                user = JsonConvert.DeserializeObject<User>(json);
                if (json == "")
                    UserErstellung();

                else
                    BarcodeScannen(user.ID, user.Vorname + " " + user.Nachname, "");
            });
        }

        private void ButtonErstelleUser_Click(object sender, EventArgs e)
        {
            user.Vorname = this.FindViewById<EditText>(Resource.Id.editTextVorname).Text;
            user.Nachname = this.FindViewById<EditText>(Resource.Id.editTextNachname).Text;

            // wichtig try catch
            user.Geburtsjahr = Convert.ToInt32((this.FindViewById<EditText>(Resource.Id.editTextGeburtsjahr).Text));
            user.Benutzername = user.Vorname + user.Nachname + user.Geburtsjahr.ToString() + "";
            user.Gesamtscore = 0;

            WebClient client = new WebClient();
            Uri uri = new Uri("http://www.wurzelpark.at/App/PHP/CreateUser.php");
            NameValueCollection parameters = new NameValueCollection();

            parameters.Add("Vorname", user.Vorname);
            parameters.Add("Nachname", user.Nachname);
            parameters.Add("Geburtsjahr", user.Geburtsjahr.ToString());
            parameters.Add("Benutzername", user.Benutzername);
            parameters.Add("Gesamtscore", user.Gesamtscore.ToString());
            parameters.Add("IMEI", user.IMEI);

            client.UploadValuesAsync(uri, parameters);
            client.UploadValuesCompleted += Client_UploadValuesCompleted;            
        }

        private void FragenClient_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {
            // BEVOR der Benutzer eine Frage beantwortet hat
            RunOnUiThread(() =>
            {
                string json = System.Text.Encoding.UTF8.GetString(e.Result);

                if (json == "[]")
                    this.RunOnUiThread(() => Toast.MakeText(this, "Barcode wurde nicht gefunden", ToastLength.Short).Show());
                else
                {
                    fragen = JsonConvert.DeserializeObject<List<Frage>>(json);
                    count = fragen.Count-1;              
                    ScoreSuche(user.ID, fragen[count].ID);                    
                }
            });  
        }        

        private void Client_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {
            string echo = System.Text.Encoding.UTF8.GetString(e.Result);
            user.ID = Convert.ToInt32(echo);

            user.Vorname = this.FindViewById<EditText>(Resource.Id.editTextVorname).Text;
            user.Nachname = this.FindViewById<EditText>(Resource.Id.editTextNachname).Text;

            this.FindViewById<EditText>(Resource.Id.editTextGeburtsjahr).Text = "";
            this.FindViewById<EditText>(Resource.Id.editTextVorname).Text = "";
            this.FindViewById<EditText>(Resource.Id.editTextNachname).Text = "";

            this.FindViewById<Button>(Resource.Id.buttonErstelleUser).Enabled = false;

            BarcodeScannen(user.ID, user.Vorname + " " + user.Nachname, "");
        }

        private async void ButtonFragebeantworten_Click(object sender, EventArgs e)
        {
            RadioGroup radioGroup = FindViewById<RadioGroup>(Resource.Id.radioGroupAntworten);
            RadioButton radioButton = FindViewById<RadioButton>(radioGroup.CheckedRadioButtonId);
            TextView mytextView = this.FindViewById<TextView>(Resource.Id.textViewAntwort);

            int gegebeneAntwort = radioButton.Id - 2131034123;
            if (gegebeneAntwort == fragen[count].RichtigeAntwort)
            {
                mytextView.Text = "Du hast die Frage richtig beantwortet";
                mytextView.SetTextColor(Color.ParseColor("green"));
                await Task.Delay(1500);
                score = 1;
                //Datenbank Speicherung user.score +1 -> wenn, dann php lösung
            }

            else
            {
                radioButton.Enabled = false;                
                mytextView.Text = "Du hast die Frage falsch beantwortet";
                mytextView.SetTextColor(Color.ParseColor("red"));
                score = 0;
            }

            //Datenbank Speicherung user_frage
            WebClient userFrage = new WebClient();
            Uri uriUserFrage = new Uri("http://www.wurzelpark.at/App/PHP/SaveFrageBeantwortet.php");
            NameValueCollection parameters = new NameValueCollection();

            parameters.Add("User_ID", user.ID.ToString());
            parameters.Add("Frage_ID", fragen[count].ID.ToString());
            parameters.Add("GegebeneAntwort", gegebeneAntwort.ToString());
            parameters.Add("Score", score.ToString());

            userFrage.UploadValuesAsync(uriUserFrage, parameters);
            userFrage.UploadValuesCompleted += UserFrage_UploadValuesCompleted;            
        }

        private void UserFrage_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {
            // NACHDEM der User eine Frage beantwortet hat
            //View schließen
            if (count >= 0)
            {
                if(score == 1)
                /// neue Methode ->nachdem er Score hat                
                    ScoreBekommen(score);                
            }
            else
                BarcodeScannen(user.ID, user.Vorname + " " + user.Nachname, "");
        }

        private void ScoreBekommen(int score)
        {
            if (score == 1)
            {
                count--;
                if (count >= 0)
                    ScoreSuche(user.ID, fragen[count].ID);

                else
                {
                    if (info == "")
                        info = "Du hast hier alle Fragen richtig beantwortet!";

                    BarcodeScannen(user.ID, user.Vorname + " " + user.Nachname, info);
                }
            }

            else
            {
                info = "Du hast nicht alle Fragen richtig beantwortet";
                count--;
                if (count >= 0)
                    Fragebeantworten(count);

                else
                    BarcodeScannen(user.ID, user.Vorname + " " + user.Nachname, info);
            }
            
        }

        private void ClientScore_UploadValuesCompleted(object sender, UploadValuesCompletedEventArgs e)
        {            
            string echo = System.Text.Encoding.UTF8.GetString(e.Result);            

            try
            {
                int score = Int32.Parse(echo);

                if (score == 1)
                    ScoreBekommen(score);
                else
                    Fragebeantworten(count);
            }

            //wenn score keine Zahl
            catch
            {
                Fragebeantworten(count);
            }            
        }        

        private void UserErstellung()
        {
            //View neuen User erstellen
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            user = new User();
            var telephonyManager = (TelephonyManager)GetSystemService(TelephonyService);
            user.IMEI = telephonyManager.DeviceId.ToString();
            user.Vorname = "";
            user.Nachname = "";
            user.Benutzername = "";
            user.Geburtsjahr = 0000;
            user.Gesamtscore = 0;

            buttonErstelleUser = this.FindViewById<Button>(Resource.Id.buttonErstelleUser);
            buttonErstelleUser.Click += ButtonErstelleUser_Click;
        }

        private void BarcodeScannen(int id, string name, string info)
        {
            // Set our view from the "scanview" layout resource
            SetContentView(Resource.Layout.ScanView);            

            TextView textViewWelcomeback = this.FindViewById<TextView>(Resource.Id.textViewWelcomeback);
            textViewWelcomeback.Text = "Willkommen " + name + "! \nDrücke auf Scannen um einen Barcode zu scannen";

            TextView textViewInfo = this.FindViewById<TextView>(Resource.Id.textViewInfo);
            textViewInfo.Text = info;

            buttonScanDefaultView = this.FindViewById<Button>(Resource.Id.buttonScanDefaultView);
            buttonScanDefaultView.Click +=  delegate
            {
                //async delegate WICHTIG
                ////Tell our scanner to use the default overlay
                //scanner.UseCustomOverlay = false;

                ////We can customize the top and bottom text of the default overlay
                //scanner.TopText = "Halte die Kamera über den Barcode\nCa. 15 Zentimeter entfernt";
                //scanner.BottomText = "Warte bis der Barcode gescannt wurde";

                ////Start scanning
                //var result = await scanner.Scan();

                var result = "123456";
                HandleScanResult(result);
            };
        }

        private void Fragebeantworten(int count)
        {
            // neue View (QuizView)
            SetContentView(Resource.Layout.Quizview);

            this.FindViewById<TextView>(Resource.Id.textViewFrage).Text = fragen[count].FrageText;
            this.FindViewById<RadioButton>(Resource.Id.radioButtonAntwort1).Text = fragen[count].Antwort1;
            this.FindViewById<RadioButton>(Resource.Id.radioButtonAntwort2).Text = fragen[count].Antwort2;
            this.FindViewById<RadioButton>(Resource.Id.radioButtonAntwort3).Text = fragen[count].Antwort3;
            this.FindViewById<RadioButton>(Resource.Id.radioButtonAntwort4).Text = fragen[count].Antwort4;

            // Bild laden
            Uri url = new Uri(fragen[count].FrageBildPfad);
            ImageView imagen = this.FindViewById<ImageView>(Resource.Id.imageViewFrage);
            var imageBitmap = GetImageBitmapFromUrl(url.ToString());
            imagen.SetImageBitmap(imageBitmap);


            // Audio abspielen
            string urlAudio = "http://www.wurzelpark.at/App/Audio/D10.mp3"; // your URL here
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.SetDataSource(urlAudio);
            mediaPlayer.Prepare(); // might take long! (for buffering, etc)
            mediaPlayer.Start();

            buttonFragebeantworten = this.FindViewById<Button>(Resource.Id.buttonBeantworten);
            buttonFragebeantworten.Click += ButtonFragebeantworten_Click;
        }        

        private void ScoreSuche(int userID, int frageID)
        {
            //Datenbank Suche user_frage
            WebClient clientScore = new WebClient();
            Uri uriScore = new Uri("http://www.wurzelpark.at/App/PHP/CheckFrageBeantwortet.php");
            NameValueCollection parameters = new NameValueCollection();

            parameters.Add("User_ID", userID.ToString());
            parameters.Add("Frage_ID", frageID.ToString());
            
            clientScore.UploadValuesAsync(uriScore, parameters);
            clientScore.UploadValuesCompleted += ClientScore_UploadValuesCompleted;
        }        

        private void GetFrage(string barcode)
        {
            WebClient FragenClient = new WebClient();
            Uri urlfragen = new Uri("http://www.wurzelpark.at/App/PHP/GetFragen.php");

            NameValueCollection parameters = new NameValueCollection();

            parameters.Add("Barcode", barcode);
            FragenClient.UploadValuesAsync(urlfragen, parameters);
            FragenClient.UploadValuesCompleted += FragenClient_UploadValuesCompleted;
        }

        string HandleScanResult(string result)
        {
            //ZXing.Result
            ///result.text
            string msg = "";

            if (result != null && !string.IsNullOrEmpty(result))
                msg = "Barcode gefunden: " + result;
            else
                msg = "Scannen abgebrochen!";

            GetFrage(result);
            return msg;
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }
    }
}