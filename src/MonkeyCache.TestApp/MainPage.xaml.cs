using System;
using System.Text;
using Xamarin.Forms;
using BarrelSQL = MonkeyCache.SQLite.Barrel;
using BarrelFile = MonkeyCache.FileStore.Barrel;
using BarrelLite = MonkeyCache.LiteDB.Barrel;
using BarrelRealm = MonkeyCache.Realm.Barrel;

namespace MonkeyCache.TestApp
{
	public partial class MainPage : ContentPage
	{
		private class Monkey
		{
			public string Name { get; set; }
		}

		private IBarrel sql;
		private IBarrel file;
		private IBarrel lite;
		private IBarrel realm;

		public MainPage()
		{
			InitializeComponent();

			ButtonLoad.Clicked += ButtonLoad_Clicked;
			ButtonSave.Clicked += ButtonSave_Clicked;
			ButtonSaveExpired.Clicked += ButtonSaveExpired_Clicked;

			BarrelLite.ApplicationId = "com.refractored.monkeycachetestlite";
			BarrelFile.ApplicationId = "com.refractored.monkeycachetestfile";
			BarrelSQL.ApplicationId = "com.refractored.monkeycachetestsql";
			BarrelRealm.ApplicationId = "com.refractored.monkeycachetestrealm";

			sql = BarrelSQL.Current;
			lite = BarrelLite.Current;
			file = BarrelFile.Current;
			realm = BarrelRealm.Current;

			ButtonExpired.Clicked += ButtonExpired_Clicked;
			ButtonAllKeys.Clicked += ButtonAllKeys_Clicked;
			ButtonEmpty.Clicked += ButtonEmpty_Clicked;
		}

		private void ButtonEmpty_Clicked(object sender, EventArgs e)
		{
			GetCurrent().EmptyAll();
		}

		private void ButtonAllKeys_Clicked(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Message.Text))
			{
				var state = CacheState.None;
				if (SwitchActive.IsToggled)
					state = state | CacheState.Active;
				if (SwitchExpired.IsToggled)
					state = state | CacheState.Expired;

				var keys = GetCurrent().GetKeys(state);
				var message = new StringBuilder();
				foreach (var item in keys)
				{
					message.Append(item);
					message.Append(Environment.NewLine);
				}

				Message.Text = message.ToString();
			}
			else
			{
				Message.Text = string.Empty;
			}
		}

		private void ButtonExpired_Clicked(object sender, EventArgs e)
		{
			DisplayAlert("Is Expired?", GetCurrent().IsExpired("monkey") ? "Yes" : "No", "OK");
		}

		private IBarrel GetCurrent()
		{
			IBarrel current = null;
			if (UseSQLite.IsToggled)
				current = sql;
			else if (UseFileStore.IsToggled)
				current = file;
			else if (UseLiteDB.IsToggled)
				current = lite;
			else if (UseRealm.IsToggled)
				current = realm;
			else
				current = sql;//fallback

			return current;
		}

		private void AddItem(TimeSpan timeSpan)
		{
			if (string.IsNullOrWhiteSpace(EntryName.Text))
			{
				DisplayAlert("Info", "Please enter a name", "OK");
				return;
			}
			var monkey = new Monkey { Name = EntryName.Text };
			GetCurrent().Add<Monkey>("monkey", monkey, timeSpan);
			DisplayAlert(":)", "Saved!", "OK");
		}

		private void ButtonSave_Clicked(object sender, EventArgs e)
		{
			AddItem(TimeSpan.FromDays(1));
		}

		private void ButtonSaveExpired_Clicked(object sender, EventArgs e)
		{
			AddItem(TimeSpan.FromDays(-1));
		}

		private void ButtonLoad_Clicked(object sender, EventArgs e)
		{
			var monkey = GetCurrent().Get<Monkey>("monkey");
			if (monkey == null)
				DisplayAlert(":(", "No Monkey", "OK");
			else
			{
				DisplayAlert(":)", monkey.Name, "OK");
				//var expired = GetCurrent()GetExpiration("monkey");
			}
		}

		private void SwitchAutoExpire_Toggled(object sender, ToggledEventArgs e)
		{
			GetCurrent().AutoExpire = e.Value;
		}
	}
}