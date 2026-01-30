using System.Runtime.InteropServices;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForgeToolbox
{
	public class RegionMapCache
	{
		public class RegionEntry
		{
			public Bitmap bitmap;
			public DateTime regionTimestamp;
			public bool renderComplete = false;

			public RegionEntry(Bitmap bitmap, DateTime timestamp, bool renderComplete)
			{
				this.bitmap = bitmap;
				regionTimestamp = timestamp;
				this.renderComplete = renderComplete;
			}

			public NBTCompound Serialize()
			{
				var nbt = new NBTCompound();
				nbt.Add("timestamp", regionTimestamp.ToBinary());
				nbt.Add("resolution", bitmap.Width);

				var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				var length = bitmapData.Stride * bitmapData.Height;
				byte[] pixelData = new byte[length];
				// Copy bitmap to byte[]
				Marshal.Copy(bitmapData.Scan0, pixelData, 0, length);
				bitmap.UnlockBits(bitmapData);
				nbt.Add("bitmap", pixelData);
				return nbt;
			}

			public static RegionEntry Deserialize(NBTCompound nbt)
			{
				var timestamp = DateTime.FromBinary(nbt.Get<long>("timestamp"));
				var resolution = nbt.Get<int>("resolution");
				var pixelData = nbt.Get<byte[]>("bitmap");
				Bitmap bitmap = new Bitmap(resolution, resolution, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				var bitmapData = bitmap.LockBits(new Rectangle(0, 0, resolution, resolution), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				var length = bitmapData.Stride * bitmapData.Height;
				// Copy byte[] to bitmap
				Marshal.Copy(pixelData, 0, bitmapData.Scan0, length);
				bitmap.UnlockBits(bitmapData);
				return new RegionEntry(bitmap, timestamp, true);
			}
		}

		public readonly Dictionary<RegionLocation, RegionEntry> cache = new();

		public Bitmap? Get(RegionLocation location)
		{
			return cache.GetValueOrDefault(location)?.bitmap;
		}

		public bool TryGet(RegionLocation regionRegionPos, out Bitmap bitmap)
		{
			if(cache.TryGetValue(regionRegionPos, out var entry))
			{
				bitmap = entry.bitmap;
				return true;
			}
			bitmap = null!;
			return false;
		}

		public bool Contains(RegionLocation location)
		{
			return cache.ContainsKey(location);
		}

		public void Set(RegionLocation location, Bitmap bitmap, DateTime timestamp, bool renderComplete)
		{
			cache[location] = new RegionEntry(bitmap, timestamp, renderComplete);
		}

		public void Save(string filename)
		{
			var nbt = new NBTCompound();
			var entries = nbt.AddCompound("entries");
			foreach (var kvp in cache)
			{
				if (!kvp.Value.renderComplete) continue; // Skip incomplete renders
				string key = $"{kvp.Key.x},{kvp.Key.z}";
				entries.Add(key, kvp.Value.Serialize());
			}
			var file = new NBTFile(nbt, null);
			file.Save(filename, false);
		}

		public static RegionMapCache Load(string filename)
		{
			var file = new NBTFile(filename);
			var nbt = file.contents;
			var entriesNbt = nbt.Get<NBTCompound>("entries");
			var cache = new RegionMapCache();
			foreach (var key in entriesNbt.contents.Keys)
			{
				var split = key.Split(',');
				var location = new RegionLocation(int.Parse(split[0]), int.Parse(split[1]));
				var entryNbt = entriesNbt.Get<NBTCompound>(key);
				var entry = RegionEntry.Deserialize(entryNbt);
				cache.cache[location] = entry;
			}
			return cache;
		}

		public void MarkRenderCompleted(RegionLocation regionRegionPos)
		{
			cache[regionRegionPos].renderComplete = true;
		}
	}
}