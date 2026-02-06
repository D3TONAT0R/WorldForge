using System.Text.Json;

namespace WorldForge
{
	public class PlayerAccountData
	{
		public readonly UUID uuid;

		private string? username;
		private Image? avatar;

		private bool usernameRequested = false;
		private bool avatarRequested = false;

		public PlayerAccountData(UUID uuid)
		{
			this.uuid = uuid;
		}

		public void BeginRequest()
		{
			_ = GetUsernameAsync();
			_ = GetAvatarAsync();
		}

		public Image GetAvatar()
		{
			return GetAvatarAsync().Result;
		}


		public async Task<Image> GetAvatarAsync()
		{
			if (avatarRequested) return avatar;
			string url = $"https://mc-heads.net/avatar/{uuid}/8";
			try
			{
				avatarRequested = true;
				using var http = new HttpClient();
				await using var stream = await http.GetStreamAsync(url);
				avatar = Image.FromStream(stream);
			}
			catch (Exception e)
			{
				avatar = null;
			}
			return avatar;
		}

		public string GetUsername()
		{
			return GetUsernameAsync().Result;
		}

		public async Task<string> GetUsernameAsync()
		{
			if(usernameRequested) return username;
			string url = $"https://api.minecraftservices.com/minecraft/profile/lookup/{uuid.ToString(false)}";
			try
			{
				usernameRequested = true;
				using var http = new HttpClient();
				var response = await http.GetStringAsync(url);
				var json = JsonDocument.Parse(response);
				username = json.RootElement.GetProperty("name").GetString();
			}
			catch (Exception e)
			{
				username = "Unknown Username";
			}
			return username;
		}
	}
}