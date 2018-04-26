namespace VtNetCore.UWP.App.JsonDatastore
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Windows.Storage;

    public class UwpObjectStore : ObjectStore
    {
        public override void WriteObject(string folderPath, object toWrite)
        {
            var scheme = folderPath.Split(":").First();

            switch(scheme)
            {
                case "local":
                    Task.Run(() =>
                        WriteObjectLocal(folderPath, toWrite)
                    )
                    .Wait();

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public async override Task WriteObjectAsync(string folderPath, object toWrite)
        {
            var scheme = folderPath.Split(":").First();

            switch (scheme)
            {
                case "local":
                    await WriteObjectLocal(folderPath, toWrite);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public override void RemoveObject(string folderPath, object toRemove)
        {
            var scheme = folderPath.Split(":").First();

            switch (scheme)
            {
                case "local":
                    Task.Run(() =>
                        RemoveObjectLocal(folderPath, toRemove)
                    )
                    .Wait();

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private async Task WriteObjectLocal(string folderPath, object toWrite)
        {
            // Extract the ID property which will be used for the filename
            Type objectType = toWrite.GetType();
            IList<PropertyInfo> objectProperties = new List<PropertyInfo>(objectType.GetProperties());

            var idProperty = objectProperties.SingleOrDefault(x => x.Name == "Id");

            // If there's no Guid property with the name "Id" throw an error.
            if (idProperty == null || idProperty.PropertyType != typeof(Guid))
                throw new ArgumentException("Cannot serialize an object which lacks a index field of type Guid named 'Id'");

            var idValue = (Guid)idProperty.GetValue(toWrite);

            // Convert the object to JSON
            var jsonText = JsonConvert.SerializeObject(toWrite);

            // Create the folder relative to the local data path for the UWP app.
            var localFolder = ApplicationData.Current.LocalFolder;

            var idString = toWrite.GetType().ToString().Split(".").Last();
            var destinationPath = string.Join(@"\", folderPath.Split(":").Skip(1).Append(idString));

            var objectFolder = await localFolder.CreateFolderAsync(destinationPath, CreationCollisionOption.OpenIfExists);

            // Write the data to the file.
            var outputFile = await objectFolder.CreateFileAsync(idValue.ToString() + ".json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(outputFile, jsonText);
        }

        private async Task RemoveObjectLocal(string folderPath, object toWrite)
        {
            // Extract the ID property which will be used for the filename
            Type objectType = toWrite.GetType();
            IList<PropertyInfo> objectProperties = new List<PropertyInfo>(objectType.GetProperties());

            var idProperty = objectProperties.SingleOrDefault(x => x.Name == "Id");

            // If there's no Guid property with the name "Id" throw an error.
            if (idProperty == null || idProperty.PropertyType != typeof(Guid))
                throw new ArgumentException("Cannot serialize an object which lacks a index field of type Guid named 'Id'");

            var idValue = (Guid)idProperty.GetValue(toWrite);

            // Get the folder relative to the path
            var localFolder = ApplicationData.Current.LocalFolder;

            var idString = toWrite.GetType().ToString().Split(".").Last();
            var destinationPath = string.Join(@"\", folderPath.Split(":").Skip(1).Append(idString));

            var objectFolder = await localFolder.GetFolderAsync(destinationPath);
            if (objectFolder == null)
                return;     // Consider throwing if the folder doesn't exist

            // Get the file from the folder
            var existingFile = await objectFolder.GetFileAsync(idValue.ToString() + ".json");

            if (existingFile == null)
                return;     // Consider throwing if the file doesn't exist

            // Delete the file
            await existingFile.DeleteAsync();
        }

        public override IEnumerable<T> ReadAllObjects<T>(string folderPath)
        {
            var scheme = folderPath.Split(":").First();

            switch (scheme)
            {
                case "local":
                    var readTask = Task.Run(() =>
                        ReadAllObjectsLocal<T>(folderPath)
                    );

                    readTask.Wait();

                    if (readTask.IsCompletedSuccessfully)
                        return readTask.Result;

                    break;
            }

            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> ReadAllObjectsLocal<T>(string folderPath)
        {
            var idString = typeof(T).ToString().Split(".").Last();

            var localFolder = ApplicationData.Current.LocalFolder;
            var destinationPath = string.Join(@"\", folderPath.Split(":").Skip(1).Append(idString));
            var objectFolder = await localFolder.CreateFolderAsync(destinationPath, CreationCollisionOption.OpenIfExists);

            var result = new List<T>();

            if (objectFolder == null)
                return result;

            var files = await objectFolder.GetFilesAsync();
            foreach (var file in files.Where(x => x.FileType.ToLower() == ".json"))
            {
                var content = await FileIO.ReadTextAsync(file);

                var deserialized = JsonConvert.DeserializeObject<T>(content);
                result.Add(deserialized);
            }

            return result;
        }
    }
}
