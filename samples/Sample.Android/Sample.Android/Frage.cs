using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Sample.Android
{
    class Frage
    {
        public int ID { get; set; }
        public int Barcode { get; set; }
        public string FrageText { get; set; }
        public string FrageBildPfad { get; set; }
        public string FrageAudioPfad { get; set; }
        public string Antwort1 { get; set; }
        public string Antwort2 { get; set; }
        public string Antwort3 { get; set; }
        public string Antwort4 { get; set; }
        public int RichtigeAntwort { get; set; }
    }
}