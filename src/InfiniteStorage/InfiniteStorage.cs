namespace InfiniteStorage
{
    public class InfiniteStorage : KMonoBehaviour
    {
        private FilteredStorage filteredStorage;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            filteredStorage = new FilteredStorage(this, null, null, null, false, Db.Get().ChoreTypes.StorageFetch);
        }

        protected override void OnSpawn() { filteredStorage.FilterChanged(); }
    }
}
