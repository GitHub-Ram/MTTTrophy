
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

namespace MTTrophy
{
    [Activity(Theme = "@style/AppThemeActivity", Label = "TrophyList")]
    public class TrophyList : Activity
    {
        ListView trophyList;
        List<MyTrophyModel> TrophyAdapterList;
        MyTrophiesAdaptor trophyAdapter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.my_trouphy);
            #region Find Controls
            trophyList = FindViewById<ListView>(Resource.Id.ListViewTrophy);
            //trophyList.ItemClick -= OnTrouphyListItemClick;
            TrophyAdapterList = new List<MyTrophyModel>() {
                    new MyTrophyModel{
                        TrouphyName = "Fly To Vegas Finale",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "Beast",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "Mini Millions",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "God Father",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "Fly To Vegas Finale",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "Beast",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "Mini Millions",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "God Father",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "Fly To Vegas Finale",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "Beast",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "Mini Millions",
                        WinningText = ""
                    },
                    new MyTrophyModel{
                        TrouphyName = "God Father",
                        WinningText = ""
                    }
                };
            trophyList.ItemClick += OnTrouphyListItemClick;
            trophyList.ChoiceMode = ChoiceMode.Single;
            trophyAdapter = new MyTrophiesAdaptor(this, TrophyAdapterList);
            trophyList.Adapter = trophyAdapter;
            #endregion
        }

        private void OnTrouphyListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            MyTrophyModel model = TrophyAdapterList[e.Position];
            this.RunOnUiThread(() =>
            {
                Intent intent = new Intent(this, typeof(TrophyCameraActivity));
                intent.PutExtra("TrophyName", model.TrouphyName);
                this.StartActivity(intent);
            });
        }
    }

    public class MyTrophyModel
    {
        public string TrouphyName { set; get; }
        public string WinningText { set; get; }
    }
}
