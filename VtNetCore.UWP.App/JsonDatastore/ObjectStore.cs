namespace VtNetCore.UWP.App.JsonDatastore
{
    using System;
    using System.Collections.Generic;

    public abstract class ObjectStore
    {
        private static readonly Lazy<ObjectStore> lazy =
            new Lazy<ObjectStore>(
                () => new UwpObjectStore()
            );

        public static ObjectStore Current { get { return lazy.Value; } }

        public abstract void WriteObject(string folderPath, object toWrite);

        public abstract void RemoveObject(string folderPath, object toRemove);

        public abstract IEnumerable<T> ReadAllObjects<T>(string folderPath);
    }
}
