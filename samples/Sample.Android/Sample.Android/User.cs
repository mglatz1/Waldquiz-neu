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
    class User
    {
        public int ID { get; set; }
        public string IMEI { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public int Geburtsjahr { get; set; }
        public string Benutzername { get; set; }
        public int Gesamtscore { get; set; }
    }
}