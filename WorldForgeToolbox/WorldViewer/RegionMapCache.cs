using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using WorldForge.Coordinates;
using WorldForge.NBT;

namespace WorldForgeToolbox
{
	public class RegionMapCache
	{
		public class Entry
		{
			public Render normalRender;
			public Render? highQualityRender;
			public DateTime regionTimestamp;

			public Entry(Render normalRender, DateTime timestamp)
			{
				this.normalRender = normalRender;
				regionTimestamp = timestamp;
			}

			public NBTCompound Serialize()
			{
				var nbt = new NBTCompound();
				nbt.Add("timestamp", regionTimestamp.ToBinary());
				nbt.Add("resolution", normalRender.bitmap.Width);

				var bitmap = normalRender.bitmap;
				var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				var length = bitmapData.Stride * bitmapData.Height;
				byte[] pixelData = new byte[length];
				// Copy bitmap to byte[]
				Marshal.Copy(bitmapData.Scan0, pixelData, 0, length);
				bitmap.UnlockBits(bitmapData);
				nbt.Add("bitmap", pixelData);
				return nbt;
			}

			public static Entry Deserialize(NBTCompound nbt)
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
				return new Entry(Render.CreateCompleted(bitmap), timestamp);
			}
		}

		public readonly Dictionary<RegionLocation, Entry> cache = new();

		public Render? Get(RegionLocation location)
		{
			return cache.GetValueOrDefault(location)?.normalRender;
		}

		public bool TryGet(RegionLocation regionPos, out Entry entry)
		{
			if(cache.TryGetValue(regionPos, out entry))
			{
				return true;
			}
			entry = null!;
			return false;
		}

		public bool Contains(RegionLocation location)
		{
			return cache.ContainsKey(location);
		}

		public void Save(string filename)
		{
			// Serialize bitmaps in parallel
			var serialized = new ConcurrentDictionary<RegionLocation, NBTCompound>();
			Parallel.ForEach(cache, kvp =>
			{
				if (!kvp.Value.normalRender.renderComplete) return; // Skip incomplete renders
				serialized.TryAdd(kvp.Key, kvp.Value.Serialize());
			});
			// Create NBT structure
			var nbt = new NBTCompound();
			var entries = nbt.AddCompound("entries");
			foreach (var kvp in serialized)
			{
				string key = $"{kvp.Key.x},{kvp.Key.z}";
				entries.Add(key, serialized[kvp.Key]);
			}
			// Save to file
			var file = new NBTFile(nbt, null);
			file.Save(filename, false);
		}

		public void Clear()
		{
			cache.Clear();
		}

		public static RegionMapCache Load(string filename)
		{
			var file = new NBTFile(filename);
			var nbt = file.contents;
			var entriesNbt = nbt.Get<NBTCompound>("entries");
			var cache = new RegionMapCache();
			var deserialized = new ConcurrentDictionary<RegionLocation, Entry>();
			// Deserialize bitmaps in parallel
			Parallel.ForEach(entriesNbt.contents, kvp =>
			{
				var key = kvp.Key;
				var split = key.Split(',');
				var location = new RegionLocation(int.Parse(split[0]), int.Parse(split[1]));
				var entryNbt = entriesNbt.Get<NBTCompound>(key);
				var entry = Entry.Deserialize(entryNbt);
				deserialized.TryAdd(location, entry);
			});
			// Copy deserialized entries to cache
			foreach (var kvp in deserialized)
			{
				cache.cache[kvp.Key] = kvp.Value;
			}
			return cache;
		}
	}
}